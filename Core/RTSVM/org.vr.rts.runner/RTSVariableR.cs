using org.vr.rts.modify;

namespace org.vr.rts.runner
{

    public class RTSVariableR : IRTSRunner
    {

        protected IRTSDefine.Property mProperty;
        protected string mVar;
        protected object mValue;
        protected IRTSType mCast;

        public RTSVariableR(IRTSDefine.Property property, IRTSType castType, string varName)
        {
            mVar = varName;
            mCast = castType;
            mProperty = property;
        }

        public RTSVariableR(IRTSType castType, object value)
        {
            mCast = castType;
            mValue = castType == null ? value : castType.castValue(value);
            mProperty = IRTSDefine.Property.CONST;
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public bool isConst()
        {
            return (mProperty & IRTSDefine.Property.CONST) != 0 && (mProperty & IRTSDefine.Property.DECALRE) == 0;
        }

        public void loadedOnThread()
        {
            if (!isConst())
            {
                mValue = null;
            }
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public object getOutput()
        {
            return mValue;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (isConst())
                return 0;
            if ((mProperty & IRTSDefine.Property.GLOBAL) == 0)
            {
                mValue = stack.getVar(mVar);
            }
            else
            {
                mValue = stack.getThread().getEngine().getVar(mVar);
            }
            if (mCast != null)
                mValue = mCast.castValue(mValue);
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            if (!isConst())
            {
                if (mCast != null)
                    mValue = mCast.castValue(value);
                else
                    mValue = value;
                if (mValue != RTSVoid.VOID)
                {
                    if ((mProperty & IRTSDefine.Property.GLOBAL) != 0)
                    {
                        stack.getThread().getEngine().addVar(mVar,
                                (mProperty & IRTSDefine.Property.DECALRE) == 0 ? value : mValue);
                    }
                    else
                    {
                        stack.addVar(mVar, (mProperty & IRTSDefine.Property.DECALRE) == 0 ? value : mValue);
                    }
                }
                return true;
            }
            else
            {
                stack.getThread().catchError(IRTSDefine.Error.Runtime_DenyEvaluate, mVar + " cannot be evaluated.");
                return false;
            }
        }
    }
}