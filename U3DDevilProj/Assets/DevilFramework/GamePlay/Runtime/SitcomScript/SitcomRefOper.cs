namespace Devil.GamePlay
{
    public class SitcomRefOper : SitcomOperator
    {
        public SitcomRefOper(SitcomFile.Keyword keyword) : base(0, SitcomDefine.PRI_REFERENCE)
        {
            this.keyword = keyword;
        }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return false; } }

        public override void AddContent(SitcomFile.Keyword keyword)
        {
        }

        public override void AddLeftChild(ISitcomOper oper)
        {
            throw new SitcomCompileException(oper.keyword);
        }

        public override ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced)
        {
            throw new SitcomCompileException(oper.keyword);
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomRefOper(keyword);
            return oper;
        }

        public override void OnExecute(SitcomContext runtime)
        {
        }
        public override ISitcomResult Result
        {
            get
            {
                return new SitcomValue(keyword.text);
            }
        }
        public override void OnStop(SitcomContext runtime){ }
        public override void Compile()
        {
        }
#if UNITY_EDITOR
        public override string ToString()
        {
            return string.Format("\"{0}\"", keyword.text);
        }
#endif
    }
}