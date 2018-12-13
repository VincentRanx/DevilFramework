using System.Xml;

namespace Devil
{
    public delegate bool FilterDelegate<T>(T target);

    public delegate T PassDelegate<T>(T target);

    public delegate void TickDelegate(float deltaTime);

    public delegate T ValueDelegate<T>();

    public delegate V GetterDelegate<T, V>(T target);

    public delegate void SetterDelegate<T, V>(T target, V value);
    
    public delegate int IdentifierDelegate<T>(T target);

    public enum EExecuteMode
    {
        asynchronous,
        synchronized,
    }

    public enum ECoordType2D
    {
        local,
        uv,
    }

    public enum ECurveType
    {
        Spline,
        Bezier,
    }

    public enum ETimeType
    {
        real_time,
        scaled_time,
    }

    public interface ITick
    {
        void OnTick(float deltaTime);
    }

    public interface INamed
    {
        string Name { get; }
    }
    
    public interface IIdentified
    {
        int Identify { get; }
    }

    // 比较类型
    public enum EComparision
    {
        Equal = 0,
        NotEqual = 1,
        Greater = 2,
        Less = 3,
        GEqual = 4,
        LEqual = 5,
    }

    // 条件接口
    public interface ICondition
    {
        bool IsSuccess { get; }
    }
}