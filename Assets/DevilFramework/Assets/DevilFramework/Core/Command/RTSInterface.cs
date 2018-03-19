using UnityEngine;
using Devil.Utility;
using System.Collections.Generic;
using org.vr.rts.modify;

namespace Devil.Command
{
    [AddComponentMenu("Devilframework/Command")]
    [RequireComponent(typeof(RTSRuntimeEditor))]
    public class RTSInterface : MonoBehaviour
    {
        RTSRuntimeEditor mCmd;
        Dictionary<string, System.Type> mUnityTypes = new Dictionary<string, System.Type>();

        void LoadUnityTypes()
        {
            mUnityTypes["GameObject"] = typeof(GameObject);
            mUnityTypes["Transform"] = typeof(Transform);
            mUnityTypes["SceneManager"] = typeof(UnityEngine.SceneManagement.SceneManager);
        }

        void Start()
        {
            mCmd = GetComponent<RTSRuntimeEditor>();
            mCmd.LoadFunction(this);
            LoadUnityTypes();


            mCmd.Execute("script(\"rts.init\")", 0);
        }

        [RTSPlugin(Name = "typeForName", ArgCount = 1, Doc = "获取对象的System.Type类型")]
        object typeoForName(object[] args)
        {
            System.Type tp;
            if (!mUnityTypes.TryGetValue(args[0].ToString(), out tp))
                tp = System.Type.GetType(args[0].ToString(), false, false);
            return tp;
        }

        [RTSPlugin(Name = "interface", ArgCount = 0)]
        object getInterface(object[] args)
        {
            return this;
        }

        [RTSPlugin(Name = "runtime", ArgCount = 0)]
        object getRuntime(object[] args)
        {
            return mCmd;
        }

        [RTSPlugin(Name = "setProperty", ArgCount = 3, Define = "setProperty(target, propertyName, propertyValue)", Doc = "根据反射设置对象属性值")]
        object setProperty(object[] args)
        {
            Ref.SetProperty(args[0], args[1].ToString(), args[2]);
            return args[2];
        }

        [RTSPlugin(Name = "getProperty", ArgCount = 2, Define = "getProperty(target, propertyName")]
        object getProperty(object[] args)
        {
            return Ref.GetProperty(args[0], args[1].ToString());
        }

        [RTSPlugin(Name = "setField", ArgCount = 3, Define = "setField(target, fieldName, fieldValue)", Doc = "根据反射设置对象属性值")]
        object setField(object[] args)
        {
            Ref.SetField(args[0], args[1].ToString(), args[2]);
            return args[2];
        }

        [RTSPlugin(Name = "getField", ArgCount = 2, Define = "getField(target, fieldName")]
        object getField(object[] args)
        {
            return Ref.GetField(args[0], args[1].ToString());
        }

        [RTSPlugin(Name = "invoke", ArgCount = -1, Doc = "invoke method", Define = "invoke(target,methodname, args...)")]
        object refMethod(object[] args)
        {
            object[] argc = args.Length > 2 ? new object[args.Length - 2] : null;
            if (argc != null)
            {
                System.Array.Copy(args, 2, argc, 0, argc.Length);
            }
            return Ref.InvokeMethod(args[0], args[1].ToString(), argc);
        }

        [RTSPlugin(Name = "strongInvoke", ArgCount = -1, Doc = "invoke method", Define = "invoke(target,methodname, args...)")]
        object refAdvanceMethod(object[] args)
        {
            object[] argc = args.Length > 2 ? new object[args.Length - 2] : null;
            if (argc != null)
            {
                System.Array.Copy(args, 2, argc, 0, argc.Length);
            }
            return Ref.AdvanceInvoke(args[0], args[1].ToString(), argc);
        }

        [RTSPlugin(Name = "new", ArgCount = 1, Doc = "New Instance Of Type")]
        object refNew(object[] args)
        {
            return Ref.NewInstance(args[0] as System.Type);
        }
    }        
}