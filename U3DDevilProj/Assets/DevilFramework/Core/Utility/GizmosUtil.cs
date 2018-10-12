using System;
using UnityEngine;

namespace Devil.Utility
{
    public class GizmosUtil
    {
        public static void DrawWiredCube(Vector3 center, Vector3 size, float bodyTransparency = 0.5f)
        {
            Color color = Gizmos.color;
            float a = color.a;
            color.a = a * bodyTransparency;
            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
            color.a = a;
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);
        }

        public static void DrawWireOvalSphere2D(Vector3 pos, Vector3 forward, Vector3 up, Vector2 size, bool showCross = false, int samples = 40)
        {
            Matrix4x4 def = Gizmos.matrix;
            Gizmos.matrix = def * Matrix4x4.TRS(pos, Quaternion.LookRotation(forward, up), size);
            if (showCross)
            {
                Gizmos.DrawLine(Vector3.left, Vector3.right);
                Gizmos.DrawLine(Vector3.down, Vector3.up);
            }
            int len = Math.Max(10, samples);
            float ang = 360f / len;
            Vector3 p0 = Vector3.right * 0.5f;
            Vector3 a, b;
            a = p0;
            for (int i = 0; i < len;)
            {
                Quaternion rot = Quaternion.AngleAxis(ang * ++i, Vector3.forward);
                b = rot * p0;
                Gizmos.DrawLine(a, b);
                a = b;
            }
            Gizmos.matrix = def;
        }

        public static void MarkTransform(Transform trans, float size)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(trans.position, trans.right * size);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(trans.position, trans.forward * size);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(trans.position, trans.up * size);
        }

        public static void MarkLine(Vector3 worldPos, float length, float rad)
        {
#if UNITY_EDITOR
            UnityEditor.SceneView scene = UnityEditor.SceneView.currentDrawingSceneView;
            if (!scene || !scene.camera)
                return;
            Matrix4x4 defM = Gizmos.matrix;
            Gizmos.matrix = scene.camera.cameraToWorldMatrix;

            Vector3 p = scene.camera.worldToCameraMatrix.MultiplyPoint(worldPos);
            Vector3 sp = scene.camera.WorldToScreenPoint(worldPos);
            Vector3 sp2 = scene.camera.WorldToScreenPoint(Gizmos.matrix.MultiplyPoint(p + Vector3.right));
            float pixScale = 1f / Vector3.Distance(sp, sp2);

            Vector3 p1;
            Vector2 off = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            off *= length * pixScale;
            p1.x = p.x + off.x;
            p1.y = p.y + off.y;
            p1.z = p.z;
            Gizmos.DrawLine(p, p1);
            Gizmos.matrix = defM;
#endif
        }

        public static void MarkInScene(Vector3 worldPos, float pixel, float rad)
        {
#if UNITY_EDITOR
            UnityEditor.SceneView scene = UnityEditor.SceneView.currentDrawingSceneView;
            if (!scene || !scene.camera)
                return;
            Matrix4x4 defM = Gizmos.matrix;
            Gizmos.matrix = scene.camera.cameraToWorldMatrix;

            Vector3 p = scene.camera.worldToCameraMatrix.MultiplyPoint(worldPos);
            Vector3 sp = scene.camera.WorldToScreenPoint(worldPos);
            Vector3 sp2 = scene.camera.WorldToScreenPoint(Gizmos.matrix.MultiplyPoint(p + Vector3.right));
            float pixScale = 1f / Vector3.Distance(sp, sp2);

            Vector3 p1;
            Vector2 off = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            off *= pixel * pixScale;
            p1.x = p.x + off.x;
            p1.y = p.y + off.y;
            p1.z = p.z;
            Vector3 p2 = p1;
            p2.x = p.x * 2f - p1.x;
            p2.y = p.y * 2f - p1.y;
            Gizmos.DrawLine(p1, p2);
            p1.x = p.x + off.y;
            p1.y = p.y - off.x;
            p2.x = p.x * 2f - p1.x;
            p2.y = p.y * 2f - p1.y;
            Gizmos.DrawLine(p1, p2);
            Gizmos.matrix = defM;
#endif
        }

        //屏幕像素大小与本地单位大小比值
        public static float FactorToPixel(Camera camera, Transform trans, Vector3 localPos = new Vector3())
        {
            Vector3 lp1 = trans ? trans.localToWorldMatrix.MultiplyPoint(localPos) : localPos;

            Vector3 sp1 = camera.WorldToScreenPoint(lp1);
            Vector3 sp2 = camera.ScreenToWorldPoint(sp1 + Vector3.right);
            Vector3 lp2 = trans ? trans.worldToLocalMatrix.MultiplyPoint(sp2) : sp2;
            float dis = Vector3.Distance(lp2, localPos);
            return dis > 0f ? 1f / dis : 0f;
        }

        //本地单位大小与屏幕像素大小比值
        public static float FactorFromPixel(Camera camera, Transform trans, Vector3 localPos = new Vector3())
        {
            Vector3 lp1 = trans ? trans.localToWorldMatrix.MultiplyPoint(localPos) : localPos;

            Vector3 sp1 = camera.WorldToScreenPoint(lp1);
            Vector3 sp2 = camera.ScreenToWorldPoint(sp1 + Vector3.right);
            Vector3 lp2 = trans ? trans.worldToLocalMatrix.MultiplyPoint(sp2) : sp2;
            float dis = Vector3.Distance(lp2, localPos);
            return dis;
        }

        public static float FactorToSceneViewPixel(Transform trans, Vector3 localPos = new Vector3())
        {
#if UNITY_EDITOR
            UnityEditor.SceneView scene = UnityEditor.SceneView.currentDrawingSceneView;
            if (scene)
                return FactorToPixel(scene.camera, trans, localPos);
            else
                return 0f;
#else
        return 0f;
#endif

        }

        public static float FactorFromSceneViewPixel(Transform trans, Vector3 localPos = new Vector3())
        {
#if UNITY_EDITOR
            UnityEditor.SceneView scene = UnityEditor.SceneView.currentDrawingSceneView;
            if (scene)
                return FactorFromPixel(scene.camera, trans, localPos);
            else
                return 0f;
#else
        return 0f;
#endif
        }

    }
}