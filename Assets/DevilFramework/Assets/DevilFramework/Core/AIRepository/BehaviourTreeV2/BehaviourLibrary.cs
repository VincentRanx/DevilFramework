using System.Collections.Generic;

namespace Devil.AI
{
    public class BehaviourLibrary
    {
        public delegate BTNodeBase BTNodeGenerator(int id);
        public delegate T BTGenerator<T>();

        public static bool IsInit { get; private set; }
        static BehaviourLibrary sInstance;

        protected Dictionary<string, BTGenerator<IBTTask>> mTasks;
        protected Dictionary<string, BTGenerator<IBTCondition>> mConditions;
        protected Dictionary<string, BTGenerator<IBTService>> mServices;
        protected Dictionary<string, BTNodeGenerator> mControllers;

        public static void InitWithType<T>() where T: BehaviourLibrary, new()
        {
            sInstance = new T();
            IsInit = true;
        }

        protected BehaviourLibrary()
        {
            sInstance = this;
            mTasks = new Dictionary<string, BTGenerator<IBTTask>>();
            mConditions = new Dictionary<string, BTGenerator<IBTCondition>>();
            mServices = new Dictionary<string, BTGenerator<IBTService>>();
            mControllers = new Dictionary<string, BTNodeGenerator>();
            OnInit();
        }

        protected virtual void OnInit() { }

        public static IBTTask NewTask(string taskName)
        {
            if (sInstance == null)
                sInstance = new BehaviourLibrary();
            BTGenerator<IBTTask> gen;
            if (sInstance.mTasks.TryGetValue(taskName, out gen))
                return gen();
            else
                return null;
        }

        public static IBTCondition NewCondition(string conditionName)
        {
            if (sInstance == null)
                sInstance = new BehaviourLibrary();
            BTGenerator<IBTCondition> gen;
            if (sInstance.mConditions.TryGetValue(conditionName, out gen))
                return gen();
            else
                return null;
        }

        public static IBTService NewService(string serviceName)
        {
            if (sInstance == null)
                sInstance = new BehaviourLibrary();
            BTGenerator<IBTService> gen;
            if (sInstance.mServices.TryGetValue(serviceName, out gen))
                return gen();
            else
                return null;
        }

        public static BTNodeBase NewPlugin(string pluginName, int id)
        {
            if (sInstance == null)
                sInstance = new BehaviourLibrary();
            BTNodeGenerator gen;
            if (sInstance.mControllers.TryGetValue(pluginName, out gen))
                return gen(id);
            else
                return null;
        }
    }
}