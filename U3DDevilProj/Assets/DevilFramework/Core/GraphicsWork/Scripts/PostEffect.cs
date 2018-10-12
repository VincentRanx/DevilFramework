using UnityEngine;

namespace Devil.Effects
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class PostEffect : MonoBehaviour
    {
        public Material m_Material;
        //[MaskField(IsMask = true)]
        public DepthTextureMode m_DepthMode;

        private void Start()
        {
            GetComponent<Camera>().depthTextureMode = m_DepthMode;
        }

        private void OnValidate()
        {
            GetComponent<Camera>().depthTextureMode = m_DepthMode;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_Material)
            {
                Graphics.Blit(source, destination, m_Material);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
    
}