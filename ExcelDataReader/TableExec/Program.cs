using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TableCore;
using TableCore.Exceptions;

namespace TableExec
{
    class Program
    {
        public class Category
        {
            public string category { get; private set; }
            public string foler;
            public string csFolder;
            public Category(string cate)
            {
                category = cate;
            }
        }

        const int ERROR_GEN_CODE = 1;
        const int ERROR_GEN_DATA = 2;
        const int ERROR_USE_TABLE = 4;
        const int ERROR_PARSE_ARG = 8;
        const int ERROR_OPEN_FILE = 16;
        const int ERROR_CONFIG = 32;

        static string cfgFile;
        static int startRow = -1;
        static int startCol = -1;
        static string className = null;
        static string file = null;
        static List<Category> mExports = new List<Category>();
        static bool cleanTable;
        static string mCleanPattern = @"^.*_clean$";
        static string mSheetIgnorePattern = @"^Sheet\d+$";
        static string mModifyFile;
        static string help = @"
exec [arg1] [arg2] [arg3]... [excel file]
参数列表
-cfg [file]             :配置文件
-row [index]            :配表起始行
-col [index]            :配表起始列
-class [name]           :设置类名([AUTO] 表示自动命名)
-classdef [file]        :类定义文件
-[category] [folder]    :作为排序过的数组数据输出路径
-cs_[category] [folder] :C#代码输出路径
-clean [sheet]          :指定工作表 并清理内容
-ignore_sheet [sheet]   :忽略表格
";

        static Category GetCategory(string category)
        {
            foreach(Category cat in mExports)
            {
                if (cat.category == category)
                    return cat;
            }
            var newcat = new Category(category);
            mExports.Add(newcat);
            return newcat;
        }

        static void ParseArgs(string[] args)
        {
            if (args == null || args.Length == 0)
                throw new Exception("参数错误。");
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-cfg":
                        cfgFile = args[++i];
                        break;
                    case "-row":
                        startRow = int.Parse(args[++i]);
                        break;
                    case "-col":
                        startCol = int.Parse(args[++i]);
                        break;
                    case "-class":
                        className = args[++i];
                        if (className == "[AUTO]")
                            className = null;
                        break;
                    case "-classdef":
                        mModifyFile = args[++i];
                        break;
                    case "-clean":
                        mCleanPattern = args[++i];
                        cleanTable = true;
                        break;
                    case "-ignore_sheet":
                        mSheetIgnorePattern = args[++i];
                        break;
                    default:
                        if (Regex.IsMatch(args[i], @"^\-[a-z]+$"))
                        {
                            Category cat = GetCategory(args[i].Substring(1));
                            cat.foler = args[++i];
                        }
                        else if (Regex.IsMatch(args[i], @"^\-cs_[a-z]+$"))
                        {
                            Category cat = GetCategory(args[i].Substring(4));
                            cat.csFolder = args[++i];
                        }
                        else if (string.IsNullOrEmpty(file))
                        {
                            file = args[i];
                        }
                        else
                        {
                            throw new Exception("参数错误。");
                        }
                        break;
                }
            }
        }

        static void DoClean()
        {
            foreach (var tab in GTStatus.Instance.TableNames)
            {
                if (!Regex.IsMatch(tab, mCleanPattern))
                    continue;
                GTStatus.Instance.UseTable(tab, startRow, startCol);
                GTStatus.Instance.ClearData();
                Console.WriteLine(string.Format("\"{0}/{1}\" Was Cleaned Up.", file, tab));
            }
            GTStatus.Instance.ExportDirty();
        }

        static void DoGenerate(ref int error)
        {
            HashSet<string> csCods = new HashSet<string>();
            bool modifyname = !string.IsNullOrEmpty(className);
            for (int i = 0; i < GTStatus.Instance.TableCount; i++)
            {
                string tname = GTStatus.Instance.GetTableName(i);
                if (Regex.IsMatch(tname, mSheetIgnorePattern))
                    continue;
                string cname = modifyname ? className : tname;
                try
                {
                    GTStatus.Instance.UseTable(i, cname, startRow, startCol);
                    if (!GTStatus.Instance.ClassMod.IsIdDefined)
                        throw new NoIdDefineException(tname);
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("\n[ERROR] Use Table {0}[{1}] error\n [EXCEPTION]{2}\n",
                        GTStatus.Instance.FileName, tname, e));
                    error |= ERROR_USE_TABLE;
                    continue;
                }
                StringBuilder buff = new StringBuilder();
                try
                {
                    HashSet<string> classes = new HashSet<string>();
                    // generate list
                    foreach (var cat in mExports)
                    {
                        GTStatus.Instance.Config.ActiveCategory = cat.category;
                        GTStatus.Instance.IgnorePropertyWithCategoryPattern();
                        if (!string.IsNullOrEmpty(cat.csFolder) && classes.Add(cname))
                        {
                            string file = string.Format("{0}/{1}.cs", cat.csFolder, cname);
                            CsharpGenerater gen = new CsharpGenerater(GTStatus.Instance.Config.ActiveClass);
                            gen.GenerateCode(GTStatus.Instance, file);
                            Console.WriteLine("Generate " + file);
                        }
                        if (!string.IsNullOrEmpty(cat.foler))
                        {
                            GTStatus.Instance.Config.ActiveData.DataFolder = cat.foler;
                            GTStatus.Instance.GenerateData((x, y) => { });
                            Console.WriteLine(string.Format("Generate List: {0}", GTStatus.Instance.DataPath));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("\n[ERROR] Generate Data {0}[{1}] error\n [EXCEPTION]{2}\n",
                        GTStatus.Instance.FileName, tname, e));
                    error |= ERROR_GEN_DATA;
                }
            }

        }

        static int Main(string[] args)
        {
            int error = 0;
            try
            {
                ParseArgs(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("\n[ERROR] {0}\n{1}", e, StringUtil.LinkString(false, " ", args)));
                Console.WriteLine(help);
                Console.ReadKey();
                error |= ERROR_PARSE_ARG;
                return error;
            }
            try
            {
                if (!string.IsNullOrEmpty(cfgFile))
                {
                    GTStatus.Instance.Config.MergeCfg(cfgFile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                error |= ERROR_CONFIG;
                return error;
            }
            if(!string.IsNullOrEmpty(mModifyFile))
                GTStatus.Instance.ModifyClass(mModifyFile);
            GTStatus.Instance.OpenFile(file);
            if (cleanTable)
            {
                DoClean();
                return error;
            }
            DoGenerate(ref error);
            return error;
        }
    }
}
