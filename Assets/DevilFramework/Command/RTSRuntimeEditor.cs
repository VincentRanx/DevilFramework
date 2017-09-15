using UnityEngine;
using System.Collections.Generic;
using org.vr.rts;

using org.vr.rts.modify;
using org.vr.rts.util;
using org.vr.rts.component;
using org.vr.rts.advance;
using System.Text;
using DevilTeam.Utility;
using org.vr.rts.unity;

#if UNITY_EDITOR
using System.Reflection;
using System.IO;
using UnityEditor;
#endif

namespace DevilTeam.Command
{

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class RTSPluginAttribute : System.Attribute
    {
        public string Doc { get; set; }
        public string Name { get; set; }
        public string Define { get; set; }
        public int ArgCount { get; set; }

        public RTSPluginAttribute() { }

    }

    public class RTSRuntimeEditor : RTSUnityRuntime
    {
        #region static methods
        static RTSRuntimeEditor _default;
        public static RTSRuntimeEditor DefaultCMD { get { return _default; } }
        static RTSRuntimeEditor _current;
        public static RTSRuntimeEditor CurrentCMD { get { return _current; } }

        public static string LoadFile(string file, bool relativePath)
        {
#if UNITY_EDITOR
            try
            {
                string path = file;
                if (relativePath)
                    path = Directory.GetCurrentDirectory() + "/" + path;
                StreamReader sr = File.OpenText(path);
                if (sr == null)
                {
                    return "";
                }
                string s = "";
                string t = null;
                do
                {
                    t = sr.ReadLine();
                    if (!string.IsNullOrEmpty(t))
                        s += t + "\n";
                } while (t != null);
                sr.Close();
                return s;
            }
            catch (System.Exception e)
            {
                if (_current)
                {
                    _current.PutLog(Level.error, e.ToString(), false);
                }
                return "";
            }
#else
        return "";
#endif

        }

        public static string LoadTextAsset(string name, string[] folders, out string result)
        {
#if UNITY_EDITOR
            string[] ss = AssetDatabase.FindAssets(name + " t:TextAsset", folders);
            if (ss.Length != 1)
            {
                result = null;
                return null;
            }
            string s = AssetDatabase.GUIDToAssetPath(ss[0]);
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(s);
            result = asset.text;
            return s;
#else
        result = null;
        return null;
#endif
        }

        #endregion

        public enum Level
        {
            error,
            warning,
            debug,
            info,
        }

        InputReader ireader;

        Rect mConsoleRect;

        public bool m_OpenWhenStart;
        public bool m_OpenWhenPrint;
        public int maxLog = 100;
        public int saveCmdCount = 20;

        public bool ctrlExecute;
        public bool useStyle = true;

        public RichTextStyle errorStyle = new RichTextStyle(0, 0, "#ff4040ff", 0, 0);//错误日志样式
        public RichTextStyle warningStyle = new RichTextStyle(0, 0, "yellow", 0, TextStyle.bold);//警告日志样式
        public RichTextStyle debugStyle = new RichTextStyle(0, 0, "#f0f0f0ff", 0, TextStyle.italic);//调试日志样式
        public RichTextStyle infoStyle = new RichTextStyle(0, 0, "#40ff40ff", 0, 0);//正常日志样式

        public RichTextStyle selectionStyle = new RichTextStyle(0, 0, "orange", 0, TextStyle.bold);//选中文本样式
        public RichTextStyle keyWordStyle = new RichTextStyle(0, 0, "#80ff80ff", 0, 0);//关键字和符号样式
        public RichTextStyle valueStyle = new RichTextStyle(0, 0, "#ffa0f0ff", 0, TextStyle.italic);//值样式
        public RichTextStyle variableStyle = new RichTextStyle(0, 0, "#80f0ffff", 0, TextStyle.italic);//变量样式
        public RichTextStyle normalStyle = new RichTextStyle(0, 0, "#f0f0f0ff", 0, 0);//正常样式
        public RichTextStyle noteStyle = new RichTextStyle(0, 0, "#4040f0ff", 0, TextStyle.italic);//注释样式
        public RichTextStyle readStyle = new RichTextStyle(0, 0, "#ffa0f0ff", 24, TextStyle.bold);//读取值样式

