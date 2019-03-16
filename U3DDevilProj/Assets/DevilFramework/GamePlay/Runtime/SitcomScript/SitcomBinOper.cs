using Devil.Utility;

namespace Devil.GamePlay
{
    public abstract class SitcomBinOper : SitcomOperator
    {
        protected ISitcomOper mLeft;
        protected ISitcomOper mRight;

        public SitcomBinOper(string key, int priority) : base(StringUtil.IgnoreCaseToHash(key), priority)
        {

        }

        public SitcomBinOper(SitcomBinOper bin) : base(bin) { }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return false; } }

        public override void AddContent(SitcomFile.Keyword keyword)
        {
        }

        public override void AddLeftChild(ISitcomOper oper)
        {
            if (mLeft != null)
                throw new SitcomCompileException(oper.keyword);
            mLeft = oper;
            oper.Parent = this;
        }

        public override ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced)
        {
            replaced = mRight;
            mRight = oper;
            oper.Parent = this;
            return oper;
        }

        public override void Compile()
        {
            if (mLeft == null)
                throw new SitcomCompileException(keyword);
            if (mRight == null)
                throw new SitcomCompileException(keyword);
            mLeft.Compile();
            mRight.Compile();
        }
    }
}