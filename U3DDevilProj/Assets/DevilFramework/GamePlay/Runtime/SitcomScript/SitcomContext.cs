using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public interface ISitcomExec
    {
        void OnExecute(SitcomContext runtime);
        void OnStop(SitcomContext runtime);
        ISitcomResult Result { get; }
    }
    
    public class SitcomContext : System.IDisposable , ISitcomTask
    {
       
        SitcomCmdSequence mFile;
        //SitcomCmd mCmd;
        public string Name { get { return mFile.File; } }
        SitcomHeap mHeap;
        public SitcomHeap Heap { get { return mHeap; } }
        ISitcomResult mResult;
        ISitcomTask mTask;
        public ISitcomResult SitcomResult { get { return mResult; } }
        public object Result { get { return mResult; } }
        public ESitcomState State { get { return IsComplete ? (mResult == null ? ESitcomState.Failed : mResult.State) : ESitcomState.Doing; } }

        Queue<ISitcomExec> mExecs;
        Stack<ISitcomExec> mStack;
        List<SitcomCmdSequence> mFileStack;

        public void SetNextMark(int id)
        {
            if (mFileStack.Count > 0)
                mFileStack[mFileStack.Count - 1].SetNextMark(id);
            else
                mFile.SetNextMark(id);
        }

        public void SetNextMark(string mark)
        {
            SetNextMark(StringUtil.IgnoreCaseToHash(mark));
        }

        public bool SelectMark(string mark)
        {
            return mFile.SelectMark(mark);
        }

        public string FileName { get { return mFile == null ? "" : mFile.File; } }
        int mAssetId;
       
        public bool IsComplete { get; private set; }

        public SitcomContext()
        {
            mFile = new SitcomCmdSequence();
            mHeap = new SitcomHeap();
            mExecs = new Queue<ISitcomExec>(32);
            mStack = new Stack<ISitcomExec>(32);
            mFileStack = new List<SitcomCmdSequence>();
            IsComplete = true;
        }
        
        public void Stop()
        {
            if(!IsComplete)
            {
                OnStop();
                IsComplete = true;
            }
        }

        public void Load(TextAsset asset)
        {
            if (mAssetId != asset.GetInstanceID())
            {
                mAssetId = asset.GetInstanceID();

                if (!IsComplete)
                    OnStop();
                mFile.Load(asset);
                if (!mFile.Eof)
                {
                    OnStart();
                    Push(mFile);
                }
            }
        }
        
        protected virtual void OnStop()
        {
            mExecs.Clear();
            mStack.Clear();
            mFileStack.Clear();
            mResult = null;
            mTask = null;
            Heap.ResetStack();
            mFile.SetNextMark(0);
            IsComplete = true;
        }

        protected virtual void OnStart()
        {
            IsComplete = false;
            Heap.BeginStack();
            Heap.Alloc("context", this);
        }
        
        public void Push(ISitcomExec exec)
        {
            if (exec == null || IsComplete)
                return;
            mExecs.Enqueue(exec);
        }
        
        public virtual void Update(float deltaTime)
        {
            if (IsComplete)
                return;
            if (mTask != null)
                mTask.Update(deltaTime);
            if (mResult != null && mResult.State == ESitcomState.Doing)
                return;
            while (mExecs.Count > 0 || mStack.Count > 0)
            {
                while (mExecs.Count > 0)
                {
                    var t = mExecs.Dequeue();
                    t.OnExecute(this);
                    if (t is SitcomCmdSequence)
                        mFileStack.Add(t as SitcomCmdSequence);
                    mStack.Push(t);
                }
                if (mStack.Count > 0)
                {
                    var t = mStack.Pop();
                    t.OnStop(this);
                    if (t is SitcomCmdSequence)
                        mFileStack.Remove(t as SitcomCmdSequence);
                    mResult = t.Result;
                    if (mResult != null && mResult.State == ESitcomState.Doing)
                    {
                        mTask = mResult as ISitcomTask;
                        return;
                    }
                    mTask = null;
                }
            }
            OnStop();
        }

        public virtual void Dispose()
        {
            IsComplete = false;
            mFile = null;
            mHeap = null;
            mResult = null;
        }
	}
}