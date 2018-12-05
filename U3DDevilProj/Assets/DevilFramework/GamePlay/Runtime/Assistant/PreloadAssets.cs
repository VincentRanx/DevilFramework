using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    [CreateAssetMenu(fileName ="PreloadAssets" , menuName = "Game/预加载资源")]
    public class PreloadAssets : ScriptableObject
    {
        public int m_WarmUpNum;
        public int m_CacheSize = 128;
        public List<GameObject> m_Assets = new List<GameObject>();
    }
}