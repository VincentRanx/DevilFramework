using Devil.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BTNodeAsset), true)]
    public class BTNodeInspector : Editor
    {
       
        public class Binder : System.IDisposable
        {
            BTNode target;
            AIModules.Module mod;
            bool foldout = true;
            public Editor editor { get; private set; }
            
            public Binder()
            {

            }

            public BTNode Target
            {
                get { return target; }
                set
                {
                    if (target != value)
                    {
                        target = value;
                        mod = target == null ? null : AIModules.Get(target);
                        if (editor != null)
                            Object.DestroyImmediate(editor);
                        if (target != null && target.Asset != null)
                        {
                            editor = Editor.CreateEditor(target.Asset);
                            foldout = EditorPrefs.GetBool(target.Asset.GetType().Name, true);
                        }
                    }
                }
            }

            public void DrawGUI()
            {
                if (editor != null && mod != null)
                {
                    EditorGUILayout.BeginHorizontal((GUIStyle)"IN Title");
                    var fold = foldout;
                    foldout = EditorGUILayout.Foldout(foldout, mod.Title, true);
                    EditorGUILayout.EndHorizontal();
                    if(fold ^ foldout && target.Asset != null)
                    {
                        EditorPrefs.SetBool(target.Asset.GetType().Name, foldout);
                    }
                    if (foldout)
                    {
                        if (!string.IsNullOrEmpty(mod.Detail))
                            EditorGUILayout.HelpBox(mod.Detail, MessageType.Info);
                        EditorGUI.indentLevel++;
                        editor.OnInspectorGUI();
                        EditorGUI.indentLevel--;
                    }
                    GUILayout.Space(4);
                }
            }

            public void Dispose()
            {
                if (editor != null)
                    Object.DestroyImmediate(editor);
                editor = null;
                target = null;
            }
        }

        List<Binder> mConditions = new List<Binder>();
        AIModules.Module mMod;

        private void OnEnable()
        {
            mMod = target == null ? null : AIModules.Get(target.GetType());
        }

        private void OnDisable()
        {
            foreach(var t in mConditions)
            {
                t.Dispose();
            }
            mConditions.Clear();
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

        void UpdateBinders()
        {
            var node = target as BTNodeAsset;
            int len = node.ConditionCount;
            Resize(mConditions, len);
            for (int i = 0; i < len; i++)
            {
                mConditions[i].Target = node.TreeAsset.GetNodeById(node.GetConditionId(i));
            }
        }

        bool FoldModule(AIModules.Module mod, bool foldout)
        {
            if (mod == null)
                return foldout;
            EditorGUILayout.BeginHorizontal((GUIStyle)"IN Title");
            foldout = EditorGUILayout.Foldout(foldout, mod.Title, true);
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(mod.Detail))
                EditorGUILayout.HelpBox(mod.Detail, MessageType.Info);
            return foldout;

        }
        public override void OnInspectorGUI()
        {
            if (target != null)
            {
                OnConditionGUI();
                EditorGUILayout.LabelField(mMod.Title, (GUIStyle)"LODLevelNotifyText", GUILayout.Height(30));
                if (!string.IsNullOrEmpty(mMod.Detail))
                    EditorGUILayout.HelpBox(mMod.Detail, MessageType.Info);
            }
            if (serializedObject != null)
                base.OnInspectorGUI();
        }

        protected virtual void OnConditionGUI()
        {
            bool muledit = targets != null && targets.Length > 1;
            if (!muledit)
            {
                UpdateBinders();
            }
            if (muledit)
            {
                EditorGUILayout.HelpBox("This scope don't support multi-editing.", MessageType.Warning);
            }
            else
            {
                if (mConditions.Count > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("条件", (GUIStyle)"LODLevelNotifyText", GUILayout.Height(30));
                    foreach (var t in mConditions)
                    {
                        t.DrawGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}