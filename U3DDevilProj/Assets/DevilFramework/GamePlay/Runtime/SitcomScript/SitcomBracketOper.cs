using Devil.Utility;

namespace Devil.GamePlay
{
    public class SitcomBracketOper : SitcomOperator
    {
        bool mFinish;
        public SitcomBracketOper() : base(SitcomDefine.OPER_BRACKET, SitcomDefine.PRI_HIGHEST) { }

        public SitcomBracketOper(string keyword) : base(StringUtil.IgnoreCaseToHash(keyword), SitcomDefine.PRI_HIGHEST) { }

        ISitcomOper mContent;

        public override bool IsBlock { get { return !mFinish; } }

        public override bool RequireContent { get { return false; } }

        public override ISitcomResult Result { get { return mContent != null ? mContent.Result : new SitcomValue(null); } }

        public override void AddContent(SitcomFile.Keyword keyword) { }

        public override void AddLeftChild(ISitcomOper oper)
        {
            throw new SitcomCompileException(oper.keyword);
        }

        public override ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced)
        {
            if (mFinish)
                throw new SitcomCompileException(oper.keyword);
            if(oper.Identify == SitcomDefine.OPER_BRACKET_R || oper.Identify == SitcomDefine.OPER_BRACKET_R2)
            {
                mFinish = true;
                replaced = null;
                return this;
            }
            replaced = mContent;
            mContent = oper;
            mContent.Parent = this;
            return oper;
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomBracketOper();
            oper.keyword = keyword;
            return oper;
        }

        public override void Compile()
        {
            if (!mFinish)
                throw new SitcomCompileException(keyword);
            if (mContent != null)
                mContent.Compile();
        }

        public override void OnExecute(SitcomContext runtime)
        {
            if (mContent != null)
                runtime.Push(mContent);
        }

        public override void OnStop(SitcomContext runtime)
        {
        }
    }
}