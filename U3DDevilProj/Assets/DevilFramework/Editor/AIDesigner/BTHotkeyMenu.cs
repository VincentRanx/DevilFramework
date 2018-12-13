using System.Collections.Generic;
using UnityEngine;

namespace DevilEditor
{
    public class BTHotkeyMenu : BTEditorMenu
    {
        KeyCode mKey;
        //Vector2 mPos;
        List<AIModules.Module> mModules = new List<AIModules.Module>();

        public void Display(BehaviourTreeEditor editor, Rect pos, KeyCode hotkey)
        {
            if(mKey != hotkey)
            {
                mKey = hotkey;
                mModules.Clear();
                AIModules.Get(mModules, hotkey);
                if (mModules.Count == 0)
                    return;
                mContents = new GUIContent[mModules.Count];
                mItems.Clear();
                for (int i = 0; i < mModules.Count; i++)
                {
                    mContents[i] = new GUIContent(hotkey == KeyCode.Alpha1 || hotkey == KeyCode.Alpha2 || hotkey == KeyCode.Alpha3 || hotkey == KeyCode.Alpha4 ?
                        mModules[i].CateTitle : mModules[i].Title);
                    var it = NewItem(mContents[i], CreateNode, mModules[i]);
                    mItems.Add(it);
                }
            }
            //mPos = editor.AIGraph.CalculateGlobalPosition(editor.GlobalMousePosition);
            if(mItems.Count > 0)
                Display(editor, pos);
        }

        void CreateNode(BTEditorMenu menu, int index, object data)
        {
            var mod = data as AIModules.Module;
            if (mod != null)
            {
                menu.editor.AddNewNode(mod);
            }
        }
    }
}