        RTSAssistant assistant;

        string[] pColors;
        bool mOpened;
        RTSWaitForValueR<string> mReader;
        string mReaderName = "";
        public bool Open
        {
            get { return mOpened; }
            set
            {
                mOpened = value;
                RTSRuntimeEditor old = _current;
                if (value)
                {
                    _current = this;
                    _default = this;
                }
                else if (_current == this)
                {
                    _current = null;
                }
                if (old && old != _current)
                    old.Open = false;
            }
        }

        Dictionary<System.Type, HashSet<RTSPluginAttribute>> mFunctions = new Dictionary<System.Type, HashSet<RTSPluginAttribute>>();
        Dictionary<string, RTSPluginAttribute> mCustomDocs = new Dictionary<string, RTSPluginAttribute>();
       
        protected override void Awake()
        {
            base.Awake();

            ireader = new InputReader();
            pColors = new string[] { "orange", "cyan", "lightblue", "teal", "green", "olive" };

            _default = this;

            LoadFunction(this);
            assistant = new RTSAssistant(mEngine);
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            if (m_OpenWhenStart)
                Open = true;
        }

        protected override bool OnRuntimeTick(RTSThread t)
        {
            bool ret = base.OnRuntimeTick(t);
            if (t.isFinished())
            {
                object o = t.getOutput();
                if (o != RTSVoid.VOID)
                    PutLog(Level.info, string.Format("result: {0} {1} ", o == null ? "" : mEngine.getRTSType(o.GetType()).typeName(), o == null ? "null" : o.ToString()));
            }
            return ret;
        }

        public IRTSLog logDebug(object msg)
        {
            PutLog(Level.debug, msg == null ? "" : msg.ToString());
            return base.logInfo(msg);
        }

        public override IRTSLog logInfo(object msg)
        {
            PutLog(Level.info, msg == null ? "" : msg.ToString());
            return base.logInfo(msg);
        }

        public override IRTSLog logError(object msg)
        {
            PutLog(Level.error, msg == null ? "" : msg.ToString());
            return base.logError(msg);
        }

        public override IRTSLog logWarning(object msg)
        {
            PutLog(Level.warning, msg == null ? "" : msg.ToString());
            return base.logWarning(msg);
        }

        #region GUI

        string mText;
        float mTimer;
        bool mCursorLight;
        RTSTextReader wordReard = new RTSTextReader();

