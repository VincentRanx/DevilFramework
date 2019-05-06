using Devil.GamePlay.Assistant;
using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public struct SitcomPtr : System.IEquatable<SitcomPtr>
    {
        public int id;
        public int stack;

        public static SitcomPtr NULL
        {
            get { return default(SitcomPtr); }
        }

        public int Identify { get { return id; } }

        public static bool operator ==(SitcomPtr a, SitcomPtr b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SitcomPtr a, SitcomPtr b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object other)
        {
            if (other is SitcomPtr)
                return Equals((SitcomPtr)other);
            else
                return false;
        }

        public bool Equals(SitcomPtr other)
        {
            return this.id == other.id && this.stack == other.stack;
        }
    }

    public struct SitcomPtrCall : ISitcomDomain
    {
        public int id;
        public ISitcomDomain domain;

        public int ArgLength { get { return domain.ArgLength; } }

        public int Identify { get { return id; } }

        public int ArgIndex(int id)
        {
            return domain.ArgIndex(id);
        }

        public ISitcomResult Call(SitcomContext runtime, object target, string content, object[] args)
        {
            var t = ((SitcomHeap)target).GetValue(id);
            return domain.Call(runtime, t, content, args);
        }
    }

    public class SitcomHeap : SitcomMeta<SitcomHeap>
    {
        static SitcomHeap sGlobalHeap;
        public static SitcomHeap Global
        {
            get
            {
                if (sGlobalHeap == null)
                {
                    sGlobalHeap = new SitcomHeap(4);
                    sGlobalHeap.Alloc("global", sGlobalHeap);
                }
                return sGlobalHeap;
            }
        }

        SitcomHeap mBaseHeap;
        Dictionary<System.Type, ISitcomMeta> mMetas;

        Dictionary<int, object>[] mStacks;
        int mStackPtr;

        private SitcomHeap(int stackSize) : base()
        {
            mMetas = new Dictionary<System.Type, ISitcomMeta>();
            mMetas[typeof(object)] = new SitcomMeta<object>();
            mStacks = new Dictionary<int, object>[stackSize];
            for (int i = 0; i < mStacks.Length; i++)
            {
                mStacks[i] = new Dictionary<int, object>();
            }
            Alloc("this", this);
        }

        public SitcomHeap(SitcomHeap baseheap = null, int stackSize = 16) : base(Global.mDomains)
        {
            if (baseheap == null)
                baseheap = Global;
            mBaseHeap = baseheap;
            mMetas = baseheap.mMetas;
            mStacks = new Dictionary<int, object>[stackSize];
            for (int i = 0; i < mStacks.Length; i++)
            {
                mStacks[i] = new Dictionary<int, object>();
            }
            Alloc("this", this);
            if (sGlobalHeap != null)
                Alloc("global", sGlobalHeap);
        }
        
        public void BeginStack()
        {
            if (mStackPtr >= mStacks.Length - 1)
                throw new System.Exception("Sitcom stack overflow.");
            mStackPtr++;
        }

        public void EndStack()
        {
            if (mStackPtr < 1)
                throw new System.Exception("Sitcom stack overflow.");
            mStacks[mStackPtr--].Clear();
        }

        public void ResetStack()
        {
            for (int i = mStackPtr; i > 0; i--)
            {
                mStacks[i].Clear();
            }
            mStackPtr = 0;
        }

        public void AddMeta(ISitcomMeta meta)
        {
            mMetas[meta.MetaType] = meta;
        }

        public void AddMeta(System.Type type, ISitcomMeta meta)
        {
            mMetas[type] = meta;
        }
        
        public ISitcomMeta GetMeta(System.Type type)
        {
            if (type == GetType())
                return this;
            ISitcomMeta meta;
            if (mMetas.TryGetValue(type, out meta))
                return meta;
            var p = type.BaseType;
            if (p != null)
                return GetMeta(p);
            return this;
        }

        public SitcomPtr Alloc(string name, object value)
        {
            return Alloc(StringUtil.IgnoreCaseToHash(name), value);
        }

        public SitcomPtr Alloc(int id, object value)
        {
            mStacks[mStackPtr][id] = value;
            SitcomPtr ptr;
            ptr.id = id;
            ptr.stack = mStackPtr;
            return ptr;
        }

        public void Delloc(string name)
        {
            mStacks[mStackPtr].Remove(StringUtil.IgnoreCaseToHash(name));
        }

        public void Delloc(int id)
        {
            mStacks[mStackPtr].Remove(id);
        }

        public bool IsAlloced(SitcomPtr ptr)
        {
            return ptr.stack <= mStackPtr && ptr.stack >= 0 ? mStacks[ptr.stack].ContainsKey(ptr.id) : false;
        }

        public object GetValue(SitcomPtr ptr)
        {
            object v;
            if (ptr.stack <= mStackPtr && ptr.stack >= 0 && mStacks[mStackPtr].TryGetValue(ptr.id, out v))
                return v;
            else
                return null;
        }

        public SitcomPtr GetPtr(int id)
        {
            object v;
            for (int i = mStackPtr; i >= 0; i--)
            {
                if (mStacks[i].TryGetValue(id, out v))
                {
                    SitcomPtr ptr;
                    ptr.id = id;
                    ptr.stack = i;
                    return ptr;
                }
            }
            return mBaseHeap == null ? default(SitcomPtr) : mBaseHeap.GetPtr(id);
        }

        public object GetValue(int id)
        {
            object v;
            for (int i = mStackPtr; i >= 0; i--)
            {
                if (mStacks[i].TryGetValue(id, out v))
                    return v;
            }
            return mBaseHeap == null ? null : mBaseHeap.GetValue(id);
        }

        public object GetValue(string name)
        {
            return GetValue(StringUtil.IgnoreCaseToHash(name));
        }

        public override object GetProperty(int field, object target)
        {
            return ((SitcomHeap)target).GetValue(field);
        }

        public override ISitcomDomain GetDomain(int domain)
        {
            var dom = base.GetDomain(domain);
            if(dom == null)
            {
                var v = GetValue(domain);
                if(v != null)
                {
                    SitcomPtrCall ptrdom;
                    var meta = GetMeta(v.GetType());
                    ptrdom.domain = meta.GetDomain(0);
                    ptrdom.id = domain;
                    dom = ptrdom;
                }
            }
            return dom;
        }

        public override string ToString()
        {
            return StringUtil.Concat(base.ToString(), " (", GetHashCode().ToString("x"), ")");
        }

        #region domains
        // time:
        [SitcomDomain("print", "data", "time")]
        public ISitcomResult Dom_Print(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            if (args[0] != null)
                Debug.Log(args[0]);
            if(!string.IsNullOrEmpty(content))
                Debug.Log(content);
            return new SitcomDelay(args == null || args.Length < 1 ? 1 : SitcomUtil.ParseAsNumber(args[1]));
        }
        // time
        [SitcomDomain("delay", "time")]
        public ISitcomResult Dom_Delay(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            return new SitcomDelay((float)SitcomUtil.ParseAsNumber(args[0]));
        }

        [SitcomDomain("end")]
        public ISitcomResult Dom_End(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            runtime.SetNextMark(SitcomCmdSequence.EndMarkId);
            return null;
        }

        [SitcomDomain("active", "tag")]
        public ISitcomResult Dom_Active(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            return Dom_SetActive(true, runtime, target, content, args);
        }

        [SitcomDomain("deactive", "tag")]
        public ISitcomResult Dom_Deactive(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            return Dom_SetActive(false, runtime, target, content, args);
        }

        ISitcomResult Dom_SetActive(bool active, SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            var names = new List<string>();
            if (!StringUtil.ParseArray(content, names, '\n') || names.Count == 0)
                return new SitcomValue(false);
            if (args != null && args.Length > 0 && args[0] != null)
            {
                var goes = GameObject.FindGameObjectsWithTag(args[0].ToString());
                for (int i = 0; i < goes.Length; i++)
                {
                    if (names.Contains(goes[i].name))
                        goes[i].SetActive(active);
                }
            }
            else
            {
                for (int i = 0; i < names.Count; i++)
                {
                    var go = GameObject.Find(names[i]);
                    if (go != null)
                        go.SetActive(active);
                }
            }
            return new SitcomValue(true);
        }

        [SitcomDomain("spawn", "prefab", "position", "rotation", "radius")]
        public ISitcomResult Dom_Spawn(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            var names = new List<string>();
            if (!StringUtil.ParseArray(content, names, '\n') || names.Count == 0)
                return new SitcomValue(false);
            var ret = new SitcomAsyncOperator();
            AssetsUtil.GetAssetAsync<GameObject>(args[0].ToString(), (x) =>
            {
                float f;
                if (args[3] != null)
                    f = SitcomUtil.AsNumber(args[3]);
                else
                    f = 0;
                Vector3 pos = args[1] == null ? Vector3.zero : StringUtil.ParseVector3(args[1].ToString());
                Vector3 euler = args[2] == null ? Vector3.zero : StringUtil.ParseVector3(args[2].ToString());
                for (int i = 0; i < names.Count; i++)
                {
                    var go = GameObject.Instantiate(x);
                    go.name = names[i];
                    var rad = Random.insideUnitCircle * f;
                    go.transform.position = pos + new Vector3(rad.x, 0, rad.y);
                    go.transform.localEulerAngles = euler;
                    ret.Result = go;
                }
                ret.State = ESitcomState.Success;
            }, (error) =>
            {
                ret.Result = error;
                ret.State = ESitcomState.Failed;
            });
            return ret;
        }

        [SitcomDomain("unspawn", "tag")]
        public ISitcomResult Dom_Unspawn(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            var names = new List<string>();
            if (!StringUtil.ParseArray(content, names, '\n') || names.Count == 0)
                return new SitcomValue(false);
            if (args != null && args.Length > 0 && args[0] != null)
            {
                var goes = GameObject.FindGameObjectsWithTag(args[0].ToString());
                for (int i = 0; i < goes.Length; i++)
                {
                    if (names.Contains(goes[i].name))
                        GameObject.Destroy(goes[i]);
                }
            }
            else
            {
                for (int i = 0; i < names.Count; i++)
                {
                    var go = GameObject.Find(names[i]);
                    if (go != null)
                        GameObject.Destroy(go);
                }
            }
            return new SitcomValue(true);
        }

        [SitcomDomain("alloc", "name")]
        public ISitcomResult Dom_Alloc(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            if (args != null && args.Length > 0)
                target.Alloc(args[0].ToString(), content);
            return new SitcomValue(content);
        }

        [SitcomDomain("result", "result")]
        public ISitcomResult Dom_Result(SitcomContext runtime, SitcomHeap target, string content, object[] args)
        {
            return new SitcomValue(args == null || args.Length < 1 ? content : args[0]);
        }

        #endregion
    }

}