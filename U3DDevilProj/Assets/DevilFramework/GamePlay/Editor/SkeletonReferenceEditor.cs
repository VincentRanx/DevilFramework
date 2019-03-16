using Devil.GamePlay.Assistant;
using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(SkeletonReference))]
	public class SkeletonReferenceEditor : Editor 
	{
        static bool foldout = true;

        SkeletonReference mSkeleton;
        List<Transform> mDrops = new List<Transform>();
        bool mDirty;
        
        void GetDrops()
        {
            mDrops.Clear();
            var t = DragAndDrop.objectReferences;
            int len = t == null ? 0 : t.Length;
            for (int i = 0; i < len; i++)
            {
                var go = t[i] as GameObject;
                if (go != null && go != mSkeleton.LocalRoot.gameObject && go.transform.IsChildOf(mSkeleton.LocalRoot))
                {
                    mDrops.Add(go.transform);
                }
            }
        }

        private void OnEnable()
        {
            mSkeleton = target as SkeletonReference;
        }

        string GetBonePath(Transform bone)
        {
            if (bone == mSkeleton.LocalRoot)
                return "";
            var buf = StringUtil.GetBuilder(bone.name);
            var root = mSkeleton.LocalRoot;
            while(bone.parent != null && bone.parent != root)
            {
                bone = bone.parent;
                buf.Insert(0, '/');
                buf.Insert(0, bone.name);
            }
            return StringUtil.ReleaseBuilder(buf);
        }

        void InsertBone(Transform bone)
        {
            var index = GlobalUtil.FindIndex(mSkeleton.Binders, (x) => x.bone == bone);
            if (index != -1)
                return;
            var bind = new SkeletonReference.BoneBinder();
            bind.bone = bone;
            bind.bonePath = GetBonePath(bone);
            bind.bindTo = mSkeleton.ReferenceRoot == null ? null : mSkeleton.ReferenceRoot.Find(bind.bonePath);
            for (int i = mSkeleton.Binders.Count - 1; i >= 0; i--)
            {
                var b = mSkeleton.Binders[i];
                if (bone.IsChildOf(b.bone))
                {
                    mSkeleton.Binders.Insert(i + 1, bind);
                    mDirty = true;
                    return;
                }
            }
            mSkeleton.Binders.Insert(0, bind);
            mDirty = true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var local = EditorGUILayout.ObjectField("Local Root", mSkeleton.LocalRoot, typeof(Transform), true) as Transform;
            if(local != mSkeleton.LocalRoot && local.IsChildOf(mSkeleton.transform))
            {
                mSkeleton.LocalRoot = local;
                for(int i= mSkeleton.Binders.Count - 1; i >= 0; i--)
                {
                    var bind = mSkeleton.Binders[i];
                    bind.bonePath = GetBonePath(bind.bone);
                    if (string.IsNullOrEmpty(bind.bonePath))
                        mSkeleton.Binders.RemoveAt(i);
                }
                mDirty = true;
            }
            var refer = EditorGUILayout.ObjectField("Reference Root", mSkeleton.ReferenceRoot, typeof(Transform), true) as Transform;
            if(refer == null || !refer.IsChildOf(mSkeleton.transform))
            {
                mSkeleton.ReferenceRoot = refer;
                mDirty = true;
            }

            var rect = EditorGUILayout.BeginVertical("helpbox");
            foldout = EditorGUILayout.Foldout(foldout, "Local Bones", true);
            if(!foldout || mSkeleton.Binders.Count == 0)
            {
                EditorGUILayout.LabelField("Drag a child transform here to add.", GUILayout.Height(30));
            }
            if (foldout)
            {
                for (int i = 0; i < mSkeleton.Binders.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    var del = GUILayout.Button("", "OL Minus", GUILayout.Width(17));
                    if(mSkeleton.m_BakeSelfState != 0)
                    {
                        var root = i == mSkeleton.m_BakeSelfToBoneIndex;
                        if( EditorGUILayout.Toggle(root,root? "Icon.AvatarMaskOn": "Icon.AvatarMaskOff", GUILayout.Width(15)))
                        {
                            mDirty |= mSkeleton.m_BakeSelfToBoneIndex != i;
                            mSkeleton.m_BakeSelfToBoneIndex = i;
                        }
                    }
                    var ping = GUILayout.Button(mSkeleton.Binders[i].bone.name, "label");
                    EditorGUILayout.EndHorizontal();
                    if (ping)
                        EditorGUIUtility.PingObject(mSkeleton.Binders[i].bone);
                    if (del)
                    {
                        mSkeleton.Binders.RemoveAt(i);
                        mDirty = true;
                        break;
                    }
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                if(GUILayout.Button("Add All Children", "LargeButton"))
                {
                    var bones = mSkeleton.LocalRoot.GetComponentsInChildren<Transform>();
                    foreach(var b in bones)
                    {
                        if (b == mSkeleton.LocalRoot)
                            continue;
                        InsertBone(b);
                    }
                }
                GUILayout.Space(50);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                    GetDrops();
                DragAndDrop.visualMode = mDrops.Count > 0 ? DragAndDropVisualMode.Link : DragAndDropVisualMode.None;
                if (mDrops.Count > 0 && Event.current.type == EventType.DragPerform)
                {
                    foreach (var t in mDrops)
                    {
                        InsertBone(t);
                    }
                    DragAndDrop.AcceptDrag();
                }
            }
            if (mDirty)
            {
                mDirty = false;
                EditorUtility.SetDirty(mSkeleton);
            }
        }
    }
}