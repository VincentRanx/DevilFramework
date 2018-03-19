using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BehaviourTreeRunner : MonoBehaviour
    {

        [SerializeField]
        BehaviourTreeAsset m_BehaviourAsset = null;
        public BehaviourTreeAsset SourceAsset { get { return m_BehaviourAsset; } }

        [SerializeField]
        [Range(0.01f, 1f)]
        float m_ServiceInterval = 0.1f;

        List<IBTService> mServices = new List<IBTService>();
        BTNodeBase mRoot;
        BehaviourLooper mLooper;
        float mServiceTimer;
        float mServiceDeltaTime;

        BehaviourTreeAsset mAsset;
        public BehaviourTreeAsset BehaviourAsset { get { return mAsset ?? m_BehaviourAsset; } }
        public float TaskTime { get { return mLooper.NodeRuntime; } }

        void StartService(IBTService service)
        {
            if (service != null)
            {
                service.LiveCounter++;
                if (service.LiveCounter == 1)
                {
                    mServices.Add(service);
                    service.OnStartService(this);
                }
            }
        }

        void StopService(IBTService service)
        {
            if (service != null )
            {
                service.LiveCounter--;
                if (service.LiveCounter == 0)
                {
                    for (int i = mServices.Count - 1; i >= 0; i--)
                    {
                        if (mServices[i] == service)
                        {
                            mServices.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            mServiceDeltaTime = m_ServiceInterval;
            mServiceTimer = m_ServiceInterval;

            mAsset = m_BehaviourAsset == null ? null : m_BehaviourAsset.GetNewOrSharedInstance();
            if(mAsset != null)
                mRoot = mAsset.CreateBehaviourTree(this);
            mLooper = new BehaviourLooper(mRoot);
            //if (mRoot != null)
            //    mRoot.InitWith(this);

            //PrintTree(mRoot, "");
            //enabled = false;
        }

        //void PrintTree(BTNodeBase node, string prefix)
        //{
        //    Debug.Log(prefix + node.GetType().Name);
        //    for(int i = 0; i < node.ChildLength; i++)
        //    {
        //        PrintTree(node.ChildAt(i), prefix + "----");
        //    }
        //}

        private void FixedUpdate()
        {
            if (mServiceTimer <= 0)
            {
                for (int i = 0; i < mServices.Count; i++)
                {
                    mServices[i].OnServiceTick(this, mServiceDeltaTime);
                }
                mServiceTimer += m_ServiceInterval;
                mServiceDeltaTime = mServiceTimer;
            }
            else
            {
                mServiceTimer -= Time.fixedDeltaTime;
            }
        }

        private void Update()
        {
            if (mLooper.IsComplate)
            {
                mLooper.ResetTreeState();
            }
            mLooper.Update(this, Time.deltaTime);
        }

#if DEBUG_AI
        public event System.Action<BehaviourTreeRunner> OnBehaviourTreeFrame = (x) => { };
        public void NotifyBehaviourTreeFrame()
        {
            OnBehaviourTreeFrame(this);
        }
        public event System.Action<BehaviourTreeRunner> OnBehaviourTreeBegin = (x) => { };
        public void NotifyBehaviourTreeBegin()
        {
            OnBehaviourTreeBegin(this);
        }
        public event System.Action<BehaviourTreeRunner> OnBehaviourTreeEnd = (x) => { };
        public void NotifyBehaviourTreeEnd()
        {
            OnBehaviourTreeEnd(this);
        }
        public BTNodeBase RootNode { get { return mRoot; } }
        public BTNodeBase RuntimeNode { get { return mLooper.RuntimeNode; } }
#endif

    }

}