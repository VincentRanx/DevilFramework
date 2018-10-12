using UnityEngine;

namespace Devil.Effects
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class BlurPostEffect : MonoBehaviour
    {
        [Range(0, 5)]
        [SerializeField]
        float m_BlurStep = 1;

        [Range(0, 10)]
        [SerializeField]
        int m_BlurIters = 3;

        [SerializeField]
        Material m_BlurMaterial;

        Material mBlurMat;
        Material blurMat
        {
            get
            {
                if (m_BlurMaterial != null)
                    return m_BlurMaterial;
                if (mBlurMat == null)
                    mBlurMat = new Material(Shader.Find("DevilTeam/ImgBlur"));
                return mBlurMat;
            }
        }

        int prop_blur_step;

        private void OnEnable()
        {
            int prop_blur_step = Shader.PropertyToID("_BlurStep");
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var mat = blurMat;
            mat.SetFloat(prop_blur_step, m_BlurStep);
            //Graphics.Blit(source, destination);
            GraphicHelper.Blur(source, destination, mat, m_BlurIters);
        }

        private void OnDestroy()
        {
            if (mBlurMat != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(mBlurMat);
                else
#endif
                    Destroy(mBlurMat);
                mBlurMat = null;
            }
        }
    }
}