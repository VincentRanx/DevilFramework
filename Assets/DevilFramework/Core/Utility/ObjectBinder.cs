using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.Utility
{

    using UObject = UnityEngine.Object;

    public static class ObjectBinder
    {
        public static void BindPropertyiesByName(this MonoBehaviour mono)
        {
#if UNITY_EDITOR
            if (mono == null)
                return;
            Type type = mono.GetType();
            System.Reflection.FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            List<Transform> children = new List<Transform>();
            List<UObject> targets = new List<UObject>();
            mono.GetComponentsInChildren(children);
            for (int i = 0; i < fields.Length; i++)
            {
                System.Reflection.FieldInfo fld = fields[i];
                if (!fld.IsPublic && Ref.GetCustomAttribute<SerializeField>(fld) == null)
                {
                    continue;
                }
                bool isArray = Ref.IsTypeInheritedFrom(fld.FieldType, typeof(Array));
                Type targetType = isArray ? fld.FieldType.GetElementType() : fld.FieldType;
                bool isGameObj = targetType == typeof(GameObject);
                bool isComponent = Ref.IsTypeInheritedFrom(targetType, typeof(Component));
                if (!isGameObj && !isComponent)
                    continue;
                targets.Clear();
                for (int j = children.Count - 1; j >= 0; j--)
                {
                    Transform trans = children[j];
                    if (fld.Name == trans.name)
                    {
                        children.RemoveAt(j);
                        if (isGameObj)
                        {
                            targets.Insert(0, trans.gameObject);
                        }
                        else
                        {
                            Component cmp = trans.gameObject.GetComponent(targetType);
                            targets.Insert(0, cmp);
                        }
                    }
                }
                if (isArray)
                {
                    Array arr = Array.CreateInstance(targetType, targets.Count);
                    for (int n = 0; n < targets.Count; n++)
                    {
                        arr.SetValue(targets[n], n);
                    }
                    fld.SetValue(mono, arr);
                }
                else if (targets.Count > 0)
                {
                    fld.SetValue(mono, targets[0]);
                }
            }
#endif
        }

        public static void RenameBindableProperties(this MonoBehaviour mono)
        {
#if UNITY_EDITOR
            if (mono == null)
                return;
            Type type = mono.GetType();
            System.Reflection.FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            List<GameObject> targets = new List<GameObject>();
            for (int i = 0; i < fields.Length; i++)
            {
                System.Reflection.FieldInfo fld = fields[i];
                if (!fld.IsPublic && Ref.GetCustomAttribute<SerializeField>(fld) == null)
                {
                    continue;
                }
                bool isArray = Ref.IsTypeInheritedFrom(fld.FieldType, typeof(Array));
                Type targetType = isArray ? fld.FieldType.GetElementType() : fld.FieldType;
                bool isGameObj = targetType == typeof(GameObject);
                bool isComponent = Ref.IsTypeInheritedFrom(targetType, typeof(Component));
                if (!isGameObj && !isComponent)
                    continue;
                GameObject obj = null;
                targets.Clear();
                if (isArray)
                {
                    Array arr = fld.GetValue(mono) as Array;
                    for (int n = 0; n < arr.Length; n++)
                    {
                        if (isGameObj)
                        {
                            obj = arr.GetValue(n) as GameObject;
                        }
                        else
                        {
                            Component cmp = arr.GetValue(n) as Component;
                            obj = cmp == null ? null : cmp.gameObject;
                        }
                        if (obj)
                            targets.Add(obj);
                    }
                }
                else if (isGameObj)
                {
                    obj = fld.GetValue(mono) as GameObject;
                    if (obj)
                        targets.Add(obj);
                }
                else
                {
                    Component cmp = fld.GetValue(mono) as Component;
                    obj = cmp == null ? null : cmp.gameObject;
                    if (obj)
                        targets.Add(obj);
                }
                for (int n = 0; n < targets.Count; n++)
                {
                    targets[n].name = fld.Name;
                }
            }
#endif
        }
    }
}