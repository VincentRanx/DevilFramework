using System.Collections.Generic;

namespace Devil.GamePlay
{
    public class SitcomCmd : ISitcomExec
	{
        ISitcomOper mOper;
        List<SitcomFile.Keyword> mKeys;
        
        public SitcomCmd()
        {
            mKeys = new List<SitcomFile.Keyword>(32);
        }

        public ISitcomResult Result { get { return mOper == null ? null : mOper.Result; } }

        public void OnExecute(SitcomContext runtime)
        {
            if (mOper != null)
                runtime.Push(mOper);
        }

        public void OnStop(SitcomContext runtime) { }

        public bool Read(SitcomFile sitcom)
        {
            if (sitcom.NextCmd())
            {
                mKeys.Clear();
                while (sitcom.NextKeyword())
                {
                    mKeys.Add(sitcom.keyword);
                }
                ISitcomOper replaced;
                var dom = new SitcomDomainOper();
                dom.keyword = sitcom.keyword;
                mOper = dom;
                for (int i = 0; i < mKeys.Count; i++)
                {
                    var oper = SitcomFactory.Instance.GetOperator(mKeys[i]);
                    var root = GetRoot(mOper, oper.Priority);
                    if (!root.IsBlock && root.Priority >= oper.Priority)
                    {
                        oper.AddLeftChild(root);
                        mOper = oper;
                    }
                    else
                    {
                        mOper = root.AddRightChild(oper, out replaced);
                        if (replaced != null)
                            oper.AddLeftChild(replaced);
                    }
                }
                while (mOper.Parent != null)
                    mOper = mOper.Parent;
                if (mOper.RequireContent && sitcom.NextContent())
                    mOper.AddContent(sitcom.keyword);
                mOper.Compile();
                return true;
            }
            else
            {
                mOper = null;
                return false;
            }
        }
        
        ISitcomOper GetRoot(ISitcomOper oper, int priority)
        {
            while(!oper.IsBlock && oper.Priority >= priority && oper.Parent != null)
            {
                oper = oper.Parent;
            }
            return oper;
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            if (mOper == null)
                return "SitcomCmd:[NOT SET CMD]";
            else
                return string.Format("SitcomCmd:\n{0}", mOper);
        }
#endif
    }
}