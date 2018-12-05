using Devil.Utility;
using UnityEngine;

namespace Devil.ContentProvider
{
    [System.Serializable]
    public struct TextRef : System.IEquatable<TextRef>
    {
        public int id;
        public string txt;

        public TextRef(int id)
        {
            this.id = id;
            this.txt = "";
        }

        public TextRef(string txt)
        {
            this.id = string.IsNullOrEmpty(txt) ? 0 : StringUtil.ToHash(txt);
            this.txt = txt;
        }

        public TextRes Res { get { return id == 0 ? null : TableSet<TextRes>.Instance[id]; } }

        public static implicit operator string(TextRef txt)
        {
            var v = TableSet<TextRes>.Instance[txt.id];
            return v == null ? txt.txt : v.text;
        }

        public override string ToString()
        {
            var v = TableSet<TextRes>.Instance[id];
            return v == null ? txt : v.text;
        }

        public bool Equals(TextRef other)
        {
            return this.id == other.id;
        }
    }
}