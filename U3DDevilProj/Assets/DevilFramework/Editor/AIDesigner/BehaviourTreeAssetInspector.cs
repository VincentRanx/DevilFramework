using Devil.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using Binder = BTNodeInspector.Binder;

    [CustomEditor(typeof(BehaviourTreeAsset))]
    public class BehaviourTreeAssetInspector : Editor
    {
        List<BTNode> mServNodes = new List<BTNode>();
        List<Binder> mServices = new List<Binder>();

        void UpdateBinders()
        {
            var node = target as BehaviourTreeAsset;
            mServNodes.Clear();
            node.GetAllNodes(mServNodes, (x) => x.isService);
            Resize(mServices, mServNodes.Count);
            for (int i = 0; i < mServNodes.Count; i++)
            {
                mServices[i].Target = mServNodes[i];
            }
        }

        void Resize(List<Binder> binders, int count)
        {
            if (count < binders.Count)
            {
                for (int i = binders.Count - 1; i >= count; i--)
                {
                    var t = binders[i];
                    binders.RemoveAt(i);
                    t.Dispose();
                }
            }
            else if (count > binders.Count)
            {
                for (int i = binders.Count; i < count; i++)
                {
                    var t = new Binder();
                    binders.Add(t);
                }
            }
        }

        private void OnEnable()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            foreach (var t in mServices)
            {
                t.Dispose();
            }
            mServices.Clear();
        }

        public override void OnInspectorGUI()
        {
            UpdateBinders();
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            
            EditorGUI.EndDisabledGroup();

            if (mServices.Count > 0)
                EditorGUILayout.LabelField("服务", (GUIStyle)"LODLevelNotifyText", GUILayout.Height(30));
            foreach (var t in mServices)
            {
                t.DrawGUI();
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(40);

            if (!BehaviourTreeEditor.IsActive && GUILayout.Button("打开编辑器", "LargeButton"))
            {
                BehaviourTreeEditor.OpenBTEditor(target as BehaviourTreeAsset);
            }
            else if(BehaviourTreeEditor.IsActive && !Application.isPlaying && GUILayout.Button("保存修改", "LargeButton"))
            {
                BehaviourTreeEditor.ActiveBTEditor.Binder.SaveAsset();
            }
            //if(GUILayout.Button("清理无用资源", "LargeButton"))
            //{
            //    (target as BehaviourTreeAsset).EditorCleanup();
            //}
            GUILayout.Space(40);
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("Assets/AI/Delete Inner Asset")]
        public static void DeleteAsset()
        {
            var t = Selection.activeObject as BTAsset;
            if (t != null && t.TreeAsset != null)
            {
                t.TreeAsset.EditorDeleteAsset(t);
            }
        }
        
    }
}