using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    // .
    public class SitcomDomainOper : SitcomOperator
    {
        public class KeyValue
        {
            public ISitcomOper key;
            public ISitcomOper value;
            public bool useKey;

            public object GetValue()
            {
                var v = useKey ? value : key;
                return v == null ? null : v.Result;
            }

        }

        List<KeyValue> mKVs;
        KeyValue mKey;
        bool mUseKV;

        bool mWaitContent;
        ISitcomOper mLeft;
        SitcomFile.Keyword mRight;
        SitcomFile.Keyword mContent;

        public SitcomDomainOper() : base(SitcomDefine.OPER_DOT, SitcomDefine.PRI_DOMAIN)
        {
            mKVs = new List<KeyValue>(3);
        }

        public SitcomDomainOper(string keyword) : base(StringUtil.IgnoreCaseToHash(keyword), SitcomDefine.PRI_DOMAIN)
        {
            mKVs = new List<KeyValue>(3);
        }

        public override bool IsBlock { get { return false; } }

        public override bool RequireContent { get { return mWaitContent; } }

        ISitcomResult mResult;
        public override ISitcomResult Result { get { return mResult; } }

        public override void AddContent(SitcomFile.Keyword keyword)
        {
            mContent = keyword;
        }

        public override void AddLeftChild(ISitcomOper oper)
        {
            if (oper.Identify != SitcomDefine.OPER_DOT && oper.Identify != SitcomDefine.OPER_DOT2)
                throw new SitcomCompileException(oper.keyword);
            mLeft = oper;
        }

        public override ISitcomOper AddRightChild(ISitcomOper oper, out ISitcomOper replaced)
        {
            replaced = null;
            if (oper.Identify == SitcomDefine.OPER_COLON || oper.Identify == SitcomDefine.OPER_COLON2) // :
            {
                if (!mWaitContent)
                {
                    mWaitContent = true;
                    return this;
                }
                if (mKey == null)
                    throw new SitcomCompileException(oper.keyword);
                mKey.useKey = true;
                mUseKV = true;
                return this;
            }
            else if(!mWaitContent)
            {
                if (oper.Identify != SitcomDefine.OPER_REFERENCE)
                    throw new SitcomCompileException(oper.keyword);
                mRight = oper.keyword;
                return this;
            }
            else if(mKey == null)
            {
                mKey = new KeyValue();
                mKey.key = oper;
                oper.Parent = this;
                return oper;
            }
            else if(mKey.useKey)
            {
                replaced = mKey.value;
                mKey.value = oper;
                oper.Parent = this;
                mKVs.Add(mKey);
                mKey = null;
                return oper;
            }
            else
            {
                mKVs.Add(mKey);
                mKey = new KeyValue();
                mKey.key = oper;
                oper.Parent = this;
                return oper;

            }
            throw new SitcomCompileException(oper.keyword);
        }

        public override ISitcomOper Clone(SitcomFile.Keyword keyword)
        {
            var oper = new SitcomDomainOper();
            oper.keyword = keyword;
            return oper;
        }

        public override void OnExecute(SitcomContext runtime)
        {
            if(mLeft != null)
                runtime.Push(mLeft);
            if(mUseKV)
            {
                for(int i= 0; i < mKVs.Count; i++)
                {
                    runtime.Push(mKVs[i].useKey ? mKVs[i].value : mKVs[i].key);
                }
            }
            else
            {
                for(int i= 0; i < mKVs.Count; i++)
                {
                    runtime.Push(mKVs[i].key);
                }
            }
        }

        public override void OnStop(SitcomContext runtime)
        {
            object target;
            if (mLeft == null)
                target = runtime.Heap;
            else if (mLeft.Result != null)
                target = mLeft.Result.Result;
            else
                target = null;
            if (target == null)
                throw new SitcomNullReferenceExpception(keyword);
            var meta = runtime.Heap.GetMeta(target.GetType());
            if (!mWaitContent)
            {
                mResult = new SitcomValue(meta.GetProperty(mRight.id, target));
                return;
            }
            var domain = meta.GetDomain(mRight.id);
            if (domain == null)
                throw new SitcomNullReferenceExpception(keyword);
            object[] args;
            if (domain.ArgLength > 0)
                args = new object[domain.ArgLength];
            else
                args = new object[mKVs.Count];
            if (mUseKV)
            {
                for (int i = 0; i < mKVs.Count; i++)
                {
                    var index = mKVs[i].useKey ? domain.ArgIndex(mKVs[i].key.keyword.id) : i;
                    if (index >= 0 && index < args.Length)
                        args[index] = mKVs[i].GetValue();
                }
            }
            else
            {
                var len = Mathf.Min(args.Length, mKVs.Count);
                for (int i = 0; i < len; i++)
                {
                    var ret = mKVs[i].key.Result;
                    args[i] = ret.Result;
                }
            }
            mResult = domain.Call(runtime, target, mContent.text, args);
        }

        public override void Compile()
        {
            if (mLeft != null)
                mLeft.Compile();
            if (mKey != null)
            {
                mKVs.Add(mKey);
                mKey = null;
            }
            for(int i= 0; i < mKVs.Count; i++)
            {
                mKVs[i].key.Compile();
                if (mKVs[i].value != null)
                    mKVs[i].value.Compile();
            }
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            var buf = StringUtil.GetBuilder();
            if (mLeft != null)
                buf.Append("\"").Append(mLeft).Append(".");
            buf.Append("\"").Append(mRight.text).Append("\"");
            if (mWaitContent)
                buf.Append(": ");
            for (int i = 0; i < mKVs.Count; i++)
            {
                var kv = mKVs[i];
                buf.Append(kv.key);
                if (kv.useKey)
                    buf.Append(":").Append(kv.value);
                buf.Append(" ");
            }
            if (mWaitContent && mContent.sitcom != null)
                buf.Append("\n").Append(mContent.text);
            return StringUtil.ReleaseBuilder(buf);
        }
#endif
    }
}