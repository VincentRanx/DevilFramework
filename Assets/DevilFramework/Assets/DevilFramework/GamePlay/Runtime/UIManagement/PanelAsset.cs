using Devil.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Devil.GamePlay
{
    public class PanelAsset
    {
        public int Id { get; private set; } // id
        public string Name { get; private set; } // 名称
        public string AssetPath { get; private set; } // 资源路径
        //public Panel Asset { get; set; } // 资源
        public bool IsUsable { get; private set; }
        public EPanelMode Mode { get; private set; }
        public EPanelProperty Properties { get; private set; }
        Panel mBuiltinAsset;
        bool mIsBuiltIn;

        ObjectBuffer<Panel> mBuffer;

        public PanelAsset(JObject data)
        {
            if (data != null)
            {
                Id = data.Value<int>("id");
                Name = data.Value<string>("name");
                AssetPath = data.Value<string>("assetPath");
                Mode = (EPanelMode)data.Value<int>("mode");
                Properties = (EPanelProperty)data.Value<int>("property");
                IsUsable = !string.IsNullOrEmpty(AssetPath);
                mIsBuiltIn = false;
                mBuffer = new ObjectBuffer<Panel>(IsSingleInstance ? 1 : 5);
                mBuffer.Creater = LoadFromResources;
                mBuffer.Destroier = UnloadResources;
            }
            else
            {
                IsUsable = false;
            }
        }

        public PanelAsset(int id, string name, string resPath, EPanelMode mode, EPanelProperty property)
        {
            Id = id;
            Name = name;
            AssetPath = resPath;
            Mode = mode;
            Properties = property;
            IsUsable = !string.IsNullOrEmpty(AssetPath);
            mIsBuiltIn = false;
            mBuffer = new ObjectBuffer<Panel>(IsSingleInstance ? 1 : 5);
            mBuffer.Creater = LoadFromResources;
            mBuffer.Destroier = UnloadResources;
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
                Properties = asset.m_Properties;
                asset.gameObject.SetActive(false);
                IsUsable = true;
                mIsBuiltIn = true;
                mBuffer = new ObjectBuffer<Panel>(IsSingleInstance ? 1 : 5);
                mBuffer.Creater = LoadFromBuiltin;
                mBuffer.Destroier = UnloadResources;
                mBuffer.SaveBuffer(mBuiltinAsset);
            }
            else
            {
                IsUsable = false;
            }
        }

        public bool IsSingleInstance { get { return (Properties & EPanelProperty.SingleInstance) != 0; } }

        public bool IsUseMask { get { return (Properties & EPanelProperty.DisableMask) == 0; } }

        public void Release()
        {
            if (mBuffer != null)
            {
                mBuffer.Clear();
            }
        }

        public void UnuseAsset(Panel panel)
        {
            if(mBuffer != null)
            {
                mBuffer.SaveBuffer(panel);
            }
        }

        Panel LoadFromResources()
        {
            if (mBuiltinAsset != null)
            {
                GameObject go = Object.Instantiate(mBuiltinAsset.gameObject);
                go.name = Name;
                Panel panel = go.GetComponent<Panel>();
                panel.m_Mode = Mode;
                panel.m_Properties = Properties;
                return panel;
            }
            else
            {
                GameObject go = AssetsManager.GetAsset<GameObject>(AssetPath);
                if (go == null)
                    return null;
                GameObject inst = Object.Instantiate(go);
                inst.name = Name;
                mBuiltinAsset = inst.GetComponent<Panel>();
                mBuiltinAsset.m_Mode = Mode;
                mBuiltinAsset.m_Properties = Properties;
                return mBuiltinAsset;
            }
        }

        Panel LoadFromBuiltin()
        {
            GameObject go = Object.Instantiate(mBuiltinAsset.gameObject);
            go.name = Name;
            Panel panel = go.GetComponent<Panel>();
            panel.m_Mode = Mode;
            panel.m_Properties = Properties;
            return panel;
        }

        bool UnloadResources(Panel panel)
        {
            bool eqb = panel == mBuiltinAsset;
            if(!eqb || !mIsBuiltIn)
            {
                Object.Destroy(panel.gameObject);
                if (eqb)
                    mBuiltinAsset = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public Panel InstantiateAsset()
        {
            if (!IsUsable)
                return null;
            if (IsSingleInstance && mBuiltinAsset != null)
            {
                return mBuiltinAsset;
            }
            else if (mBuffer != null)
            {
                return mBuffer.GetAnyTarget();
            }
            else
            {
                return null;
            }
        }
    }
}