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
    
    public class SitcomContext : System.IDisposable
    {
       
        SitcomFile mFile;
        SitcomCmd mCmd;
        public string Name { get { return mFile.File; } }
        SitcomHeap mHeap;
        public SitcomHeap Heap { get { return mHeap; } }
        ISitcomResult mResult;
        ISitcomTask mTask;
        public ISitcomResult Result { get { return mResult; } }

        Queue<ISitcomExec> mExecs;
        Stack<ISitcomExec> mStack;

        bool mDontResetFile;
        public int EndMarkId { get; private set; }
        public int NextMarkId { get; set; }
        public void SetNextMark(string mark)
        {
            NextMarkId = mark == null ? 0 : StringUtil.IgnoreCaseToHash(mark);
        }
        public bool SelectMark(string mark)
        {
            var id = string.IsNullOrEmpty(mark) ? 0 : StringUtil.IgnoreCaseToHash(mark);
            if(IsPlaying)
            {
                NextMarkId = id;
                return SelectNextMark();
            }
            NextMarkId = 0;
            mDontResetFile = true;
            mFile.BeginRead();
            while (!mFile.Eof && mFile.NextMark('#'))
            {
                if (mFile.NextKeywords() && mFile.keyword.id == id)
                    return true;
            }
            return false;
        }

        public string FileName { get { return mFile == null ? "" : mFile.File; } }
        public bool IsLoaded { get; private set; }
        bool mPlaying;
        public bool IsPlaying
        {
            get
            { return mPlaying; }
            set
            {
                if (!IsLoaded || value == mPlaying)
                    return;
                mPlaying = value;
                if (value)
                    OnStart();
                else
                    OnStop();
            }
        }

        public bool IsComplete { get; private set; }

        public SitcomContext()
        {
            mFile = new SitcomFile();
            mHeap = new SitcomHeap();
            mCmd = new SitcomCmd();
            mExecs = new Queue<ISitcomExec>(32);
            mStack = new Stack<ISitcomExec>(32);
            IsComplete = true;
            EndMarkId = StringUtil.IgnoreCaseToHash("end");
        }

        public void Load(TextAsset asset)
        {
            Heap.ResetStack();
            mFile.Load(asset);
            IsPlaying = false;
            IsComplete = false;
            IsLoaded = true;
            mDontResetFile = false;
        }

        public void Load(string text)
        {
            Heap.ResetStack();
            mFile.Load(text);
            IsPlaying = false;
            IsLoaded = true;
            mDontResetFile = false;
        }

        protected virtual void OnStop()
        {
            mExecs.Clear();
            mStack.Clear();
            mResult = null;
            mTask = null;
            mDontResetFile = false;
            Heap.ResetStack();
        }

        protected virtual void OnStart()
        {
            Heap.BeginStack();
            Heap.Alloc("context", this);
            if(!mDontResetFile)
                mFile.BeginRead();
            mDontResetFile = false;
            IsComplete = false;
        }
        
        public void Push(ISitcomExec exec)
        {
            if (exec == null || !mPlaying || IsComplete)
                return;
            mExecs.Enqueue(exec);
        }

        bool SelectNextMark()
        {
            if (!IsPlaying || NextMarkId == EndMarkId)
                return false;
            if (NextMarkId == 0)
                return true;
            var line = mFile.PresentLine;
            while (!mFile.Eof && mFile.NextMark('#'))
            {
                if (mFile.NextKeywords() && mFile.keyword.id == NextMarkId)
                {
                    NextMarkId = 0;
                    return true;
                }
            }
            mFile.BeginRead();
            while (mFile.PresentLine < line && mFile.NextMark('#'))
            {
                if (mFile.NextKeywords() && mFile.keyword.id == NextMarkId)
                {
                    NextMarkId = 0;
                    return true;
                }
            }
            return false;
        }

        public virtual void Update(float deltaTime)
        {
            if (!mPlaying || IsComplete)
                return;
            if (mTask != null)
                mTask.Update(deltaTime);
            if (mResult != null && mResult.State == ESitcomState.Doing)
                return;
            while (true)
            {
                while (mExecs.Count > 0)
                {
                    var t = mExecs.Dequeue();
                    t.OnExecute(this);
                    mStack.Push(t);
                }
                if (mStack.Count > 0)
                {
                    var t = mStack.Pop();
                    t.OnStop(this);
                    mResult = t.Result;
                    if (mResult != null && mResult.State == ESitcomState.Doing)
                    {
                        mTask = mResult as ISitcomTask;
                        return;
                    }
                    mTask = null;
                }
                else
                {
                    break;
                }
            }
            if (SelectNextMark() && mCmd.Read(mFile))
            {
                Push(mCmd);
            }
            else
            {
                IsPlaying = false;
                IsComplete = true;
            }
        }

        public virtual void Dispose()
        {
            IsPlaying = false;
            IsComplete = false;
            mFile = null;
            mHeap = null;
            mCmd = null;
            mResult = null;
        }
	}
}