using Devil.Utility;

namespace Devil.GamePlay
{
    public class SitcomConditionOper : SitcomOperator
    {
        ISitcomOper mLeft;
        ISitcomOper mRight;

        public SitcomConditionOper():base(SitcomDefine.OPER_QUESTION, SitcomDefine.PRI_BIN_LOW) { }

        public SitcomConditionOper(string keyword) : base(StringUtil.IgnoreCaseToHash(keyword), SitcomDefine.PRI_BIN_LOW) { }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return mLeft != null && mLeft.RequireContent; } }

        public override ISitcomResult Result { get { return mLeft == null ? new SitcomResult(null) : mLeft.Result; } }

        public override void AddContent(SitcomFile.Keyword keyword)
        {
            if (mLeft != null)
                mLeft.AddContent(keyword);
        }

        public override void AddLeftChild(ISitcomOper oper)
        {
            if (mLeft != null)
                throw new SitcomCompileException(keyword);
            mLeft = oper;
        }

        public override ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced)
        {
            replaced = mRight;
            mRight = oper;
            mRight.Parent = this;
            return mRight;
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomConditionOper();
            oper.keyword = keyword;
            return oper;
        }

        public override void Compile()
        {
            if(mLeft != null)
                mLeft.Compile();
            if(mRight != null)
                mRight.Compile();
        }

        public override void OnExecute(SitcomContext runtime)
        {
            if(mRight != null)
                runtime.Push(mRight);
        }

        public override void OnStop(SitcomContext runtime)
        {
            var ret = mRight != null ? mRight.Result : null;
            if (ret != null && ret.State == ESitcomState.Success && mLeft != null)
                runtime.Push(mLeft);
        }
    }
}