        void formatInput()
        {
            mTimer -= Time.unscaledDeltaTime;
            if (mTimer < 0)
                mTimer = 1.6f;
            bool newCur = mTimer <= 0.8f;
            if (mText != null && !ireader.IsDirty && (newCur == mCursorLight))
                return;
            mCursorLight = newCur;
            mText = ireader.Text;
            if (mText == null)
                mText = "";

            string curColor = mCursorLight ? "#ffffffff" : "#808080ff";
            LinkedList<RichTextStyle> styles = null;
            int off;
            if (useStyle)
            {
                if (mReader == null)
                {
                    wordReard.Reload(mText);
                    off = wordReard.getTempOffset();
                    string atomStr;
                    while (wordReard.hasNext())
                    {
                        wordReard.fixCharsOffset();
                        noteStyle.start = off;
                        noteStyle.end = wordReard.getTempOffset();
                        RichTextStyle.AddStyle(ref styles, noteStyle, true);
                        off = wordReard.getTempOffset();
                        atomStr = wordReard.nextWord(mEngine);
                        if (atomStr == null)
                            break;
                        RichTextStyle set;
                        if (RTSConverter.getCompileValue(atomStr) != null || atomStr.StartsWith("\""))
                        {
                            set = valueStyle;
                        }
                        else if (mEngine.containsVar(atomStr))
                        {
                            set = variableStyle;
                        }
                        else
                        {
                            set = mEngine.isKeyWord(atomStr) ? keyWordStyle : normalStyle;
                        }
                        set.start = off;
                        set.end = wordReard.getTempOffset();
                        RichTextStyle.AddStyle(ref styles, set, true);
                        off = wordReard.getTempOffset();
                    }
                }
                else
                {
                    readStyle.start = 0;
                    readStyle.end = mText.Length;
                    RichTextStyle.AddStyle(ref styles, readStyle, true);
                }
            }
            off = Mathf.Min(ireader.SelectStart, ireader.SelectEnd);
            int end = Mathf.Max(ireader.SelectStart, ireader.SelectEnd);
            selectionStyle.start = off;
            selectionStyle.end = end;
            RichTextStyle.AddStyle(ref styles, selectionStyle, true);
            int curIndex = ireader.SelectStart;
            if (curIndex == ireader.Length)
            {
                mText += "_";
            }
            else
            {
                char c = mText[curIndex];
                if (c == ' ')
                {
                    mText = mText.Remove(curIndex, 1);
                    mText = mText.Insert(curIndex, "_");
                }
                else if (c == '\n')
                {
                    mText = mText.Insert(curIndex, "_");
                }
            }

            RichTextStyle.AddStyle(ref styles, new RichTextStyle(curIndex, curIndex + 1, curColor, mReader == null ? 0 : readStyle.size, 0), true);
            RichTextStyle.Optimization(styles);

            mText = RichTextStyle.UseStyles(styles, mText, RichTextType.gui_style);
            if (mReader != null)
                mText = mReaderName + "$" + mText;
        }

        Vector2 lastScroll;
        Vector2 scroll;
        Rect lastRect;
        string inputStr;
        string compositionStr;
        LinkedList<string> logs = new LinkedList<string>();
        List<string> cmds = new List<string>();
        int currentCmd;

        void PutCmd(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return;
            cmds.Remove(cmd);
            cmds.Add(cmd);
            while (cmds.Count > saveCmdCount)
                cmds.RemoveAt(0);
            currentCmd = cmds.Count - 1;
        }

        bool newLog;
        public void PutLog(Level lv, string log, bool prefix = true, params RichTextStyle[] styles)
        {
            if (log == null)
                log = "";
            RichTextStyle defStyle;
            switch (lv)
            {
                case Level.info:
                    defStyle = infoStyle;
                    break;
                case Level.warning:
                    defStyle = warningStyle;
                    break;
                case Level.error:
                    defStyle = errorStyle;
                    break;
                case Level.debug:
                    defStyle = debugStyle;
                    break;
                default:
                    defStyle = new RichTextStyle();
                    defStyle.color = "#ffffff";
                    break;
            }
            defStyle.start = 0;
            defStyle.end = log.Length;
            LinkedList<RichTextStyle> setting = null;
            RichTextStyle.AddStyle(ref setting, defStyle, true);
            for (int i = 0; i < styles.Length; i++)
            {
                RichTextStyle.AddStyle(ref setting, styles[i], true);
            }
            RichTextStyle.Optimization(setting);
            string s = RichTextStyle.UseStyles(setting, log);
            if (prefix)
            {
                string pcolor = pColors[Mathf.Min(pColors.Length - 1, pid)];
                string pname = pid > 0 ? ("P" + pid) : "MAIN";
                string pre = string.Format("<color={0}>[<i>{1}</i>,{2}] ></color> ", pcolor, pname, System.DateTime.Now.ToString("d/M HH:mm:ss.ffff"));
                s = pre + s;
            }

            logs.AddLast(s);
            maxLog = Mathf.Max(10, maxLog);
            while (logs.Count > maxLog)
                logs.RemoveFirst();
            newLog = true;
            if (m_OpenWhenPrint && !mOpened)
                Open = true;
        }

