using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace Devil.GamePlay
{
    public class PanelPreset : MonoBehaviour
    {
        [SerializeField]
        Panel[] m_Panels;
        [SerializeField]
        bool m_DontDestroyOnLoad;

        PanelAsset[] mAssets;
        LinkedList<int> mDefaultOpen;
        float mCheckTime;

        private void Awake()
        {
            if (m_DontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (PanelManager.Instance != null)
            {
                mDefaultOpen = new LinkedList<int>();
                mAssets = new PanelAsset[m_Panels.Length];
                for (int i = 0; i < m_Panels.Length; i++)
                {
                    if (m_Panels[i] == null)
                        continue;
                    bool act = m_Panels[i].gameObject.activeSelf;
                    PanelAsset asset = new PanelAsset(m_Panels[i]);
                    PanelManager.Instance.AddPanelAsset(asset);
                    mAssets[i] = asset;
                    if (act)
                        mDefaultOpen.AddLast(asset.Id);
                }
                if (mDefaultOpen.Count > 0)
                    mCheckTime = 1;
            }
            if (mAssets != null)
            {
                CheckOpen(2);
            }
        }

        void CheckOpen(float delayTime)
        {
            var node = mDefaultOpen.First;
            while (node != null)
            {
                int v = node.Value;
                var next = node.Next;
                if (PanelManager.OpenPanel(v))
                    mDefaultOpen.Remove(node);
                else
                    break;
                node = next;
            }
            if (mDefaultOpen.Count > 0)
                mCheckTime = delayTime;
        }

        private void Update()
        {
            if (mCheckTime > 0)
            {
                mCheckTime -= Time.deltaTime;
                if (mCheckTime <= 0)
                {
                    CheckOpen(3);
                }
            }
        }

        private void OnDestroy()
        {
            if (mAssets != null && PanelManager.Instance != null)
            {
                for (int i = 0; i < mAssets.Length; i++)
                {
                    if (mAssets[i] != null)
                        PanelManager.Instance.RemoveAsset(mAssets[i]);
                }
                mAssets = null;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Find Target")]
        void FindChild()
        {
            m_Panels = GetComponentsInChildren<Panel>(true);
            EditorUtility.SetDirty(this);
            if(gameObject.activeInHierarchy)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
#endif
    }
}