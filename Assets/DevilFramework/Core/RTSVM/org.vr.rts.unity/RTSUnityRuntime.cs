
using UnityEngine;
using org.vr.rts.modify;
using org.vr.rts.util;
using org.vr.rts.component;
using org.vr.rts.advance;
using System.Collections.Generic;

namespace org.vr.rts.unity
{
    public class RTSUnityRuntime : MonoBehaviour, IRTSLog, IRTSRuntime
    {
        
        #region static methods

        public static bool TryCaseEnum<T>(string str, out T value)
        {
            System.Array arr = System.Enum.GetValues(typeof(T));
            for (int i = 0; i < arr.Length; i++)
            {
                T t = (T)arr.GetValue(i);
                if (t.ToString() == str)
                {
                    value = t;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public static bool TryFindEnum<T>(string str, out T value)
        {
            System.Array arr = System.Enum.GetValues(typeof(T));
            for (int i = 0; i < arr.Length; i++)
            {
                T t = (T)arr.GetValue(i);
                if (t.ToString().ToLower().Contains(str.ToLower()))
                {
                    value = t;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        #endregion
        [Tooltip("max threads at the same time.")]
        [Range(1,100)]
        [SerializeField]
        int m_MaxThread = 20;
        

        [Tooltip("max executable lines per frame.")]
        [SerializeField]
        int ticks = 100;

        [SerializeField]
        bool m_SupportChinese;
        public bool IsChineseSupported { get { return m_SupportChinese; } }

        protected int pid;
        protected RTSEngine mEngine;
        protected RTSCompiler mCompiler;
        protected RTSThread[] mThreads;
        protected RTSThread mImmediateT;
        protected Dictionary<string, List<string>> mMsgHandler = new Dictionary<string, List<string>>();

        int mRunningThreads;
        bool mYield;
        bool mExecUpdate;

        protected virtual void Awake()
        {

            mEngine = new RTSEngine(this);
            mCompiler = new RTSCompiler();

            mImmediateT = new RTSThread(-1, 128);
            m_MaxThread = Mathf.Max(1, m_MaxThread);
            mThreads = new RTSThread[m_MaxThread];
            for (int i = 0; i < mThreads.Length; i++)
            {
                mThreads[i] = new RTSThread(i, i == 0 ? 40 : 20);
            }

            mEngine.addLinker("async", new AsyncL(this));

            AddFunction("type", new RTSPluginFunc(null, _typeof, 1));
            AddFunction("size", new RTSPluginFunc(null, sizeOf, 1));
            AddFunction("yield", new RTSPluginFunc(null, rtsYield, 0));
            AddFunction("sleep", new RTSPluginFunc(null, sleep, 1));
            AddFunction("mk_list", new RTSPluginFunc(null, mk_list, 1));
            AddFunction("toInt", new RTSPluginFunc(null, toInt, 1));
            AddFunction("toFloat", new RTSPluginFunc(null, toFloat, 1));
            AddFunction("toLong", new RTSPluginFunc(null, toLong, 1));
            AddFunction("toDouble", new RTSPluginFunc(null, toDouble, 1));
            AddFunction("toUint", new RTSPluginFunc(null, toUint, 1));
            AddFunction("toBool", new RTSPluginFunc(null, toBool, 1));
            AddFunction("toString", new RTSPluginFunc(null, toRTSString, 1));
            AddFunction("split", new RTSPluginFunc(null, split, 2));
            AddFunction("exist", new RTSPluginFunc(null, rtsExist, 1));
            AddFunction("logi", new RTSPluginFunc(null, rtsLogI, -1));
            AddFunction("logw", new RTSPluginFunc(null, rtsLogW, -1));
            AddFunction("loge", new RTSPluginFunc(null, rtsLogE, -1));
            AddFunction("compile", new RTSPluginFunc(null, inlineCompile, 1));
            AddFunction("aliasOperator", new RTSPluginFunc(null, aliasOperator, 2));
            AddFunction("aliasFunction", new RTSPluginFunc(null, aliasFunction, 3));
            AddFunction("ticks", new RTSPluginFunc(null, dateTimeTick, 0));
            AddFunction("time", new RTSPluginFunc(null, dateTime, -1));
            AddFunction("registMsg", new RTSPluginFunc(null, registMessage, 2));
            AddFunction("unregistMsg", new RTSPluginFunc(null, unregistMessage, 2));
            AddFunction("sendMsg", new RTSPluginFunc(null, sendMessage, -1));

            if (m_SupportChinese)
            {
                RTSTextReader reader = mCompiler.getReader();
                reader.SetOperators("+-*/%&|~!^=<>?:;.,@：；。，");
                reader.SetBrackets("{}()[]（）【】");
                mEngine.aliasLinker(";", "；");
                mEngine.aliasLinker(".", "。");
                mEngine.aliasLinker(",", "，");
                mEngine.aliasLinker("(", "（");
                mEngine.aliasLinker(")", "）");
                mEngine.aliasLinker("[", "【");
                mEngine.aliasLinker("]", "】");
                mEngine.aliasLinker("if", "如果");
                mEngine.aliasLinker("then", "就");
                mEngine.aliasLinker("else", "否则");
                mEngine.aliasLinker("for", "循环");
                mEngine.aliasLinker("delete", "删除");
                mEngine.aliasLinker("global", "全局");
                mEngine.aliasLinker("&&", "并且");
                mEngine.aliasLinker("&&", "和");
                mEngine.aliasLinker("||", "或者");
                mEngine.aliasLinker("!", "不是");
                mEngine.aliasLinker("!", "非");
                mEngine.aliasLinker("^", "不同于");
                mEngine.aliasLinker(">", "大于");
                mEngine.aliasLinker("<", "小于");
                mEngine.aliasLinker("=", "赋值");
                mEngine.aliasLinker("==", "等于");
                mEngine.aliasLinker("return", "返回");
                mEngine.aliasLinker("break", "结束");
                mEngine.aliasLinker("continue", "继续");
                mEngine.aliasLinker("async", "同时");
            }
        }

        protected virtual bool OnRuntimeTick(RTSThread t)
        {
            try
            {
                t.run(mEngine);
                return true;
            }
            catch (System.Exception ex)
            {
                t.catchError(IRTSDefine.Error.Runtime_IndexOutOfBounds, ex.ToString());
                return false;
            }
        }

        protected virtual void Update()
        {
            if (mExecUpdate)
            {
                mRunningThreads = 0;
                for (int i = 0; i < mThreads.Length; i++)
                {
                    pid = i;
                    int counter = 0;
                    while (!mThreads[pid].isFinished())
                    {
                        if (mThreads[pid].isSleeping())
                            break;
                        counter++;
                        mYield = false;
                        if (!OnRuntimeTick(mThreads[pid]))
                            break;
                        if (mYield || (counter > ticks && ticks > 0))
                        {
                            break;
                        }
                    }
                    if (!mThreads[pid].isFinished())
                        mRunningThreads++;
                }
                pid = 0;
                mExecUpdate = mRunningThreads > 0;
            }
        }

        private IRTSThread FindFreeThread(int threadId)
        {
            int p = threadId;
            IRTSThread t = null;
            if (threadId >= 0)
            {
                t = threadId < mThreads.Length ? mThreads[threadId] : null;
            }
            else
            {
                for (int i = 1; i <= mThreads.Length; i++)
                {
                    p = i % mThreads.Length;
                    if (mThreads[p].isFinished())
                    {
                        t = mThreads[p];
                        break;
                    }
                }
            }
            if (t == null || !t.isFinished())
                return null;
            else
                return t;
        }

        #region plugin functions

        public object aliasOperator(object[] args)
        {
            mEngine.aliasLinker(RTSString.stringOf(args[0]), RTSString.stringOf(args[1]));
            return RTSVoid.VOID;
        }

        public object aliasFunction(object[] args)
        {
            mEngine.aliasFunction(RTSString.stringOf(args[0]),RTSInteger.valueOf(args[1]), RTSString.stringOf(args[2]));
            return RTSVoid.VOID;
        }

        public object sizeOf(object[] args)
        {
            object o = args[0];
            if (o == null)
                return 0;
            else if (o is System.Collections.IList)
                return ((System.Collections.IList)o).Count;
            else
                return 1;
        }

        public object _typeof(object[] args)
        {
            object o = args[0];
            if (o == null)
                return null;
            else
                return mEngine.getRTSType(o.GetType());
        }

        protected object rtsYield(object[] args)
        {
            mYield = true;
            return RTSVoid.VOID;
        }

        protected object rtsLogW(object[] args)
        {
            if (args != null)
            {
                string msg = "";
                for (int i = 0; i < args.Length; i++)
                {
                    msg += args[i];
                }
                logWarning(msg);
            }
            return RTSVoid.VOID;
        }

        protected object rtsLogE(object[] args)
        {
            if (args != null)
            {
                string msg = "";
                for (int i = 0; i < args.Length; i++)
                {
                    msg += args[i];
                }
                logError(msg);
            }
            return RTSVoid.VOID;
        }

        protected object rtsLogI(object[] args)
        {
            if (args != null)
            {
                string msg = "";
                for (int i = 0; i < args.Length; i++)
                {
                    msg += args[i];
                }
                logInfo(msg);
            }
            return RTSVoid.VOID;
        }

        protected object sleep(object[] args)
        {
            object a = args[0];
            long t = RTSLong.valueOf(a);
            if (t > 0)
            {
                mThreads[pid].sleep(t);
            }
            return RTSVoid.VOID;
        }

        protected object mk_list(object[] args)
        {
            int n = RTSInteger.valueOf(args[0]);
            if (n <= 0)
            {
                ((IRTSLog)this).logError("mk_list 方法的参数必须是 大于0 的整数");
                return null;
            }
            return new object[n];// RTSarray(n);
        }

        protected object inlineCompile(object[] args)
        {
            return new RTSInlineCompileR(args[0].ToString());
        }

        protected object toInt(object[] args)
        {
            if (args[0] is string)
                return int.Parse(args[0].ToString());
            else if (args[0] is uint)
                return (int)(uint)args[0];
            else
                return RTSInteger.valueOf(args[0]);
        }

        protected object toUint(object[] args)
        {
            if (args[0] is string)
                return uint.Parse(args[0].ToString());
            else if (args[0] is uint)
                return args[0];
            else
                return (uint)RTSInteger.valueOf(args[0]);
        }

        protected object toFloat(object[] args)
        {
            if (args[0] is string)
                return float.Parse(args[0].ToString());
            else
                return RTSFloat.valueOf(args[0]);
        }

        protected object toLong(object[] args)
        {
            if (args[0] is string)
                return long.Parse(args[0].ToString());
            else
                return RTSLong.valueOf(args[0]);
        }

        protected object toDouble(object[] args)
        {
            if (args[0] is string)
                return double.Parse(args[0].ToString());
            else
                return RTSDouble.valueOf(args[0]);
        }

        protected object toBool(object[] args)
        {
            if (args[0] is string)
                return bool.Parse(args[0].ToString());
            else
                return RTSBool.valueOf(args[0]);
        }

        protected object toRTSString(object[] args)
        {
            return args[0] == null ? null : args[0].ToString();
        }

        protected object split(object[] args)
        {
            string v = args[0] == null ? null : args[0].ToString();
            string s = args[1] == null ? null : args[1].ToString();
            if (v == null)
                return null;
            return v.Split(s.ToCharArray());
        }

        protected object rtsExist(object[] args)
        {
            return mEngine.containsVar(RTSString.stringOf(args[0]));
        }

        protected object dateTimeTick(object[] args)
        {
            return System.DateTime.Now.Ticks;
        }

        protected object dateTime(object[] args)
        {
            if (args != null && args.Length == 1)
                return System.DateTime.Now.ToString(RTSString.stringOf(args[0]));
            else
                return System.DateTime.Now.ToString();
        }

        object sendMessage(object[] args)
        {
            if (args == null || args.Length < 1)
            {
                logError("Can't match parameters.");
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

        object registMessage(object[] args)
        {
            string msg = RTSString.stringOf(args[0]);
            string func = RTSString.stringOf(args[1]);
            if (!string.IsNullOrEmpty(msg) && !string.IsNullOrEmpty(func))
            {
                List<string> funcs;
                if (!mMsgHandler.TryGetValue(msg, out funcs))
                {
                    funcs = new List<string>();
                    mMsgHandler[msg] = funcs;
                }
                funcs.Add(func);
            }
            else
            {
                logWarning("Both msg name and function name are required.");
            }
            return RTSVoid.VOID;
        }

        object unregistMessage(object[] args)
        {
            string msg = RTSString.stringOf(args[0]);
            string func = RTSString.stringOf(args[1]);
            if (!string.IsNullOrEmpty(msg) && !string.IsNullOrEmpty(func))
            {
                List<string> funcs;
                if (mMsgHandler.TryGetValue(msg, out funcs))
                {
                    funcs.Remove(func);
                }
            }
            else
            {
                logWarning("Both msg name and function name are required.");
            }
            return RTSVoid.VOID;
        }

        #endregion

        public virtual IRTSLog logInfo(object msg)
        {
            Debug.Log(msg, this);
            return this;
        }

        public virtual IRTSLog logWarning(object msg)
        {
            Debug.LogWarning(msg, this);
            return this;
        }

        public virtual IRTSLog logError(object msg)
        {
            Debug.LogError(msg);
            return this;
        }

        #region opened method

        public void AddFunction(string funcName, IRTSFunction func, bool autoLink = true)
        {
            mEngine.addFunction(funcName, func);
            if(autoLink && func.argSize() <= 0)
            {
                mEngine.addLinker(funcName, new RTSFuncShortcutL());
            }
        }

        public void Yield()
        {
            mYield = true;
        }

        public bool Execute(string cmd)
        {
            return Execute(cmd, -1);
        }
        
        public bool LoadRunner(IRTSRunner runner, int threadId)
        {
            if (runner != null)
            {
                IRTSThread t = FindFreeThread(threadId) ;
                if (t == null)
                    return false;
                bool ret = t.loadRunner(runner);
                mExecUpdate |= ret;
                return ret;
            }
            return false;
        }

        public bool Execute(string cmd, int threadId)
        {
            if (!string.IsNullOrEmpty(cmd))
            {

                IRTSThread t = FindFreeThread(threadId);
                if (t == null)
                    return false;
                mCompiler.reset();
                mCompiler.loadSource(cmd);
                while (mCompiler.isCompiling())
                {
                    IRTSDefine.Error error = mCompiler.onCompile(mEngine);
                    if (error != 0)
                    {
                        logError("rts compile error:" + RTSUtil.getEnumDescript(typeof(IRTSDefine.Error), (int)error) + "-" + mCompiler.getCompilingLinker());
                        return false;
                    }
                }
                IRTSLinker root = mCompiler.getRootLinker();
                if (root != null)
                {
                    bool ret = t.loadRunner(root.createRunner());
                    mExecUpdate |= ret;
                    return ret;
                }
                return false;
            }
            return false;
        }

        public object ExecuteImmediate(string cmd)
        {
            mCompiler.reset();
            mCompiler.loadSource(cmd);
            while (mCompiler.isCompiling())
            {
                IRTSDefine.Error error = mCompiler.onCompile(mEngine);
                if (error != 0)
                {
                    logError("rts compile error:" + RTSUtil.getEnumDescript(typeof(IRTSDefine.Error), (int)error) + "-" + mCompiler.getCompilingLinker());
                    return null;
                }
            }
            IRTSLinker root = mCompiler.getRootLinker();
            if (root != null)
            {
                mImmediateT.loadRunner(root.createRunner());
                while (!mImmediateT.isFinished())
                {
                    try
                    {
                        mImmediateT.run(mEngine);
                    }
                    catch (System.Exception ex)
                    {
                        mImmediateT.catchError(IRTSDefine.Error.Runtime_IndexOutOfBounds, ex.ToString());
                        break;
                    }
                }
                return mImmediateT.getOutput();
            }
            else
            {
                return null;
            }
        }

        public object ExecuteFunction(string funcName, object[] args, bool immediate)
        {
            if (RTSUtil.isGoodName(funcName))
            {
                IRTSThread t = null;
                if (immediate)
                {
                    t = mImmediateT;
                }
                else
                {
                    int p = 0;
                    for (int i = 1; i <= mThreads.Length; i++)
                    {
                        p = i % mThreads.Length;
                        if (mThreads[p].isFinished())
                        {
                            t = mThreads[p];
                            break;
                        }
                    }
                }
                if (t == null || !t.isFinished())
                    return false;
                IRTSFunction func = mEngine.getFunction(funcName, args == null ? 0 : args.Length);
                if (func == null)
                    return false;
                bool ret = t.loadRunner(func.createRunner(args));
                if (immediate)
                {
                    while (!t.isFinished())
                    {
                        t.run(mEngine);
                    }
                    return t.getOutput();
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}