        void ProcessInput()
        {
            Input.imeCompositionMode = IMECompositionMode.On;

            Event ev = Event.current;
            bool b = false;
            if (ev.rawType == EventType.KeyDown)
            {
                b = ProcessKeyCode(ev);
                scroll.y = lastRect.yMax;
                newLog = false;
            }
            if (!Open)
            {
                ireader.Text = null;
                return;
            }
            if (!b)
            {
                string t = Input.inputString;
                string t2 = Input.compositionString;
                if (!string.IsNullOrEmpty(t))
                {
                    inputStr = t;
                }
                else if (!string.IsNullOrEmpty(inputStr))
                {
                    int i;
                    while ((i = inputStr.IndexOf('\r')) >= 0)
                        inputStr = inputStr.Remove(i, 1);
                    while ((i = inputStr.IndexOf('\b')) >= 0)
                        inputStr = inputStr.Remove(i, 1);
                    while (inputStr.EndsWith("\n"))
                        inputStr = inputStr.Substring(0, inputStr.Length - 1);
                    ireader.Insert(inputStr);
                    inputStr = null;
                    compositionStr = null;
                }
                else if (!string.IsNullOrEmpty(t2) && t2 != compositionStr)
                {
                    compositionStr = t2;
                    if (ireader.SelectLength > 0)
                        ireader.Backspace();
                    int off = ireader.SelectStart;
                    ireader.Insert(compositionStr);
                    ireader.SelectEnd = off;
                }
            }
        }

        void NextCmd(int p)
        {
            if (cmds.Count == 0)
                return;
            if (ireader.Text != null && cmds.Contains(ireader.Text))
                currentCmd -= p;
            if (currentCmd < 0)
                currentCmd = cmds.Count - 1;
            else if (currentCmd >= cmds.Count)
                currentCmd = 0;
            ireader.Text = cmds[currentCmd];
        }

