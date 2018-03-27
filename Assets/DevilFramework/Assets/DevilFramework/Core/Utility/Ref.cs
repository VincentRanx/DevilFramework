// reflection methods.
#if !UNITY_IOS || UNITY_EDITOR
#define USE_REFLECTION
#endif

using UnityEngine;

#if USE_REFLECTION
using System.Reflection;
#endif
using System.Collections.Generic;

namespace Devil.Utility
{

    public static class Ref
    {
        public static object NewInstance(System.Type tp)
        {
#if USE_REFLECTION
            if (tp == null)
                return null;
            ConstructorInfo cons = tp.GetConstructor(new System.Type[0]);
            if (cons != null)
            {
                return cons.Invoke(null);
            }
            else
            {
                return null;
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.NewInstance(Type)\" require it!");
            return null;
#endif
        }

        public static bool IsTypeInheritedFrom(System.Type type, System.Type baseType, bool considerSelf = true)
        {
            System.Type tp = considerSelf ? type : type.BaseType;
            while(tp != null)
            {
                if (tp == baseType)
                    return true;
                tp = tp.BaseType;
            }
            return false;
        }

        public static object GetProperty(object target, string propertyName)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(propertyName))
                return null;
            System.Type type;
            object tar;
            BindingFlags flag;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    tar = null;
                    flag = BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    type = target.GetType();
                    tar = target;
                    flag = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
                }
                PropertyInfo pinfo = type.GetProperty(propertyName, flag);
                return pinfo.GetValue(tar, null);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.GetProperty(object, string)\" require it!");
            return null;
#endif
        }

        public static void SetProperty(object target, string propertyName, object value)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(propertyName))
                return;
            System.Type type;
            object tar = null;
            BindingFlags flag;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    tar = null;
                    flag = BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    type = target.GetType();
                    tar = target;
                    flag = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
                }
                PropertyInfo pinfo = type.GetProperty(propertyName, flag);
                pinfo.SetValue(tar, value, null);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("faild to set property:" + propertyName + " for " + tar);
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.SetProperty(object, string, object)\" require it!");
#endif
        }

        public static object GetField(object target, string fieldName)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(fieldName))
                return null;
            System.Type type;
            object tar;
            BindingFlags flag;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    tar = null;
                    flag = BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    type = target.GetType();
                    tar = target;
                    flag = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
                }
                FieldInfo pinfo = type.GetField(fieldName, flag);
                return pinfo.GetValue(tar);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.GetField(object, string)\" require it!");
            return null;
#endif
        }


        public static void SetField(object target, string fieldName, object value)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(fieldName))
                return;
            System.Type type;
            object tar;
            BindingFlags flag;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    tar = null;
                    flag = BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    type = target.GetType();
                    tar = target;
                    flag = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
                }
                FieldInfo pinfo = type.GetField(fieldName, flag);
                pinfo.SetValue(tar, value);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.SetField(object, object)\" require it!");
#endif
        }

        public static object InvokeMethod(object target, string method, object[] args)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(method))
                return null;
            System.Type type;
            object tar;
            BindingFlags flag;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    tar = null;
                    flag = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    type = target.GetType();
                    tar = target;
                    flag = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public;
                }
                MethodInfo pinfo = type.GetMethod(method, flag);
                return pinfo.Invoke(tar, args);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.InvokeMethod(object, string, object[])\" require it!");
            return null;
#endif
        }

        public static object AdvanceInvoke(object target, string method, object[] args)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(method))
                return null;
            System.Type type;
            object tar;
            BindingFlags flag;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    tar = null;
                    flag = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    type = target.GetType();
                    tar = target;
                    flag = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public;
                }
                MethodInfo[] infos = type.GetMethods(flag);
                for (int i = 0; i < infos.Length; i++)
                {
                    MethodInfo pinfo = infos[i];
                    if (pinfo.Name == method && MatchParam(pinfo, args))
                    {
                        return pinfo.Invoke(tar, args);
                    }
                }
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.AdvanceInvoke(object, string, object[])\" require it!");
            return null;
#endif
        }

#if USE_REFLECTION
        private static bool MatchParam(MethodInfo minfo, object[] args)
        {
            int alen = args == null ? 0 : args.Length;
            ParameterInfo[] pams = minfo.GetParameters();
            int blen = pams == null ? 0 : pams.Length;
            if (alen != blen)
                return false;
            for (int i = 0; i < blen; i++)
            {
                if (args[i] != null && args[i].GetType() != pams[i].ParameterType && !args[i].GetType().IsSubclassOf(pams[i].ParameterType))
                    return false;
            }
            return true;
        }

#endif

        public static T GetCustomAttribute<T>(object memberInfo) where T : System.Attribute
        {
#if USE_REFLECTION
            MemberInfo field = memberInfo as MemberInfo;
            return System.Attribute.GetCustomAttribute(field, typeof(T)) as T;
            //object[] attr = field.GetCustomAttributes(true);
            //for (int i = 0; i < attr.Length; i++)
            //{
            //    if (attr[i] is T)
            //        return attr[i] as T;
            //}
            //return null;
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.GetCustomAttribute<T>(MemberInfo)\" require it!");
            return null;
#endif
        }


        public static object[] GetMethodsWithAttribute<T>(System.Type type, bool inherit = true) where T : System.Attribute
        {
#if USE_REFLECTION
            List<MethodInfo> methods = new List<MethodInfo>();
            MethodInfo[] mtds = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for(int i= 0; i < mtds.Length; i++)
            {
                object[] attr = mtds[i].GetCustomAttributes(typeof(T), inherit);
                int len = attr == null ? 0 : attr.Length;
                if (len > 0)
                {
                    methods.Add(mtds[i]);
                }
            }
            return methods.ToArray();
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.GetMethodsWithAttribute<T>(Type)\" require it!");
            return null;
#endif
        }

        public static object[] GetMethodsWithParams(System.Type type, System.Type retType, System.Type[] paramTypes)
        {
#if UNITY_EDITOR
            List<MethodInfo> methods = new List<MethodInfo>();
            MethodInfo[] mtds = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for(int i = 0; i < mtds.Length; i++)
            {
                if(MatchMethodRetAndParams(mtds[i], retType, paramTypes))
                {
                    methods.Add(mtds[i]);
                }
            }
            return methods.ToArray();
#else
            return null;
#endif
        }

        public static bool MatchMethodRetAndParams(object method, System.Type retType,System.Type[] paramTypes)
        {
#if USE_REFLECTION
            MethodInfo mtd = method as MethodInfo;
            if (mtd == null)
                return false;
            if (mtd.ReturnType != retType)
                return false;
            ParameterInfo[] mparam = mtd.GetParameters();
            int mlen = mparam == null ? 0 : mparam.Length;
            int slen = paramTypes == null ? 0 : paramTypes.Length;
            if (mlen != slen)
                return false;
            for(int i = 0; i < mlen; i++)
            {
                if (mparam[i].ParameterType != paramTypes[i])
                {
                    return false;
                }
            }
            return true;
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.MatchMethodWithDelegate<T>(object, T)\" require it!");
            return false;
#endif
        }
    }
}