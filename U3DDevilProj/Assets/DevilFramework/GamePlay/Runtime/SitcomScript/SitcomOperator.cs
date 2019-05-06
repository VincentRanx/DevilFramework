using Devil.Utility;

namespace Devil.GamePlay
{

    public enum KeywordType
    {
        None,
        Cmd,
        Keyword,
        Content,
        Operator,
    }

    //public interface ISitcomSequence
    //{
    //    bool NextCmd();
    //    bool NextKeyword();
    //    bool NextContent();
    //    SitcomFile.Keyword keyword { get; }
    //    void SetNextMark(int mark);
    //    void SetNextMark(string mark);
    //}
    
    public interface ISitcomOper: IIdentified , ISitcomExec
    {
        int Priority { get; }
        ISitcomOper Parent { get; set; }
        SitcomFile.Keyword keyword { get; }
        //  括号等锁定
        bool IsBlock { get; }
        bool RequireContent { get; }
        ISitcomOper Clone(SitcomFile.Keyword keyword);
        void AddLeftChild(ISitcomOper oper);
        ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced);
        void AddContent(SitcomFile.Keyword keyword);
        void Compile();
    }

    public abstract class SitcomOperator : ISitcomOper
    {
        public int Priority { get; private set; }
        public int Identify { get; private set; }
        public SitcomFile.Keyword keyword { get; set; }
        public ISitcomOper Parent { get; set; }

        public SitcomOperator(int id, int priority)
        {
            Identify = id;
            Priority = priority;
        }

        public SitcomOperator(string name, int priority)
        {
            Identify = StringUtil.IgnoreCaseToHash(name);
            Priority = priority;
        }

        public SitcomOperator(SitcomOperator oper)
        {
            this.Identify = oper.Identify;
            this.keyword = oper.keyword;
            this.Priority = oper.Priority;
        }

        public abstract ISitcomOper Clone(SitcomFile.Keyword keyword);
        public abstract bool IsBlock { get; }
        public abstract void AddLeftChild(ISitcomOper oper);
        public abstract ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced);
        public abstract bool RequireContent { get; }
        public abstract void AddContent(SitcomFile.Keyword keyword);

        #region runtime
        public abstract void OnExecute(SitcomContext runtime);
        public abstract void OnStop(SitcomContext runtime);
        public abstract void Compile();
        public abstract ISitcomResult Result { get; }
        #endregion
    }

    public class SitcomFixOper: SitcomOperator
    {
        public SitcomFixOper(string keyword, int priority):base(StringUtil.IgnoreCaseToHash(keyword), priority) { }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return false; } }

        public override void AddContent(SitcomFile.Keyword keyword)
        {
            throw new SitcomCompileException(keyword);
        }
        public override ISitcomResult Result { get { return null; } }
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
            this.keyword = keyword;
            return this;
        }

        public override void OnExecute(SitcomContext runtime)
        {
            throw new SitcomCompileException(keyword);
        }

        public override void OnStop(SitcomContext runtime)
        {
            throw new SitcomCompileException(keyword);
        }
        public override void Compile()
        {
            throw new SitcomCompileException(keyword);
        }
#if UNITY_EDITOR
        public override string ToString()
        {
            return keyword.text;
        }
#endif
    }
}