using UnityEngine;

namespace Devil.Effects
{
    public static class GraphicHelper
    {
        static int prop_blur_iters = Shader.PropertyToID("_BlurIters");
        static int prop_color = Shader.PropertyToID("_Color");

        //static Material mBlitMat;
        //static Material GetMaterial()
        //{
        //    if (mBlitMat == null)
        //    {
        //        var shad = Shader.Find("DevilTeam/BlitImg");
        //        mBlitMat = new Material(shad);
        //        prop_blur_iters = Shader.PropertyToID("_BlurIters");
        //        prop_color = Shader.PropertyToID("_Color");
        //    }
        //    return mBlitMat;
        //}

        public static void GrabScreen(Camera cam, RenderTexture tex)
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
        /// <param name="mat">材质</param>
        /// <param name="iters">迭代次数</param>
        public static void Blur(Texture source, RenderTexture destination, Material mat, int iters, int pass = 0)
        {
            if (mat == null || iters < 1)
            {
                if (source != destination)
                    Graphics.Blit(source, destination);
                return;
            }
            int width = source.width;
            int height = source.height;
            RenderTexture rt0 = RenderTexture.GetTemporary(width, height);
            RenderTexture rt1 = RenderTexture.GetTemporary(width, height);
            mat.SetFloat(prop_blur_iters, 1);
            Graphics.Blit(source, rt1, mat, pass);
            for (int i = 1; i < iters;)
            {
                mat.SetFloat(prop_blur_iters, i++);
                Graphics.Blit(rt1, rt0, mat, pass);
                mat.SetFloat(prop_blur_iters, i++);
                Graphics.Blit(rt0, rt1, mat, pass);
            }
            Graphics.Blit(rt1, destination, mat, pass);
            RenderTexture.ReleaseTemporary(rt0);
            RenderTexture.ReleaseTemporary(rt1);
        }

        public static void Clear(RenderTexture destination, Color color, Material mat, int pass = 1)
        {
            if(mat != null)
            {
                mat.SetColor(prop_color, color);
                Graphics.Blit(null, destination, mat, pass);
            }
        }

    }
}