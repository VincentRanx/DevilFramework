using Devil.GamePlay.Assistant;
using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay
{
    public class PanelAsset
    {
        public int Id { get; private set; } // id
        public string Name { get; private set; } // 名称
        public string AssetPath { get; private set; } // 资源路径
        public bool IsUsable { get; private set; }
        public EPanelMode Mode { get; private set; }
        public EPanelProperty Properties { get; private set; }
        Panel mBuiltinAsset;
        bool mIsBuiltIn;
        Panel mSingleInstance; // 单例

        ObjectPool<Panel> mPool;

        public PanelAsset(int id, string name, string resPath, EPanelMode mode, EPanelProperty property)
        {
            Id = id;
            Name = name;
            AssetPath = resPath;
            Mode = mode;
            Properties = property;
            IsUsable = !string.IsNullOrEmpty(AssetPath);
            mIsBuiltIn = false;
            if (!IsSingleInstance)
            {
                mPool = new ObjectPool<Panel>(16);
                mPool.SetConstructor(InstantiatePanel);
                mPool.SetCleaner(DestroyPanel);
            }
        }

        public PanelAsset(Panel asset)
        {
            if (asset != null)
            {
                Name = asset.name;
                Id = string.IsNullOrEmpty(Name) ? 0 : StringUtil.ToHash(Name);
                AssetPath = null;
                mBuiltinAsset = asset;
                Mode = asset.m_Mode;
                Properties = asset.Properties;
                IsUsable = true;
                mIsBuiltIn = true;
                if (!IsSingleInstance)
                {
                    mPool = new ObjectPool<Panel>(16);
                    mPool.SetConstructor(InstantiatePanel);
                    mPool.SetCleaner(DestroyPanel);
                    mPool.Add(mBuiltinAsset);
                }
                else
                {
                    mSingleInstance = mBuiltinAsset;
                }
            }
            else
            {
                IsUsable = false;
            }
        }

        public bool IsSingleInstance { get { return (Properties & EPanelProperty.SingleInstance) != 0; } }

        public bool IsUseMask { get { return (Properties & EPanelProperty.DisableMask) == 0; } }

        public bool AutoCloseOnLoadScene { get { return (Properties & EPanelProperty.AutoCloseOnLoadScene) != 0; } }

        public void Release()
        {
            if (mPool != null)
            {
                mPool.Clear();
                if (mBuiltinAsset != null)
                    mPool.Add(mBuiltinAsset);
            }
            if (mSingleInstance != null)
            {
                DestroyPanel(mSingleInstance);
                mSingleInstance = null;
            }
        }

        public void UnuseAsset(Panel panel)
        {
            if (mPool != null && panel != null)
            {
                mPool.Add(panel);
            } 
        }

        Panel InstantiatePanel()
        {
            if (mBuiltinAsset != null)
            {
                GameObject go = Object.Instantiate(mBuiltinAsset.gameObject, mBuiltinAsset.transform.parent);
                go.name = Name;
                Panel panel = go.GetComponent<Panel>();
                panel.m_Mode = Mode;
                panel.Properties = Properties;
                return panel;
            }
            else
            {
                GameObject go = AssetsUtil.GetAsset<GameObject>(AssetPath);
                if (go == null)
                    return null;
                GameObject inst = Object.Instantiate(go, PanelManager.Instance.transform);
                inst.name = Name;
                var panel = inst.GetComponent<Panel>();
                panel.m_Mode = Mode;
                panel.Properties = Properties;
                return panel;
            }
        }

        void DestroyPanel(Panel panel)
        {
            if(panel != null && panel != mBuiltinAsset)
            {
                if (mSingleInstance == panel)
                    mSingleInstance = null;
                Object.Destroy(panel.gameObject);
            }
        }

        public Panel InstantiateAsset()
        {
            if (!IsUsable)
                return null;
            Panel ret;
            if (IsSingleInstance)
            {
                if (mSingleInstance == null)
                    mSingleInstance = InstantiatePanel();
                ret = mSingleInstance;
            }
            else
            {
                ret = mPool.Get();
            }
            if (ret == null)
                IsUsable = false;
            return ret;
        }
    }
}