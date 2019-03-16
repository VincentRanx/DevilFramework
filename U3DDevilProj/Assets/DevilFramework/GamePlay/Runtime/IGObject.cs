using Devil.AI;
using UnityEngine;

namespace Devil.GamePlay
{

    /// <summary>
    /// 游戏世界中的基本物体
    /// </summary>
    public interface IGObject : IIdentified
    {
        string tag { get; }
        string Name { get; }
        bool isDestroied { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
        T GetComponent<T>();
    }

}