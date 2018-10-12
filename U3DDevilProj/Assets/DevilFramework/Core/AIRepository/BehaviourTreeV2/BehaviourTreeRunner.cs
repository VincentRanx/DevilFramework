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

        //[SerializeField]
        //[Range(0.01f, 1f)]
        //float m_ServiceInterval = 0.1f;

        LinkedList<BTServiceBase> mServices = new LinkedList<BTServiceBase>();
        LinkedList<BTServiceBase> mStopServices = new LinkedList<BTServiceBase>();
        BehaviourLooper mLooper;
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
            }
        }

        public void StopService(BTServiceBase service)
        {
            if (service != null)
            {
                mStopServices.AddLast(service);
            }
        }

        public void SetAsset(BehaviourTreeAsset behaviourAsset)
        {
            if (behaviourAsset == mAsset)
                return;
            if(mAsset != null)
            {
                mAsset.ClearBehaviourTree(this, mRootNode);
            }
            mAsset = behaviourAsset;
            if (mAsset != null)
            {
                mRootNode = mAsset.CreateBehaviourTree(this);
                mLooper = new BehaviourLooper(mRootNode);
            }
            else
            {
                mRootNode = null;
                mLooper = null;
            }
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
#if UNITY_EDITOR
            OnMonoStart(this);
#endif
        }

        void LoopService(float deltaTime)
        {
            LinkedListNode<BTServiceBase> serv = mServices.First;
            while (serv != null)
            {
                serv.Value.OnServiceTick(this, deltaTime);
                serv = serv.Next;
            }
        }

        void CleanService()
        {
            var node = mStopServices.Last;
            while(node != null)
            {
                mServices.Remove(node.Value);
                node = node.Previous;
            }
            mStopServices.Clear();
        }
        
        void LoopTask(float deltaTime)
        {
            if (mLooper.IsComplate)
            {
                mLooper.Reset();
            }
            mLooper.Update(this, deltaTime);
            BehaviourTime += deltaTime;
        }

        protected virtual void Update()
        {
            if (mLooper == null)
                return;
            LoopTask(Time.deltaTime);
            LoopService(Time.unscaledDeltaTime);
            CleanService();
#if UNITY_EDITOR
            OnBehaviourTreeFrame(this);
#endif
        }

        protected virtual void OnDestroy()
        {
            if(mAsset != null)
            {
                mAsset.ClearBehaviourTree(this, mRootNode);
                mAsset = null;
                mRootNode = null;
            }
        }

        public BTNodeBase FindRuntimeNode(int nodeId)
        {
            return FindNodeRecursize(nodeId, mRootNode);
        }

        private BTNodeBase FindNodeRecursize(int nodeId, BTNodeBase root)
        {
            if (root == null || root.NodeId == nodeId)
                return root;
            for (int i = 0; i < root.ChildLength; i++)
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
            while (serv != null)
            {
                if (serv.Value.Id == servId)
                    return true;
                serv = serv.Next;
            }
            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(Application.isPlaying && mAsset != m_BehaviourAsset && mAsset != null)
            {
                SetAsset(m_BehaviourAsset);
            }
        }
        public event System.Action<BehaviourTreeRunner> OnMonoStart = (x) => { };
        public event System.Action<BehaviourTreeRunner> OnBehaviourTreeFrame = (x) => { };
        public BTNodeBase RootNode { get { return mRootNode; } }
        public BTNodeBase RuntimeNode { get { return mLooper.RuntimeNode; } }
#endif

    }

}