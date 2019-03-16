using Devil;
using Devil.Utility;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(EmojiAnims))]
    public class EmojiAnimsInspector : Editor
    {
        int mDropIndex;
        EmojiAnims mTarget;
        Vector2 mPos;
        bool mDirty;

        private void OnEnable()
        {
            mTarget = target as EmojiAnims;
        }
        
        void InsertAnim(int index, EmojiAnims.Anim current = null)
        {
            var anim = new EmojiAnims.Anim();
            anim.m_Name = "new emoj";
            anim.m_Duration = current == null ? 0.4f : current.m_Duration;
            anim.m_Frames = new EmojiAnims.Frame[0];
            EmojiAnims.Anim[] anims;
            if (mTarget.m_Anims != null && mTarget.m_Anims.Length > 0)
            {
                anims = new EmojiAnims.Anim[mTarget.m_Anims.Length + 1];
                index = Mathf.Min(index, anims.Length - 1);
                if (index > 0)
                    System.Array.Copy(mTarget.m_Anims, anims, index);
                if (index < mTarget.m_Anims.Length)
                    System.Array.Copy(mTarget.m_Anims, index, anims, index + 1, mTarget.m_Anims.Length - index);
                anims[index] = anim;
                mTarget.m_Anims = anims;
                mDirty = true;
            }
            else
            {
                anims = new EmojiAnims.Anim[1];
                anims[0] = anim;
                mTarget.m_Anims = anims;
                mDirty = true;
            }
        }

        void RemoveAnim(int index)
        {
            if (mTarget.m_Anims != null && index < mTarget.m_Anims.Length)
            {
                var anims = new EmojiAnims.Anim[mTarget.m_Anims.Length - 1];
                if (index > 0)
                    System.Array.Copy(mTarget.m_Anims, anims, index);
                if (index < anims.Length)
                    System.Array.Copy(mTarget.m_Anims, index + 1, anims, index, anims.Length - index);
                mTarget.m_Anims = anims;
                mDirty = true;
            }
        }

        void OnFrameGUI(EmojiAnims.Anim anim)
        {
            EditorGUILayout.BeginVertical("Icon.ClipSelected");
            var frames = anim.m_Frames;
            if (frames == null || frames.Length == 0)
            {
                frames = new EmojiAnims.Frame[1];
                frames[0] = new EmojiAnims.Frame();
                frames[0].m_Sprite = anim.m_Name;
                anim.m_Frames = frames;
            }
            for (int i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                EditorGUILayout.BeginHorizontal();
                bool del = GUILayout.Button("", "OL Minus", GUILayout.Width(20));
                EditorGUILayout.LabelField(StringUtil.FormatTime(anim.m_Duration * i));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                bool add = GUILayout.Button("", "OL Plus", GUILayout.Width(20));
                EditorGUILayout.LabelField("sprite", GUILayout.Width(40));
                var spr = EditorGUILayout.TextField(frame.m_Sprite);
                mDirty |= spr != frame.m_Sprite;
                frame.m_Sprite = spr;
                EditorGUILayout.LabelField("rotate", GUILayout.Width(40));
                var rot = EditorGUILayout.FloatField(frame.m_Rotate);
                mDirty |= rot != frame.m_Rotate;
                frame.m_Rotate = rot;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                if (add)
                {
                    var newf = new EmojiAnims.Frame();
                    newf.m_Sprite = frame.m_Sprite;
                    newf.m_Rotate = frame.m_Rotate;
                    GlobalUtil.Insert(ref frames, i + 1, newf);
                    anim.m_Frames = frames;
                    mDirty = true;
                    GUI.FocusControl(null);
                    break;
                }
                else if (del)
                {
                    GlobalUtil.RemoveAt(ref frames, i);
                    anim.m_Frames = frames;
                    mDirty = true;
                    GUI.FocusControl(null);
                    break;
                }
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            mPos = EditorGUILayout.BeginScrollView(mPos);
            if (mTarget.m_Anims != null)
            {
                for (int i = 0; i < mTarget.m_Anims.Length; i++)
                {
                    bool drop = mDropIndex == i;
                    EditorGUILayout.BeginVertical(drop ? "flow overlay box" : "box");
                    var anim = mTarget.m_Anims[i];
                    int ret = QuickGUI.TitleBarWithBtn(anim.m_Name, 12, 15, "OL Minus", "OL Plus");
                    if (ret == 0)
                    {
                        mDropIndex = i;
                        GUI.FocusControl(null);
                    }
                    if (drop)
                    {
                        var str = EditorGUILayout.TextField("Name", anim.m_Name ?? "");
                        mDirty |= str != anim.m_Name;
                        anim.m_Name = str;
                        var dur = Mathf.Max(0.0001f, EditorGUILayout.FloatField("Duration", anim.m_Duration));
                        mDirty |= dur != anim.m_Duration;
                        anim.m_Duration = dur;
                        OnFrameGUI(anim);
                    }
                    if (drop)
                        GUILayout.Space(10);
                    EditorGUILayout.EndVertical();
                    if (ret == 1)
                    {
                        RemoveAnim(i);
                        mDirty = true;
                        GUI.FocusControl(null);
                        break;
                    }
                    else if (ret == 2)
                    {
                        InsertAnim(i, anim);
                        mDropIndex = i;
                        mDirty = true;
                        GUI.FocusControl(null);
                        break;
                    }
                }
            }
            EditorGUILayout.BeginVertical("U2D.createRect");
            if (QuickGUI.TitleBar("New Emoji Animation", 13) == 0)
            {
                InsertAnim(mTarget.m_Anims == null ? 0 : mTarget.m_Anims.Length);
                mDropIndex = mTarget.m_Anims.Length - 1;
                mDirty = true;
                GUI.FocusControl(null);
            }
            if (mDirty)
            {
                mDirty = false;
                EditorUtility.SetDirty(target);
                serializedObject.Update();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}