namespace Devil.GamePlay
{
    public class SitcomGotoOper : SitcomOperator
    {
        ISitcomOper mLeft;
        ISitcomOper mRight;

        public SitcomGotoOper(string mark) : base(mark, SitcomDefine.PRI_BIN_LOW) { }

        public SitcomGotoOper(SitcomFile.Keyword keyword):base(keyword.id, SitcomDefine.PRI_BIN_LOW)
        {
            this.keyword = keyword;
        }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return mLeft != null && mLeft.RequireContent; } }

        public override ISitcomResult Result { get { return mLeft == null ? new SitcomValue(null) : mLeft.Result; } }

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
            var oper = new SitcomGotoOper(keyword);
            return oper;
        }

        public override void Compile()
        {
            if (mLeft != null)
                mLeft.Compile();
            if (mRight != null)
                mRight.Compile();
        }

        public override void OnExecute(SitcomContext runtime)
        {
            if(mLeft != null)
                runtime.Push(mLeft);
            if(mRight != null)
                runtime.Push(mRight);
        }

        public override void OnStop(SitcomContext runtime)
        {
            var ret = mRight == null ? null : mRight.Result;
            if (ret != null && ret.Result != null)
            {
                runtime.SetNextMark(ret.Result.ToString());
            }
        }
    }
}