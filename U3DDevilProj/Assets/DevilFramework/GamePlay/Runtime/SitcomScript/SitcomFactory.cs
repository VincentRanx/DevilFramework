using Devil.Utility;
using System.Collections.Generic;

namespace Devil.GamePlay
{
    public static class SitcomDefine
    {
        public const int OPER_REFERENCE = 0;

        public const string OPER_DOT_NAME = ".";
        public readonly static int OPER_DOT = StringUtil.ToHash(OPER_DOT_NAME);

        public const string OPER_DOT_NAME2 = "。";
        public readonly static int OPER_DOT2 = StringUtil.ToHash(OPER_DOT_NAME2);

        public const string OPER_QUESTION_NAME = "?";
        public readonly static int OPER_QUESTION = StringUtil.ToHash(OPER_QUESTION_NAME);

        public const string OPER_QUESTION_NAME2 = "？";
        public readonly static int OPER_QUESTION2 = StringUtil.ToHash(OPER_QUESTION_NAME2);

        public const string OPER_COLON_NAME = ":";
        public readonly static int OPER_COLON = StringUtil.ToHash(OPER_COLON_NAME);

        public const string OPER_COLON_NAME2 = "：";
        public readonly static int OPER_COLON2 = StringUtil.ToHash(OPER_COLON_NAME2);

        public const string OPER_BRACKET_NAME = "(";
        public readonly static int OPER_BRACKET = StringUtil.ToHash(OPER_BRACKET_NAME);

        public const string OPER_BRACKET_NAME2 = "（";
        public readonly static int OPER_BRACKET_2 = StringUtil.ToHash(OPER_BRACKET_NAME2);

        public const string OPER_BRACKET2_NAME = ")";
        public readonly static int OPER_BRACKET_R = StringUtil.ToHash(OPER_BRACKET2_NAME);

        public const string OPER_BRACKET2_NAME2 = "）";
        public readonly static int OPER_BRACKET_R2 = StringUtil.ToHash(OPER_BRACKET2_NAME2);

        public const string OPER_ADD_NAME = "+";
        public readonly static int OPER_ADD = StringUtil.ToHash(OPER_ADD_NAME);

        public const string OPER_SUB_NAME = "-";
        public readonly static int OPER_SUB = StringUtil.ToHash(OPER_SUB_NAME);

        public const string OPER_MUL_NAME = "*";
        public readonly static int OPER_MUL = StringUtil.ToHash(OPER_MUL_NAME);

        public const string OPER_DIV_NAME = "/";
        public readonly static int OPER_DIV = StringUtil.ToHash(OPER_DIV_NAME);
        
        public const string DOMAIN_UNNAME = "unname";
        public readonly static int DOMAIN_UNNAME_ID = StringUtil.ToHash(DOMAIN_UNNAME);

        // 常用运算符优先级
        public const int PRI_LOWEST = 0;
        public const int PRI_HIGHEST = 100000;
        public const int PRI_REFERENCE = 90000;
        public const int PRI_DOMAIN = 80000;

        // 前缀
        public const int PRI_PREFIX = 40000;

        // 双目运算
        public const int PRI_BIN_HIGHT = 10000;
        public const int PRI_BIN_MIDDLE = 7000;
        public const int PRI_BIN_LOW = 4000;

        // 三目运算
        public const int PRI_TRI = 1000;

        //  命令运算符 @ ?
        public const int PRI_CMD = 100;
    }
    
    public class SitcomFactory : System.IDisposable
	{
        static SitcomFactory sInst;
        public static SitcomFactory Instance
        {
            get
            {
                if (sInst == null)
                    sInst = new SitcomFactory();
                return sInst;
            }
        }
        public static int GetMarkId(string file, string mark)
        {
            return StringUtil.IgnoreCaseToHash(StringUtil.Concat(file, '/', mark));
        }

        ISitcomOper mDefaultOper;
        List<ISitcomOper> mOperators = new List<ISitcomOper>();
        
        private SitcomFactory()
        {
            mDefaultOper = new SitcomRefOper(default(SitcomFile.Keyword));
            AddOperator(new SitcomDomainOper());
            AddOperator(new SitcomDomainOper("。"));

            AddOperator(new SitcomFixOper(":", SitcomDefine.PRI_DOMAIN + 1));
            AddOperator(new SitcomFixOper("：", SitcomDefine.PRI_DOMAIN + 1));

            AddOperator(new SitcomBracketOper());
            AddOperator(new SitcomBracketOper("（"));

            AddOperator(new SitcomFixOper(")", SitcomDefine.PRI_LOWEST));
            AddOperator(new SitcomFixOper("）", SitcomDefine.PRI_LOWEST));

            AddOperator(new SitcomConditionOper());
            AddOperator(new SitcomConditionOper("？"));

            AddOperator(new SitcomNotOper("not"));
            AddOperator(new SitcomGotoOper(">>"));
            AddOperator(new SitcomGotoOper("》"));

            AddOperator(new SitcomAndOper("and", SitcomDefine.PRI_BIN_MIDDLE + 1));
            AddOperator(new SitcomOrOper("or", SitcomDefine.PRI_BIN_MIDDLE));
        }

        public void AddOperator(ISitcomOper oper)
        {
            var index = GlobalUtil.BinsearchFromRightIndex(mOperators, oper.Identify);
            if(index == -1)
            {
                mOperators.Add(oper);
            }
            else if(mOperators[index].Identify == oper.Identify)
            {
                throw new System.Exception(string.Format("Duplicate Define Of Operator \"{0}\"", oper.Identify.ToString("x")));
            }
            else
            {
                mOperators.Insert(index, oper);
            }
        }

        public ISitcomOper GetOperator(SitcomFile.Keyword keyword)
        {
            if (keyword.type == KeywordType.Content)
                return mDefaultOper.Clone(keyword);
            var oper = GlobalUtil.Binsearch(mOperators, keyword.id);
            if (oper == null)
                oper = mDefaultOper;
            return oper.Clone(keyword);
        }

        public ISitcomOper GetDefine(int id)
        {
            return GlobalUtil.Binsearch(mOperators, id);
        }

        public void Dispose()
        {
            mOperators.Clear();
        }
        
    }
}