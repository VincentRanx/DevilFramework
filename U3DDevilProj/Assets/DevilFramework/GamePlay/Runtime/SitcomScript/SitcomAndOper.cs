using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class SitcomAndOper : SitcomBinOper
    {
        public SitcomAndOper(string key, int priority) : base(key, priority) { }

        private SitcomAndOper(SitcomAndOper oper) : base(oper) { }

        public override ISitcomResult Result
        {
            get
            {
                var l = mLeft.Result;
                var r = mRight.Result;
                return new SitcomValue(l != null && r != null && l.State == ESitcomState.Success && r.State == ESitcomState.Success);
            }
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomAndOper(this);
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