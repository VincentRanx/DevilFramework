

using Devil.Utility;
using System.IO;
using UnityEngine;
/**
* ?X.isTrue @X.DoSomething(arg1:value arg2:value2):
* contents
*/
namespace Devil.GamePlay
{
    
    public class SitcomFile
    {
        public struct Keyword
        {
            public int id;
            public int line;
            public int lineOffset;
            public int offset;
            public int length;
            public KeywordType type;
            public SitcomFile sitcom;

            public string text { get { return sitcom == null ? "" : new string(sitcom.mSequence, offset, length); } }
            public int linePos { get { return offset - lineOffset; } }

            public override string ToString()
            {
                if (sitcom == null || sitcom.mSequence == null || sitcom.mSequence.Length == 0)
                    return "[Empty File]";
                return string.Format("\"{0}\" \t@file: \"{1}\" line[{2}:{3}]", new string(sitcom.mSequence, offset, length), sitcom.mFile, line, linePos);
            }
        }

        public enum CharType
        {
            Text,
            Operator,
            Word,
            Bracket,
        }

        public static bool IsSpace(char c)
        {
            return c == ' '  || c == '\t' || c == '\r' || c == '\n';
        }

        public static bool IsOperator(char c)
        {
            return c == '@' || c == '?' || c == '.' || c == ':' || c == '+' || c == '-' || c == '*' || c == '/'
                || c == '？' || c == '。' || c == '：' || c == '》';
        }

        public static bool IsBracket(char c)
        {
            return c == '(' || c == ')' || c == '（' || c == '）';
        }

        public static CharType CTP(char c)
        {
            if (c == '\"' || c == '“')
                return CharType.Text;
            else if (IsBracket(c))
                return CharType.Bracket;
            else if (IsOperator(c))
                return CharType.Operator;
            else
                return CharType.Word;
        }
       
        int mOffset; // 当前所在位置

        int mLine; // 当前所在行
        int mLineOffset; // 行开始位置

        int mSkipLines; // 相对上文换行数

        bool mEof;
        char[] mSequence;

        string mFile;
        Keyword mInfo;

        public bool Eof { get { return mEof; } }
        public string File { get { return mFile; } }
        public int PresentLine { get { return mLine; } }
        public Keyword keyword { get { return mInfo; } }
        public int keywordId { get { return mInfo.id; } }
        public KeywordType keywordType { get { return mInfo.type; } }

#if UNITY_EDITOR
        TextAsset mAsset;
#endif

        public SitcomFile()
        {
            mEof = true;
            mInfo.sitcom = this;
        }

        public void Load(string text)
        {
            mFile = "[runtime]";
            var buf = StringUtil.GetBuilder(text);
            buf.Replace("\r", "");
            mSequence = new char[buf.Length];
            buf.CopyTo(0, mSequence, 0, mSequence.Length);
            Load(mSequence);
            StringUtil.Release(buf);
        }
        
        public void Load(TextAsset asset)
        {
#if UNITY_EDITOR
            mAsset = asset;
            mFile = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(mFile))
                mFile = asset.name;
#else
            mFile = asset.name;
#endif
            var buf = StringUtil.GetBuilder(asset.text);
            buf.Replace("\r", "");
            mSequence = new char[buf.Length];
            buf.CopyTo(0, mSequence, 0, mSequence.Length);
            Load(mSequence);
            StringUtil.Release(buf);
        }

        public void Load(string fileName, Stream stream)
        {
            mFile = fileName;
            using (StreamReader reader = new StreamReader(stream))
            {
                var str = reader.ReadToEnd();
                Load(str.ToCharArray());
                reader.Close();
            }
        }

        void Load(char[] sequence)
        {
            mSequence = sequence;
            mLine = 0;
            mLineOffset = 0;
            mEof = sequence == null || sequence.Length == 0;
            
        }
        
        void SkipToNext()
        {
            char c;
            int len = mSequence.Length;
            while (mOffset < len)
            {
                c = mSequence[mOffset];
                if (c == '\n')
                {
                    mSkipLines++;
                    mLineOffset = ++mOffset;
                    mLine++;
                }
                else if (c != ' ' && c != '\r' && c != '\t')
                {
                    break;
                }
                else
                {
                    mOffset++;
                }
            }
            mEof = mOffset >= len;
        }

        public virtual bool BeginRead()
        {
#if UNITY_EDITOR
            if (mAsset != null)
                Load(mAsset);
#endif
            mEof = mSequence == null || mSequence.Length == 0;
            mLine = 1;
            mLineOffset = 0;
            mOffset = 0;
            mSkipLines = 1;
            mInfo.length = 0;
            mInfo.line = 1;
            mInfo.offset = 0;
            mInfo.lineOffset = 0;
            return !mEof;
        }

