using Devil.AI;
using Devil.Utility;
using UnityEngine;

namespace DevilEditor
{
    public class BehaviourMeta
    {
        public string Name { get { return TargetType.Name; } }
        public string Namespace { get { return TargetType.Namespace; } }
        public string DisplayName { get; private set; }
        public System.Type TargetType { get; private set; }
        public BehaviourTreeAttribute Attribute { get; private set; }
        public string SearchName { get; private set; }
        public EBTNodeType NodeType { get; private set; }
        public Texture2D Icon
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
            DisplayName = Attribute == null ? null : Attribute.DisplayName;
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