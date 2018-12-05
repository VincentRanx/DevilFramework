using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class PreloadEffects : MonoBehaviour
    {
        [SerializeField]
        int m_CacheSize = 128;
        [SerializeField]
        int m_WarmUpNum = 10;
        [SerializeField]
        List<PreloadAssets> m_Assets = new List<PreloadAssets>();

        private void Start()
        {
            PrepareEffects();
        }

        private void OnDestroy()
        {
            ClearEffects();
        }

        [ContextMenu("Prepare Effects")]
        void PrepareEffects()
        {
            var inst = EffectsManager.Instance;
            if (inst == null)
                return;

            var len = transform.childCount;
            for (int i = 0; i < len; i++)
            {
                inst.AddPrefab(transform.GetChild(i).gameObject, m_CacheSize, m_WarmUpNum);
            }
            for (int i = 0; i < m_Assets.Count; i++)
            {
                inst.AddPrefabs(m_Assets[i]);
            }
        }

        [ContextMenu("Clear Effects")]
        void ClearEffects()
        {
            var inst = EffectsManager.Instance;
            if (inst == null)
                return;
            var len = transform.childCount;
            for (int i = 0; i < len; i++)
            {
                inst.RemovePrefab(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < m_Assets.Count; i++)
            {
                inst.RemovePrefabs(m_Assets[i]);
            }
        }
    }
}