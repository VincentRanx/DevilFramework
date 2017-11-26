using UnityEngine;
namespace DevilTeam.Utility
{
    //copy from iTween (平滑曲线)
    public class TCRSpline
    {
        /// <summary>
        /// 根据采样点获取一条平滑曲线
        /// </summary>
        /// <param name="samplePoints">最小长度为2点</param>
        /// <returns></returns>
        public static TCRSpline GetCRSpline(Vector3[] samplePoints)
        {
            if (samplePoints == null || samplePoints.Length < 2)
                return null;
            Vector3[] vector3s;
            if (samplePoints.Length == 2)
            {
                vector3s = new Vector3[2];
                System.Array.Copy(samplePoints, vector3s, 2);
                return new TCRSpline(samplePoints);
            }
            Vector3[] suppliedPath = samplePoints;
            vector3s = new Vector3[suppliedPath.Length + 2];
            System.Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);
            vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
            vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);
            if (vector3s[1] == vector3s[vector3s.Length - 2])
            {
                Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
                System.Array.Copy(vector3s, tmpLoopSpline, vector3s.Length);
                tmpLoopSpline[0] = tmpLoopSpline[tmpLoopSpline.Length - 3];
                tmpLoopSpline[tmpLoopSpline.Length - 1] = tmpLoopSpline[2];
                vector3s = new Vector3[tmpLoopSpline.Length];
                System.Array.Copy(tmpLoopSpline, vector3s, tmpLoopSpline.Length);
            }
            return new TCRSpline(vector3s);
        }

        Vector3[] pts;

        private TCRSpline(params Vector3[] pts)
        {
            this.pts = pts;
        }

        //获取插值点
        public Vector3 Lerp(float t)
        {
            if (pts.Length == 2)
                return Vector3.Lerp(pts[0], pts[1], t);
            int numSections = pts.Length - 3;
            int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currPt;
            Vector3 a = pts[currPt];
            Vector3 b = pts[currPt + 1];
            Vector3 c = pts[currPt + 2];
            Vector3 d = pts[currPt + 3];
            return .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
        }

        public void DrawGizmos(int lerpPoints, Color normalColor, ref float normalSpeed)
        {
            float d = 1f / (float)lerpPoints;
            float t = 0f;
            Vector3 p0 = Lerp(0f);
            Vector3 p1;
            float end = 1f;
            float maxDis = 0;
            while (t < end)
            {
                t = t + d;
                float f = Mathf.Clamp01(t);
                p1 = Lerp(f);
                float dis = Vector3.Distance(p0, p1);
                Gizmos.color = normalColor * (dis / normalSpeed);
                Gizmos.DrawLine(p0, p1);
                p0 = p1;
                if (dis > maxDis)
                    maxDis = dis;
            }
            normalSpeed = maxDis;
        }
    }
}