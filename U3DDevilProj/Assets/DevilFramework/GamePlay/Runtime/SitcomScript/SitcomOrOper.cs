using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class SitcomOrOper : SitcomBinOper
    {
        public SitcomOrOper(string key, int priority) : base(key, priority) { }

        private SitcomOrOper(SitcomOrOper oper) : base(oper) { }

        public override ISitcomResult Result
        {
            get
            {
                var l = mLeft.Result;
                var r = mRight.Result;
                return new SitcomResult(l != null && l.State == ESitcomState.Success || r != null && r.State == ESitcomState.Success);
            }
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomOrOper(this);
            oper.keyword = keyword;
            return oper;
        }

        public override void OnExecute(SitcomContext runtime)
        {
            runtime.Push(mLeft);
            runtime.Push(mRight);
        }

        public override void OnStop(SitcomContext runtime)
        {

        }
    }
}