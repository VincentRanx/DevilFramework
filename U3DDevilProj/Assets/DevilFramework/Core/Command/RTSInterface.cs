using UnityEngine;
using Devil.Utility;
using System.Collections.Generic;

namespace Devil.Command
{
    [AddComponentMenu("Devilframework/Command")]
    [RequireComponent(typeof(RTSRuntimeEditor))]
    public class RTSInterface : MonoBehaviour
    {
        protected RTSRuntimeEditor mCmd;
        protected Dictionary<string, System.Type> mUnityTypes = new Dictionary<string, System.Type>();

        protected virtual void LoadUnityTypes()
        {
            mUnityTypes["GameObject"] = typeof(GameObject);
            mUnityTypes["Transform"] = typeof(Transform);
            mUnityTypes["SceneManager"] = typeof(UnityEngine.SceneManagement.SceneManager);
        }

        protected virtual void Start()
        {
            mCmd = GetComponent<RTSRuntimeEditor>();
            mCmd.LoadFunction(this);
            LoadUnityTypes();


            mCmd.Execute("script(\"rts.init\")", 0);
        }

        [RTSPlugin(Name = "typeForName", ArgCount = 1, Doc = "获取对象的System.Type类型")]
        public object typeoForName(object[] args)
        {
            System.Type tp;
            if (!mUnityTypes.TryGetValue(args[0].ToString(), out tp))
                tp = System.Type.GetType(args[0].ToString(), false, false);
            return tp;
        }

        [RTSPlugin(Name = "interface", ArgCount = 0)]
        public object getInterface(object[] args)
        {
            return this;
        }

        [RTSPlugin(Name = "runtime", ArgCount = 0)]
        public object getRuntime(object[] args)
        {
            return mCmd;
        }

        [RTSPlugin(Name = "setProperty", ArgCount = 3, Define = "setProperty(target, propertyName, propertyValue)", Doc = "根据反射设置对象属性值")]
        public object setProperty(object[] args)
        {
            bool set;
            Ref.SetProperty(args[0], args[1].ToString(), args[2], out set);
            return args[2];
        }

        [RTSPlugin(Name = "getProperty", ArgCount = 2, Define = "getProperty(target, propertyName")]
        public object getProperty(object[] args)
        {
            bool set;
            return Ref.GetProperty(args[0], args[1].ToString(), out set);
        }

        [RTSPlugin(Name = "setField", ArgCount = 3, Define = "setField(target, fieldName, fieldValue)", Doc = "根据反射设置对象属性值")]
        public object setField(object[] args)
        {
            bool set;
            Ref.SetField(args[0], args[1].ToString(), args[2], out set);
            return args[2];
        }

        [RTSPlugin(Name = "getField", ArgCount = 2, Define = "getField(target, fieldName")]
        public object getField(object[] args)
        {
            bool set;
            return Ref.GetField(args[0], args[1].ToString(), out set);
        }

        [RTSPlugin(Name = "invoke", ArgCount = -1, Doc = "invoke method", Define = "invoke(target,methodname, args...)")]
        public object refMethod(object[] args)
        {
            object[] argc = args.Length > 2 ? new object[args.Length - 2] : null;
            if (argc != null)
            {
                System.Array.Copy(args, 2, argc, 0, argc.Length);
            }
            return Ref.InvokeMethod(args[0], args[1].ToString(), argc);
        }

        [RTSPlugin(Name = "strongInvoke", ArgCount = -1, Doc = "invoke method", Define = "invoke(target,methodname, args...)")]
        public object refAdvanceMethod(object[] args)
        {
            object[] argc = args.Length > 2 ? new object[args.Length - 2] : null;
            if (argc != null)
            {
                System.Array.Copy(args, 2, argc, 0, argc.Length);
            }
            return Ref.AdvanceInvoke(args[0], args[1].ToString(), argc);
        }

        [RTSPlugin(Name = "new", ArgCount = 1, Doc = "New Instance Of Type")]
        public object refNew(object[] args)
        {
            return Ref.NewInstance(args[0] as System.Type);
        }
    }        
}