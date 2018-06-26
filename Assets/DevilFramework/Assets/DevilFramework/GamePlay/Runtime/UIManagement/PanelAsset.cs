using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class PanelAsset
    {
        public int Id { get; private set; } // id
        public string Name { get; private set; } // 名称
        public string AssetPath { get; private set; } // 资源路径
        public Panel Asset { get; set; } // 资源
        public bool IsUsable { get; private set; }
        public EPanelProperty Properties { get; private set; }
        bool mIsBuiltInAsset;

        public PanelAsset(JObject data)
        {
            if (data != null)
            {
                Id = data.Value<int>("id");
                Name = data.Value<string>("name");
                AssetPath = data.Value<string>("assetPath");
                Properties = (EPanelProperty)data.Value<int>("property");
                IsUsable = !string.IsNullOrEmpty(AssetPath);
                mIsBuiltInAsset = false;
            }
            else
            {
                IsUsable = false;
            }
        }

        public PanelAsset(Panel asset)
        {
            if (asset != null)
            {
                Id = asset.GetInstanceID();
                Name = asset.name;
                AssetPath = null;
                Asset = asset;
                Properties = asset.Properties;
                IsUsable = true;
                mIsBuiltInAsset = true;
            }
            else
            {
                IsUsable = false;
            }
        }

        public bool IsSingleInstance { get { return (Properties & EPanelProperty.SingleInstance) != 0; } }

        public Panel InstantiateAsset()
        {
            if (!IsUsable)
                return null;
            if(Asset != null)
            {
                if(IsSingleInstance)
                    return Asset;
                GameObject go = Object.Instantiate(Asset.gameObject);
                return go.GetComponent<Panel>();
            }
            if (!mIsBuiltInAsset)
            {
                GameObject go = AssetsManager.GetAsset<GameObject>(AssetPath);
                if(go == null)
                {
                    IsUsable = false;
                    return null;
                }
                GameObject inst = Object.Instantiate(go);
                Asset = inst.GetComponent<Panel>();
                if (Asset == null)
                    IsUsable = false;
                else
                    Properties = Asset.Properties;
                return Asset;
            }
            return null;
        }
    }
}