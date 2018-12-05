using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Devil.Effects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraPostProcessing : MonoBehaviour
    {
        [SerializeField]
        CameraPostProcessingAsset[] m_PostEffects = new CameraPostProcessingAsset[0];

        List<PostProcessingRenderer> mPostProcessings;
        readonly RenderTexture[] tmpTex = new RenderTexture[3];

        private void Awake()
        {
            mPostProcessings = new List<PostProcessingRenderer>(m_PostEffects.Length);
            for(int i= 0; i < m_PostEffects.Length; i++)
            {
                var eff = m_PostEffects[i] == null ? null : m_PostEffects[i].Create();
                if (eff != null)
                    mPostProcessings.Add(eff);
            }
        }

        private void OnEnable()
        {
            PreparePostEffects();
        }

        private void OnDisable()
        {
            ReleasePostEfffects();
        }

        void PreparePostEffects()
        {
            for (int i = 0; i < mPostProcessings.Count; i++)
            {
                var eff = mPostProcessings[i];
                if (eff != null)
                {
                    eff.Prepare();
                }
            }
        }

        void ReleasePostEfffects()
        {
            for (int i = 0; i < mPostProcessings.Count; i++)
            {
                var eff = mPostProcessings[i];
                if (eff != null)
                {
                    eff.Dispose();
                }
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            tmpTex[2] = source;
            tmpTex[0] = destination;
            int src = 2;
            int dest = 0;
            for(int i= 0; i < mPostProcessings.Count; i++)
            {
                var eff = mPostProcessings[i];
                for(int j = 0; j < eff.IterationCount; j++)
                {
                    if ((src == 1 || dest == 1) && tmpTex[1] == null)
                    {
                        tmpTex[1] = RenderTexture.GetTemporary(source.width, source.height);
                    }
                    eff.RenderOnce(j, tmpTex[src], tmpTex[dest]);
                    dest = (dest + 1) & 0x1;
                    src = (dest + 1) & 0x1;
                }
            }
            if(dest == 0)
            {
                Graphics.Blit(tmpTex[src], tmpTex[dest]);
            }
            if(tmpTex[1] != null)
            {
                RenderTexture.ReleaseTemporary(tmpTex[1]);
                tmpTex[1] = null;
            }
            tmpTex[0] = null;
            tmpTex[1] = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mPostProcessings != null)
            {
                ReleasePostEfffects();
                mPostProcessings.Clear();
            }
            else
            {
                mPostProcessings = new List<PostProcessingRenderer>(m_PostEffects.Length);
            }
            for (int i = 0; i < m_PostEffects.Length; i++)
            {
                var eff = m_PostEffects[i] == null ? null : m_PostEffects[i].Create();
                if (eff != null)
                    mPostProcessings.Add(eff);
            }
            if (isActiveAndEnabled)
                PreparePostEffects();
        }
#endif
    }
}