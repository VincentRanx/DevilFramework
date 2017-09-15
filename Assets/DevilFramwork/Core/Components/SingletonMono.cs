using UnityEngine;

namespace DevilTeam
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
            }
            return sInstance;
        }

        protected virtual void Awake()
        {
            if (sInstance && (sInstance != this))
            {
#if UNITY_EDITOR
                Debug.LogError(string.Format("There are more than one SingletonMono of {0}, check it first.\nWe found {1} and {2}",
                    GetType().ToString(), name, sInstance.name));
#endif
                Destroy(this);
            }
            else
            {
                sInstance = this as T;
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