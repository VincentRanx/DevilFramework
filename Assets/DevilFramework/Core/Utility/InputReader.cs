using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.Utility
{
    [System.Serializable]
    public class InputReader
    {
        static public string clipboard
        {
            get
            {
                TextEditor te = new TextEditor();
                te.Paste();
                return te.text;
            }
            set
            {
                if (value == null)
                    return;
                TextEditor te = new TextEditor();
                te.text = value;
                te.OnFocus();
                te.Copy();
            }
        }
        string text;
        bool dirty = true;
        public bool IsDirty { get { return dirty; } }
        public void Use() { dirty = false; }
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    selectStart = 0;
                    selectEnd = text == null ? 0 : text.Length;
                    dirty = true;
                }
            }
        }
        public string SelectionText
        {
            get
            {
                int len = SelectLength;
                if (len > 0)
                    return text.Substring(SelectOffset, len);
                else
                    return null;
            }
        }
        public int Length { get { return text == null ? 0 : text.Length; } }
        int selectStart;
        public int SelectStart
        {
            get { return selectStart; }
            set
            {
                int v = Mathf.Clamp(value, 0, Length);
                if (selectStart == v)
                    return;
                dirty = true;
                selectStart = v;
            }
        }
        public int SelectOffset { get { return Mathf.Min(selectStart, selectEnd); } }
        int selectEnd;
        public int SelectEnd
        {
            get { return selectEnd; }
            set
            {
                int v = Mathf.Clamp(value, 0, Length);
                if (selectEnd == v)
                    return;
                dirty = true;
                selectEnd = v;
            }
        }
        public int SelectLength { get { return Mathf.Abs(selectStart - selectEnd); } }

        string inputStr;

        public void Insert(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return;
            dirty = true;
            if (string.IsNullOrEmpty(text))
            {
                text = txt;
                selectStart = text.Length;
                selectEnd = selectStart;
                return;
            }

            string t = text;
            int off = SelectOffset;
            int len = SelectLength;
            if (selectEnd != selectStart)
            {
                t = text.Remove(off, Mathf.Min(text.Length - off, len));
            }
            text = t.Insert(off, txt);
            selectStart = off + txt.Length;
            selectEnd = selectStart;
        }

        public void Backspace()
        {
            dirty = true;
            int off = SelectOffset;
            int len = SelectLength;
            if (len > 0)
            {
                text = text.Remove(off, len);
                selectStart = off;
                selectEnd = selectStart;
            }
            else if (selectStart > 0)
            {
                text = text.Remove(--selectStart, 1);
                selectEnd = selectStart;
            }
        }

        public int[] GetLineOffsets(int offset, out int lineIndex)
        {
            List<int> num = new List<int>();
            num.Add(0);
            lineIndex = 0;
            if (text != null)
            {
                int len = text.Length;
                for (int i = 0; i < len; i++)
                {
                    if (text[i] == '\n')
                    {
                        num.Add(i + 1);
                    }
                    if (offset > i)
                        lineIndex = num.Count - 1;
                }
            }
            return num.ToArray();
        }

        public int LineStart
        {
            get
            {
                int off = selectStart;
                for (int i = off - 1; i >= 0; i--)
                {
                    if (text[i] == '\n')
                        break;
                    off = i;
                }
                return off;
            }
        }

        public int LineEnd
        {
            get
            {
                int end = selectStart;
                int len = text == null ? 0 : text.Length;
                for (int i = selectStart; i < len; i++)
                {
                    end = i + 1;
                    if (text[i] == '\n')
                        break;
                }
                return end;
            }
        }

        public int FixedLineEnd
        {
            get
            {
                int end = selectStart;
                int len = text == null ? 0 : text.Length;
                for (int i = selectStart; i < len; i++)
                {
                    end = i + 1;
                    if (text[i] == '\n')
                    {
                        end--;
                        break;
                    }
                }
                return end;
            }
        }

        public void SelectLine()
        {
            if (text == null)
                return;
            int off = LineStart;
            int end = LineEnd;
            dirty = end != selectEnd || off != selectStart;
            selectStart = off;
            selectEnd = end;
        }

        public int MoveSelectorLine(int nextLine, bool shift)
        {
            if (nextLine == 0 || text == null)
                return 0;
            int line;
            int[] arr = GetLineOffsets(selectStart, out line);
            int newLine = Mathf.Clamp(line + nextLine, 0, arr.Length - 1);
            if (newLine == line)
                return 0;
            int newOff = arr[newLine] + (selectStart - arr[line]);
            int min = newLine == 0 ? 0 : arr[newLine];
            int max = newLine < arr.Length - 1 ? arr[newLine + 1] : text.Length;
            if (min == max || newOff < min)
                newOff = min;
            else if (newOff >= max)
                newOff = max - 1;
            return MoveSelector(newOff - selectStart, shift);
        }

        public int MoveSelector(int next, bool shift)
        {
            if (next == 0 && selectStart == selectEnd)
                return 0;
            int len = text == null ? 0 : text.Length;
            dirty = true;
            int start = selectStart + next;
            if (start < 0)
            {
                start = 0;
            }
            else if (start > len)
            {
                start = len;
            }
            int ret = start - selectStart;
            selectStart = start;
            if (!shift)
                selectEnd = selectStart;
            return ret;
        }

    }

}