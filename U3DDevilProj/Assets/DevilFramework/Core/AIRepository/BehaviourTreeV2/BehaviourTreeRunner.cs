using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        public const int MAX_SUB_TREE_DEEP = 4;

        public enum EResetMode
        {
            NeverReset,
            ResetWhenLoopEnd,
            AlwaysReset,
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
            public BehaviourTreeAsset RuntimeTree { get { return mRuntime; } }
            public BehaviourTreeAsset Source { get { return mSource; } }
            public AssetBinder Parent { get { return mParent; } }
            public int DeepAsSubTree { get { return mAssetDeep; } }
            public int SubAssetCount { get { return mSubAssets.Count; } }
            public AssetBinder SubAssetAt(int index)
            {
                return mSubAssets[index];
            }

            public static AssetBinder GetBinder(BehaviourTreeRunner runner)
            {
                if(runner == null)
                    return null;
                if(runner.mAssetBinder == null)
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
                if(binder.DeepAsSubTree >= MAX_SUB_TREE_DEEP)
                {
                    var error = string.Format("在创建子行为树是超过了支持的最大深度值：{0}", MAX_SUB_TREE_DEEP);
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

            void Cleanup()
            {
                if (mRuntime != null)
                {
                    if (mServiceStarted)
                    {
                        mServiceStarted = false;
                        for (int i = mRuntime.Services.Count - 1; i >= 0; i--)
                        {
                            mRuntime.Services[i].OnStop();
                        }
                    }
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
        }
        [SerializeField]
        EResetMode m_LoopMode = EResetMode.ResetWhenLoopEnd;
        [SerializeField]
        BehaviourTreeAsset m_BehaviourAsset = null;
        public BehaviourTreeAsset SourceAsset { get { return m_BehaviourAsset; } }

        [SerializeField]
        BlackboardAsset m_Blackboard;
        public BlackboardAsset BlackboardAsset { get { return m_Blackboard; } }
        
        public BTBlackboard Blackboard { get; private set; }

        public float BehaviourTime { get; private set; }

        AssetBinder mAssetBinder;
        Stack<AssetBinder> mSearchStack = new Stack<AssetBinder>();

        public AssetBinder GetBinder(BehaviourTreeAsset asset)
        {
            if (asset == null || mAssetBinder == null)
                return null;
            mSearchStack.Push(mAssetBinder);
            while(mSearchStack.Count > 0)
            {
                var bind = mSearchStack.Pop();
                if (bind.Source == asset || bind.RuntimeTree == asset)
                    return bind;
                for(int i= 0; i < bind.SubAssetCount; i++)
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
        
        protected virtual void Awake()
        {
            if (m_Blackboard != null)
                Blackboard = new BTBlackboard(m_Blackboard);
            else
                Blackboard = new BTBlackboard();
        }

        protected virtual void Start()
        {
            SetAsset(m_BehaviourAsset);
        }

        protected virtual void Update()
        {
            if(mAssetBinder != null && mAssetBinder.RuntimeTree != null)
            {
                var t = Time.deltaTime;
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