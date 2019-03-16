using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        //public const int MAX_SUB_TREE_DEEP = 4;

        public enum EResetMode
        {
            NeverReset,
            ResetWhenLoopEnd,
            AlwaysReset,
            ResetWhenBegin,
        }

        // ai binder
        public class AssetBinder : System.IDisposable
        {
            EResetMode mLoopMode;
            BehaviourTreeAsset mSource;
            BehaviourTreeAsset mRuntime;
            BehaviourTreeRunner mRunner;
            BehaviourLooper mLooper;
            AssetBinder mParent;
            List<AssetBinder> mSubAssets;
            bool mServiceStarted;
            int mAssetDeep; // 树深度
            string mName;
            string mAssetName;
            public string Name
            {
                get
                {
                    if (!string.IsNullOrEmpty(mName))
                        return mName;
                    if (!string.IsNullOrEmpty(mAssetName))
                        return mAssetName;
                    return "BehaviourAsset Binder";
                }
                set
                {
                    mName = value;
                }
            }
            public BehaviourTreeRunner Runner { get { return mRunner; } }
            public BehaviourLooper Looper { get { return mLooper; } }
            public BTBlackboard Blackboard { get { return mRunner.Blackboard; } }
            public BehaviourTreeAsset RuntimeTree { get { return mRuntime; } }
            public BehaviourTreeAsset Source { get { return mSource; } }
            public AssetBinder Parent { get { return mParent; } }
            public int DeepAsSubTree { get { return mAssetDeep; } }
            public int SubAssetCount { get { return mSubAssets.Count; } }
            public bool IsAvailable { get { return mLooper != null && mRunner != null; } }
            public AssetBinder SubAssetAt(int index)
            {
                return mSubAssets[index];
            }

            public GameObject gameObject { get { return mRunner == null ? null : mRunner.gameObject; } }
            public Transform transform { get { return mRunner == null ? null : mRunner.transform; } }

            private AssetBinder(BehaviourTreeRunner runner)
            {
                mRunner = runner;
                mAssetDeep = 1;
                mLooper = new BehaviourLooper(runner);
                mSubAssets = new List<AssetBinder>();
            }

            private AssetBinder(AssetBinder parent)
            {
                mRunner = parent.mRunner;
                mAssetDeep = parent.mAssetDeep + 1;
                mLooper = new BehaviourLooper(mRunner);
                mSubAssets = new List<AssetBinder>();
            }

            public T GetComponent<T>()
            {
                if (mRunner == null)
                    return default(T);
                else
                    return mRunner.GetComponent<T>();
            }

            public void BindAsset(BehaviourTreeAsset asset, EResetMode loopMode)
            {
                if (mRunner == null)
                    return;
                if (asset == null)
                {
                    mAssetName = null;
                    Cleanup();
                    return;
                }
                mLoopMode = loopMode;
                if (asset == mSource)
                    return;
                Cleanup();
                mAssetName = asset.name;
                mSource = asset;
                mRuntime = asset.Clone();
                mRuntime.Prepare(this);
                mLooper.SetBehaviuor(mRuntime.Root);
            }

            public void UpdateService(float deltaTime)
            {
                if (!mServiceStarted)
                {
                    mServiceStarted = true;
                    for (int i = 0; i < mRuntime.Services.Count; i++)
                    {
                        mRuntime.Services[i].OnStart();
                    }
                }
                for (int i = 0; i < mRuntime.Services.Count; i++)
                {
                    mRuntime.Services[i].OnUpdate(deltaTime);
                }
            }

            public void UpdateLooper(float deltaTime)
            {
                if (mLoopMode != EResetMode.NeverReset && mLooper.IsComplate)
                {
                    mLooper.Reset();
                }
                mLooper.Update(deltaTime);
            }
            
            public void StopService()
            {
                if (mServiceStarted)
                {
                    mServiceStarted = false;
                    for (int i = mRuntime.Services.Count - 1; i >= 0; i--)
                    {
                        mRuntime.Services[i].OnStop();
                    }
                }
            }

            void Cleanup()
            {
                if (mRuntime != null)
                {
                    StopService();
                    mRuntime.Services.Clear();
                    BehaviourTreeAsset.DestroyAsset(mRuntime);
                    mRuntime = null;
                }
            }

            public void Dispose()
            {
                Cleanup();
                var tmp = mSubAssets;
                mSubAssets = null;
                for (int i = 0; i < tmp.Count; i++)
                {
                    tmp[i].mParent = null;
                }
                if (mParent != null && mParent.mSubAssets != null)
                {
                    mParent.mSubAssets.Remove(this);
                    mParent = null;
                }
                if (mLooper != null)
                {
                    mLooper.Dispose();
                }
                mLooper = null;
                mRunner = null;
            }

            public static AssetBinder GetBinder(BehaviourTreeRunner runner)
            {
                if (runner == null)
                    return null;
                if (runner.mAssetBinder == null || !runner.mAssetBinder.IsAvailable)
                {
                    var asset = new AssetBinder(runner);
                    runner.mAssetBinder = asset;
                }
                return runner.mAssetBinder;
            }

            public static AssetBinder NewSubBinder(AssetBinder binder)
            {
                if (binder == null || binder.mRunner == null)
                    return null;
                if (binder.DeepAsSubTree >= binder.Runner.MaxSubTreeDeep)
                {
                    var error = string.Format("在创建子行为树是超过了支持的最大深度值：{0}", binder.Runner.MaxSubTreeDeep);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayDialog("Error", error, "OK");
#endif
                    RTLog.LogError(LogCat.AI, error);
                    return null;
                }
                var subbind = new AssetBinder(binder);
                subbind.mParent = binder;
                binder.mSubAssets.Add(subbind);
                return subbind;
            }

        }

        [SerializeField]
        protected EResetMode m_LoopMode = EResetMode.ResetWhenLoopEnd;
        [Range(1,8)]
        [SerializeField]
        int m_MaxSubTreeDeep = 4;
        public int MaxSubTreeDeep { get { return m_MaxSubTreeDeep; } }
        [SerializeField]
        BehaviourTreeAsset m_BehaviourAsset = null;
        public BehaviourTreeAsset SourceAsset { get { return m_BehaviourAsset; } }

        [SerializeField]
        BlackboardAsset m_Blackboard;
        public BlackboardAsset BlackboardAsset
        {
            get { return m_Blackboard; }
        }

        BTBlackboard mBlackboard;
        public BTBlackboard Blackboard { get { if (mBlackboard == null) mBlackboard = new BTBlackboard(m_Blackboard); return mBlackboard; } }

        public float BehaviourTime { get; private set; }

        AssetBinder mAssetBinder;
        public AssetBinder ActiveBinder { get { return mAssetBinder; } }
        Stack<AssetBinder> mSearchStack = new Stack<AssetBinder>();

        public AssetBinder GetBinder(BehaviourTreeAsset asset)
        {
            return GetBinder((x) => x.Source == asset || x.RuntimeTree == asset);
        }

        public AssetBinder GetBinder(FilterDelegate<AssetBinder> filter)
        {
            if (mAssetBinder == null || !mAssetBinder.IsAvailable)
                return null;
            mSearchStack.Push(mAssetBinder);
            while (mSearchStack.Count > 0)
            {
                var bind = mSearchStack.Pop();
                if (filter(bind))
                    return bind;
                for (int i = 0; i < bind.SubAssetCount; i++)
                {
                    mSearchStack.Push(bind.SubAssetAt(i));
                }
            }
            return null;
        }

        public void GetBinders(BehaviourTreeAsset asset, ICollection<AssetBinder> binders)
        {
            if (asset == null || mAssetBinder == null || binders == null)
                return;
            mSearchStack.Push(mAssetBinder);
            while (mSearchStack.Count > 0)
            {
                var bind = mSearchStack.Pop();
                if (bind.Source == asset || bind.RuntimeTree == asset)
                    binders.Add(bind);
                for (int i = 0; i < bind.SubAssetCount; i++)
                {
                    mSearchStack.Push(bind.SubAssetAt(i));
                }
            }
        }

        public void SetAsset(BehaviourTreeAsset behaviourAsset)
        {
            var bind = AssetBinder.GetBinder(this);
            bind.BindAsset(behaviourAsset, m_LoopMode);
        }
        
        protected virtual void OnEnable()
        {
            AIManager.Instance.Add(this);
            if (m_LoopMode == EResetMode.ResetWhenBegin && mAssetBinder != null && mAssetBinder.RuntimeTree != null)
                mAssetBinder.Looper.Reset();
        }

        protected virtual void OnDisable()
        {
            AIManager.Instance.Remove(this);
            if(m_LoopMode == EResetMode.AlwaysReset && mAssetBinder != null && mAssetBinder.RuntimeTree != null)
            {
                mAssetBinder.StopService();
                mAssetBinder.Looper.Reset();
            }
        }

        protected virtual void Start()
        {
            SetAsset(m_BehaviourAsset);
        }

        protected virtual void FixedUpdate()
        {
            if(mAssetBinder != null && mAssetBinder.RuntimeTree != null)
            {
                var t = Time.fixedDeltaTime;
                mAssetBinder.UpdateLooper(t);
                mAssetBinder.UpdateService(t);
                BehaviourTime += t;
            }
        }

        protected virtual void OnDestroy()
        {
            if(mAssetBinder != null)
            {
                mAssetBinder.Dispose();
                mAssetBinder = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (m_BehaviourAsset != null  && mAssetBinder != null)
                {
                    mAssetBinder.BindAsset(m_BehaviourAsset, m_LoopMode);
                }
            }
        }
     
#endif
    }

}