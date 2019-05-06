using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public interface IManagedEffect
    {
        int poolId { get; set; }
        GameObject gameObject { get; }
        Transform transform { get; }
        T GetComponent<T>();
        float lifeTime { get; set; }
        bool isAlive { get; }
        void Reactive();
        void Deactive();
    }

    public class EffectsManager : MonoBehaviour
    {
        public static int StringToId(string str)
        {
            return StringUtil.IgnoreCaseToHash(str);
        }

        public static IManagedEffect SpawnEffect(string eff, Vector3 pos, Quaternion rot, float lifeTime = 0)
        {
            return SpawnEffect(StringToId(eff), pos, rot, lifeTime);
        }

        public static EffectsManager Instance { get; private set; }

        public static IManagedEffect SpawnEffect(string eff, float lifeTime = -1)
        {
            return SpawnEffect(StringToId(eff), lifeTime);
        }

        public static IManagedEffect SpawnEffect(int poolId, float lifeTime = -1)
        {
            var inst = Instance;
            if (inst == null || inst.mPools == null)
                return null;
            var pool = inst.mPools.GetData(poolId);
            if (pool == null)
                return null;
            var eff = pool.Get();
            eff.lifeTime = lifeTime;
            eff.Reactive();
            return eff;
        }

        /// <summary>
        /// 生成特效对象
        /// </summary>
        /// <param name="poolId"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="lifeTime">特效生存时间，如果大于等于0，将在这段时间之后自动加入到对象池，否则需要开发者通过 Unspawn 方法主动回收</param>
        /// <returns></returns>
        public static IManagedEffect SpawnEffect(int poolId, Vector3 pos, Quaternion rot, float lifeTime = -1)
        {
            var inst = Instance;
            if (inst == null || inst.mPools == null)
                return null;
            var pool = inst.mPools.GetData(poolId);
            if (pool == null)
                return null;
            pool.mTmpPos = pos;
            pool.mTmpRot = rot;
            IManagedEffect go = pool.Get();
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.lifeTime = lifeTime;
            go.Reactive();
            return go;
        }

        public static void UnSpawnEffect(IManagedEffect eff)
        {
            var inst = Instance;
            if (inst != null && eff != null)
            {
                var pool = inst.mPools.GetData(eff.poolId);
                if (pool == null)
                {
                    Destroy(eff.gameObject);
                }
                else
                {
                    pool.Cache(eff);
                    if (eff.transform.parent != inst.transform)
                        eff.transform.SetParent(inst.transform);
                }
            }
            else if (eff != null)
            {
                Destroy(eff.gameObject);
            }
        }
        
        [Header("预加载特效")]
        [SerializeField]
        PreloadAssets[] m_PreloadAssets;

        [SerializeField]
        bool m_DontDestroyOnLoad;

        private AvlTree<Pool> mPools;
        
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                mPools = new AvlTree<Pool>((x) => x.PoolId);
                if (m_PreloadAssets != null)
                {
                    for (int i = 0; i < m_PreloadAssets.Length; i++)
                    {
                        AddPrefabs(m_PreloadAssets[i]);
                    }
                }
                if (m_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
                name = name + "-unused";
            }
        }

        public void AddPrefabs(PreloadAssets assets)
        {
            if (assets != null)
            {
                for (int j = 0; j < assets.m_Assets.Count; j++)
                {
                    var t = assets.m_Assets[j];
                    if (t != null)
                        AddPrefab(t, assets.m_CacheSize, assets.m_WarmUpNum);
                }
            }
        }

        public void RemovePrefabs(PreloadAssets assets)
        {
            if(assets != null)
            {
                for (int j = 0; j < assets.m_Assets.Count; j++)
                {
                    var t = assets.m_Assets[j];
                    if (t != null)
                        RemovePrefab(t);
                }
            }
        }

        public void AddPrefab(GameObject prefab, int cachesize = 128, int warmUpNum = 0)
        {
            if (prefab == null)
                return;
            var id = StringToId(prefab.name);
            var pool = mPools.GetData(id);
            if (pool == null)
                pool = new Pool(prefab, cachesize);
            pool.mPrefab = prefab;
            mPools.Add(pool);
            for(int i= 0; i < warmUpNum; i++)
            {
                var eff = pool.InstantiatePrefab();
                eff.transform.SetParent(transform);
                pool.Cache(eff);
            }
        }

        public void RemovePrefab(GameObject prefab)
        {
            if (prefab == null)
                return;
            var id = StringToId(prefab.name);
            var pool = mPools.GetData(id);
            if(pool != null && pool.mPrefab == prefab)
            {
                pool.Clear();
                mPools.RemoveById(id);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                if (mPools != null)
                    mPools.ClearWithCallback((x) => x.Clear());
            }
        }

        public class Pool
        {
            public GameObject mPrefab;
            public string Name { get; private set; }
            public int PoolId { get; private set; }
            public Vector3 mTmpPos;
            public Quaternion mTmpRot;
            ObjectPool<IManagedEffect> mPool;

            public Pool(GameObject prefab, int capacity)
            {
                Name = prefab.name;
                PoolId = StringToId(Name);
                mPrefab = prefab;
                mTmpPos = prefab.transform.position;
                mTmpRot = prefab.transform.rotation;
                mPool = new ObjectPool<IManagedEffect>(capacity, InstantiatePrefab, DestroyPrefabInstance);
            }

            public IManagedEffect InstantiatePrefab()
            {
                GameObject go = GameObject.Instantiate(mPrefab, mTmpPos, mTmpRot);
                var mg = go.GetComponent<IManagedEffect>();
                if (mg == null)
                    mg = go.AddComponent<EffectActivitor>();
                mg.poolId = PoolId;
                return mg;
            }

            public void DestroyPrefabInstance(IManagedEffect effect)
            {
                if (effect != null)
                {
                    GameObject.Destroy(effect.gameObject);
                }
            }

            public IManagedEffect Get()
            {
                return mPool.Get();
            }

            public void Cache(IManagedEffect eff)
            {
                mPool.Add(eff);
            }

            public void Remove(IManagedEffect eff)
            {
                mPool.Remove(eff);
            }

            public void Clear()
            {
                mPool.Clear();
            }

            public int Length { get { return mPool.Length; } }
            public int Capacity { get { return mPool.Capacity; } }
        }
        
    }
}