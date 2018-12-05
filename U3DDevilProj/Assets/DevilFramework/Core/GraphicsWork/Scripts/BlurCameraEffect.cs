using UnityEngine;

namespace Devil.Effects
{
    [CreateAssetMenu(fileName = "Blur Camera", menuName = "Game/Effects/Blur Camera")]
    public class BlurCameraEffect : CameraPostProcessingAsset
    {
        public float m_BlurAmount = 1;
        public int m_BlurIters = 3;

        public override PostProcessingRenderer Create()
        {
            return PostProcessingRenderer.Create<BlurCameraRender>(this);
        }
    }

    public class BlurCameraRender : PostProcessingRenderer
    {
        BlurCameraEffect blurAsset;

        public override void Prepare()
        {
            blurAsset = Asset as BlurCameraEffect;
        }

        public override void Dispose()
        {
            blurAsset = null;
        }

        public override void RenderOnce(int renderIndex, RenderTexture source, RenderTexture destination)
        {
            GraphicHelper.Blur(source, destination, blurAsset.m_BlurAmount, blurAsset.m_BlurIters);
        }
    }
}
