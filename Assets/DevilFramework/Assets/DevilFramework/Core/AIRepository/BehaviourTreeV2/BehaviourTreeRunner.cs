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
        BlackboardAsset m_Blackboard;

        [SerializeField]
        [Range(0.01f, 1f)]
        float m_ServiceInterval = 0.1f;

        [SerializeField]
        bool m_BreakAtStart;

        LinkedList<BTServiceBase> mServices = new LinkedList<BTServiceBase>();
        BehaviourLooper mLooper;
        float mServiceTimer;
        float mServiceDeltaTime;
        private BTNodeBase mRootNode;
        BehaviourTreeAsset mAsset;
        public BTBlackboard Blackboard { get; private set; }
        public BehaviourTreeAsset BehaviourAsset { get { return mAsset ?? m_BehaviourAsset; } }
        public float TaskTime { get { return mLooper.NodeRuntime; } }
        public float BehaviourTime { get; private set; }

        public void StartService(BTServiceBase service)
        {
            if (service != null)
            {
                mServices.AddLast(service);
                service.OnServiceStart(this);
            }
        }

        public void StopService(BTServiceBase service)
        {
            if (service != null)
            {
                service.OnServiceStop(this);
                LinkedListNode<BTServiceBase> serv = mServices.Last;
                while (serv != null)
                {
                    if (serv.Value == service)
                    {
                        mServices.Remove(serv);
                        break;
                    }
                    serv = serv.Previous;
                }
            }
        }

        protected virtual void Start()
        {
            mServiceDeltaTime = m_ServiceInterval;
            mServiceTimer = 0;

            if (m_Blackboard != null)
                Blackboard = new BTBlackboard(m_Blackboard);
            else
                Blackboard = new BTBlackboard();

            mAsset = m_BehaviourAsset;
            if (mAsset != null)
                mRootNode = mAsset.CreateBehaviourTree(this);
            mLooper = new BehaviourLooper(mRootNode);
#if UNITY_EDITOR
            if (m_BreakAtStart)
                Debug.Break();
#endif
        }

        protected virtual void FixedUpdate()
        {
            if (mServiceTimer >= m_ServiceInterval)
            {
                float t = mServiceTimer - mServiceDeltaTime;
                LinkedListNode<BTServiceBase> serv = mServices.First;
                while (serv != null)
                {
                    serv.Value.OnServiceTick(this, t);
                    serv = serv.Next;
                }
                mServiceTimer -= m_ServiceInterval;
                mServiceDeltaTime = mServiceTimer;
            }
            else
            {
                mServiceTimer += Time.fixedDeltaTime;
            }
        }

        protected virtual void Update()
        {
            if (mLooper.IsComplate)
            {
                mLooper.Reset();
            }
            mLooper.Update(this, Time.deltaTime);
            BehaviourTime += Time.deltaTime;
        }

        public BTNodeBase FindRuntimeNode(int nodeId)
        {
            return FindNodeRecursize(nodeId, mRootNode);
        }

        private BTNodeBase FindNodeRecursize(int nodeId, BTNodeBase root)
        {
            if (root == null || root.NodeId == nodeId)
                return root;
            for(int i = 0; i < root.ChildLength; i++)
            {
                BTNodeBase node = FindNodeRecursize(nodeId, root.ChildAt(i));
                if (node != null)
                    return node;
            }
            return null;
        }

        public bool IsServiceActive(int servId)
        {
            LinkedListNode<BTServiceBase> serv = mServices.First;
            while(serv != null)
            {
                if (serv.Value.Id == servId)
                    return true;
                serv = serv.Next;
            }
            return false;
        }

#if UNITY_EDITOR
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
        public BTNodeBase RootNode { get { return mRootNode; } }
        public BTNodeBase RuntimeNode { get { return mLooper.RuntimeNode; } }
#endif

    }

}