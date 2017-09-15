using UnityEngine;
using DevilTeam.Utility;
using System.Collections.Generic;
using org.vr.rts.advance;
using org.vr.rts.modify;

namespace DevilTeam.Command
{
    [AddComponentMenu("Devilframework/Command")]
    [RequireComponent(typeof(RTSRuntimeEditor))]
    public class RTSInterface : MonoBehaviour
    {
        RTSRuntimeEditor mCmd;
        Dictionary<string, System.Type> mUnityTypes = new Dictionary<string, System.Type>();
        Dictionary<string, List<string>> mMsgHandler = new Dictionary<string, List<string>>();

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

        [RTSPlugin(Name = "sendMsg", ArgCount = -1, Define = "runner sendMsg(msgName, args... )", Doc = "Send Message")]
        object sendMessage(object[] args)
        {
            if(args == null || args.Length < 1)
            {
                mCmd.logError("Can't match parameters.");
            }
            List<string> funcs = mMsgHandler[RTSString.stringOf(args[0])];
            object[] argValues = args.Length > 1 ? new object[args.Length - 1] : null;
            if (argValues != null)
            {
                for (int i = 0; i < argValues.Length; i++)
                {
                    argValues[i] = args[i + 1];
                }
            }
            return new RTSFuncListR(null, funcs.ToArray(), args.Length - 1, argValues);
        }

        [RTSPlugin(Name = "handleMsg", ArgCount = 2, Define = "void handleMsg(msgName, functionName)", Doc = "Handle Message")]
        object handleMessage(object[] args)
        {
            string msg = RTSString.stringOf(args[0]);
            string func = RTSString.stringOf(args[1]);
            if (!string.IsNullOrEmpty(msg) && !string.IsNullOrEmpty(func))
            {
                List<string> funcs;
                if(!mMsgHandler.TryGetValue(msg,out funcs))
                {
                    funcs = new List<string>();
                    mMsgHandler[msg] = funcs;
                }
                funcs.Add(func);
            }
            else
            {
                mCmd.logWarning("Both msg name and function name are required.");
            }
            return RTSVoid.VOID;
        }
    }
}