using Devil.UI;
using Devil.Utility;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(FreePolygon))]
    public class FreePolygonInspector : Editor
    {

        bool drop;
        FreePolygon mPolygon;

        private void OnEnable()
        {
            mPolygon = target as FreePolygon;
        }

        private void OnDisable()
        {
            mPolygon = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (QuickGUI.TitleBar("Add Point", 12) == 0)
            {
                mPolygon.AddPoint(Vector2.zero);
                return;
            }
            drop = QuickGUI.DrawHeader("Points", null, drop);
            if(drop)
            {
                QuickGUI.BeginContents(20);
                for(int i = 0; i < mPolygon.Points.Length; i++)
                {
                    bool del = false;
                    Vector2 p = mPolygon.Points[i];
                    EditorGUILayout.BeginHorizontal();
                    Vector2 newp = EditorGUILayout.Vector2Field("", p);
                    if(p != newp)
                    {
                        mPolygon.Points[i] = newp;
                        mPolygon.SetVerticesDirty();
                    }
                    if (i > 2 && GUILayout.Button("-", GUILayout.Width(20)))
                        del = true;
                    EditorGUILayout.EndHorizontal();
                    if (del)
                    {
                        mPolygon.RemovePoint(i);
                        break;
                    }
                }
                QuickGUI.EndContents();
            }
        }

        int GetPoint(Rect rect, Vector3 p, float size)
        {
            for (int i = 0; i < mPolygon.Points.Length; i++)
            {
                Vector3 tmp = mPolygon.Lerp(rect, mPolygon.Points[i]);
                tmp.z = p.z;
                if (Vector3.Distance(tmp, p) <= size)
                    return i;
            }
            return -1;
        }

        private void OnSceneGUI()
        {
            if (mPolygon == null || mPolygon.Points == null || mPolygon.Points.Length == 0)
                return;
            RectTransform trans = mPolygon.rectTransform;
            Matrix4x4 m = trans.localToWorldMatrix;
            Matrix4x4 m2 = trans.worldToLocalMatrix;
            Rect rect = mPolygon.GetPixelAdjustedRect();

            bool dirty = false;
          
            var v = SceneView.currentDrawingSceneView;
            if (v != null && Event.current.control)
            {
                Vector3 mpos = Event.current.mousePosition;
                float mul = EditorGUIUtility.pixelsPerPoint;
                mpos.y = v.camera.pixelHeight - mpos.y * mul;
                mpos.x *= mul;
                mpos.z = 20;
                var p = v.camera.ScreenToWorldPoint(mpos);
                float size = HandleUtility.GetHandleSize(p) * 0.085f;
                mul = m2.MultiplyVector(v.camera.transform.right).magnitude;
                int old = mPolygon.Points.Length > 3 ? GetPoint(rect, m2.MultiplyPoint(p), size * mul) : -1;
                Handles.color = old == -1 ? Color.green : Color.red;
                Handles.CapFunction func;
                if (old == -1)
                    func = Handles.DotHandleCap;
                else
                    func = Handles.CircleHandleCap;
                if (Handles.Button(p, v.camera.transform.rotation, size, size * 0.8f, func))
                {
                    if (old != -1)
                    {
                        mPolygon.RemovePoint(old);
                    }
                    else
                    {
                        p = m2.MultiplyPoint(p);
                        mPolygon.AddPoint(mPolygon.GetPoint(rect, p));
                    }
                }
            }
            else
            {
                Handles.color = Color.blue;
                for (int i = 0; i < mPolygon.Points.Length; i++)
                {
                    Vector3 p = mPolygon.Lerp(rect, mPolygon.Points[i]);
                    p = m.MultiplyPoint(p);
                    float handlesize = HandleUtility.GetHandleSize(p);
                    Handles.Label(p, StringUtil.Concat("p", i.ToString()));
                    Vector3 newp = Handles.FreeMoveHandle(p, trans.rotation, handlesize * 0.05f, Vector3.one * 0.0001f, Handles.DotHandleCap);
                    newp.z = p.z;
                    if (newp != p)
                    {
                        dirty = true;
                        newp = m2.MultiplyPoint(newp);
                        mPolygon.Points[i] = mPolygon.GetPoint(rect, newp);
                    }
                }
            }
            if (dirty)
                mPolygon.SetVerticesDirty();
        }
    }
}