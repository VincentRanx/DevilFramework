namespace Devil.GamePlay
{
    public class SitcomNotOper : SitcomOperator 
	{
        ISitcomOper mCondition;

        public SitcomNotOper(string name):base(name, SitcomDefine.PRI_PREFIX) { }
        public SitcomNotOper(SitcomNotOper oper) : base(oper) { }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return false; } }

        public override ISitcomResult Result
        {
            get
            {
                return new SitcomResult(mCondition.Result.State == ESitcomState.Failed);
            }
        }

        public override void AddContent(SitcomFile.Keyword keyword)
        {
            throw new SitcomCompileException(keyword);
        }

        public override void AddLeftChild(ISitcomOper oper)
        {
            throw new SitcomCompileException(keyword);
        }

        public override ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced)
        {
            replaced = mCondition;
            mCondition = oper;
            oper.Parent = this;
            return oper;
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomNotOper(this);
            oper.keyword = keyword;
            return oper;
        }

        public override void Compile()
        {
            if (mCondition == null)
                throw new SitcomCompileException(keyword);
            mCondition.Compile();
        }

        public override void OnExecute(SitcomContext runtime)
        {
            runtime.Push(mCondition);
        }

        public override void OnStop(SitcomContext runtime)
        {
        }
    }
}