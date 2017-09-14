
using DevilTeam.Utility;
using org.vr.rts;
using System.Collections.Generic;

namespace DevilTeam.Command
{

    //输入辅助
    public class RTSAssistant
    {
        public int Offset { get; private set; }
        public string KeyWord { get; private set; }
        LinkedList<string> assistants = new LinkedList<string>();
        LinkedListNode<string> current;

        public RTSAssistant(IRTSEngine engine)
        {

        }

        bool accept(string text)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(KeyWord))
                return false;
            return text.ToLower().Contains(KeyWord.ToLower());
        }

        public void Release(InputReader ireader)
        {
            if (ireader != null && HasAssistant)
            {
                ireader.SelectEnd = ireader.SelectStart;
                ireader.Insert(" ");
            }
            Close();
        }

        public void Close()
        {
            KeyWord = null;
            assistants.Clear();
            current = null;
        }

        public bool Assistant(InputReader ireader, IRTSEngine engine)
        {
            Close();
            if (ireader.SelectLength > 0 || string.IsNullOrEmpty(ireader.Text))
                return false;
            CharSequence cs = new CharSequence(ireader.Text);
            int off = cs.TempOffset;
            string keyword = null;
            while (cs.HasNextAtom)
            {
                cs.fixCharsOffset();
                int off2 = cs.TempOffset;
                if (off2 >= ireader.SelectStart)
                    return false;
                string word = cs.nextAtom();
                off2 = cs.TempOffset;

                if (off2 >= ireader.SelectStart)
                {
                    keyword = word;
                    off = off2 - keyword.Length;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                Offset = off;
                KeyWord = keyword;
                Dictionary<string, IRTSFunction> funcs = Ref.GetField(engine, "mFuncs") as Dictionary<string, IRTSFunction>;
                int flen = funcs == null ? 0 : funcs.Count;
                Dictionary<string, object> vars = Ref.GetField(engine, "mVars") as Dictionary<string, object>;
                int vlen = vars == null ? 0 : vars.Count;
                string[] atoms = new string[flen + vlen];
                if (flen > 0)
                    funcs.Keys.CopyTo(atoms, 0);
                if (vlen > 0)
                    vars.Keys.CopyTo(atoms, flen);
                for (int i = 0; i < atoms.Length; i++)
                {
                    if (i < flen)
                    {
                        int n = atoms[i].IndexOf('-');
                        if (n > 0)
                            atoms[i] = atoms[i].Substring(0, n) + '(';
                    }
                    else
                    {
                        atoms[i] = "global " + atoms[i];
                    }
                    if (!accept(atoms[i]))
                        continue;
                    assistants.AddLast(atoms[i]);
                }
                if (assistants.Count > 0)
                {
                    ireader.SelectStart = Offset + KeyWord.Length;
                    ireader.SelectEnd = Offset;
                }
                else
                {
                    KeyWord = null;
                }
                return true;
            }
            else
                return false;
        }

        public void NextAssistant(InputReader ireader)
        {
            if (ireader == null)
                return;
            string t = Next;
            if (string.IsNullOrEmpty(t))
                return;
            ireader.Insert(t);
            ireader.SelectStart = Offset + t.Length;
            ireader.SelectEnd = Offset;
        }

        public bool HasAssistant { get { return assistants.Count > 0; } }

        string Next
        {
            get
            {
                if (!HasAssistant)
                    return null;
                if (current != null)
                    current = current.Next;
                if (current == null)
                    current = assistants.First;
                return current.Value;
            }
        }
    }
}