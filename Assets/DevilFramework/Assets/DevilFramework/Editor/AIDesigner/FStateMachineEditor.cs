using Devil.AI;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{

    public class FStateMachineEditor : GraphViewEditorWindow
    {
        [MenuItem("Devil Framework/FSM Editor")]
        static void OpenWindow()
        {
            FStateMachineEditor window = GetWindow<FStateMachineEditor>();
            window.Show();
        }

        static GUIStyle nodeTitleStyle = new GUIStyle();
        static GUIContent nodeTitleContent = new GUIContent();

        string uid;
        FStateMachineRunner fsmInst;
        bool lockFsm;

        protected override void InitParameters()
        {
            mGraphLayers = 1;
            mViewportTitle = @"<size=30><b>FINITE STATE MACHINE DESIGN</b></size><size=23> (ver.1.0.0)</size>
<size=18>  NOTE: 
  按住【滚轮】平移视图
  按住【左键】拖动节点或者框选节点
  点击【右键】取消当前操作
  按住【SHIFT】多选
  按住【ALT】选择所有子节点
  点击【CTRL+F】居中视图</size>";
            nodeTitleStyle.alignment = TextAnchor.MiddleCenter;
            nodeTitleStyle.richText = false;
            nodeTitleStyle.fontSize = 13;
            nodeTitleStyle.wordWrap = false;
            nodeTitleStyle.fontStyle = FontStyle.Bold;
            nodeTitleStyle.normal.textColor = Color.green;
            nodeTitleStyle.onHover.textColor = Color.yellow;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Selection.selectionChanged += OnReload;

#if UNITY_2017_2
            EditorApplication.playModeStateChanged += OnSelectionChanged;
#else
            EditorApplication.playmodeStateChanged += OnSelectionChanged;
#endif
            ValidateFSM();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnReload;

#if UNITY_2017_2
            EditorApplication.playModeStateChanged -= OnSelectionChanged;
#else
            EditorApplication.playmodeStateChanged -= OnSelectionChanged;
#endif
        }

        protected override void OnTitleGUI()
        {
            GUILayout.Label("UID:", GUILayout.Width(30));
            string id = GUILayout.TextField(uid);
            if (id != uid)
            {
                ValidateUID(id);
            }
            bool tog = GUILayout.Toggle(lockFsm, "", "IN LockButton", GUILayout.Width(40));
            if (lockFsm ^ tog)
            {
                lockFsm = tog;
            }
            //EditorGUILayout.SelectableLabel("UID:" + uid, "textfield");
            if (!Application.isPlaying && GUILayout.Button("刷新", "TE toolbarbutton", GUILayout.Width(70)))
            {

            }
            if (!Application.isPlaying && Selection.activeGameObject != null &&
                GUILayout.Button("保存", "TE toolbarbutton", GUILayout.Width(70)))
            {

            }
        }

        void ValidateUID(string newid)
        {
            uid = (newid ?? "").Trim();
            if (!string.IsNullOrEmpty(uid) && fsmInst)
            {
                Ref.SetField(fsmInst, "m_Uid", uid);
                EditorUtility.SetDirty(fsmInst);
            }
        }

        void OnReload()
        {
            ValidateFSM(null);
        }

#if UNITY_2017_2
        private void OnSelectionChanged(PlayModeStateChange state)
        {
            ValidateFSM();
            Repaint();
        }
#else
        private void OnSelectionChanged()
        {
            ValidateFSM();
            Repaint();
        }
#endif

        private void ValidateFSM(GameObject oldObj = null)
        {
            if (oldObj == Selection.activeGameObject && oldObj != null)
                return;
            if (Selection.activeGameObject)
            {
                fsmInst = Selection.activeGameObject.GetComponent<FStateMachineRunner>();
                uid = fsmInst == null ? "" : ((string)Ref.GetField(fsmInst, "m_Uid"));
                uid = (uid ?? "").Trim();
                if (string.IsNullOrEmpty(uid))
                {
                    uid = string.Format("g_{0}", Selection.activeGameObject.GetInstanceID().ToString("x"));
                    if (fsmInst)
                    {
                        Ref.SetField(fsmInst, "m_Uid", uid);
                        EditorUtility.SetDirty(fsmInst);
                    }
                }
            }
            else
            {
                fsmInst = null;
                uid = "nil";
            }
        }

        //protected override void OnNodeSocketGUI(GraphNode node, bool selected)
        //{
        //    base.OnNodeSocketGUI(node, selected);
        //}

        protected override bool OnContextMenuGUI(Vector2 position, ref Rect rect)
        {
            if (RaycastNode != null)
            {
                return false;
            }
            return true;
        }
    }
}