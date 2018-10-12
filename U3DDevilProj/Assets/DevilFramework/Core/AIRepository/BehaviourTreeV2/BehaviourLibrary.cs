using Devil.Utility;
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
        
        protected virtual void Update()
        {
            
        }

        protected virtual void OnDestroy()
        {
            if (mTasks != null)
                mTasks.Clear();
            if (mConditions != null)
                mConditions.Clear();
            if (mServices != null)
                mServices.Clear();
            if (mControllers != null)
                mControllers.Clear();
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
                RTLog.LogError(LogCat.AI, "在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTTaskBase> gen;
            BTTaskBase task;
            if (sInstance.mTasks.TryGetValue(taskName, out gen))
                task = gen(id);
            else
                task = null;
#if UNITY_EDITOR
            if (task == null)
                RTLog.LogErrorFormat(LogCat.AI, "Faild to generate task instance of \"{0}\"", taskName);
#endif
            return task;
        }

        public static BTConditionBase NewCondition(string conditionName, int id)
        {
            if (sInstance == null)
            {
                RTLog.LogError(LogCat.AI, "在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTConditionBase> gen;
            BTConditionBase con;
            if (sInstance.mConditions.TryGetValue(conditionName, out gen))
                con = gen(id);
            else
                con = null;
#if UNITY_EDITOR
            if (con == null)
                RTLog.LogErrorFormat(LogCat.AI, "Faild to generate condition instance of \"{0}\"", conditionName);
#endif
            return con;
        }

        public static BTServiceBase NewService(string serviceName, int id)
        {
            if (sInstance == null)
            {
                RTLog.LogError(LogCat.AI, "在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTServiceBase> gen;
            BTServiceBase serv;
            if (sInstance.mServices.TryGetValue(serviceName, out gen))
                serv = gen(id);
            else
                serv = null;
#if UNITY_EDITOR
            if (serv == null)
                RTLog.LogErrorFormat(LogCat.AI, "Faild to generate service instance of \"{0}\"", serviceName);
#endif
            return serv;
        }

        public static BTNodeBase NewController(string pluginName, int id)
        {
            if (sInstance == null)
            {
                RTLog.LogError(LogCat.AI, "在使用行为树之前，您需要初始化行为树模块。");
                return null;
            }
            BTGenerator<BTNodeBase> gen;
            BTNodeBase node;
            if (sInstance.mControllers.TryGetValue(pluginName, out gen))
                node = gen(id);
            else
                node = null;
#if UNITY_EDITOR
            if (node == null)
                RTLog.LogErrorFormat(LogCat.AI, "Faild to generate composite instance of \"{0}\"", pluginName);
#endif
            return node;
        }
    }
}