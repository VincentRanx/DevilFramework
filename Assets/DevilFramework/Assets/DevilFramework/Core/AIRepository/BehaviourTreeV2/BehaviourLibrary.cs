using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public abstract class BehaviourLibrary : MonoBehaviour
    {
        public delegate T BTGenerator<T>(int id);

        public static bool IsInit { get; private set; }
        static BehaviourLibrary sInstance;

        protected Dictionary<string, BTGenerator<BTTaskBase>> mTasks;
        protected Dictionary<string, BTGenerator<BTConditionBase>> mConditions;
        protected Dictionary<string, BTGenerator<BTServiceBase>> mServices;
        protected Dictionary<string, BTGenerator<BTNodeBase>> mControllers;

        [SerializeField]
        bool m_DontDestroyOnLoad;
        [SerializeField]
        BlackboardAsset m_GlobalBlackboard;

        BTBlackboard mBlackboard;

        protected virtual void Awake()
        {
            sInstance = this;
            if (m_DontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            if (m_GlobalBlackboard == null)
                mBlackboard = new BTBlackboard();
            else
                mBlackboard = new BTBlackboard(m_GlobalBlackboard);
            mTasks = new Dictionary<string, BTGenerator<BTTaskBase>>();
            mConditions = new Dictionary<string, BTGenerator<BTConditionBase>>();
            mServices = new Dictionary<string, BTGenerator<BTServiceBase>>();
            mControllers = new Dictionary<string, BTGenerator<BTNodeBase>>();
            OnInit();
        }

        protected virtual void OnDestroy()
        {
            if (sInstance == this)
                sInstance = null;
        }

        protected abstract void OnInit();

        public static BTBlackboard GlobalBlackboard
        {
            get { return sInstance == null ? null : sInstance.mBlackboard; }
        }

        public static BTTaskBase NewTask(string taskName, int id)
        {
            if (sInstance == null)
            {
                Debug.LogError("在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTTaskBase> gen;
            if (sInstance.mTasks.TryGetValue(taskName, out gen))
                return gen(id);
            else
                return null;
        }

        public static BTConditionBase NewCondition(string conditionName, int id)
        {
            if (sInstance == null)
            {
                Debug.LogError("在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTConditionBase> gen;
            if (sInstance.mConditions.TryGetValue(conditionName, out gen))
                return gen(id);
            else
                return null;
        }

        public static BTServiceBase NewService(string serviceName, int id)
        {
            if (sInstance == null)
            {
                Debug.LogError("在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTServiceBase> gen;
            if (sInstance.mServices.TryGetValue(serviceName, out gen))
                return gen(id);
            else
                return null;
        }

        public static BTNodeBase NewController(string pluginName, int id)
        {
            if (sInstance == null)
            {
                Debug.LogError("在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTNodeBase> gen;
            if (sInstance.mControllers.TryGetValue(pluginName, out gen))
                return gen(id);
            else
                return null;
        }
    }
}