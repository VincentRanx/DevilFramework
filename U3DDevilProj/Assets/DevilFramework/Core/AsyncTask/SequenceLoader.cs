using System.Collections.Generic;
using UnityEngine;

namespace Devil.AsyncTask
{
    public class SequenceLoader : IAsyncTask
    {
        class CallbackTask : IAsyncTask
        {
            public System.Action callback;

            public float Progress { get { return IsDone ? 1 : 0; } }

            public bool IsDone { get; private set; }

            public CallbackTask() { }
            public CallbackTask(System.Action callback) { this.callback = callback; }

            public void Abort() { }

            public void OnTick(float deltaTime) { }

            public void Reset() { IsDone = false; }

            public void Start()
            {
                IsDone = true;
                if (callback != null)
                    callback();
            }
        }

        List<IAsyncTask> mTasks = new List<IAsyncTask>();
        float mProgressScale;
        int mCurrentTask;
        bool mStarted;

        public float Progress { get; private set; }

        public bool IsDone { get; private set; }

        public void Abort()
        {
            if (mStarted)
            {
                mStarted = false;
                if (mCurrentTask < mTasks.Count)
                    mTasks[mCurrentTask].Abort();
            }
        }

        public void AddTask(IAsyncTask task)
        {
            mTasks.Add(task);
            mProgressScale = 1f / mTasks.Count;
        }

        public void AddCallback(System.Action callback)
        {
            if (callback != null)
            {
                var t = mTasks.Count > 0 ? mTasks[mTasks.Count - 1] : null;
                var task = t as CallbackTask;
                if (task == null)
                {
                    task = new CallbackTask(callback);
                    AddTask(task);
                }
                else
                    task.callback += callback;
            }
        }

        public void Reset()
        {
            Abort();
            mTasks.Clear();
        }

        float CalculateProgress()
        {
            return mTasks[mCurrentTask].Progress * mProgressScale + mCurrentTask * mProgressScale;
        }

        public void OnTick(float deltaTime)
        {
            if (mCurrentTask < mTasks.Count)
            {
                var t = mTasks[mCurrentTask];
                t.OnTick(deltaTime);
                Progress = Mathf.Max(Progress, CalculateProgress());
                if(t.IsDone)
                {
                    mCurrentTask++;
                    if (mCurrentTask < mTasks.Count)
                        mTasks[mCurrentTask].Start();
                }
            }
            else
            {
                Progress = 1;
                IsDone = true;
            }
        }

        public void Start()
        {
            if(!mStarted)
            {
                mStarted = true;
                mCurrentTask = 0;
                Progress = 0;
                if(mCurrentTask < mTasks.Count)
                {
                    mTasks[mCurrentTask].Start();
                }
            }
        }
    }
}