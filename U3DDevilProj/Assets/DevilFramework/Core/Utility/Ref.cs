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

#if USE_REFLECTION
        static object GetStaticProperty(System.Type type, string propertyName, out bool hasproperty)
        {
            BindingFlags flag = BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
            PropertyInfo pinfo = type.GetProperty(propertyName, flag);
            if (pinfo != null)
            {
                hasproperty = true;
                return pinfo.GetValue(type, null);
            }
            else if (type.BaseType != null)
            {
                return GetStaticProperty(type.BaseType, propertyName, out hasproperty);
            }
            else
            {
                hasproperty = false;
                return null;
            }
        }

        static object GetProperty(object target, System.Type type, string propertyName, out bool hasproperty)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
            PropertyInfo pinfo = type.GetProperty(propertyName, flag);
            if (pinfo != null)
            {
                hasproperty = true;
                return pinfo.GetValue(target, null);
            }
            else if (type.BaseType != null)
            {
                return GetProperty(target, type.BaseType, propertyName, out hasproperty);
            }
            else
            {
                hasproperty = false;
                return null;
            }
        }
#endif

        public static object GetProperty(object target, string propertyName, out bool hasproperty)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(propertyName))
            {
                hasproperty = false;
                return null;
            }
            System.Type type;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    return GetStaticProperty(type, propertyName, out hasproperty);
                }
                else
                {
                    type = target.GetType();
                    return GetProperty(target, type, propertyName, out hasproperty);
                }
            }
            catch (System.Exception e)
            {
                hasproperty = false;
                Debug.LogError(e);
                return null;
            }
#else
            hasproperty = false;
            Debug.LogWarning("Reflection is not supported but \"Ref.GetProperty(object, string)\" require it!");
            return null;
#endif
        }

#if USE_REFLECTION
        static void SetStaticProperty(System.Type type, string propertyName, object value, out bool hasproperty)
        {
           BindingFlags  flag = BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
            PropertyInfo pinfo = type.GetProperty(propertyName, flag);
            if (pinfo != null)
            {
                hasproperty = true;
                pinfo.SetValue(null, value, null);
            }
            else if (type.BaseType != null)
                SetStaticProperty(type.BaseType, propertyName, value, out hasproperty);
            else
                hasproperty = false;
        }

        static void SetProperty(object target ,System.Type type, string propertyName, object value, out bool hasproperty)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
            PropertyInfo pinfo = type.GetProperty(propertyName, flag);
            if (pinfo != null)
            {
                hasproperty = true;
                pinfo.SetValue(target, value, null);
            }
            else if (type.BaseType != null)
                SetProperty(target, type.BaseType, propertyName, value, out hasproperty);
            else
                hasproperty = false;
        }
#endif

        public static void SetProperty(object target, string propertyName, object value, out bool hasproperty)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(propertyName))
            {
                hasproperty = false;
                return;
            }
            System.Type type;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    SetStaticProperty(type, propertyName, value, out hasproperty);
                }
                else
                {
                    type = target.GetType();
                    SetProperty(target, type, propertyName, value, out hasproperty);
                }
            }
            catch (System.Exception e)
            {
                hasproperty = false;
                Debug.LogError(e);
                Debug.LogError("faild to set property:" + propertyName + " for " + target);
            }
#else
            hasproperty = false;
            Debug.LogWarning("Reflection is not supported but \"Ref.SetProperty(object, string, object)\" require it!");
#endif
        }

#if USE_REFLECTION
        static object GetStaticField(System.Type type, string fieldName, out bool hasfield)
        {
            BindingFlags flag = BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo pinfo = type.GetField(fieldName, flag);
            if (pinfo != null)
            {
                hasfield = true;
                return pinfo.GetValue(null);
            }
            else if (type.BaseType != null)
            {
                return GetStaticField(type.BaseType, fieldName, out hasfield);
            }
            else
            {
                hasfield = false;
                return null;
            }
        }
        static object GetField(object target, System.Type type, string fieldName, out bool hasfield)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo pinfo = type.GetField(fieldName, flag);
            if (pinfo != null)
            {
                hasfield = true;
                return pinfo.GetValue(target);
            }
            else if (type.BaseType != null)
            {
                return GetField(target, type.BaseType, fieldName, out hasfield);
            }
            else
            {
                hasfield = false;
                return null;
            }
        }
#endif

        public static object GetField(object target, string fieldName, out bool hasfield)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(fieldName))
            {
                hasfield = false;
                return null;
            }
            System.Type type;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    return GetStaticField(type, fieldName, out hasfield);
                }
                else
                {
                    type = target.GetType();
                    return GetField(target, type, fieldName, out hasfield);
                }
            }
            catch (System.Exception e)
            {
                hasfield = false;
                Debug.LogError(e);
                return null;
            }
