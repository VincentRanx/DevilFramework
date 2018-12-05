using System;
using UnityEngine;

namespace Devil.Effects
{
    public abstract class CameraPostProcessingAsset : ScriptableObject
    {
        [SerializeField]
        int m_IterationCount = 1;
        public int IterationCount { get { return m_IterationCount; } set { m_IterationCount = value; } }

        [SerializeField]
        Material m_Material;
        public Material material
        {
            get { return m_Material; }
            set { m_Material = value; }
        }

        public abstract PostProcessingRenderer Create();
    }

    public abstract class PostProcessingRenderer : IDisposable
    {
        public CameraPostProcessingAsset Asset { get; private set; }
        
        public virtual void Prepare() { }

        public virtual void Dispose() { }

        // 迭代次数
        public int IterationCount { get { return Asset.IterationCount; } }

        public abstract void RenderOnce(int renderIndex, RenderTexture source, RenderTexture destination);

        public static T Create<T>(CameraPostProcessingAsset asset) where T : PostProcessingRenderer, new()
        {
            var rend = new T();
            rend.Asset = asset;
            return rend;
        }
    }
}