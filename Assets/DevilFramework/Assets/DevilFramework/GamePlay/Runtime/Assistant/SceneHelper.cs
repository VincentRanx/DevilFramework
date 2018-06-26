using Devil.AsyncTask;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Devil.GamePlay.Assistant
{
    public class SceneHelper : SingletonMono<SceneHelper>
    {
        public class LoadSceneTask : IAsyncTask
        {
            AsyncOperation mLoading;
            float mRatio;
            float mTimeScale;
            string mSceneName;

            public LoadSceneTask(string sceneName, float suspectTime = 2f)
            {
                mSceneName = sceneName;
                mTimeScale = suspectTime > 0 ? (1 / suspectTime) : 1;
            }

            public void OnStart()
            {
                mLoading = SceneManager.LoadSceneAsync(mSceneName);
            }

            public float Progress { get { return Mathf.Max(mRatio, mLoading.progress); } }

            public bool IsDone { get { return mLoading != null && mLoading.isDone; } }

            public void OnInterrupt()
            {
            }

            public void OnTick(float deltaTime)
            {
                mRatio = Mathf.Min(0.99f, mRatio + deltaTime * mTimeScale);
            }
        }

        public class TimeDelayTask : IAsyncTask
        {
            float mTimeScale;
            float mRatio;
            IAsyncTask mPreTask;

            public TimeDelayTask(IAsyncTask preTask, float time)
            {
                mPreTask = preTask;
                mTimeScale = time > 0 ? (1 / time) : 1;
            }

            public void OnStart()
            {

            }

            public float Progress { get { return mRatio; } }

            public bool IsDone { get { return mRatio >= 1; } }

            public void OnInterrupt()
            {
                mRatio = 1;
            }

            public void OnTick(float deltaTime)
            {
                if (mPreTask == null || mPreTask.IsDone)
                    mRatio = Mathf.Clamp01(mRatio + deltaTime * mTimeScale);
            }
        }

        public class TimeDisplayTask : IAsyncTask
        {
            float mTimeScale;
            float mRatio;

            public TimeDisplayTask(float time)
            {
                mTimeScale = time > 0 ? (1 / time) : 1;
            }

            public void OnStart()
            {

            }

            public float Progress { get { return mRatio; } }

            public bool IsDone { get { return mRatio >= 1; } }

            public void OnInterrupt()
            {
                mRatio = 1;
            }

            public void OnTick(float deltaTime)
            {
                mRatio += deltaTime * mTimeScale;
            }
        }

        [Range(0,50f)]
        public float m_WaitForTimeWeight = 10; 

        AsyncLoader mLoader;
        bool mLoading;
        string mLoadingScene;
        public float LoadProgress { get { return mLoader.Progress; } }
        public bool IsDone { get { return mLoader.IsDone; } }
        public bool IsLoading { get { return mLoading && mLoader.HasTask; } }
        System.Action mLoadEndCallback;
        public event System.Action<string> OnLoadBegin = (x) => { };
        public event System.Action<string> OnLoadEnd = (x) => { };

        protected override void Awake()
        {
            base.Awake();
            mLoader = new AsyncLoader();
        }

        protected override void OnDestroy()
        {
            mLoader.Reset();
            base.OnDestroy();
        }

        private void Update()
        {
            if (mLoader.HasTask && mLoading)
            {
                mLoader.OnTick(Time.unscaledDeltaTime);
                if (mLoader.IsDone)
                {
                    if(mLoadEndCallback != null)
                    {
                        mLoadEndCallback();
                        mLoadEndCallback = null;
                    }
                    OnLoadEnd(mLoadingScene);
                    mLoadingScene = null;
                    mLoading = false;
                }
            }
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="displayTime"></param>
        public void LoadScene(string sceneName,  float displayTime = 0, IList<IAsyncTask> additiveTask = null, float additiveWeight = 1, System.Action complateCallback = null)
        {
            if (mLoadingScene == sceneName)
            {
#if UNITY_EDITOR
                Debug.LogWarning(string.Format("Scene:{0} is loading, don't need to load it agin.", sceneName));
#endif
                if (complateCallback != null)
                    mLoadEndCallback += complateCallback;
                if (additiveTask != null)
                {
                    mLoader.AddTasks(additiveTask, additiveWeight);
                }
                return;
            }
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
#if UNITY_EDITOR
                Debug.LogWarning(string.Format("Scene:{0} was loaded, don't need to load it agin.", scene.name));
#endif
                return;
            }
            mLoadingScene = sceneName;
            mLoader.Reset();
            mLoadEndCallback = complateCallback;
            mLoader.AddTask(new LoadSceneTask(mLoadingScene));
            if (displayTime > 0)
            {
                mLoader.AddTask(new TimeDisplayTask(displayTime), m_WaitForTimeWeight);
            }
            if (additiveTask != null)
            {
                mLoader.AddTasks(additiveTask, additiveWeight);
            }
            mLoader.OnStart();
            OnLoadBegin(mLoadingScene);
            mLoading = true;
        }

        public bool AddAdditiveTask(IAsyncTask task, float weight = 0.1f)
        {
            return mLoader.AddTask(task, weight);
        }

        public bool IsDoneExcept(FilterDelegate<IAsyncTask> filter)
        {
            return mLoader.IsDoneExcept(filter);
        }

    }
}