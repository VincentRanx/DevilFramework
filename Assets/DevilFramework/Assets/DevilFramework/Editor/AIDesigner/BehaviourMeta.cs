using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace DevilEditor
{
    public enum EInputType
    {
        text,
        raw,
    }

    public class BehaviourInputProperty
    {
        public string PropertyName { get; private set; }
        public EInputType InputType { get; private set; }
        public string InputData { get; set; }

        public BehaviourInputProperty(string desc)
        {
            int n = desc.IndexOf(':');
            if (n > 0)
                PropertyName = desc.Substring(0, n).Trim();
            if (n > 0 && n < desc.Length - 1)
                InputType = desc.Substring(n + 1).Trim().ToLower() == "raw" ? EInputType.raw : EInputType.text;
            else
                InputType = EInputType.text;
            if (InputType == EInputType.raw)
                InputData = "0";
        }

        public BehaviourInputProperty(string propertyName, EInputType type)
        {
            PropertyName = PropertyName;
            InputType = type;
            if (InputType == EInputType.raw)
                InputData = "0";
        }

        public BehaviourInputProperty(BehaviourInputProperty prop)
        {
            PropertyName = prop.PropertyName;
            InputType = prop.InputType;
            InputData = prop.InputData;
        }

        public string GetJsonPattern()
        {
            if (InputType == EInputType.raw)
                return string.Format("\"{0}\":{1}", PropertyName, string.IsNullOrEmpty(InputData) ? "0" : InputData);
            else
                return string.Format("\"{0}\":\"{1}\"", PropertyName, InputData);
        }
    }

    public class BehaviourMeta
    {
        public int SortOrder { get { return Attribute == null ? 0 : Attribute.SortOrder; } }
        public string Name { get { return TargetType.Name; } }
        public string Namespace { get { return TargetType.Namespace; } }
        public string DisplayName { get; private set; }
        public System.Type TargetType { get; private set; }
        public BehaviourTreeAttribute Attribute { get; private set; }
        public string SearchName { get; private set; }
        public EBTNodeType NodeType { get; private set; }
        public BehaviourInputProperty[] Properties { get; private set; }
        public string SubTitle { get { return Attribute == null ? "" : Attribute.SubTitle; } }
        public Texture2D Background
        {
            get
            {
                if (Attribute == null || string.IsNullOrEmpty(Attribute.IconPath))
                    return null;
                else
                    return DevilEditorUtility.GetTexture(Attribute.IconPath);
            }
        }

        public string FrameStyle
        {
            get
            {
                string style = Attribute == null ? null : Attribute.FrameStyle;
                if (string.IsNullOrEmpty(style))
                    style = "flow node 0";
                return style;
            }
        }



        public BehaviourMeta(System.Type target)
        {
            TargetType = target;
            Attribute = Ref.GetCustomAttribute<BehaviourTreeAttribute>(target);
            List<BehaviourInputProperty> properties = new List<BehaviourInputProperty>();
            if(Attribute != null)
            {
                DisplayName = Attribute.DisplayName;
                if (!string.IsNullOrEmpty(Attribute.InputDatas))
                {
                    string[] data = Attribute.InputDatas.Split(',');
                    for(int i=0;i<data.Length;i++)
                    {
                        string str = data[i].Trim();
                        if (string.IsNullOrEmpty(str))
                            continue;
                        properties.Add(new BehaviourInputProperty(data[i]));
                    }
                }
            }
            Properties = properties.ToArray();
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = target.Name;

            SearchName = Name.ToLower() + " " + DisplayName.ToLower();
            System.Type[] interfaces = target.GetInterfaces();
            foreach (System.Type i in interfaces)
            {
                if (i == typeof(IBTTask))
                {
                    NodeType = EBTNodeType.task;
                    break;
                }
                else if (i == typeof(IBTCondition))
                {
                    NodeType = EBTNodeType.condition;
                    break;
                }
                else if (i == typeof(IBTService))
                {
                    NodeType = EBTNodeType.service;
                    break;
                }
            }
            if (target.IsSubclassOf(typeof(BTNodeBase)) && target.GetConstructor(new System.Type[] { typeof(int) }) != null)
            {
                NodeType = EBTNodeType.controller;
            }
        }

    }
}