        bool ProcessKeyCode(Event ev)
        {
            bool ctrl = ev.control;
            bool shift = ev.shift;
            bool alt = ev.alt;
            bool clt = ctrl || alt;
            if (ev.keyCode == KeyCode.BackQuote)
            {
                if (clt)
                {
                    ev.Use();
                    Open = !Open;
                    return true;
                }
            }
            if (!Open)
                return false;
            switch (ev.keyCode)
            {
                case KeyCode.LeftArrow:
                    ev.Use();
                    if (clt)
                        ireader.MoveSelector(ireader.LineStart - ireader.SelectStart, shift);
                    else if (ireader.SelectStart != ireader.SelectEnd && !shift)
                        ireader.MoveSelector(Mathf.Min(ireader.SelectStart, ireader.SelectEnd) - ireader.SelectStart, shift);
                    else
                        ireader.MoveSelector(-1, shift);
                    //assistant.Close();
                    return true;
                case KeyCode.RightArrow:
                    ev.Use();
                    if (clt)
                        ireader.MoveSelector(ireader.FixedLineEnd - ireader.SelectStart, shift);
                    else if (ireader.SelectStart != ireader.SelectEnd && !shift)
                        ireader.MoveSelector(Mathf.Max(ireader.SelectStart, ireader.SelectEnd) - ireader.SelectStart, shift);
                    else
                        ireader.MoveSelector(1, shift);
                    //assistant.Close();
                    return true;
                case KeyCode.UpArrow:
                    ev.Use();
                    if (clt)
                    {
                        NextCmd(-1);
                    }
                    else
                    {
                        ireader.MoveSelectorLine(-1, shift);
                    }
                    return true;
                case KeyCode.DownArrow:
                    ev.Use();
                    if (clt)
                    {
                        NextCmd(1);
                    }
                    else
                    {
                        ireader.MoveSelectorLine(1, shift);
                    }
                    assistant.Close();
                    return true;
                case KeyCode.Backspace:
                    ev.Use();
                    ireader.Backspace();
                    assistant.Close();
                    return true;
                case KeyCode.Tab:
                    ev.Use();
                    if (ireader.SelectLength == 0)
                    {
                        assistant.Assistant(ireader, mEngine);
                    }
                    if (assistant.HasAssistant)
                    {
                        assistant.NextAssistant(ireader);
                    }
                    return true;
                case KeyCode.Return:
                    ev.Use();
                    if ((clt && ctrlExecute) || (!ctrlExecute && !clt))
                    {
                        if (mReader != null)
                        {
                            mReader.value = ireader.Text;
                            mReader.StopWait();
                            PutLog(Level.debug, ireader.Text, false);
                            ireader.Text = null;
                            mReader = null;
                            mReaderName = "";
                        }
                        else
                        {
                            string s = ireader.Text == null ? null : ireader.Text.Trim();

                            if (!string.IsNullOrEmpty(s))
                            {
                                int threadId = 0;
                                if (s.StartsWith(">"))
                                {
                                    threadId = -1;
                                    s = s.Substring(1);
                                }
                                else if (s.StartsWith("<"))
                                {
                                    int off = s.IndexOf('>');
                                    int tid;
                                    if (off > 0 && int.TryParse(s.Substring(1, off - 1), out tid))
                                    {
                                        threadId = tid;
                                        s = s.Substring(off + 1);
                                    }
                                }
                                if (Execute(s, threadId))
                                {
                                    logDebug(s);
                                    PutCmd(ireader.Text);
                                    ireader.Text = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        ireader.Insert("\n");
                    }
                    return true;
                default:
                    bool p = ProcessQuickKey(ev.keyCode, shift, ctrl);
                    if (p)
                        ev.Use();
                    if (ev.keyCode != KeyCode.None && assistant.HasAssistant)
                        assistant.Release(ireader);
                    return p;
            }
        }

        bool ProcessQuickKey(KeyCode key, bool shift, bool ctrl)
        {
            switch (key)
            {
                case KeyCode.L:
                    if (ctrl)
                        ireader.SelectLine();
                    return ctrl;
                case KeyCode.C:
                    if (ctrl)
                        InputReader.clipboard = ireader.SelectionText;
                    return ctrl;
                case KeyCode.X:
                    if (ctrl)
                    {
                        InputReader.clipboard = ireader.SelectionText;
                        if (ireader.SelectLength > 0)
                            ireader.Backspace();
                    }
                    return ctrl;
                case KeyCode.V:
                    if (ctrl)
                    {
                        string t = InputReader.clipboard;
                        if (!string.IsNullOrEmpty(t))
                            ireader.Insert(t);
                    }
                    return ctrl;
                default:
                    return false;
            }
        }

        void OnGUI()
        {
            if (mThreads[0].isFinished() || mReader != null)
                ProcessInput();
            if (mOpened)
            {
                mConsoleRect = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Window(1, mConsoleRect, OnConsoleWindow, "CMD");
            }
        }

        void OnConsoleWindow(int id)
        {
            if (newLog && scroll.y < lastRect.yMax)
            {
                scroll.y += Mathf.Clamp(Mathf.Lerp(scroll.y, lastRect.yMax, 0.3f) - scroll.y, 4, 50);
            }

            GUI.skin.label.richText = true;
            formatInput();
            scroll = GUILayout.BeginScrollView(scroll);
            LinkedListNode<string> node = logs.First;
            while (node != null)
            {
                GUILayout.Label(node.Value);
                node = node.Next;
            }
            GUILayout.Label(mText);

            if (Event.current.type == EventType.repaint)
            {
                lastRect = GUILayoutUtility.GetLastRect();
            }

            GUILayout.EndScrollView();

            if (scroll.y < lastScroll.y)
                newLog = false;

            lastScroll = scroll;
            ireader.Use();
        }

        public void LoadFunction(object target)
        {
#if UNITY_EDITOR
            if (target == null)
                return;
            System.Type tp = target.GetType();
            HashSet<RTSPluginAttribute> sets = new HashSet<RTSPluginAttribute>();
            mFunctions[tp] = sets;
            MethodInfo[] methods = tp.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            //bool first = true;
            for (int i = 0; i < methods.Length; i++)
            {
                try
                {
                    object[] cmds = methods[i].GetCustomAttributes(typeof(RTSPluginAttribute), false);
                    if (cmds.Length > 0)
                    {
                        RTSPluginAttribute cmd = cmds[0] as RTSPluginAttribute;
                        if (!RTSUtil.isGoodName(cmd.Name))
                            continue;
                        RTSPluginDelegate func = (RTSPluginDelegate)System.Delegate.CreateDelegate(typeof(RTSPluginDelegate), target, methods[i].Name);
                        RTSPluginFunc f = new RTSPluginFunc(null, func, cmd.ArgCount);
                        mEngine.addFunction(cmd.Name, f);
                        if (cmd.ArgCount <= 0)
                            mEngine.addLinker(cmd.Name, new RTSFuncShortcutL());
                        if (!string.IsNullOrEmpty(cmd.Doc))
                            sets.Add(cmd);
                        //PutLog(Level.debug,
                        //    string.Format("{0}Load function: <color=orange><b>{1}</b></color>", first ? "\n\t" : "\t", cmd.Name), first);
                        //first = false;
                    }

                }
                catch (System.Exception e)
                {
                    PutLog(Level.error, e.ToString());
                    Debug.LogException(e);
                }
            }
#endif
        }

        public void UnloadFunction(object target)
        {
#if UNITY_EDITOR
            if (target == null)
                return;
            System.Type tp = target.GetType();
            HashSet<RTSPluginAttribute> sets;
            if (mFunctions.TryGetValue(tp, out sets))
            {
                mFunctions.Remove(tp);
                foreach (RTSPluginAttribute pl in sets)
                {
                    mEngine.removeFunction(pl.Name, pl.ArgCount);
                }
            }
#endif
        }

        #endregion

        #region plugin functions

        [RTSPlugin(Name = "log", ArgCount = -1)]
        object rtsLogDebug(object[] args)
        {
            if (args != null)
            {
                string msg = "";
                for (int i = 0; i < args.Length; i++)
                {
                    msg += args[i];
                }
                logDebug(msg);
            }
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "getType", ArgCount = 1, Doc = "对象System.Type类型")]
        object _getType(object[] args)
        {
            if (args[0] == null)
                return null;
            else
                return args[0].GetType();
        }

        [RTSPlugin(Name = "readFile", ArgCount = 1, Doc = "读取文件文本")]
        public object readFile(object[] args)
        {
            return LoadFile(args[0].ToString(), false);
        }

        [RTSPlugin(Name = "readKeyboard", ArgCount = 0, Doc = "从键盘读取字符串")]
        object readStr(object[] args)
        {
            mReader = new RTSWaitForValueR<string>(this);
            mReaderName = "";
            return mReader;
        }

        [RTSPlugin(Name = "readKeyboard", ArgCount = 1, Doc = "从键盘读取字符串")]
        object readStrWithName(object[] args)
        {
            mReader = new RTSWaitForValueR<string>(this);
            mReaderName = args[0] == null ? "" : args[0].ToString();
            return mReader;
        }
      
        [RTSPlugin(Name = "cls", ArgCount = 0, Doc = "清空屏幕打印日志")]
        object clearLog(object[] args)
        {
            logs.Clear();
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "mk_doc", ArgCount = 3, Define = "mk_doc(string name,string format,string desc)", Doc = "添加描述文档")]
        object mk_doc(object[] args)
        {
            RTSPluginAttribute att = new RTSPluginAttribute();
            if (args[0] != null)
                att.Name = args[0].ToString();
            if (args[1] != null)
                att.Define = args[1].ToString();
            if (args[2] != null)
                att.Doc = args[2].ToString();
            if (!string.IsNullOrEmpty(att.Name) && !string.IsNullOrEmpty(att.Doc))
                mCustomDocs[att.Name] = att;
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "mk_link", ArgCount = 1, Define = "void mk_link(string funcName)", Doc = "创建方法的快捷方式，从而省略无参数的括号")]
        object mk_link(object[] args)
        {
            string fname = args[0] == null ? null : args[0].ToString();
            if (!string.IsNullOrEmpty(fname))
            {
                RTSFuncShortcutL fl = new RTSFuncShortcutL();
                mEngine.addLinker(fname, fl);
            }
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "vars", ArgCount = 0, Doc = "查看所有定义的变量")]
        object variables(object[] args)
        {
            Dictionary<string, object> dics = Ref.GetField(mEngine, "mVars") as Dictionary<string, object>;
            int counter = 0;
            StringBuilder builder = new StringBuilder();
            if (dics != null)
            {
                builder.Append("\nglobal variables >>>");
                foreach (string s in dics.Keys)
                {
                    object o = dics[s];
                    builder.Append("\n   ").Append(counter++).Append(": global ");
                    if (o != null)
                        builder.Append(o.GetType()).Append(" ");
                    builder.Append(s).Append(" = ").Append(o);
                }
            }
            RTSStack stack = Ref.GetField(mThreads[pid], "mStack") as RTSStack;
            while (stack != null)
            {
                dics = Ref.GetField(stack, "mVars") as Dictionary<string, object>;
                if (dics != null)
                {
                    builder.Append("\nvariables in current thread >>> stack_").Append(stack.getId()).Append(" >>>");
                    foreach (string s in dics.Keys)
                    {
                        object o = dics[s];
                        builder.Append("\n   ").Append(counter++).Append(": ");
                        if (o != null)
                            builder.Append(o.GetType()).Append(" ");
                        builder.Append(s).Append(" = ").Append(o);
                    }
                }
                stack = stack.getSuper() as RTSStack;
            }
            PutLog(Level.info, builder.ToString());
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "findVar", ArgCount = 1)]
        object find_var(object[] args)
        {
            string key = args[0] == null ? null : args[0].ToString();
            if (string.IsNullOrEmpty(key))
                return null;
            RTSStack stack = Ref.GetField(mThreads[pid], "mStack") as RTSStack;
            Dictionary<string, object> dics;
            while (stack != null)
            {
                dics = Ref.GetField(stack, "mVars") as Dictionary<string, object>;
                if (dics != null && dics.ContainsKey(key))
                    return true;
                stack = stack.getSuper() as RTSStack;
            }
            dics = Ref.GetField(mEngine, "mVars") as Dictionary<string, object>;
            return dics != null && dics.ContainsKey(key);
        }

        [RTSPlugin(Name = "valueof", ArgCount = 1)]
        object deep_var(object[] args)
        {
            string key = args[0] == null ? null : args[0].ToString();
            if (string.IsNullOrEmpty(key))
                return null;
            RTSStack stack = Ref.GetField(mThreads[pid], "mStack") as RTSStack;
            Dictionary<string, object> dics;
            while (stack != null)
            {
                dics = Ref.GetField(stack, "mVars") as Dictionary<string, object>;
                if (dics != null && dics.ContainsKey(key))
                    return dics[key];
                stack = stack.getSuper() as RTSStack;
            }
            dics = Ref.GetField(mEngine, "mVars") as Dictionary<string, object>;
            if (dics != null && dics.ContainsKey(key))
                return dics[key];
            return null;
        }

        [RTSPlugin(Name = "funcs", ArgCount = -1, Doc = "列举所有的方法定义")]
        object funcs(object[] args)
        {
            Dictionary<string, IRTSFunction> funcs = Ref.GetField(mEngine, "mFuncs") as Dictionary<string, IRTSFunction>;
            StringBuilder b = new StringBuilder("\n============== FUNCTIONS ==============\n");
            int num = 0;
            foreach (string key in funcs.Keys)
            {
                string fname;
                int n = key.IndexOf('-');
                if (n > 0)
                    fname = key.Substring(0, n);
                else
                    fname = key;
                if (!filterName(fname, args))
                    continue;
                b.Append(num++).Append(":    ");
                b.Append(fname);
                b.Append(" ( ");
                IRTSFunction func = funcs[key];
                int argc = func.argSize();
                if (argc < 0)
                {
                    b.Append("args [ ]");
                }
                else if (argc > 0)
                {
                    for (int i = 0; i < argc; i++)
                    {
                        if (i > 0)
                            b.Append(", ");
                        b.Append(func.getArgDef(i));
                    }
                }
                b.Append(" )\n");
            }
            PutLog(Level.info, b.ToString());
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "doc_all", ArgCount = -1)]
        object all_doc(object[] args)
        {
            StringBuilder b = new StringBuilder("\n============== DOC ==============\n");
            foreach (HashSet<RTSPluginAttribute> cmds in mFunctions.Values)
            {
                foreach (RTSPluginAttribute cmd in cmds)
                {
                    if (filterName(cmd.Name, args))
                        b.Append(desc(cmd)).Append('\n');
                }
            }
            foreach (RTSPluginAttribute cmd in mCustomDocs.Values)
            {
                if (filterName(cmd.Name, args))
                    b.Append(desc(cmd)).Append('\n');
            }
            PutLog(Level.info, b.ToString());
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "doc", ArgCount = -1)]
        object cus_doc(object[] args)
        {
            StringBuilder b = new StringBuilder("\n============== DOC ==============\n");
            foreach (RTSPluginAttribute cmd in mCustomDocs.Values)
            {
                if (filterName(cmd.Name, args))
                    b.Append(desc(cmd)).Append('\n');
            }
            PutLog(Level.info, b.ToString());
            return RTSVoid.VOID;
        }

        [RTSPlugin(Name = "script", ArgCount = 1, Doc = "加载文件")]
        public object rtsScript(object[] args)
        {
            string res;
            string folder;
#if UNITY_EDITOR
            folder = EditorPrefs.GetString("Devil.Root", "Assets");
#else
            folder = Application.streamingAssetsPath;
#endif
            string path = LoadTextAsset(args[0].ToString(), new string[] { folder + "/DevilFramework/Command" }, out res);
            if (!string.IsNullOrEmpty(path))
            {
                PutLog(Level.debug, "loading: " + path);
                return new RTSInlineCompileR(res);
            }
            return RTSVoid.VOID;
        }

        #endregion

        bool filterName(string nameStr, object[] args)
        {

            bool filter = true;
            string p = nameStr.ToLower();
            if (args != null && args.Length > 0)
            {
                filter = false;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != null && p.Contains(args[i].ToString().ToLower()))
                    {
                        filter = true;
                        break;
                    }
                }
            }
            return filter;
        }

        string desc(RTSPluginAttribute cmd)
        {
            string sc;
            if (!string.IsNullOrEmpty(cmd.Define))
            {
                sc = cmd.Define;
            }
            else
            {
                sc = cmd.Name + '(';
                if (cmd.ArgCount >= 0)
                {
                    for (int i = 0; i < cmd.ArgCount; i++)
                    {
                        if (i > 0)
                            sc += ',';
                        sc += ' ';
                        sc += (char)('a' + i);
                    }
                }
                else
                {
                    sc += " a...";
                }
                sc += " )";
            }
            if (!string.IsNullOrEmpty(cmd.Doc))
            {
                sc += "\n\t" + cmd.Doc;
            }
            return sc;
        }
    }
}