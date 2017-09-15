using UnityEngine;

namespace DevilTeam.Effects
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class PostEffect : MonoBehaviour
    {
        public Material m_Material;

        private void OnPreCull()
        {
            Debug.Log("Precull");
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