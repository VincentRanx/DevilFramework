using UnityEngine;
namespace DevilTeam.Utility
{
    public class CharSequence
    {

        public const string SEPERATORS = " \r\n\t";
        public const string OPERATORS = "+-*/%&|~!^=<>?:;,@";
        public const string BRACKET_OPERATORS = "{}()[]";

        public const string ESCAPE_CHARACTOR = "btrn0";
        public const string ESCAPE_VALUE = "\b\t\r\n\0";

        char[] chars;
        bool started;
        int fragStart;
        int fragEnd;
        bool considerOperator;
        bool considerBracket;
        bool considerSeperator;
        bool considerNote;

        string seperators;
        string operators;
        string brackets;

        public CharSequence()
        {
            this.considerOperator = true;
            this.considerBracket = true;
            this.considerSeperator = true;
            this.considerNote = true;
            seperators = SEPERATORS;
            operators = OPERATORS;
            brackets = BRACKET_OPERATORS;
        }

        public CharSequence(string text)
        {
            this.considerOperator = true;
            this.considerBracket = true;
            this.considerSeperator = true;
            this.considerNote = true;
            seperators = SEPERATORS;
            operators = OPERATORS;
            brackets = BRACKET_OPERATORS;
            Reload(text);
        }

        public bool isConsiderOperator() { return considerOperator; }
        public void setConsiderOperator(bool value)
        {
            considerOperator = value;
            if (value && string.IsNullOrEmpty(operators))
                operators = OPERATORS;
        }
        public bool ConsiderOperator
        {
            get { return isConsiderOperator(); }
            set { setConsiderOperator(value); }
        }

        public bool isConsiderBracket() { return considerBracket; }
        public void setConsiderBracket(bool value)
        {
            considerBracket = value;
            if (value && string.IsNullOrEmpty(brackets))
                brackets = BRACKET_OPERATORS;
        }
        public bool ConsiderBracket
        {
            get { return isConsiderBracket(); }
            set { setConsiderBracket(value); }
        }

        public bool isConsiderSeperator() { return considerSeperator; }
        public void setConsiderSeperator(bool value)
        {
            considerSeperator = value;
            if (value && string.IsNullOrEmpty(seperators))
                seperators = SEPERATORS;
        }
        public bool ConsiderSeperator
        {
            get { return isConsiderSeperator(); }
            set { setConsiderSeperator(value); }
        }

        public bool isConsiderNote() { return considerNote; }
        public void setConsiderNote(bool value) { considerNote = value; }
        public bool ConsiderNote
        {
            get { return isConsiderNote(); }
            set { setConsiderNote(value); }
        }

        public void SetOperators(string operators)
        {
            this.considerOperator &= !string.IsNullOrEmpty(operators);
            this.operators = operators;
        }

        public void SetBrackets(string brackets)
        {
            this.considerBracket &= !string.IsNullOrEmpty(brackets);
            this.brackets = brackets;
        }

        public void SetSeperator(string seperators)
        {
            this.seperators = seperators;
            considerSeperator &= !string.IsNullOrEmpty(seperators);
        }

        public void Reload(string text)
        {
            chars = text.ToCharArray();
            Reset();
        }

        public void Reset()
        {
            tempOffset = 0;
            tempType = 0;
            fragStart = 0;
            fragEnd = 0;
            started = false;
        }

        int tempOffset;
        int tempType;

        int charType(char c)
        {
            if (c == '\"')
                return 1;
            else if (considerOperator && operators.IndexOf(c) >= 0)
                return 3;
            else if (considerBracket && brackets.IndexOf(c) >= 0)
                return 5;
            else if (considerSeperator && seperators.IndexOf(c) >= 0)
                return 4;
            else
                return 0;
        }

        int charType(int preType, char c)
        {
            if (c == '\"')
                return 1;
            else if (considerOperator && operators.IndexOf(c) >= 0)
                return 3;
            else if (considerBracket && brackets.IndexOf(c) >= 0)
                return preType == 5 ? -5 : 5;
            else if (considerSeperator && seperators.IndexOf(c) >= 0)
                return 4;
            else
                return 0;
        }

        public void fixCharsOffset()
        {
            bool note1 = false; // 
            bool note2 = false; /* */
            for (; tempOffset < chars.Length; tempOffset++)
            {
                char c = chars[tempOffset];
                if (note1)
                {
                    if (c == '\n')
                        note1 = false;
                }
                else if (note2)
                {
                    if (c == '*' && tempOffset < chars.Length - 1 && chars[tempOffset + 1] == '/')
                    {
                        tempOffset++;
                        note2 = false;
                    }
                }
                else if (!considerSeperator || seperators.IndexOf(c) < 0)
                {
                    if (considerNote && c == '/' && tempOffset < chars.Length - 1)
                    {
                        c = chars[tempOffset + 1];
                        if (c == '/')
                        {
                            note1 = true;
                            tempOffset++;
                        }
                        else if (c == '*')
                        {
                            note2 = true;
                            tempOffset++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public string fixedNextAtom()
        {
            fixCharsOffset();
            string s = nextAtom();
            if (s != null)
            {
                int len = s.Length;
                if (len > 0 && s[0] == '\"')
                {
                    len--;
                    if (len > 0 && s[len] == '\"')
                        len--;
                    s = s.Substring(1, len);
                }

            }
            return s;
        }

        public string nextAtom()
        {
            int fixoff = tempOffset;
            tempOffset = fixoff + 1;
            if (fixoff >= chars.Length)
                return null;
            string s;
            char c = chars[fixoff];
            s = "" + c;
            tempType = charType(c);
            for (int i = fixoff + 1; i < chars.Length; i++)
            {
                c = chars[i];
                tempOffset = i;
                if (tempType == 1)
                {
                    if (c == '\\' && i < chars.Length - 1)
                    {
                        c = chars[i + 1];
                        int trans = ESCAPE_CHARACTOR.IndexOf(c);
                        if (trans >= 0)
                        {
                            c = ESCAPE_VALUE[trans];
                        }
                        s += c;
                        i++;
                        tempOffset = i + 1;
                        continue;
                    }
                    s += c;
                    tempOffset++;
                    if (c == '\"')
                    {
                        break;
                    }
                }
                else
                {
                    int type = charType(tempType, c);
                    if (tempType == type)
                    {
                        s += c;
                        tempOffset++;
                    }
                    else
                    {
                        break;
                    }
                }

            }
            return s;
        }

        public bool HasNextAtom { get { return tempOffset < chars.Length; } }

        public int getTempOffset() { return tempOffset; }
        public void setTempOffset(int value) { tempOffset = value; }
        public int TempOffset { get { return getTempOffset(); } set { setTempOffset(value); } }

        public void StartFragment()
        {
            fixCharsOffset();
            started = true;
            fragStart = tempOffset;
            fragEnd = tempOffset;
        }

        public string Fragment
        {
            get
            {
                if (started)
                    fragEnd = Mathf.Max(fragEnd, tempOffset);
                fragEnd = Mathf.Min(fragEnd, chars.Length);
                if (fragStart >= fragEnd)
                    return "";
                return new string(chars, fragStart, fragEnd - fragStart);
            }
        }
        public void EndFragment()
        {
            fragEnd = tempOffset;
            started = false;
            fixCharsOffset();
        }
    }
}