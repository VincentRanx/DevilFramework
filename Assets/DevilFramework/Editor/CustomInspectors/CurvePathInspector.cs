using UnityEngine;
using UnityEditor;

namespace DevilTeam.Editor
{

    [CustomEditor(typeof(CurvePath))]
    public class CurvePathInspector : UnityEditor.Editor
    {

        private CurvePath mPath;
        private Transform mPathTrans;

        private Vector3 tmpPoint;
        private Vector3[] caches;

        private float snapDis = 0.001f;

        private string[] addSubBtn = new string[] { "+","-"};

        private bool mFreezeX;
        private bool mFreezeY;
        private bool mFreezeZ;
        private int mFreezeAxis;
        private string[] freezeOption = new string[] { "Freeze X", "Freeze Y", "Freeze Z" };

        private Vector2 mPointsArea;
        private int mCurrentPoint;

        private void OnEnable()
        {
            mPath = target as CurvePath;
            mPathTrans = mPath.transform;
            CheckTarget();

            caches = new Vector3[mPath.m_Points.Length];
            System.Array.Copy(mPath.m_Points, caches, caches.Length);

            mFreezeAxis = EditorPrefs.GetInt("CurvePath.freezeAxis");
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt("CurvePath.freezeAxis", mFreezeAxis);
            System.Array.Copy(caches, mPath.m_Points, caches.Length);
            mPath.RecalculateLater();
            mPath = null;
            mPathTrans = null;
        }

        private void CheckTarget()
        {
            if (mPath.m_Points == null || mPath.m_Points.Length == 0)
            {
                mPath.m_Points = new Vector3[1];
            }
            else
            {
                mPath.m_Points[0] = Vector3.zero;
            }
        }

        private void InsertAt(int index)
        {
            Vector3[] points = new Vector3[caches.Length + 1];
            if (index > 0)
                System.Array.Copy(caches, points, index);
            if (index < caches.Length)
                System.Array.Copy(caches, index, points, index + 1, caches.Length - index);
            points[index] = (points[(index - 1 + points.Length) % points.Length] + points[(index + 1) % points.Length]) * 0.5f;
            mPath.m_Points = points;
        }

        private void DeleteAt(int index)
        {
            Vector3[] points = new Vector3[caches.Length - 1];
            System.Array.Copy(caches, points, index);
            if(index< caches.Length - 1)
                System.Array.Copy(caches, index + 1, points, index, points.Length - index);
            mPath.m_Points = points;
        }

        public override void OnInspectorGUI()
        {
            GUI.skin.label.richText = true;
            CheckTarget();
            bool dirty = false;
            GUILayout.Space(10);
            ECurveType tp = (ECurveType)EditorGUILayout.EnumPopup(mPath.m_Type);
            if(tp != mPath.m_Type)
            {
                mPath.m_Type = tp;
                dirty = true;
            }
            int insertAt = -1;
            int deleteAt = -1;
            mFreezeAxis = QuickGUI.MultiOptionBar(mFreezeAxis, freezeOption);
            mPointsArea = GUILayout.BeginScrollView(mPointsArea, GUILayout.MaxHeight(200));
            if (QuickGUI.DrawHeader("Sample Points", "sample points", false))
            {
                QuickGUI.BeginContents(170);
                for(int i = 1; i < caches.Length;i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    int n = QuickGUI.GroupButton(addSubBtn, GUILayout.Width(40));
                    caches[i] = EditorGUILayout.Vector3Field(string.Format( "P{0}{1}" , i, i == mCurrentPoint?" [Moving]":""), caches[i]);
                    EditorGUILayout.EndHorizontal();
                    if(n != -1)
                    {
                        insertAt = n == 0 ? i : -1;
                        deleteAt = n == 1 ? i : -1;
                    }
                    if(Vector3.Distance(caches[i],mPath.m_Points[i])> snapDis)
                    {
                        dirty = true;
                        mPath.m_Points[i] = caches[i];
                    }
                }
                if(GUILayout.Button("+", GUILayout.Width(20)))
                {
                    insertAt = caches.Length;
                    deleteAt = -1;
                }
                QuickGUI.EndContents();
            }
            GUILayout.EndScrollView();

            if (insertAt > 0)
            {
                InsertAt(insertAt);
                dirty = true;
            }
            else if (deleteAt > 0)
            {
                DeleteAt(deleteAt);
                dirty = true;
            }
            
            if (dirty)
            {
                caches = new Vector3[mPath.m_Points.Length];
                System.Array.Copy(mPath.m_Points, caches, caches.Length);
                mPath.RecalculateLater();
                SceneView.RepaintAll();
            }
        }

        void FreezePoint(ref Vector3 p, Vector3 oldValue)
        {
            if( (mFreezeAxis & 1 )!= 0)
            {
                p.x = oldValue.x;
            }
            if ((mFreezeAxis & 2) != 0)
            {
                p.y = oldValue.y;
            }
            if ((mFreezeAxis & 4) != 0)
            {
                p.z = oldValue.z;
            }
        }

        private void OnSceneGUI()
        {
            Matrix4x4 m = mPathTrans.localToWorldMatrix;
            Matrix4x4 m2 = mPathTrans.worldToLocalMatrix;
            Handles.color = Color.gray;
            bool dirty = false;
            for (int i = 1; i < caches.Length; i++)
            {
                Vector3 p = m.MultiplyPoint(caches[i]);
                Vector3 newP = Handles.FreeMoveHandle(p, mPathTrans.rotation, HandleUtility.GetHandleSize(p) * 0.05f, Vector3.one * 0.0001f, Handles.DotHandleCap);
                FreezePoint(ref newP, p);
                caches[i] = m2.MultiplyPoint(newP);
                if (Vector3.Distance(mPath.m_Points[i], caches[i]) > snapDis)
                {
                    dirty = true;
                    mCurrentPoint = i;
                    mPath.m_Points[i] = caches[i];
                }
            }
            if (Event.current.type == EventType.mouseDown && Event.current.button == 1)
            {
                Event.current.Use();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Delete Point"), true, () => { });
                menu.ShowAsContext();
            }

            if (dirty)
            {
                System.Array.Copy(caches, mPath.m_Points, caches.Length);
                mPath.RecalculateLater();
            }
        }

    }
}