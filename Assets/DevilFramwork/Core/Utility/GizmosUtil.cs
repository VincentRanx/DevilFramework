using UnityEngine;

namespace DevilTeam.Utility
{
    public class GizmosUtil
    {

        public static void DrawWireOvalSphere2D(Vector3 pos, Vector2 size, Quaternion rotate, int mount)
        {
            if (size.x == 0 || size.y == 0)
                return;
            Vector2 rad = size * 0.5f;
            Matrix4x4 rotM = new Matrix4x4();
            rotM.SetTRS(pos, rotate, Vector3.one);
            Matrix4x4 defM = Gizmos.matrix;
            Gizmos.matrix = defM * rotM;
            Vector3 p0 = new Vector3(-rad.x, 0);
            float f = Mathf.PI / (float)mount;
            Vector3 p1 = new Vector3();
            Vector3 t0, t1;
            for (int i = 1; i <= mount; i++)
            {
                p1.x = -Mathf.Cos(f * i) * rad.x;
                p1.y = Mathf.Sqrt(rad.y * rad.y * (1f - p1.x * p1.x / (rad.x * rad.x)));
                Gizmos.DrawLine(p0, p1);
                t0 = p0;
                t0.y = -t0.y;
                t1 = p1;
                t1.y = -t1.y;
                Gizmos.DrawLine(t0, t1);
                p0 = p1;
            }
            Gizmos.matrix = defM;
        }

        public static void DrawWireOvalSphere3D(Vector3 pos, Vector3 size, int mount)
        {
            Gizmos.color = Color.blue;
            GizmosUtil.DrawWireOvalSphere2D(pos, new Vector2(size.x, size.y),
                Quaternion.identity, 30);

            Gizmos.color = Color.red;
            GizmosUtil.DrawWireOvalSphere2D(pos, new Vector2(size.z, size.y),
                Quaternion.LookRotation(Vector3.right), 30);

            Gizmos.color = Color.green;
            GizmosUtil.DrawWireOvalSphere2D(pos, new Vector2(size.x, size.z),
                Quaternion.LookRotation(Vector3.up), 30);
        }

        public static void DrawWireOvalSphere3D(Color color, Vector3 pos, Vector3 size, int mount)
        {
            Gizmos.color = color;
            GizmosUtil.DrawWireOvalSphere2D(pos, new Vector2(size.x, size.y),
                Quaternion.identity, 30);

            GizmosUtil.DrawWireOvalSphere2D(pos, new Vector2(size.z, size.y),
                Quaternion.LookRotation(Vector3.right), 30);

            GizmosUtil.DrawWireOvalSphere2D(pos, new Vector2(size.x, size.z),
                Quaternion.LookRotation(Vector3.up), 30);
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