using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-1000)]
    public class MainThread : MonoBehaviour
    {
        public delegate bool LooperDelegate(float deltatime);
        
        public enum ETaskType
        {
            task,
            lateTask,
        }

        public interface ILooper : ITick
        {
            bool IsActive { get; }
            bool LoopOnce { get; }
        }

        private interface ITask : IIdentified
        {
            void Execute();
        }

        private class Looper : ILooper
        {
            LooperDelegate mDelegate;
            public Looper(LooperDelegate dele)
            {
                mDelegate = dele;
                IsActive = dele != null;
            }

            public bool IsActive { get; private set; }

            public bool LoopOnce { get { return true; } }

            public void OnTick(float deltaTime)
            {
                IsActive = mDelegate(deltaTime);
            }
        }

        private class TaskWithoutArg : ITask
        {
            public int Identify { get; set; }
            Object mTarget;
            System.Action mTask;
            bool mCheckTarget;
            public System.Action Task
            {
                get { return mTask; }
                set
                {
                    mTask = value;
                    mTarget = value.Target as Object;
                    mCheckTarget = mTarget != null;
                }
            }

            public TaskWithoutArg() { }

            public TaskWithoutArg(int id)
            {
                Identify = id;
            }

            public void Execute()
            {
                if (!mCheckTarget || mTarget != null)
                    Task();
            }
        }

        private class TaskWithArg<T> : ITask
        {
            public T mArg;
            bool mCheckTarget;
            Object mTarget;
            System.Action<T> mTask;
            public System.Action<T> Task
            {
                get { return mTask; }
                set
                {
                    mTask = value;
                    mTarget = value.Target as Object;
                    mCheckTarget = mTarget != null;
                }
            }
            public int Identify { get; set; }

            public TaskWithArg() { }

            public TaskWithArg(int id)
            {
                Identify = id;
            }
            public void Execute()
            {
                if (!mCheckTarget || mTarget != null)
                    mTask(mArg);
            }
        }

        private class TaskWithArg2<T1, T2> : ITask
        {
            public T1 mArg1;
            public T2 mArg2;
            bool mCheckTarget;
            Object mTarget;
            System.Action<T1, T2> mTask;
            public System.Action<T1, T2> Task
            {
                get { return mTask; }
                set
                {
                    mTask = value;
                    mTarget = value.Target as Object;
                    mCheckTarget = mTarget != null;
                }
            }
            public int Identify { get; set; }
            public TaskWithArg2() { }

            public TaskWithArg2(int id)
            {
                Identify = id;
            }
            public void Execute()
            {
                if (!mCheckTarget || mTarget != null)
                    mTask(mArg1, mArg2);
            }
        }

        private static MainThread sInstance;
        private static bool sInitilized;
        public static bool IsInitilized { get { return sInitilized; } }

        public static void Install()
        {
            if (null == sInstance)
            {
                GameObject obj = new GameObject("[MainThread]");
                sInstance = obj.AddComponent<MainThread>();
                DontDestroyOnLoad(obj);
                sInitilized = true;
            }
        }

        public static ILooper AddLooper(LooperDelegate looper)
        {
            if (sInitilized && looper != null)
            {
                var loop = new Looper(looper);
                sInstance.mLoopers.AddLast(loop);
                return loop;
            }
            else
            {
#if UNITY_EDITOR
                if (!sInitilized && Application.isPlaying)
                    RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
                return null;
            }
        }

        public static void AddLooper(ILooper looper)
        {
            if (sInitilized && looper != null)
            {
                if (!sInstance.mLoopers.Contains(looper))
                {
                    sInstance.mLoopers.AddLast(looper);
                }
            }
#if UNITY_EDITOR
            else if (!sInitilized && Application.isPlaying)
                RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
        }

        public static void RemoveLooper(ILooper looper)
        {
            if(sInitilized && looper != null)
            {
                sInstance.mLoopers.Remove(looper);
            }
#if UNITY_EDITOR
            else if (!sInitilized && Application.isPlaying)
                RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
        }

        public static bool QueryTask(int id, ETaskType type = ETaskType.task)
        {
            if(sInitilized && id != 0)
            {
                lock (sInstance.mLock)
                {
                    Queue<ITask> queue;
                    if (type == ETaskType.task)
                        queue = sInstance.mTasks;
                    else if (type == ETaskType.lateTask)
                        queue = sInstance.mLateTasks;
                    else
                        queue = null;
                    if(queue != null)
                    {
                        foreach(var t in queue)
                        {
                            if (t.Identify == id)
                                return true;
                        }
                    }
                }
            }
#if UNITY_EDITOR
            else if (!sInitilized && Application.isPlaying)
                RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
            return false;
        }

        public static void RunOnMainThread(System.Action action, ETaskType type = ETaskType.task, int id = 0)
        {
            if (sInitilized && action != null)
            {
                lock (sInstance.mLock)
                {
                    TaskWithoutArg task = sInstance.GetTask(id);
                    task.Task = action;
                    if (type == ETaskType.task)
                        sInstance.mTasks.Enqueue(task);
                    else if (type == ETaskType.lateTask)
                        sInstance.mLateTasks.Enqueue(task);
                }
            }
#if UNITY_EDITOR
            else if (!sInitilized && Application.isPlaying)
                RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
        }
        
        public static void RunOnMainThread<T>(System.Action<T> action, T arg, ETaskType type = ETaskType.task, int id = 0)
        {
            if (sInitilized && action != null)
            {
                lock (sInstance.mLock)
                {
                    TaskWithArg<T> task = sInstance.GetTaskWithArg<T>(id);
                    task.Task = action;
                    task.mArg = arg;
                    if (type == ETaskType.task)
                        sInstance.mTasks.Enqueue(task);
                    else if (type == ETaskType.lateTask)
                        sInstance.mLateTasks.Enqueue(task);
                }
            }
#if UNITY_EDITOR
            else if (!sInitilized && Application.isPlaying)
                RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
        }

        public static void RunOnMainThread<T1, T2> (System.Action<T1, T2> action , T1 arg1, T2 arg2, ETaskType type = ETaskType.task, int id = 0)
        {
            if(sInitilized && action != null)
            {
                lock (sInstance.mLock)
                {
                    var task = sInstance.GetTaskWithArg2<T1, T2>(id);
                    task.Task = action;
                    task.mArg1 = arg1;
                    task.mArg2 = arg2;
                    if (type == ETaskType.task)
                        sInstance.mTasks.Enqueue(task);
                    else if (type == ETaskType.lateTask)
                        sInstance.mLateTasks.Enqueue(task);
                }
            }
#if UNITY_EDITOR
            else if (!sInitilized && Application.isPlaying)
                RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
        }

        public static Coroutine RunCoroutine(IEnumerator cor)
        {
            if (sInitilized && cor != null)
            {
                return sInstance.StartCoroutine(cor);
            }
            else
            {
#if UNITY_EDITOR
                if (!sInitilized && Application.isPlaying)
                    RTLog.LogError(LogCat.Game, "MainThread not initialized.");
#endif
                return null;
            }
        }
        
        private object mLock = new object();
        private Queue<ITask> mTasks = new Queue<ITask>(64);
        private Queue<ITask> mLateTasks = new Queue<ITask>(16);
        private LinkedList<ILooper> mLoopers = new LinkedList<ILooper>();
        private ObjectPool<TaskWithoutArg> mTaskPool = new ObjectPool<TaskWithoutArg>(256, () => new TaskWithoutArg());

        TaskWithArg<T> GetTaskWithArg<T>(int id)
        {
            return new TaskWithArg<T>(id);
        }

        TaskWithoutArg GetTask(int id)
        {
            var task = mTaskPool.Get();
            task.Identify = id;
            return task;
        }

        TaskWithArg2<T1, T2> GetTaskWithArg2<T1, T2>(int id)
        {
            return new TaskWithArg2<T1, T2>(id);
        }

        private void Awake()
        {
            if (null == sInstance)
            {
                sInstance = this;
                if (mTasks == null)
                {
                    mTasks = new Queue<ITask>();
                }
                if(mLateTasks == null)
                {
                    mLateTasks = new Queue<ITask>();
                }
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    DontDestroyOnLoad(gameObject);
                sInitilized = true;
            }
        }

        private void OnDestroy()
        {
            mTasks.Clear();
            mLateTasks.Clear();
            if (sInstance == this)
            {
                sInstance = null;
                sInitilized = false;
            }
        }

        private void Update()
        {
            lock (mLock)
            {
                while (mTasks != null && mTasks.Count > 0)
                {
                    ITask task = mTasks.Dequeue();
                    task.Execute();
                }
                var node = mLoopers.First;
                while(node != null)
                {
                    var next = node.Next;
                    if (node.Value.IsActive)
                        node.Value.OnTick(Time.deltaTime);
                    else if(node.Value.LoopOnce)
                        mLoopers.Remove(node);
                    node = next;
                }
            }
        }

        private void LateUpdate()
        {
            lock (mLock)
            {
                while(mLateTasks != null && mLateTasks.Count > 0)
                {
                    ITask task = mLateTasks.Dequeue();
                    task.Execute();
                }
            }
        }
    }
}