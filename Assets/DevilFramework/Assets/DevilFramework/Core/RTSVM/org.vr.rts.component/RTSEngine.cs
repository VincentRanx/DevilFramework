using Devil;
using org.vr.rts.linker;
using org.vr.rts.modify;
using org.vr.rts.util;

namespace org.vr.rts.component
{
    public class RTSEngine : IRTSEngine
    {
        private IRTSLinker mDefaultLinker;
        private System.Collections.Generic.Dictionary<string, IRTSLinker> mLinkers;
        private System.Collections.Generic.Dictionary<string, IRTSFunction> mFuncs;
        private System.Collections.Generic.Dictionary<string, object> mVars;
        private RTSSortedMap<System.Type, IRTSType> mTypes;
        private IRTSLog mLog;

        public RTSEngine()
        {
            init();
        }

        public RTSEngine(IRTSLog log)
        {
            init();
            mLog = log;
        }

        private void init()
        {

            mDefaultLinker = new RTSVariableL();

            IRTSLinker lin = null;
            mLinkers = new System.Collections.Generic.Dictionary<string, IRTSLinker>();

            lin = new RTSEndingL(IRTSDefine.Linker.SEMICOLON);
            mLinkers.Add(";", lin);
            //
            lin = new RTSExecL(IRTSDefine.Linker.BRACKET_FLOWER, IRTSDefine.Linker.BRACKET_FLOWER2);
            mLinkers.Add("{", lin);
            lin = new RTSEndingL(IRTSDefine.Linker.BRACKET_FLOWER2);
            mLinkers.Add("}", lin);
            //
            lin = new RTSIfL();
            mLinkers.Add("if", lin);
            lin = new RTSEndingL(IRTSDefine.Linker.THEN);
            mLinkers.Add("then", lin);
            lin = new RTSElseL();
            mLinkers.Add("else", lin);
            lin = new RTSForL();
            mLinkers.Add("for", lin);

            lin = new RTSDeleteL();
            mLinkers.Add("delete", lin);
            //
            lin = new RTSBracketL(IRTSDefine.Linker.BRACKET, IRTSDefine.Linker.BRACKET2);
            mLinkers.Add("(", lin);
            lin = new RTSEndingL(IRTSDefine.Linker.BRACKET2);
            mLinkers.Add(")", lin);

            lin = new RTSSquareL();
            mLinkers.Add("[", lin);
            lin = new RTSEndingL(IRTSDefine.Linker.BRACKET_SQUARE2);
            mLinkers.Add("]", lin);
            //
            lin = new RTSCommaL();
            mLinkers.Add(",", lin);

            lin = new RTSPropertyL(IRTSDefine.Property.GLOBAL);
            mLinkers.Add("global", lin);

            lin = new RTSEndingL(IRTSDefine.Linker.COLON);
            mLinkers.Add(":", lin);
            lin = new RTSQuestionL();
            mLinkers.Add("?", lin);

            lin = new RTSNotL();
            mLinkers.Add("!", lin);
            mLinkers.Add("not", lin);
            lin = new RTSSelfRaiseL(IRTSDefine.Linker.SELFADD);
            mLinkers.Add("++", lin);
            lin = new RTSSelfRaiseL(IRTSDefine.Linker.SELFSUB);
            mLinkers.Add("--", lin);
            //
            lin = new RTSArithmeticL(IRTSDefine.Linker.ADD);
            mLinkers.Add("+", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("+=", lin);
            lin = new RTSArithmeticL(IRTSDefine.Linker.SUB);
            mLinkers.Add("-", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("-=", lin);
            lin = new RTSArithmeticL(IRTSDefine.Linker.MUL);
            mLinkers.Add("*", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("*=", lin);
            lin = new RTSArithmeticL(IRTSDefine.Linker.DIV);
            mLinkers.Add("/", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("/=", lin);
            lin = new RTSArithmeticL(IRTSDefine.Linker.MOD);
            mLinkers.Add("%", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("%=", lin);
            lin = new RTSArithmeticL(IRTSDefine.Linker.BITAND);
            mLinkers.Add("&", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("&=", lin);
            lin = new RTSArithmeticL(IRTSDefine.Linker.BITOR);
            mLinkers.Add("|", lin);
            lin = new RTSEvaluateL((RTSBinaryL)lin);
            mLinkers.Add("|=", lin);

            lin = new RTSEvaluateL(null);
            mLinkers.Add("=", lin);

            lin = new RTSCompareL(IRTSDefine.Linker.MORE);
            mLinkers.Add(">", lin);
            lin = new RTSCompareL(IRTSDefine.Linker.MOREQU);
            mLinkers.Add(">=", lin);
            lin = new RTSCompareL(IRTSDefine.Linker.LESS);
            mLinkers.Add("<", lin);
            lin = new RTSCompareL(IRTSDefine.Linker.LESSEQU);
            mLinkers.Add("<=", lin);
            lin = new RTSCompareL(IRTSDefine.Linker.EQUAL);
            mLinkers.Add("==", lin);
            lin = new RTSCompareL(IRTSDefine.Linker.NOTEQU);
            mLinkers.Add("!=", lin);

            lin = new RTSLogicL(IRTSDefine.Linker.AND);
            mLinkers.Add("&&", lin);
            mLinkers.Add("and", lin);
            lin = new RTSLogicL(IRTSDefine.Linker.OR);
            mLinkers.Add("||", lin);
            mLinkers.Add("or", lin);
            lin = new RTSLogicL(IRTSDefine.Linker.XOR);
            mLinkers.Add("^", lin);
            mLinkers.Add("xor", lin);
            //
            lin = new RTSStackActL(IRTSDefine.Stack.ACTION_RETURN);
            mLinkers.Add("return", lin);
            lin = new RTSStackActL(IRTSDefine.Stack.ACTION_BREAK);
            mLinkers.Add("break", lin);
            lin = new RTSStackActL(IRTSDefine.Stack.ACTION_CONTINUE);
            mLinkers.Add("continue", lin);

            lin = new RTSVariableL(null);
            mLinkers.Add("null", lin);
            lin = new RTSVariableL(true);
            mLinkers.Add("true", lin);
            lin = new RTSVariableL(false);
            mLinkers.Add("false", lin);
            //
            lin = new RTSTypeL(RTSGeneral.TYPE);
            mLinkers.Add("var", lin);
            lin = new RTSTypeL(RTSString.TYPE);
            mLinkers.Add("string", lin);
            lin = new RTSTypeL(RTSInteger.TYPE);
            mLinkers.Add("int", lin);
            lin = new RTSTypeL(RTSLong.TYPE);
            mLinkers.Add("long", lin);
            lin = new RTSTypeL(RTSFloat.TYPE);
            mLinkers.Add("float", lin);
            lin = new RTSTypeL(RTSDouble.TYPE);
            mLinkers.Add("double", lin);
            lin = new RTSTypeL(RTSBool.TYPE);
            mLinkers.Add("bool", lin);
            lin = new RTSTypeL(RTSVoid.TYPE);
            mLinkers.Add("void", lin);
            lin = new RTSTypeL(RTSRegisterType.TYPE);
            mLinkers.Add("register", lin);

            mTypes = new RTSSortedMap<System.Type, IRTSType>(8);
            mTypes.add(typeof(bool), RTSBool.TYPE);
            mTypes.add(typeof(int), RTSInteger.TYPE);
            mTypes.add(typeof(long), RTSLong.TYPE);
            mTypes.add(typeof(float), RTSFloat.TYPE);
            mTypes.add(typeof(double), RTSDouble.TYPE);
            mTypes.add(typeof(string), RTSString.TYPE);
            mTypes.add(typeof(RTSRegister), RTSRegisterType.TYPE);

        }

        public void aliasLinker(string key, string alias)
        {
            IRTSLinker link;
            if (mLinkers.TryGetValue(key, out link))
            {
                mLinkers[alias] = link;
            }
        }

        public void addLinker(string key, IRTSLinker linker)
        {
            mLinkers[key] = linker;
        }

        public void removeLinker(string key)
        {
            mLinkers.Remove(key);
        }

        public bool isKeyWord(string word)
        {
            return mLinkers.ContainsKey(word);
        }

        public IRTSLinker newLinker(string src)
        {
            IRTSLinker oper;
            if (!mLinkers.TryGetValue(src, out oper))
            {
                oper = mDefaultLinker;
            }
            return oper.createInstance(src);
        }

        public object getVar(string varName)
        {
            object ret;
            if (mVars != null && mVars.TryGetValue(varName, out ret))
                return ret;
            else
                return null;
        }

        public bool containsVar(string varName)
        {
            if (mVars != null && mVars.ContainsKey(varName))
                return true;
            else
                return false;
        }

        public void removeVar(string varName)
        {
            if (mVars != null)
            {
                mVars.Remove(varName);
            }
        }

        public void addVar(string varName, object var)
        {
            if (mVars == null)
                mVars = new System.Collections.Generic.Dictionary<string, object>();
            mVars[varName] = var;
        }

        public void clearVars()
        {
            if (mVars != null)
            {
                mVars.Clear();
            }
        }

        public void addFunction(string funcName, IRTSFunction func)
        {
            if (mFuncs == null)
            {
                mFuncs = new System.Collections.Generic.Dictionary<string, IRTSFunction>();
            }
            mFuncs[RTSUtil.keyOfFunc(funcName, func.argSize())] = func;
        }

        public IRTSFunction getFunction(string funcName, int argCount)
        {
            if (mFuncs == null)
            {
                return null;
            }
            else
            {
                IRTSFunction func;
                if (!mFuncs.TryGetValue(RTSUtil.keyOfFunc(funcName, argCount), out func) && argCount != -1)
                {
                    if (!mFuncs.TryGetValue(funcName, out func))
                        func = null;
                }
                return func;
            }
        }

        public void aliasFunction(string funcName, int argC, string alias)
        {
            if (mFuncs == null)
                return;
            string fname = RTSUtil.keyOfFunc(funcName, argC);
            IRTSFunction func;
            if(mFuncs.TryGetValue(fname,out func))
            {
                mFuncs[RTSUtil.keyOfFunc(alias, argC)] = func;
            }
        }

        public void removeFunction(string funcName, int argCount)
        {
            if (mFuncs == null)
            {
                return;
            }
            else
            {
                string key = RTSUtil.keyOfFunc(funcName, argCount);
                mFuncs.Remove(key);
            }
        }

        public IRTSLog getLogger()
        {
            return mLog;
        }

        public IRTSType getRTSType(System.Type c)
        {
            if (c == null)
                return RTSGeneral.TYPE;
            for (int i = mTypes.lenth() - 1; i >= 0; i--)
            {
                System.Type c1 = mTypes.keyAt(i);
                if (c1 == c || isInherited(c, c1))
                    return mTypes.valueAt(i);
            }
            return RTSGeneral.TYPE;
        }

        public static bool isInherited(System.Type t1, System.Type from)
        {
            if (from == null || t1 == null)
                return false;
            return t1.IsSubclassOf(from);
        }

        public void addType(System.Type t1, IRTSType type)
        {
            if (t1 == null || type == null)
                return;
            int index = mTypes.lenth();
            for (int i = index - 1; i >= 0; i--)
            {
                System.Type t2 = mTypes.keyAt(i);
                if (isInherited(t1, t2))
                {
                    index = i;
                }
            }
            mTypes.insertAt(index, t1, type);
        }
    }
}