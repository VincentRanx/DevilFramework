using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay
{
    [System.AttributeUsage(System.AttributeTargets.Method ,AllowMultiple = true,Inherited = true)]
    public class SitcomDomainAttribute : System.Attribute
    {
        public string Name { get; set; }
        public string[] Args { get; set; }
        public bool IsDefault { get; set; }
        public string Doc { get; set; }

        public SitcomDomainAttribute(string name, params string[] args)
        {
            this.Name = name;
            this.Args = args;
        }

        public SitcomDomainAttribute(string name)
        {
            this.Name = name;
        }
    }

    public enum ESitcomState
    {
        Doing,
        Success,
        Failed,
    }

    // 命令执行结果
    public interface ISitcomResult
    {
        object Result { get; }
        ESitcomState State { get; }
    }

    public interface ISitcomTask : ISitcomResult
    {
        void Update(float deltaTime);
    }
    
    public struct SitcomValue : ISitcomResult
    {
        public object data;
        public object Result { get { return data; } }
        public ESitcomState State
        {
            get
            {
                return SitcomUtil.AsBool(data) ? ESitcomState.Success : ESitcomState.Failed;
            }
        }
        public SitcomValue(object v)
        {
            data = v;
        }

        public override string ToString()
        {
            return data == null ? "" : data.ToString();
        }

        public static implicit operator SitcomValue (bool b)
        {
            return new SitcomValue(b);
        }
    }
    
    public struct SitcomDelay: ISitcomResult
    {
        public float time;
        public object Result { get { return null; } }
        public ESitcomState State
        {
            get { return Time.time >= time ? ESitcomState.Success : ESitcomState.Doing; }
        }
        public SitcomDelay(float delayTime)
        {
            time = Time.time + delayTime;
        }
        public static explicit operator SitcomDelay(float time)
        {
            return new SitcomDelay(time);
        }
    }

    public class SitcomResult : ISitcomResult
    {
        public object Result { get; set; }

        public ESitcomState State { get; set; }

        public SitcomResult()
        {
            State = ESitcomState.Doing;
        }
    }

    public class SitcomAsyncOperator : ISitcomResult
    {
        public object Result { get; set; }

        public ESitcomState State { get; set; }
    }
    
    public interface ISitcomDomain : IIdentified
    {
        int ArgLength { get; }
        int ArgIndex(int id);
        ISitcomResult Call(SitcomContext runtime, object target, string content, object[] args);
    }

    public interface ISitcomMeta : IIdentified
    {
        string Name { get; }
        bool AsBool(object target);
        float AsNumber(object target);
        System.Type MetaType { get; }
        ISitcomDomain GetDomain(int domainId);
        object GetProperty(int domain, object target);
    }

    public delegate ISitcomResult SitcomCall<T>(SitcomContext context, T target, string content, object[] args);

    public class SitcomMeta<T> : ISitcomMeta
    {
        public class Domain : ISitcomDomain
        {
            public int Identify { get; private set; }
            public string Name { get; private set; }
            int[] mArgs;
            public int ArgLength { get; private set; }
            public int ArgIndex(int id) { return GlobalUtil.FindIndex(mArgs, (x) => x == id); }
            SitcomCall<T> callback;
            public Domain(string name, string[] args, SitcomCall<T> call)
            {
                Identify = StringUtil.IgnoreCaseToHash(name);
                Name = name;
                ArgLength = args == null ? 0 : args.Length;
                mArgs = new int[ArgLength];
                for (int i = 0; i < mArgs.Length; i++)
                {
                    mArgs[i] = StringUtil.IgnoreCaseToHash(args[i]);
                }
                callback = call;
            }
            public ISitcomResult Call(SitcomContext context, object target, string content, object[] args)
            {
                return callback(context, (T)target, content, args);
            }

            public override string ToString()
            {
                return string.Format("SitcomDomain[{0}.{1}(argc:{2})]", typeof(T).Name, Name, ArgLength);
            }
        }
        
        public int Identify { get; private set; }
        public string Name { get; private set; }
        public System.Type MetaType { get { return typeof(T); } }
        public bool RequireContent { get; private set; }
        protected AvlTree<Domain> mDomains;
        Domain mDefaultDomain;

        public SitcomMeta()
        {
            Name = typeof(T).Name;
            Identify = StringUtil.IgnoreCaseToHash(Name);
            mDomains = new AvlTree<Domain>((x) => x.Identify);
            AddDomain("hash", null, Sit_HashCode, false);
            AddDomain("tostring", null, Sit_ToString, true);
            FindDomains();
        }

        protected SitcomMeta(AvlTree<Domain> domains)
        {
            Name = typeof(T).Name;
            Identify = StringUtil.IgnoreCaseToHash(Name);
            mDomains = domains;
            //AddDomain("hash", null, Sit_HashCode, false);
            //AddDomain("tostring", null, Sit_ToString, true);
            //FindDomains();
        }

        protected virtual void FindDomains()
        {
            var type = GetType();
            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                var mtd = methods[i];
                if (mtd.ReturnType != typeof(ISitcomResult))
                    continue;
                var args = mtd.GetParameters();
                if (args == null || args.Length != 4)
                    continue;
                if (args[0].ParameterType != typeof(SitcomContext) || args[1].ParameterType != typeof(T)
                    || args[2].ParameterType != typeof(string) || args[3].ParameterType != typeof(object[]))
                {
                    RTLog.LogErrorFormat(LogCat.Game, " [SitcomDomain]\"{0}/{1}\" don't match args", type.Name, mtd.Name);
                    continue;
                }
                var attr = mtd.GetCustomAttributes(typeof(SitcomDomainAttribute), true);
                if (attr == null || attr.Length == 0)
                    continue;
                var call = (SitcomCall<T>)System.Delegate.CreateDelegate(typeof(SitcomCall<T>), this, mtd);
                for (int j = 0; j < attr.Length; j++)
                {
                    var domain = attr[j] as SitcomDomainAttribute;
                    if (domain == null)
                        continue;
                    AddDomain(domain.Name, domain.Args, call, domain.IsDefault);
                }
            }
        }

        public virtual void Dispose(T value) { }
        
        public void AddDomain(string name, string[] args, SitcomCall<T> call, bool isDefault)
        {
            var domain = new Domain(name, args, call);
            mDomains.Add(domain);
            if (isDefault)
                mDefaultDomain = domain;
        }

        public virtual bool AsBool(object target)
        {
            return SitcomUtil.ParseAsBool(target);
        }

        public virtual float AsNumber(object target)
        {
            return SitcomUtil.ParseAsNumber(target);
        }

        public virtual ISitcomDomain GetDomain(int domain)
        {
            return domain == 0 ? mDefaultDomain : mDomains.GetData(domain);
        }

        public virtual object GetProperty(int field, object target)
        {
            return null;
        }

        #region domains
        public virtual ISitcomResult Sit_HashCode(SitcomContext context, T target, string content, object[] args)
        {
            return new SitcomValue(target.GetHashCode());
        }
        public virtual ISitcomResult Sit_ToString(SitcomContext context, T target, string content, object[] args)
        {
            return new SitcomValue(target.ToString());
        }
        #endregion
    }
}