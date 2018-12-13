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

        string missAssets;
        bool getMissAsset;
        readonly string comHint = "Edit Comment...";

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

        void TryGetMissingAssets()
        {
            if (!getMissAsset)
                return;
            var tree = target as BehaviourTreeAsset;
            if (tree == null)
                return;
            missAssets = tree.EditorMissingAssets();
            if (!string.IsNullOrEmpty(missAssets))
                missAssets = "Missing " + missAssets;
            getMissAsset = false;
        }

        private void OnEnable()
        {
            getMissAsset = true;
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
            TryGetMissingAssets();
            UpdateBinders();
            if(!string.IsNullOrEmpty(missAssets))
            {
                EditorGUILayout.HelpBox(missAssets, MessageType.Warning);
            }
            var tree = target as BehaviourTreeAsset;
            if (tree != null)
            {
                EditorGUILayout.BeginHorizontal("helpbox");
                var s = EditorGUILayout.TextArea(string.IsNullOrEmpty(tree.m_Comment) ? comHint : tree.m_Comment, DevilEditorUtility.HintStyle("label"), GUILayout.Height(120));
                if (s == comHint)
                    tree.m_Comment = "";
                else
                    tree.m_Comment = s;
                EditorGUILayout.EndHorizontal();
            }
            //EditorGUI.BeginDisabledGroup(true);
            //base.OnInspectorGUI();
            
            //EditorGUI.EndDisabledGroup();

            if (mServices.Count > 0)
                EditorGUILayout.LabelField("服务", (GUIStyle)"LODLevelNotifyText", GUILayout.Height(30));
            foreach (var t in mServices)
            {
                t.DrawGUI();
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(40);

            var editing = BehaviourTreeEditor.ActiveBTEditor == null ? null : BehaviourTreeEditor.ActiveBTEditor.TargetTree;
            if (target != editing && GUILayout.Button("打开编辑器", "LargeButton"))
            {
                BehaviourTreeEditor.OpenBTEditor(target as BehaviourTreeAsset);
            }
            if(target != editing && GUILayout.Button("修复资源引用", "LargeButton"))
            {
                ((BehaviourTreeAsset)target).EditorResovleAsset();
                getMissAsset = true;
                if (BehaviourTreeEditor.ActiveBTEditor != null && BehaviourTreeEditor.ActiveBTEditor.Binder != null)
                    BehaviourTreeEditor.ActiveBTEditor.Binder.Reset();
            }
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