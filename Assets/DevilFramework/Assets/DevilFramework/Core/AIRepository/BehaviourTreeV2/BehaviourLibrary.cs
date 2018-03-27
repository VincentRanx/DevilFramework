using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BehaviourLibrary
    {
        public delegate T BTGenerator<T>(int id);

        public static bool IsInit { get; private set; }
        static BehaviourLibrary sInstance;

        protected Dictionary<string, BTGenerator<BTTaskBase>> mTasks;
        protected Dictionary<string, BTGenerator<BTConditionBase>> mConditions;
        protected Dictionary<string, BTGenerator<BTServiceBase>> mServices;
        protected Dictionary<string, BTGenerator<BTNodeBase>> mControllers;

        public static void InitWithType<T>() where T: BehaviourLibrary, new()
        {
            if (sInstance == null || sInstance.GetType() != typeof(T))
                sInstance = new T();
            IsInit = true;
        }

        protected BehaviourLibrary()
        {
            sInstance = this;
            mTasks = new Dictionary<string, BTGenerator<BTTaskBase>>();
            mConditions = new Dictionary<string, BTGenerator<BTConditionBase>>();
            mServices = new Dictionary<string, BTGenerator<BTServiceBase>>();
            mControllers = new Dictionary<string, BTGenerator<BTNodeBase>>();
            OnInit();
        }

        protected virtual void OnInit() { }

        public static BTTaskBase NewTask(string taskName, int id)
        {
            if (sInstance == null)
                sInstance = new BehaviourLibrary();
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
                sInstance = new BehaviourLibrary();
                Debug.LogError("在使用行为树之前，您需要通过 BehaviourLibrary.InitWithType<T>() 方法初始化行为树模块。");
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
                sInstance = new BehaviourLibrary();
                Debug.LogError("在使用行为树之前，您需要通过 BehaviourLibrary.InitWithType<T>() 方法初始化行为树模块。");
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
                sInstance = new BehaviourLibrary();
                Debug.LogError("在使用行为树之前，您需要通过 BehaviourLibrary.InitWithType<T>() 方法初始化行为树模块。");
            }
            BTGenerator<BTNodeBase> gen;
            if (sInstance.mControllers.TryGetValue(pluginName, out gen))
                return gen(id);
            else
                return null;
        }
    }
}