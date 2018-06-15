using UnityEngine;

namespace Devil
{
    public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        private static T sInstance = null;

        public static T Instance
        {
            get
            {
                return sInstance;
            }
        }

        public static T GetOrNewInstance()
        {
            if (!sInstance)
            {
                GameObject obj = new GameObject(string.Format("{0}[Singleton]", typeof(T).ToString()));
                sInstance = obj.AddComponent<T>();
#if UNITY_EDITOR
                Debug.LogFormat("Instance Singleton<{0}>.", sInstance.GetType());
#endif
            }
            return sInstance;
        }

        [SerializeField]
        protected bool m_DontDestroyOnLoad = true;

        protected virtual void Awake()
        {
            if (sInstance && (sInstance != this))
            {
#if UNITY_EDITOR
                Debug.LogWarning(string.Format("There are more than one SingletonMono of {0}, check it first.\nWe found {1} and {2}",
                    GetType().ToString(), name, sInstance.name));
#endif
                Destroy(this);
            }
            else
            {
                sInstance = this as T;
                if(m_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
        }


        protected virtual void OnDestroy()
        {
            if(sInstance == this)
            {
                sInstance = null;
            }
        }
    }
}