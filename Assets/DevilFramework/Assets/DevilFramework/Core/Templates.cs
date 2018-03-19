using System.Xml;

namespace Devil
{
    public delegate bool FilterDelegate<T>(T target);

    public delegate T PassDelegate<T>(T target);

    public delegate int ComparableDelegate<T>(T a, T b);

    public delegate int SerializerDelegate<T>(T target);

    public delegate float FactorDelegate<T>(T target);

    public delegate void TickDelegate(float deltaTime);

    public delegate T ValueDelegate<T>();

    public delegate V GetterDelegate<T, V>(T target);

    public delegate void SetterDelegate<T, V>(T target, V value);

    public delegate void EditDelegate<T>(T target);

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
    
    public interface IXmlSerializable
    {
        XmlElement Serialize(XmlDocument doc);

        bool Deserialize(XmlElement element);
    }
}