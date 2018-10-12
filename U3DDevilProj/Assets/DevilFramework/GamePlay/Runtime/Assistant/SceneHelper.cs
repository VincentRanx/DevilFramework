using Devil.AsyncTask;
using Devil.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Devil.GamePlay.Assistant
{
    public class SceneHelper : SingletonMono<SceneHelper>
    {
        public class LoadSceneTask : DependenceTask
        {
            AsyncOperation mLoading;
            float mRatio;
            float mTimeScale;
            string mSceneName;

            public LoadSceneTask(string sceneName, float suspectTime = 2f)
            {
                mSceneName = sceneName;
                mTimeScale = suspectTime > 0 ? (1 / suspectTime) : 0;
            }
            
            protected override void OnTaskStart()
            {
                SceneHelper inst = Instance;
                if(inst != null)
                    inst.NotifySceneWillLoad(mSceneName);
                mLoading = SceneManager.LoadSceneAsync(mSceneName);
            }

            protected override float TickAndGetTaskProgress(float deltaTime)
            {
                float progress;
                if (mTimeScale > 0)
                {
                    mRatio = Mathf.Min(mRatio + deltaTime * mTimeScale);
                    progress = mLoading == null ? mRatio : Mathf.Min(mRatio, mLoading.progress);
                }
                else
                {
                    progress = mLoading == null ? 1 : mLoading.progress;
                }
                SceneHelper inst = Instance;
                if (inst != null && mLoading != null && mLoading.isDone)
                    inst.NotifySceneLoaded(mSceneName);
                return progress;
            }
        }

        public class WaitTask : IAsyncTask
        {
            public ValueDelegate<bool> IsDoneHandler { get; set; }

            public WaitTask()
            {

            }

            public WaitTask(ValueDelegate<bool> isdone)
            {
                IsDoneHandler = isdone;
            }

            public float Progress { get { return IsDone ? 1 : 0; } }

            public bool IsDone { get {return IsDoneHandler == null ? true : IsDoneHandler(); } }

            public void Abort()
            {
            }

            public void Start()
            {
            }

            public void OnTick(float deltaTime)
            {
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
        public IAsyncTask SceneTask { get; private set; }
        System.Action mLoadEndCallback;
        public event System.Action<string> OnLoadBegin = (x) => { };
        public event System.Action<string> OnSceneWillLoad = (x) => { };
        public event System.Action<string> OnSceneLoaded = (x) => { };
        public event System.Action<string> OnLoadEnd = (x) => { };
        public Scene ActiveScene
        {
            get { return SceneManager.GetActiveScene(); }
        }

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

        public void NotifySceneWillLoad(string scene)
        {
            OnSceneWillLoad(scene);
        }

        public void NotifySceneLoaded(string sceneName)
        {
            OnSceneLoaded(sceneName);
        }

        public IAsyncTask TaskOperator { get { return mLoader; } } 

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
                    SceneTask = null;
                }
            }
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="displayTime"></param>
        public void LoadScene(string sceneName,  float displayTime = 0, System.Action complateCallback = null)
        {
            if (mLoadingScene == sceneName)
            {
#if UNITY_EDITOR
                RTLog.LogWarning(LogCat.Game, string.Format("Scene:{0} is loading, don't need to load it agin.", sceneName));
#endif
                if (complateCallback != null)
                    mLoadEndCallback += complateCallback;
                return;
            }
//            Scene scene = SceneManager.GetSceneByName(sceneName);
//            if (scene.isLoaded)
//            {
//#if UNITY_EDITOR
//                Debug.LogWarning(string.Format("Scene:{0} was loaded, don't need to load it agin.", scene.name));
//#endif
//                return;
//            }
            mLoadingScene = sceneName;
            mLoader.Reset();
            mLoadEndCallback = complateCallback;
            mLoading = true;
            OnLoadBegin(mLoadingScene);
            LoadSceneTask task = new LoadSceneTask(mLoadingScene, displayTime);
            task.PresetTask = new WaitTask(() => !PanelManager.HasAnyPanelClosing);
            SceneTask = task;
            mLoader.AddTask(SceneTask);
            mLoader.Start();
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