        public bool NextMark(char mark)
        {
            if (mEof)
                return false;
            char c;
            if (mSkipLines == 0)
            {
                while (mOffset < mSequence.Length)
                {
                    c = mSequence[mOffset++];
                    if (c == '\n')
                    {
                        mLine++;
                        mLineOffset = mOffset;
                        break;
                    }
                }
            }
            bool isEmpty = true;
            mSkipLines = 0;
            while (mOffset < mSequence.Length)
            {
                c = mSequence[mOffset];
                if (c == '\n')
                {
                    mLine++;
                    mLineOffset = ++mOffset;
                    isEmpty = true;
                    continue;
                }
                else if (c == mark && isEmpty)
                {
                    mInfo.type = KeywordType.Cmd;
                    mInfo.line = mLine;
                    mInfo.lineOffset = mLineOffset;
                    mInfo.offset = mOffset++;
                    mInfo.length = 1;
                    mInfo.id = 0;
                    return true;
                }
                else if (!IsSpace(c))
                {
                    isEmpty = false;
                }
                mOffset++;
            }
            mEof = true;
            return false;
        }
        
        public bool NextCmd()
        {
            return NextMark('@');
        }

        // 该行剩余文本
        public bool NextKeywords()
        {
            if (mEof)
                return false;
            SkipToNext();
            if (mSkipLines > 0)
                return false;
            int line = mLine;
            int linePos = mLineOffset;
            int offset = mOffset++;

            mInfo.type = KeywordType.Keyword;
            mInfo.line = line;
            mInfo.lineOffset = linePos;
            mInfo.offset = offset;
            char c;
            while(mOffset < mSequence.Length)
            {
                c = mSequence[mOffset++];
                if(c == '\n')
                {
                    mSkipLines++;
                    mLine++;
                    mLineOffset = mOffset;
                    mInfo.length = mOffset - 1 - offset;
                    mInfo.id = StringUtil.IgnoreCaseToHash(mSequence, mInfo.offset, mInfo.length);
                    return true;
                }
            }
            mInfo.length = mOffset - offset;
            mInfo.id = StringUtil.IgnoreCaseToHash(mSequence, mInfo.offset, mInfo.length);
            return true;
        }

        public bool NextKeyword()
        {
            if (mEof)
                return false;
            SkipToNext();
            if (mSkipLines > 0)
                return false;
            char c = mSequence[mOffset];
            CharType tp = CTP(c);
            int line = mLine;
            int linePos = mLineOffset;
            int offset;
            if(tp == CharType.Text)
            {
                offset = ++mOffset;
                while (mOffset < mSequence.Length)
                {
                    c = mSequence[mOffset];
                    if (c == '\"' || c == '”')
                    {
                        mSkipLines = 0;
                        mInfo.type = KeywordType.Content;
                        mInfo.line = line;
                        mInfo.lineOffset = linePos;
                        mInfo.offset = offset;
                        mInfo.length = mOffset++ - offset;
                        mInfo.id = StringUtil.IgnoreCaseToHash(mSequence, mInfo.offset, mInfo.length);
                        mEof = mOffset >= mSequence.Length;
                        return true;
                    }
                    mOffset++;
                }
                mEof = mOffset >= mSequence.Length;
                return false;
            }
            else if(tp == CharType.Bracket)
            {
                offset = mOffset++;
                mInfo.type = KeywordType.Operator;
                mInfo.line = line;
                mInfo.lineOffset = linePos;
                mInfo.offset = offset;
                mInfo.length = 1;
                mInfo.id = StringUtil.IgnoreCaseToHash(mSequence, mInfo.offset, mInfo.length);
                mEof = mOffset >= mSequence.Length;
                return true;
            }
            else
            {
                offset = mOffset++;
                while (mOffset < mSequence.Length)
                {
                    c = mSequence[mOffset];
                    if(IsSpace(c) || tp != CTP(c))
                    {
                        break;
                    }
                    mOffset++;
                }
                mSkipLines = 0;
                mInfo.type = tp == CharType.Operator ? KeywordType.Operator : KeywordType.Keyword;
                mInfo.line = line;
                mInfo.lineOffset = linePos;
                mInfo.offset = offset;
                mInfo.length = mOffset - offset;
                mInfo.id = StringUtil.IgnoreCaseToHash(mSequence, mInfo.offset, mInfo.length);
                mEof = mOffset >= mSequence.Length;
                return true;
            }
        }

        public bool NextContent()
        {
            if (mEof || mSkipLines > 1)
                return false;
            char c;
            mInfo.line = mLine;
            mInfo.lineOffset = mLineOffset;
            mInfo.type = KeywordType.Content;
            mInfo.offset = mLineOffset;
            mInfo.id = 0;
            int end = mOffset;
            mSkipLines = 0;
            bool isEmptyLine = true;
            while (mOffset < mSequence.Length)
            {
                c = mSequence[mOffset];
                if (c == '\n')
                {
                    if (isEmptyLine )
                    {
                        mSkipLines++;
                        mLine++;
                        mLineOffset = ++mOffset;
                        break;
                    }
                    end = mOffset;
                    mLine++;
                    mLineOffset = ++mOffset;
                    isEmptyLine = true;
                    continue;
                }
                else if (c != ' ' && c != '\t' && c != '\r')
                {
                    isEmptyLine = false;
                    end = ++mOffset;
                    continue;
                }
                mOffset++;
            }
            mInfo.length = end - mInfo.offset;
            mEof = mOffset >= mSequence.Length;
            return true;
        }
    }
}