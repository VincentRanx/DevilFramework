using org.vr.rts.util;

namespace org.vr.rts.component
{

    public class RTSTextReader
    {
        public const string SEPERATORS = " \r\n\t";
        public const string OPERATORS = "+-*/%&|~!^=<>?:;.,@";
        public const string BRACKET_OPERATORS = "{}()[]";

        public const string ESCAPE_CHARACTOR = "btrn0";
        public const string ESCAPE_VALUE = "\b\t\r\n\0";
        public const string EMPTY = "";

        private string source;
        private char[] chars;
        private bool considerOperator;
        private bool considerBracket;
        private bool considerSeperator;
        private bool considerNote;
        private bool lineEnd;

        private string seperators;
        private string operators;
        private string brackets;

        public RTSTextReader()
        {
            this.considerOperator = true;
            this.considerBracket = true;
            this.considerSeperator = true;
            this.considerNote = true;
            seperators = SEPERATORS;
            operators = OPERATORS;
            brackets = BRACKET_OPERATORS;
            tempBuffer = new System.Text.StringBuilder();
        }

        public RTSTextReader(string text)
        {
            this.considerOperator = true;
            this.considerBracket = true;
            this.considerSeperator = true;
            this.considerNote = true;
            seperators = SEPERATORS;
            operators = OPERATORS;
            brackets = BRACKET_OPERATORS;
            tempBuffer = new System.Text.StringBuilder();
            Reload(text);
        }

        public bool isConsiderOperator()
        {
            return considerOperator;
        }

        public void setConsiderOperator(bool value)
        {
            considerOperator = value;
            if (value && RTSUtil.isNullOrEmpty(operators))
                operators = OPERATORS;
        }

        public bool isConsiderBracket()
        {
            return considerBracket;
        }

        public void setConsiderBracket(bool value)
        {
            considerBracket = value;
            if (value && RTSUtil.isNullOrEmpty(brackets))
                brackets = BRACKET_OPERATORS;
        }

        public bool isConsiderSeperator()
        {
            return considerSeperator;
        }

        public void setConsiderSeperator(bool value)
        {
            considerSeperator = value;
            if (value && RTSUtil.isNullOrEmpty(seperators))
                seperators = SEPERATORS;
        }

        public bool isConsiderNote()
        {
            return considerNote;
        }

        public void setConsiderNote(bool value)
        {
            considerNote = value;
        }

        public bool IsLineEnd()
        {
            return lineEnd;
        }

        public void SetOperators(string operators)
        {
            this.considerOperator &= !RTSUtil.isNullOrEmpty(operators);
            this.operators = operators;
        }

        public void SetBrackets(string brackets)
        {
            this.considerBracket &= !RTSUtil.isNullOrEmpty(brackets);
            this.brackets = brackets;
        }

        public void SetSeperator(string seperators)
        {
            this.seperators = seperators;
            considerSeperator &= !RTSUtil.isNullOrEmpty(seperators);
        }

        public void Reload(string text)
        {
            source = text;
            if (text != null)
            {
                chars = text.ToCharArray();
            }
            else
            {
                chars = null;
            }
            Reset();
        }

        public void Reset()
        {
            tempOffset = 0;
            tempType = 0;
        }

        private int tempOffset;
        /**
         * 1:string 2:number 3:operator 4:seperator 5:bracket
         */
        private int tempType;
        private System.Text.StringBuilder tempBuffer;

        private int charType(char c)
        {
            if (c == '\"')
                return 1;
            else if (c >= '0' && c <= '9')
                return 2;
            else if (considerOperator && operators.IndexOf(c) >= 0)
                return 3;
            else if (considerBracket && brackets.IndexOf(c) >= 0)
                return 5;
            else if (considerSeperator && seperators.IndexOf(c) >= 0)
                return 4;
            else
                return 0;
        }

        private int charType(int preType, char c)
        {
            if (c == '\"')
                return 1;
            else if (preType == 2 && c == '.')
                return 2;
            else if (considerOperator && operators.IndexOf(c) >= 0)
                return 3;
            else if (considerBracket && brackets.IndexOf(c) >= 0)
                return preType == 5 ? -5 : 5;
            else if (considerSeperator && seperators.IndexOf(c) >= 0)
                return 4;
            else
                return preType == 2 ? 2 : 0;
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
                    {
                        note1 = false;
                        lineEnd = true;
                    }
                }
                else if (note2)
                {
                    if (c == '*' && tempOffset < chars.Length - 1
                            && chars[tempOffset + 1] == '/')
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

        public string nextWord(IRTSEngine lib)
        {
            int fixoff = tempOffset;
            tempOffset = fixoff + 1;
            if (fixoff >= chars.Length)
                return null;
            tempBuffer.Remove(0, tempBuffer.Length);
            char c = chars[fixoff];
            tempBuffer.Append(c);
            tempType = charType(c);
            if (tempType == 5)
            {
                return tempBuffer.ToString();
            }
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
                        tempBuffer.Append(c);
                        i++;
                        tempOffset = i + 1;
                        continue;
                    }
                    tempBuffer.Append(c);
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
                        tempBuffer.Append(c);
                        if (tempType == 3
                                && (lib == null || !lib.isKeyWord(tempBuffer
                                        .ToString())))
                        {
                            tempBuffer.Remove(tempBuffer.Length - 1, 1);
                            break;
                        }
                        tempOffset++;
                    }
                    else
                    {
                        break;
                    }
                }

            }
            return tempBuffer.ToString();
        }

        public bool hasNext()
        {
            return chars != null && tempOffset < chars.Length;
        }

        public int getTempOffset()
        {
            return tempOffset;
        }

        public int getSourceLen()
        {
            if (chars == null)
                return 0;
            else
                return chars.Length;
        }

        public string getSource()
        {
            return source;
        }

        public string getSourceAt(int start, int end)
        {
            if (chars == null)
                return null;
            else
                return source.Substring(start, end - start);
        }
    }
}