#else
            hasfield = false;
            Debug.LogWarning("Reflection is not supported but \"Ref.GetField(object, string)\" require it!");
            return null;
#endif
        }

#if USE_REFLECTION
        static void SetStaticField(System.Type type, string fieldName, object value, out bool hasfield)
        {
            BindingFlags flag = BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo pinfo = type.GetField(fieldName, flag);
            if (pinfo != null)
            {
                hasfield = true;
                pinfo.SetValue(null, value);
            }
            else if (type.BaseType != null)
                SetStaticField(type.BaseType, fieldName, value, out hasfield);
            else
                hasfield = false;
        }
        static void SetField(object target, System.Type type, string fieldName, object value, out bool hasfield)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo pinfo = type.GetField(fieldName, flag);
            if (pinfo != null)
            {
                hasfield = true;
                pinfo.SetValue(target, value);
            }
            else if (type.BaseType != null)
                SetField(target, type.BaseType, fieldName, value, out hasfield);
            else
                hasfield = false;
        }
#endif

        public static void SetField(object target, string fieldName, object value, out bool hasfield)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(fieldName))
            {
                hasfield = false;
                return;
            }
            System.Type type;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    SetStaticField(type, fieldName, value, out hasfield);
                }
                else
                {
                    type = target.GetType();
                    SetField(target, type, fieldName, value, out hasfield);
                }
            }
            catch (System.Exception e)
            {
                hasfield = false;
                Debug.LogError(e);
            }
#else
            hasfield = false;
            Debug.LogWarning("Reflection is not supported but \"Ref.SetField(object, object)\" require it!");
#endif
        }

        public static object GetFieldOrProperty(object target, string fieldName)
        {
            bool hasfield;
            object ret = GetField(target, fieldName, out hasfield);
            if (hasfield)
                return ret;
            ret = GetProperty(target, fieldName, out hasfield);
            if (hasfield)
                return ret;
            return null;
        }

        public static bool TryGetFieldOrProperty(object target, string fieldname, out object value)
        {
            bool hasfield;
            object ret = GetField(target, fieldname, out hasfield);
            if (!hasfield)
                ret = GetProperty(target, fieldname, out hasfield);
            value = ret;
            return hasfield;
        }

        public static bool SetFieldOrProperty(object target , string fieldName, object value)
        {
            bool hasfield;
            SetField(target, fieldName, value, out hasfield);
            if (hasfield)
                return hasfield;
            SetProperty(target, fieldName, value, out hasfield);
            return hasfield;
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

#if USE_REFLECTION
        static bool CallStatic(System.Type type, string method, object[] args, out object ret)
        {
            BindingFlags flag = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public;
            MethodInfo[] infos = type.GetMethods(flag);
            for (int i = 0; i < infos.Length; i++)
            {
                MethodInfo pinfo = infos[i];
                if (pinfo.Name == method && MatchParam(pinfo, args))
                {
                    ret = pinfo.Invoke(null, args);
                    return true;
                }
            }
            if (type.BaseType != null)
                return CallStatic(type.BaseType, method, args, out ret);
            else
            {
                ret = null;
                return false;
            }
        }
        static bool CallMethod(object target, System.Type type, string method, object[] args, out object ret)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public;
            MethodInfo[] infos = type.GetMethods(flag);
            for (int i = 0; i < infos.Length; i++)
            {
                MethodInfo pinfo = infos[i];
                if (pinfo.Name == method && MatchParam(pinfo, args))
                {
                    ret = pinfo.Invoke(target, args);
                    return true;
                }
            }
            if (type.BaseType != null)
                return CallMethod(target, type.BaseType, method, args, out ret);
            else
            {
                ret = null;
                return false;
            }
        }
#endif
        public static bool CallMethod(object target, string method, object[] args, out object ret)
        {
#if USE_REFLECTION
            if (target == null || string.IsNullOrEmpty(method))
            {
                ret = null;
                return false;
            }
            System.Type type;
            try
            {
                if (target is System.Type)
                {
                    type = target as System.Type;
                    return CallStatic(type, method, args, out ret);
                }
                else
                {
                    type = target.GetType();
                    return CallMethod(target, type, method, args, out ret);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                ret = null;
                return false;
            }
#else
            Debug.LogWarning("Reflection is not supported but \"Ref.AdvanceInvoke(object, string, object[])\" require it!");
            ret = null;
            return false;
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
#if USE_REFLECTION
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