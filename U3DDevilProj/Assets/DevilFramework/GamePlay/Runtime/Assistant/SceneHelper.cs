using Devil.AsyncTask;
using Devil.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Devil.GamePlay.Assistant
{
    public class SceneHelper : MonoBehaviour
    {
        public static SceneHelper Instance { get; private set; }

        public class LoadSceneTask : DependenceAsyncTask
        {
            AsyncOperation mLoading;
            float mRatio;
            float mTimeScale;
            string mSceneName;
            float mSuspectWeight;

            public LoadSceneTask(string sceneName, float suspectTime = 2f, float suspectWeight = 0.5f)
            {
                mSceneName = sceneName;
                mTimeScale = suspectTime > 0 ? (1 / suspectTime) : 0;
                mSuspectWeight = Mathf.Clamp01(suspectWeight);
            }

            public override void Start()
            {
                base.Start();
                mRatio = 0;
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
                bool isDone = mLoading == null ? true : mLoading.isDone;
                float progress = mLoading == null ? 1 : mLoading.progress;
                if (mTimeScale > 0)
                {
                    float max = isDone ? 1 : Mathf.Max(0.9f, progress);
                    mRatio = Mathf.Min(mRatio + deltaTime * mTimeScale, max);
                    progress = mRatio * mSuspectWeight + progress * (1 - mSuspectWeight);
                }
                SceneHelper inst = Instance;
                if (inst != null && isDone)
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
        
        [Range(0, 1)]
        public float m_WaitForTimeWeight = 0.5f;

        [SerializeField]
        private bool m_DontDestroyOnLoad = true;

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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                mLoader = new AsyncLoader();
                if (m_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                mLoader.Reset();
            }
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
            mLoadingScene = sceneName;
            mLoader.Reset();
            mLoadEndCallback = complateCallback;
            mLoading = true;
            OnLoadBegin(mLoadingScene);
            LoadSceneTask task = new LoadSceneTask(mLoadingScene, displayTime, m_WaitForTimeWeight);
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