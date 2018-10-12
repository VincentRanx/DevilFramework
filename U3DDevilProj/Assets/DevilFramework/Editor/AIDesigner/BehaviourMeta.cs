using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DevilEditor
{
    public class BTSharedValue
    {
        public System.Type SharedType { get; private set; }
        public string Name { get; private set; }
        public Deserializer Ctor { get; set; }

        public BTSharedValue(System.Type type)
        {
            SharedType = SharedType;
            Name = type.FullName;
        }

        public object Instantiate(string data)
        {
            if(Ctor != null)
            {
                return Ctor(data);
            }
            else
            {
                return null;
            }
        }
    }

    public class BTInputProperty
    {
        public string PropertyName { get; private set; }
        public string TypeName { get; private set; }
        public string DefaultValue { get; set; }
        string mInputData;
        bool mIsDataDirty;
        public string InputData
        {
            get
            {
                return mInputData;
            }
            set
            {
                if (mInputData != value)
                {
                    mIsDataDirty = true;
                    mInputData = value;
                }
            }
        }

        public bool IsDefaultValue { get { return mInputData == DefaultValue; } }

        public BTInputProperty Copy()
        {
            BTInputProperty pro = new BTInputProperty(PropertyName, TypeName);
            pro.DefaultValue = DefaultValue;
            pro.mInputData = mInputData;
            pro.mIsDataDirty = mIsDataDirty;
            return pro;
        }

        public bool ReguexData()
        {
            if (mIsDataDirty)
            {
                mIsDataDirty = false;
                string reg = DevilCfg.ReguexTypeValue(TypeName, mInputData, DefaultValue);
                bool dirty = reg != mInputData;
                if (dirty)
                {
                    mInputData = reg;
                }
                else
                {
                    DefaultValue = reg;
                }
                return dirty;
            }
            else
            {
                return false;
            }
        }

        public BTInputProperty(string propertyName, string type)
        {
            PropertyName = propertyName;
            TypeName = type;
            InputData = DevilCfg.DefaultTypeValue(TypeName);
            DefaultValue = InputData;
        }

        public BTInputProperty(BTInputProperty prop)
        {
            PropertyName = prop.PropertyName;
            TypeName = prop.TypeName;
            InputData = prop.InputData;
            DefaultValue = prop.DefaultValue;
        }

        public BTInputProperty(FieldInfo field, BTVariableAttribute attr)
        {
            if (string.IsNullOrEmpty(attr.Name))
                PropertyName = field.Name;
            else
                PropertyName = attr.Name;
            if (string.IsNullOrEmpty(attr.TypeName))
                TypeName = field.FieldType.Name;
            else
                TypeName = attr.TypeName;
            if (string.IsNullOrEmpty(attr.DefaultVallue))
                DefaultValue = DevilCfg.DefaultTypeValue(TypeName);
            else
                DefaultValue = attr.DefaultVallue;
            InputData = DefaultValue;
        }
    }

    public class BehaviourMeta
    {
        public int SortOrder { get; private set; }
        public string Name { get; private set; }
        public string Namespace { get; private set; }
        public string DisplayName { get; private set; }
        public string NotDisplayName { get; private set; }
        //public System.Type TargetType { get; private set; }
        public string SearchName { get; private set; }
        public EBTNodeType NodeType { get; private set; }
        public BTInputProperty[] Properties { get; private set; }
        public string SubTitle { get; private set; }
        public string Category { get; private set; }
        public Texture2D Icon { get; private set; }
        public bool HideProperty { get; private set; }
        public bool IsDecoratorNode { get { return NodeType == EBTNodeType.condition || NodeType == EBTNodeType.service; } }
        public bool IsCompositeNode { get { return NodeType == EBTNodeType.controller || NodeType == EBTNodeType.task; } }
        public Color color { get; set; }

        public BehaviourMeta(System.Type target)
        {
            //TargetType = target;
            Name = target.Name;
            DisplayName = Name;
            Namespace = target.Namespace;
            string iconPath = "";
            if (target.IsSubclassOf(typeof(BTNodeBase)))
            {
                NodeType = EBTNodeType.controller;
                Category = "Composite";
                iconPath = Installizer.InstallRoot + "/DevilFramework/Editor/Icons/composite.png";
            }
            else if (target.IsSubclassOf(typeof(BTTaskBase)))
            {
                NodeType = EBTNodeType.task;
                Category = "Task";
                iconPath = Installizer.InstallRoot + "/DevilFramework/Editor/Icons/task.png";
            }
            else if (target.IsSubclassOf(typeof(BTConditionBase)))
            {
                NodeType = EBTNodeType.condition;
                Category = "Condition";
                iconPath = Installizer.InstallRoot + "/DevilFramework/Editor/Icons/condition.png";
            }
            else if (target.IsSubclassOf(typeof(BTServiceBase)))
            {
                NodeType = EBTNodeType.service;
                Category = "Service";
                iconPath = Installizer.InstallRoot + "/DevilFramework/Editor/Icons/service.png";
            }
            else
            {
                NodeType = EBTNodeType.invalid;
                Category = "Invalid";
            }

            BTCompositeAttribute attr = Ref.GetCustomAttribute<BTCompositeAttribute>(target);
            if (attr != null)
            {
                if (!string.IsNullOrEmpty(attr.Title))
                    DisplayName = attr.Title;
                if (!string.IsNullOrEmpty(attr.Detail))
                    SubTitle = attr.Detail;
                if (!string.IsNullOrEmpty(attr.IconPath))
                    iconPath = attr.IconPath;
                if (!string.IsNullOrEmpty(attr.Category))
                    Category = attr.Category;
                HideProperty = attr.HideProperty;
            }
            FieldInfo[] fields = target.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<BTInputProperty> propperties = new List<BTInputProperty>();
            for(int i = 0; i < fields.Length; i++)
            {
                BTVariableAttribute vatt = Ref.GetCustomAttribute<BTVariableAttribute>(fields[i]);
                if(vatt != null)
                {
                    BTInputProperty pro = new BTInputProperty(fields[i], vatt);
                    propperties.Add(pro);
                }
            }
            Icon = DevilEditorUtility.GetTexture(iconPath);
            Properties = propperties.ToArray();
            NotDisplayName = string.Format("<b><color=yellow>NOT</color></b> {0}", DisplayName);
            SearchName = Name.ToLower() + " " + DisplayName.ToLower();
            color = BehaviourModuleManager.GetOrNewInstance().GetCategoryColor(Category);
        }
        
    }
}