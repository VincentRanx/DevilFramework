using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam
{

    public class MainThread : MonoBehaviour
    {
        private static MainThread sInstance;
        private static bool sInitilized;
        private static Queue<Task> sTasks;

        public static void Install()
        {
            if (!sInstance)
            {
                sTasks = new Queue<Task>();
                GameObject obj = new GameObject("[MainThread]");
                sInstance = obj.AddComponent<MainThread>();
                DontDestroyOnLoad(obj);
                sInitilized = true;
            }
        }

        public static void RunOnMainThread(System.Action action)
        {
            if (sInitilized && action != null)
            {
                lock (sInstance.mLock)
                {
                    TaskWithoutArg task = new TaskWithoutArg();
                    task.mTask = action;
                    sTasks.Enqueue(task);
                }
            }
        }

        public static void RunOnMainThread<T>(System.Action<T> action, T arg)
        {
            if (sInitilized && action != null)
            {
                lock (sInstance.mLock)
                {
                    TaskWithArg<T> task = new TaskWithArg<T>();
                    task.mTask = action;
                    task.mArg = arg;
                    sTasks.Enqueue(task);
                }
            }
        }

        private abstract class Task
        {
            public abstract void Execute();
        }

        private class TaskWithoutArg : Task
        {
            public System.Action mTask;

            public override void Execute()
            {
                mTask();
            }
        }

        private class TaskWithArg<T> :Task
        {
            public T mArg;
            public System.Action<T> mTask;

            public override void Execute()
            {
                mTask(mArg);
            }
        }

        private object mLock = new object();

        private void Awake()
        {
            if (!sInstance)
            {
                sInstance = this;
                if(sTasks == null)
                {
                    sTasks = new Queue<Task>();
                }
                DontDestroyOnLoad(gameObject);
                sInitilized = true;
            }
        }

        private void OnDestroy()
        {
            if(sInstance == this)
            {
                sInstance = null;
                sInitilized = false;
                sTasks.Clear();
                sTasks = null;
            }
        }

        private void Update()
        {
            lock (mLock)
            {
                while (sTasks.Count > 0)
                {
                    Task task = sTasks.Dequeue();
                    task.Execute();
                }
            }
        }
    }
}