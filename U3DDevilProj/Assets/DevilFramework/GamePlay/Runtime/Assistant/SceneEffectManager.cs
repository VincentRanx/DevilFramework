using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class SceneEffectManager : MonoBehaviour
    {
        static SceneEffectManager sActiveMgr;

        public class Effect
        {
            public int Group { get; private set; }
            public int Id { get; private set; }
            public GameObject Gobject { get; private set; }
            public float LifeTime { get; set; }
            public bool IsActive { get; set; }

            public Effect(int group, GameObject go)
            {
                Group = group;
                Gobject = go;
                Id = go.GetInstanceID();
            }

            public void Reactive()
            {
                if (Gobject.activeSelf)
                    Gobject.SetActive(false);
                Gobject.SetActive(true);
            }
        }

        /// <summary>
        /// 生成特效对象
        /// </summary>
        /// <param name="group"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="lifeTime">特效生存时间，如果大于0，将在这段时间之后自动加入到对象池，否则需要开发者通过 Unspawn 方法主动回收</param>
        /// <returns></returns>
        public static Effect SpawnEffect(int group, Vector3 pos, Quaternion rot, float lifeTime = 0)
        {
            if (!sActiveMgr || group < 0 || group >= sActiveMgr.m_Prefabs.Count)
                return null;
            sActiveMgr.mTmpPos = pos;
            sActiveMgr.mTmpRot = rot;
            Effect go = sActiveMgr.mEffectPool.GetAnyTarget(group);
            go.Gobject.transform.position = pos;
            go.Gobject.transform.rotation = rot;
            go.IsActive = true;
            go.LifeTime = lifeTime;
            if (lifeTime > 0)
            {
                sActiveMgr.mTickLifeEffects.AddLast(go);
                sActiveMgr.mGlobalActiveNums++;
                if (sActiveMgr.mGlobalActiveNums > sActiveMgr.m_MaxNumForAllEffect)
                {
                    Effect eff = sActiveMgr.mTickLifeEffects.First.Value;
                    sActiveMgr.mTickLifeEffects.RemoveFirst();
                    sActiveMgr.mGlobalActiveNums--;
                    UnSpawnEffect(eff);
                }
            }
            return go;
        }

        public static Effect SpawnEffect(int group)
        {
            if (!sActiveMgr || group < 0 || group >= sActiveMgr.m_Prefabs.Count)
                return null;
            Effect go = sActiveMgr.mEffectPool.GetAnyTarget(group);
            go.IsActive = true;
            return go;
        }

        public static void UnSpawnEffect(Effect eff)
        {
            if (sActiveMgr && eff != null && eff.IsActive)
            {
                eff.IsActive = false;
                sActiveMgr.mEffectPool.SaveBuffer(eff.Group, eff);
            }
        }

        public static int FindEffectGroupByName(string effName)
        {
            if (!sActiveMgr)
                return -1;
            for (int i = 0; i < sActiveMgr.mUseablePrefabs.Length; i++)
            {
                if (sActiveMgr.mUseablePrefabs[i].name == effName)
                    return i;
            }
            return -1;
        }

        [Header("所有特效预设体")]
        public List<GameObject> m_Prefabs = new List<GameObject>();
        [Header("所有特效数量上限(仅限自动销毁的对象)")]
        public int m_MaxNumForAllEffect = 200;

        [Range(5, 100)]
        public int m_InitCapacity = 10;

        ObjectBuffers<Effect> mEffectPool;
        LinkedList<Effect> mTickLifeEffects;
        Vector3 mTmpPos;
        Quaternion mTmpRot;
        Effect mTmpEff; // 临时特效对象
        int mGlobalActiveNums; // 全局特效数量
        GameObject[] mUseablePrefabs; // 可用的特效预设提

        Effect InitEffect(int group)
        {
            GameObject go = Instantiate(mUseablePrefabs[group], mTmpPos, mTmpRot);
            return new Effect(group, go);
        }

        bool DestroyEffect(int group, Effect effect)
        {
            if (effect != null)
                Destroy(effect.Gobject);
            return true;
        }

        private void Awake()
        {
            if (!sActiveMgr)
            {
                sActiveMgr = this;
                enabled = true;
                mTickLifeEffects = new LinkedList<Effect>();
                mUseablePrefabs = new GameObject[m_Prefabs.Count];
                m_Prefabs.CopyTo(mUseablePrefabs);

                mEffectPool = new ObjectBuffers<Effect>(mUseablePrefabs.Length, m_InitCapacity, InitEffect, DestroyEffect);
                for (int i = 0; i < mUseablePrefabs.Length; i++)
                {
                    if (mUseablePrefabs[i].activeInHierarchy)
                    {
                        mEffectPool.SaveBuffer(i, new Effect(i, mUseablePrefabs[i]));
                    }
                }
            }
            else
            {
                RTLog.LogError(LogCat.Game, "同一场景中只允许存在一个 SceneEffectManager 实例");
                enabled = false;
            }
        }

        private void Update()
        {
            LinkedListNode<Effect> node = mTickLifeEffects.First;
            while (node != null)
            {
                mTmpEff = node.Value;
                mTmpEff.LifeTime -= Time.deltaTime;
                LinkedListNode<Effect> next = node.Next;
                if (!mTmpEff.IsActive || mTmpEff.LifeTime <= 0)
                {
                    mTickLifeEffects.Remove(node);
                    if (mGlobalActiveNums > 0)
                        mGlobalActiveNums--;
                    UnSpawnEffect(mTmpEff);
                }
                node = next;
            }
            mTmpEff = null;
        }

        private void OnDestroy()
        {
            if (sActiveMgr == this)
            {
                mTickLifeEffects.Clear();
                mEffectPool.Clear();
                sActiveMgr = null;
            }
        }
    }
}