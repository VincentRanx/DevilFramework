using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class BTEditorMenu
    {
        public delegate void OnItemSelected(BTEditorMenu menu, int index, object itemData);

        public class Item
        {
            public object data;
            public GUIContent content;
            public OnItemSelected selectedCallback;
        }

        public static Item NewItem(string txt, OnItemSelected selector, object data = null)
        {
            var it = new Item();
            it.content = new GUIContent(txt);
            it.selectedCallback = selector;
            it.data = data;
            return it;
        }

        public static Item NewItem(GUIContent content, OnItemSelected selector, object data = null)
        {
            var it = new Item();
            it.content = content;
            it.selectedCallback = selector;
            it.data = data;
            return it;
        }

        public BehaviourTreeEditor editor { get; private set; }
        protected List<Item> mItems = new List<Item>();
        protected GUIContent[] mContents;
        bool isDirty;

        public bool IsExist(string item)
        {
            return GlobalUtil.FindIndex(mItems, (x) => x.content.text == item) != -1;
        }

        public void AddItem(Item item)
        {
            mItems.Add(item);
            isDirty = true;
        }

        public void AddItem(string content, OnItemSelected selector, object data = null)
        {
            var index = GlobalUtil.FindIndex(mItems, (x) => x.content.text == content);
            if (index == -1)
            {
                mItems.Add(NewItem(content, selector, data));
                isDirty = true;
            }
            else
            {
                var item = mItems[index];
                item.content.text = content;
                item.selectedCallback = selector;
                item.data = data;
            }
        }
        
        public void AddItems(params Item[] items)
        {
            mItems.AddRange(items);
            isDirty = true;
        }

        public void RemoveItem(string name)
        {
            var id = GlobalUtil.FindIndex(mItems, (x) => x.content.text == name);
            if(id != -1)
            {
                mItems.RemoveAt(id);
                isDirty = true;
            }
        }

        public void RemoveRange(string startName, int offset)
        {
            int i0;
            var index = string.IsNullOrEmpty(startName) ? 0 : GlobalUtil.FindIndex(mItems, (x) => x.content.text == startName);
            i0 = index + offset;
            if (i0 < mItems.Count - 1 && i0 >= 0)
            {
                mItems.RemoveRange(i0, mItems.Count - i0);
                isDirty = true;
            }
        }

        public bool RenameItem(string oldName, string newName)
        {
            var index = GlobalUtil.FindIndex(mItems, (x) => x.content.text == oldName);
            if(index >= 0)
            {
                mItems[index].content.text = newName;
                return true;
            }
            return false;
        }

        public void SetItems(ICollection<Item> items)
        {
            mItems = new List<Item>();
            mItems.AddRange(items);
            mContents = new GUIContent[mItems.Count];
            for (int i = 0; i < mContents.Length; i++)
            {
                mContents[i] = mItems[i].content;
            }
            isDirty = true;
        }

        public void Display(BehaviourTreeEditor editor, Rect pos)
        {
            if (isDirty)
            {
                isDirty = false;
                mContents = new GUIContent[mItems.Count];
                for (int i = 0; i < mContents.Length; i++)
                {
                    mContents[i] = mItems[i].content;
                }
            }
            this.editor = editor;
            EditorUtility.DisplayCustomMenu(pos, mContents, -1, OnSelected, null);

        }

        void OnSelected(object userData, string[] options, int selected)
        {
            if (isDirty)
                return;
            var item = mItems[selected];
            if (item.selectedCallback != null)
            {
                item.selectedCallback(this, selected, item.data);
            }
        }

    }
}