using UnityEngine;

namespace DevilTeam.Utility
{
    public static class GlobalUtil
    {
        /// <summary>
        /// 二分查找索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="serializer"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int BinsearchIndex<T>(T[] array, SerializerDelegate<T> serializer, int start, int end)
        {
            int l = start;
            int r = end - 1;
            int c;
            int cs;
            while (l <= r)
            {
                c = (l + r) >> 1;
                T ct = array[c];
                cs = serializer(ct);
                if (cs == 0)
                {
                    return c;
                }
                else if (cs > 0)
                {
                    r = c - 1;
                }
                else
                {
                    l = c + 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// 二分查找对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="serializer"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static T Binsearch<T>(T[] array, SerializerDelegate<T> serializer, int start, int end)
        {
            int index = BinsearchIndex(array, serializer, start, end);
            return index == -1 ? default(T) : array[index];
        }

        /// <summary>
        /// 二分查找左边最接近目标的索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="serializer"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int BinsearchLessEqualIndex<T>(T[] array, SerializerDelegate<T> serializer, int start, int end)
        {
            int l = start;
            int r = end - 1;
            int c = l;
            int cs;
            int ret = -1;
            while (l <= r)
            {
                c = (l + r) >> 1;
                T ct = array[c];
                cs = serializer(ct);
                if (cs == 0)
                {
                    return c;
                }
                else if (cs > 0)
                {
                    r = c - 1;
                    ret = c;
                }
                else
                {
                    l = c + 1;
                }
            }
            return ret;
        }

        /// <summary>
        /// 二分查找右边最接近目标的索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="serializer"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int BinsearchGreaterEqualIndex<T>(T[] array, SerializerDelegate<T> serializer, int start, int end)
        {
            int l = start;
            int r = end - 1;
            int c = l;
            int cs;
            int ret = -1;
            while (l <= r)
            {
                c = (l + r) >> 1;
                T ct = array[c];
                cs = serializer(ct);
                if (cs == 0)
                {
                    return c;
                }
                else if (cs > 0)
                {
                    r = c - 1;
                }
                else
                {
                    l = c + 1;
                    ret = c;
                }
            }
            return ret;
        }

        /// <summary>
        /// 计算以normal为发现确定的平面上，从from旋转到to的旋转方向
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static float RotateDirection(Vector3 from, Vector3 to, Vector3 normal)
        {
            Vector3 projFrom = Vector3.ProjectOnPlane(from, normal);
            Vector3 projTo = Vector3.ProjectOnPlane(to, normal);
            Vector3 cross = Vector3.Cross(projFrom, projTo);
            float dir = Vector3.Dot(cross, normal);
            if (dir < 0)
                dir = -1;
            else if (dir > 0)
                dir = 1;
            return dir;
        }

        /// <summary>
        /// 计算以normal为发现确定的平面上，从from旋转到to的旋转角度
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static float RotateAngleFromTo(Vector3 from, Vector3 to, Vector3 normal)
        {
            Vector3 projFrom = Vector3.ProjectOnPlane(from, normal);
            Vector3 projTo = Vector3.ProjectOnPlane(to, normal);
            Vector3 cross = Vector3.Cross(projFrom, projTo);
            float dir = Vector3.Dot(cross, normal);
            if (dir < 0)
                dir = -1;
            else
                dir = 1;
            return dir * Vector3.Angle(projFrom, projTo);
        }

        public static Vector3 AddMagnitude(Vector3 v, float magnitude)
        {
            float m = v.magnitude;
            float ma = m + magnitude;
            if (ma <= 0 || m == 0)
                return Vector3.zero;
            else
                return v * ma / m;
        }

        public static Vector3 CloseTo(Vector3 from, Vector3 to, float strength, float devition)
        {
            Vector3 v = Vector3.Lerp(from, to, strength);
            if (Vector3.Distance(v, to) <= devition)
            {
                return to;
            }
            else
            {
                return v;
            }
        }

        public static Bounds ScreenBoundsInParent(Camera c, Transform parent)
        {
            if (!c)
                return default(Bounds);
            Vector3 min = c.ScreenToWorldPoint(Vector3.zero);
            Vector3 max = c.ScreenToWorldPoint(new Vector3(c.pixelWidth, c.pixelHeight));
            if (parent)
            {
                Matrix4x4 m = parent.worldToLocalMatrix;
                min = m.MultiplyPoint(min);
                max = m.MultiplyPoint(max);
            }
            Bounds b = new Bounds((min + max) * 0.5f, (max - min));
            return b;
        }

        public static bool IsCurveUseful(AnimationCurve curve)
        {
            return curve != null && curve.length > 1;
        }

        public static float MaxTimeOfCurve(AnimationCurve curve)
        {
            return curve.keys[curve.length - 1].time;
        }

        public static float EndValueOfCurve(AnimationCurve curve)
        {
            return curve.keys[curve.length - 1].value;
        }

        public static float StartValueOfCurve(AnimationCurve curve)
        {
            return curve.keys[0].value;
        }

        public static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}