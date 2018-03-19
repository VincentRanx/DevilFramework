
using System.Collections.Generic;

namespace Devil.AsyncTask
{
    public class AsyncLoader : ITick
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

        public event System.Action OnAllTaskComplish = ()=> { };

        public float Progress { get; private set; }
        public bool IsDone { get; private set; }
        public bool HasTask { get { return mTasks.Count > 0; } }

        public AsyncLoader()
        {
            mTasks = new List<Task>();
            mLock = new object();
            IsDone = false;
            Progress = 0;
        }

        public void Reset()
        {
            lock (mLock)
            {
                if (!IsDone)
                {
                    for (int i = mTasks.Count - 1; i >= 0; i--)
                    {
                        Task task = mTasks[i];
                        if (!task.isDone && !task.task.IsDone)
                            task.task.Interrupt();
                    }
                }
                mTasks.Clear();
                IsDone = false;
                Progress = 0;
                mTotalWeight = 0;
            }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 添加任务列表
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="weight"></param>
        /// <returns>开始任务数量</returns>
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

        /// <summary>
        /// 刷新
        /// </summary>
        public void OnTick(float deltaTime)
        {
            lock (mLock)
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
                        task.task.OnTick(deltaTime);
                        f += w * task.task.Progress;
                    }
                }
                if (doneNum == mTasks.Count)
                {
                    Progress = 1;
                    IsDone = true;
                    OnAllTaskComplish();
                }
                else if (f > Progress)
                {
                    Progress = f;
                }
            }
        }

        public bool IsDoneExcept(FilterDelegate<IAsyncTask> filter)
        {
            for(int i = 0; i < mTasks.Count; i++)
            {
                Task task = mTasks[i];
                if (filter(task.task))
                    continue;
                if (!task.isDone)
                    return false;
            }
            return true;
        }
    }

}
