
using System.Collections.Generic;

namespace DevilTeam.AsyncTask
{
    public class AsyncLoader
    {

        private class Task
        {
            public IAsyncTask task;
            public float weight = 1;
            public bool isDone = false;
        }

        private object mLock;
        private List<Task> mTasks;
        private float mTotalWeight;

        public event System.Action OnAllTaskComplish;

        public float Progress { get; private set; }
        public bool IsDone { get; private set; }

        public AsyncLoader()
        {
            mTasks = new List<Task>();
            mLock = new object();
        }

        public bool AddTask(IAsyncTask task, float weight = 1)
        {
            lock (mLock)
            {
                if (IsDone || task == null || task.IsDone || weight <= 0)
                    return false;
                Task t = new Task();
                t.task = task;
                t.weight = weight;
                mTotalWeight += weight;
                mTasks.Add(t);
                return true;
            }
        }

        public int AddTasks(IList<IAsyncTask> tasks, float weight = 1)
        {
            lock (mLock)
            {
                if (IsDone || weight <= 0 || tasks == null || tasks.Count == 0)
                    return 0;
                float eweight = weight / tasks.Count;
                int len = 0;
                for(int i = 0; i < tasks.Count; i++)
                {
                    IAsyncTask task = tasks[i];
                    if (task != null && !task.IsDone)
                    {
                        Task t = new Task();
                        t.task = task;
                        t.weight = eweight;
                        mTotalWeight += eweight;
                        mTasks.Add(t);
                        len++;
                    }
                }
                return len;
            }
        }

        // 刷新
        public void Validate()
        {
            if (IsDone || mTotalWeight <= 0)
                return;
            float f = 0;
            int doneNum = 0;
            for (int i = 0; i < mTasks.Count; i++)
            {
                Task task = mTasks[i];
                float w = task.weight / mTotalWeight;
                if (task.isDone || task.task.IsDone)
                {
                    task.isDone = true;
                    doneNum++;
                    f += w;
                }
                else
                {
                    f += w * task.task.Progress;
                }
            }
            if (doneNum == mTasks.Count)
            {
                Progress = 1;
                IsDone = true;
                if (OnAllTaskComplish != null)
                    OnAllTaskComplish();
            }
            else if (f > Progress)
            {
                Progress = f;
            }
        }
    }

}
