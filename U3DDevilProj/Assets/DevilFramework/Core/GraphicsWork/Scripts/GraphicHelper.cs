using UnityEngine;

namespace Devil.Effects
{
    public static class GraphicHelper
    {
        static bool install;
        static int prop_blur_iters;
        static int prop_blur_step;
        static int prop_color;
        //static int prop_clip;
        static Material blitMat;

        static readonly int blur_pass = 0;
        static readonly int clear_pass = 1;
        
        static void Install()
        {
            if(!install || blitMat == null)
            {
                install = true;
                prop_blur_iters = Shader.PropertyToID("_BlurIters");
                prop_blur_step = Shader.PropertyToID("_BlurStep");
                prop_color = Shader.PropertyToID("_Color");
                //prop_clip = Shader.PropertyToID("_ClipRect");
                blitMat = new Material(Resources.Load<Shader>("Shaders/BlitImg"));
            }
        }

        public static void GrabCamera(Camera cam, RenderTexture tex)
        {
            var old = cam.targetTexture;
            cam.targetTexture = tex;
            cam.Render();
            cam.targetTexture = old;
        }

        /// <summary>
        /// 模糊渲染
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="destination"></param>
        /// <param name="amount">强度</param>
        /// <param name="iters">迭代次数</param>
        public static void Blur(Texture source, RenderTexture destination, float amount, int iters)
        {
            if (iters < 1 || amount <= 0)
            {
                if (source != destination)
                    Graphics.Blit(source, destination);
                return;
            }
            Install();
            int width = source.width;
            int height = source.height;
            RenderTexture rt0 = RenderTexture.GetTemporary(width, height);
            RenderTexture rt1 = RenderTexture.GetTemporary(width, height);
            blitMat.SetFloat(prop_blur_step, amount);
            blitMat.SetFloat(prop_blur_iters, 1);
            Graphics.Blit(source, rt1, blitMat, blur_pass);
            for (int i = 1; i < iters - 1;)
            {
                blitMat.SetFloat(prop_blur_iters, 0.5f + 0.5f * i++);
                Graphics.Blit(rt1, rt0, blitMat, blur_pass);
                blitMat.SetFloat(prop_blur_iters, 0.5f + 0.5f * i++);
                Graphics.Blit(rt0, rt1, blitMat, blur_pass);
            }
            Graphics.Blit(rt1, destination, blitMat, blur_pass);
            RenderTexture.ReleaseTemporary(rt0);
            RenderTexture.ReleaseTemporary(rt1);
        }

        public static void Clear(RenderTexture destination, Color color)
        {
            Install();
            blitMat.SetColor(prop_color, color);
            Graphics.Blit(null, destination, blitMat, clear_pass);
        }

    }
}