namespace DevilTeam
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

    public enum ExecuteMode
    {
        asynchronous,
        synchronized,
    }

    public enum CoordType2D
    {
        local,
        uv,
    }

    public enum CurveType
    {
        Spline,
        Bezier,
    }

    public interface ITick
    {
        void OnTick(float deltaTime);
    }
    
}