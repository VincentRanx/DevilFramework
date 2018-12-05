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

        public void AddItem(Item item)
        {
            mItems.Add(item);
            isDirty = true;
        }

        public void AddItem(GUIContent content, OnItemSelected selector, object data = null)
        {
            mItems.Add(NewItem(content, selector, data));
            isDirty = true;
        }

        public void AddItem(string txt, OnItemSelected selector, object data = null)
        {
            mItems.Add(NewItem(txt, selector, data));
            isDirty = true;
        }

        public void AddItems(params Item[] items)
        {
            mItems.AddRange(items);
            isDirty = true;
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
            var item = mItems[selected];
            if (item.selectedCallback != null)
            {
                item.selectedCallback(this, selected, item.data);
            }
        }

    }
}