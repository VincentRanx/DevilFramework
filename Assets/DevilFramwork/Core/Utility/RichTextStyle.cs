using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.Utility
{

    public struct TextTag
    {
        public int offset;
        public string tag;

        public TextTag(int offset, string tag)
        {
            this.offset = offset;
            this.tag = tag;
        }
    }

    public enum RichTextType
    {
        gui_style,
        ngui_style,
    }

    public enum TextStyle
    {
        none = 0,
        bold = 0x1,
        italic = 0x2,
        strickout = 0x4,
        underline = 0x8,
        subscript = 0x10,
        superscript = 0x20,
    }

    [System.Serializable]
    public struct RichTextStyle
    {
        [HideInInspector]
        public int start;
        [HideInInspector]
        public int end;
        public bool hasColor { get { return !string.IsNullOrEmpty(color); } }
        public string color;
        public bool hasSize { get { return size > 0; } }
        public int size;//<size=12>

        public TextStyle styles;

        //public bool bold;//<b>
        //public bool italic;//<i>
        //[HideInInspector]
        //public bool deleteLine;//<s>
        //[HideInInspector]
        //public bool underline;//<u>
        public bool hasAnyStyle { get { return styles != 0 || hasColor || hasSize; } }// hasColor || hasSize || italic || bold || underline || deleteLine; } }

        public RichTextStyle(int start, int end, string color, int size, TextStyle styles)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.size = size;
            this.styles = styles;
        }

        public RichTextStyle CopyTo(int start, int end)
        {
            RichTextStyle set = new RichTextStyle(start, end, this.color, this.size, this.styles);
            return set;
        }

        public void OverrideSet(RichTextStyle newSet)
        {
            if (newSet.hasColor)
                this.color = newSet.color;
            if (newSet.hasSize)
                this.size = newSet.size;
            this.styles |= newSet.styles;
        }

        public static bool SameStyle(RichTextStyle set1, RichTextStyle set2)
        {
            if (set1.size != set2.size)
                return false;
            else if (set1.hasColor ^ set2.hasColor)
                return false;
            else if (set1.hasColor && set1.color != set2.color)
                return false;
            else if (set1.styles != set2.styles)
                return false;
            else
                return true;
        }

        public TextTag StartTag(RichTextType type)
        {
            if (type == RichTextType.gui_style)
                return GUIStartTag;
            else if (type == RichTextType.ngui_style)
                return NGUIStartTag;
            else
                return default(TextTag);
        }

        public TextTag EndTag(RichTextType type)
        {
            if (type == RichTextType.gui_style)
                return GUIEndTag;
            else if (type == RichTextType.ngui_style)
                return NGUIEndTag;
            else
                return default(TextTag);
        }

        public TextTag GUIStartTag
        {
            get
            {
                TextTag tag;
                tag.offset = start;
                tag.tag = "";
                if (hasColor)
                    tag.tag += string.Format("<color={0}>", color);
                if (hasSize)
                    tag.tag += string.Format("<size={0}>", size);
                if ((styles & TextStyle.bold) != 0)
                    tag.tag += "<b>";
                if ((styles & TextStyle.italic) != 0)
                    tag.tag += "<i>";
                return tag;
            }
        }

        public TextTag GUIEndTag
        {
            get
            {
                TextTag tag;
                tag.offset = end;
                tag.tag = "";
                if ((styles & TextStyle.italic) != 0)
                    tag.tag += "</i>";
                if ((styles & TextStyle.bold) != 0)
                    tag.tag += "</b>";
                if (hasSize)
                    tag.tag += "</size>";
                if (hasColor)
                    tag.tag += "</color>";
                return tag;
            }
        }

        public TextTag NGUIStartTag
        {
            get
            {
                TextTag tag;
                tag.offset = start;
                tag.tag = "";
                if (hasColor)
                    tag.tag += string.Format("[{0}]", color);
                if ((styles & TextStyle.bold) != 0)
                    tag.tag += "[b]";
                if ((styles & TextStyle.italic) != 0)
                    tag.tag += "[i]";
                if ((styles & TextStyle.underline) != 0)
                    tag.tag += "[u]";
                if ((styles & TextStyle.strickout) != 0)
                    tag.tag += "[s]";
                if ((styles & TextStyle.subscript) != 0)
                    tag.tag += "[sub]";
                if ((styles & TextStyle.superscript) != 0)
                    tag.tag += "[sup]";
                return tag;
            }
        }

        public TextTag NGUIEndTag
        {
            get
            {
                TextTag tag;
                tag.offset = end;
                tag.tag = "";
                if ((styles & TextStyle.superscript) != 0)
                    tag.tag += "[/sup]";
                if ((styles & TextStyle.subscript) != 0)
                    tag.tag += "[/sub]";
                if ((styles & TextStyle.strickout) != 0)
                    tag.tag += "[/s]";
                if ((styles & TextStyle.underline) != 0)
                    tag.tag += "[/u]";
                if ((styles & TextStyle.italic) != 0)
                    tag.tag += "[/i]";
                if ((styles & TextStyle.bold) != 0)
                    tag.tag += "[/b]";
                if (hasColor)
                    tag.tag += "[-]";
                return tag;
            }
        }

        public static void AddStyle(ref LinkedList<RichTextStyle> settings, int start, int end, string color, int size, TextStyle styles)
        {
            AddStyle(ref settings, new RichTextStyle(start, end, color, size, styles), true);
        }

        public static void AddStyle(ref LinkedList<RichTextStyle> styles, RichTextStyle newSet, bool overrideOldStyle)
        {
            if (newSet.end <= newSet.start || newSet.start < 0 || !newSet.hasAnyStyle)
                return;
            if (styles == null)
                styles = new LinkedList<RichTextStyle>();
            if (styles.Count == 0)
            {
                styles.AddLast(newSet);
                return;
            }
            LinkedListNode<RichTextStyle> node = styles.First;
            while (node != null)
            {
                RichTextStyle set = node.Value;
                LinkedListNode<RichTextStyle> next = node.Next;
                if (newSet.end <= set.start)
                {
                    styles.AddBefore(node, newSet);
                    return;
                }
                else if (newSet.start >= set.end)
                {
                    node = next;
                    continue;
                }
                //newSet.end>set.Start, newSet.start<set.end
                int off0, off1;
                //左边区域
                off0 = Mathf.Min(newSet.start, set.start);
                off1 = Mathf.Max(newSet.start, set.start);
                if (off0 < off1)
                {
                    RichTextStyle insert = newSet.start < set.start ? newSet.CopyTo(off0, off1) : set.CopyTo(off0, off1);
                    styles.AddBefore(node, insert);
                    newSet.start = off1;
                    set.start = off1;
                    node.Value = set;
                }
                //公共区域
                off0 = off1;
                off1 = Mathf.Min(set.end, newSet.end);
                if (off0 < off1)
                {
                    RichTextStyle insert;
                    if (overrideOldStyle)
                    {
                        insert = set.CopyTo(off0, off1);
                        insert.OverrideSet(newSet);
                    }
                    else
                    {
                        insert = newSet.CopyTo(off0, off1);
                        insert.OverrideSet(set);
                    }
                    node.Value = insert;
                    set.start = off1;
                    newSet.start = off1;
                }
                if (newSet.start >= newSet.end)
                {
                    styles.AddAfter(node, set);
                    break;
                }
                else
                {
                    node = next;
                }
            }
            if (newSet.start < newSet.end)
                styles.AddLast(newSet);
        }

        public static void Optimization(LinkedList<RichTextStyle> settings)
        {
            if (settings == null || settings.Count < 2)
                return;
            LinkedListNode<RichTextStyle> node1 = settings.First;
            LinkedListNode<RichTextStyle> node2 = node1.Next;
            while (node1 != null && node2 != null)
            {
                RichTextStyle r1 = node1.Value;
                RichTextStyle r2 = node2.Value;
                if (r1.end == r2.start && RichTextStyle.SameStyle(r1, r2))
                {
                    r1.end = r2.end;
                    node1.Value = r1;
                    settings.Remove(node2);
                }
                else
                {
                    node1 = node2;
                }
                node2 = node1 == null ? null : node1.Next;
            }
        }

        public static string UseStyles(LinkedList<RichTextStyle> styles, string text, RichTextType type = RichTextType.gui_style, int offset = 0)
        {
            if (text == null || styles == null || styles.Count == 0)
                return text;
            int len = text.Length;
            System.Text.StringBuilder builder = new System.Text.StringBuilder(text);
            LinkedListNode<RichTextStyle> node = styles.Last;
            while (node != null)
            {
                RichTextStyle set = node.Value;
                node = node.Previous;
                if (set.start + offset >= len)
                    continue;
                TextTag tag = set.EndTag(type);
                if (!string.IsNullOrEmpty(tag.tag))
                    builder.Insert(Mathf.Clamp(tag.offset + offset, 0, len), tag.tag);
                tag = set.StartTag(type);
                if (!string.IsNullOrEmpty(tag.tag))
                    builder.Insert(Mathf.Clamp(tag.offset + offset, 0, len), tag.tag);
            }
            return builder.ToString();
        }

        static bool IsStyleTag(string tag, RichTextType type)
        {
            if (string.IsNullOrEmpty(tag))
                return false;
            if (type == RichTextType.gui_style)
            {
                if (tag == "<i>" || tag == "</i>" || tag == "<b>" || tag == "</b>")
                {
                    return true;
                }
                else if (tag.Length > 8 && tag.StartsWith("<color="))
                {
                    return false;
                }
                else if (tag.StartsWith("<size="))
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (type == RichTextType.ngui_style)
            {
                return false;
            }
            else
            {
                return false;
            }
        }
        
    }

}