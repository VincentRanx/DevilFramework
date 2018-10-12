using Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TableCore.Exceptions;
using TableCore.Plugin;

namespace TableCore
{
    public class GTStatus
    {
        public const string NAME_PATTERN = @"^[_A-Za-z]+[_A-Za-z0-9]*$";
            

        static GTStatus _instance;
        public static GTStatus Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GTStatus();
                return _instance;
            }
        }

        public static GTStatus GetTemporary()
        {
            return new GTStatus();
        }

        DataTable m_UseTab;
        int m_StartRow = 0;
        int m_StartCol = 1;
        public int TableCols { get; private set; }
        public DataSet Data { get; private set; }
        public ClassModel ClassMod { get; private set; }
        public bool IsTableUsed { get { return ClassMod != null; } }
        IList<string> mTables;
        public IList<string> TableNames { get { return mTables; } }
        public int UsedTableIndex { get; private set; }
        public string UsedTableName { get; private set; }
        public string FileName { get; private set; }
        public string FileFolder { get; private set; }
        public int TableCount { get { return mTables == null ? 0 : mTables.Count; } }
        HashSet<int> mDirtyTables = new HashSet<int>();
        public int StartRow { get { return m_StartRow; } }
        public int StartCol { get { return m_StartCol; } }
        JObject mModifyPropertyType; // <propertyName, propertyType>
        GTConfig mCfg;
        public GTConfig Config
        {
            get
            {
                if (mCfg == null)
                    mCfg = GTConfig.NewDefaultCfg();
                return mCfg;
            }
            set
            {
                if (value != null)
                    mCfg = value;
                else
                    mCfg = GTConfig.NewDefaultCfg();
            }
        }

        public string DataPath
        {
            get
            {
                return string.Format("{0}/{1}.{2}", Config.ActiveData.DataFolder, m_UseTab.TableName, Config.ActiveData.DataExtension);
            }
        }

        public string GetTableName(int index)
        {
            return mTables[index];
        }

        private GTStatus()
        {
            UsedTableIndex = -1;
        }

        public void SetData(DataSet data)
        {
            if (data == Data)
                return;
            mDirtyTables.Clear();
            UsedTableIndex = -1;
            UsedTableName = "";
            Data = data;
            m_UseTab = null;
            ClassMod = null;
            TableCols = 0;
            mTables = data == null ? null : GetTablenames(data.Tables);
        }
        
        public void OpenFile(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                throw new FileNotFoundException("GTStatus Can't open file: " + file, file);
            FileName = file;
            FileFolder = new FileInfo(file).DirectoryName;
            using (var stream = new FileStream(file, FileMode.Open))
            {
                IExcelDataReader reader = null;
                if (file.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (file.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                if (reader == null)
                    return;
                reader.IsFirstRowAsColumnNames = false;
                while (reader.Read()) { }
                SetData(reader.AsDataSet());
                stream.Close();
            }
        }

        private IList<string> GetTablenames(DataTableCollection tables)
        {
            var tableList = new List<string>();
            foreach (var table in tables)
            {
                tableList.Add(table.ToString());
            }

            return tableList;
        }

        public void ModifyClass(string modifyFile)
        {
            mModifyPropertyType = null;
            string file = Utils.ReadFile(modifyFile);
            if (string.IsNullOrEmpty(file))
                return;
            mModifyPropertyType = JsonConvert.DeserializeObject<JObject>(file);
        }

        public bool GetModify(string classname, string property, out string type)
        {
            if (mModifyPropertyType == null)
            {
                type = null;
                return false;
            }
            JToken tok;
            JObject tab = null;
            if (mModifyPropertyType.TryGetValue(classname, out tok))
                tab = tok as JObject;
            if (tab != null && tab.TryGetValue(property, out tok))
            {
                type = tok.ToString();
                return true;
            }
            else
            {
                type = null;
                return false;
            }
        }
    
        public void UseTable(string table, int startRow, int startCol, bool alwaysuse = false, bool skipTypeDef = false)
        {
            int tab = mTables.IndexOf(table);
            UseTable(tab, table, startRow, startCol, alwaysuse, skipTypeDef);
        }

        public void UseTable(int tabIndex, string className, int startRow, int startCol, bool alwaysuse = false, bool skipTypeDef = false)
        {
            if (Data == null || tabIndex < 0 || tabIndex >= mTables.Count)
                throw new Exception(string.Format("引用了一个非法的表格索引"));

            if (startRow < 0 || startCol < 0)
            {
                Utils.FindCell((x) => Utils.EqualIgnoreCase(x, "id"), Data.Tables[tabIndex], out startRow, out startCol);
            }
            if (startRow < 0 || startCol < 0)
            {
                startRow = 0;
                startCol = 0;
                //   throw new Exception(string.Format("单元格({0},{1})不能作为数据定义起始单元格。", startRow, startCol));
            }
            if (!alwaysuse && tabIndex == UsedTableIndex && m_StartRow == startRow && m_StartCol == startCol)
            {
                if (ClassMod != null && !string.IsNullOrEmpty(className))
                    ClassMod.ChangeClassName(className, this);
                return;
            }
            UsedTableIndex = tabIndex;
            UsedTableName = mTables[tabIndex];
            m_UseTab = Data.Tables[tabIndex];
            TableCols = m_UseTab.Columns.Count - startCol;
            m_StartRow = startRow;
            m_StartCol = startCol;
            if (skipTypeDef && (mModifyPropertyType == null || mModifyPropertyType.Property(className) == null))
                throw new Exception(string.Format("没有找到对象（{0}）的定义文件", className));
            ClassMod = new ClassModel(className, this);
            ClassMod.IgnoreWithPattern(Config.ActiveData.IgnorePattern);
        }

        public void IgnorePropertyWithCategoryPattern()
        {
            if (ClassMod != null)
                ClassMod.IgnoreWithPattern(Config.ActiveData.IgnorePattern);
        }

        public bool GetPropertyDefine(int pindex, string classname, out ClassModel.Property property)
        {
            if (m_UseTab == null)
            {
                property = default(ClassModel.Property);
                return false;
            }
            ClassModel.Property p = new ClassModel.Property();
            string str = Utils.GetCell(m_UseTab, m_StartRow, m_StartCol + pindex);
            if (string.IsNullOrEmpty(str))
            {
                property = default(ClassModel.Property);
                return false;
            }
            p.Name = str;
            if (!GetModify(classname, p.Name, out str))
                str = Utils.GetCell(m_UseTab, m_StartRow + 1, m_StartCol + pindex);
            if (string.IsNullOrEmpty(str))
            {
                property = default(ClassModel.Property);
                return false;
            }
            GTType type = Config.ActiveClass.GetGTType(str);
            p.GenType = type ?? throw new TypeNotDefinedException(str);
            p.Comment = Utils.GetCell(m_UseTab, m_StartRow + 2, m_StartCol + pindex);
            p.Ignore = false;
            property = p;
            return true;
        }
        
        int FindRow(int id)
        {
            string ids = id.ToString();
            int rows = m_UseTab.Rows.Count;
            for (int i= m_StartRow + 3; i < rows; i++)
            {
                object v = m_UseTab.Rows[i][m_StartCol];
                if (v ==  null || string.IsNullOrEmpty(v.ToString()) || v.ToString() == ids)
                    return i;
            }
            return -1;
        }

        int FindCol(string property)
        {
            int cols = m_UseTab.Columns.Count;
            for(int i = 0; i < cols; i++)
            {
                object v = m_UseTab.Rows[m_StartRow][i];
                if (Utils.EqualAsString(v, property))
                    return i;
            }
            return -1;
        }
        
        public void ClearData()
        {
            if (m_UseTab == null)
                return;
            int rows = m_UseTab.Rows.Count;
            if (rows > m_StartRow + 3)
                mDirtyTables.Add(UsedTableIndex);
            for (int r = rows - 1; r >= m_StartRow + 3; r--)
            {
                m_UseTab.Rows.RemoveAt(r);
            }
        }

        public void UpdateData(JObject data)
        {
            if (m_UseTab == null)
                throw new Exception(FileName + " No Excel Opened.");
            int row = FindRow(data.Value<int>("id"));
            DataRow rowdata;
            if (row == -1)
            {
                rowdata = m_UseTab.NewRow();
                m_UseTab.Rows.Add(rowdata);
            }
            else
            {
                rowdata = m_UseTab.Rows[row];
            }
            foreach(JProperty pro in data.Properties())
            {
                int col = FindCol(pro.Name);
                if(col != -1)
                {
                    object s = rowdata[col];
                    object v = pro.Value.ToString();
                    if (!Utils.EqualAsString(v, s))
                    {
                        rowdata[col] = v == null ? "" : v.ToString();
                        mDirtyTables.Add(UsedTableIndex);
                    }
                }
            }
        }

        public void ExportDirty()
        {
            if (mDirtyTables.Count == 0)
                return;
            var excel = new ExcelPackage(new FileInfo(FileName));
            foreach (int tabindex in mDirtyTables)
            {
                var sheet = excel.Workbook.Worksheets[mTables[tabindex]];
                DataTable tab = Data.Tables[tabindex];
                int rows = tab.Rows.Count;
                if (rows < sheet.Cells.Rows)
                    sheet.Cells.Clear();
                int cols = tab.Columns.Count;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        sheet.Cells[r + 1, c + 1].Value = tab.Rows[r][c];
                    }
                }
            }
            mDirtyTables.Clear();
            excel.Save();
            excel.Dispose();
        }

        // 插入排序
        bool InsertBefore(LinkedList<GenData> data, ref LinkedListNode<GenData> first, GenData value)
        {
            var node = first;
            while(node != null)
            {
                var v = node.Value;
                if (v.id == value.id)
                    return false;
                if (v.id < value.id)
                {
                    first = data.AddAfter(node, value);
                    return true;
                }
                node = node.Previous;
            }
            first = data.AddFirst(value);
            return true;
        }

        bool InsertAfter(LinkedList<GenData> data, ref LinkedListNode<GenData> first, GenData value)
        {
            var node = first;
            while(node != null)
            {
                var v = node.Value;
                if (v.id == value.id)
                    return false;
                if(v.id > value.id)
                {
                    first = data.AddBefore(node, value);
                    return true;
                }
                node = node.Next;
            }
            first = data.AddLast(value);
            return true;
        }

        void GenerateData(Action<int, string> progressHandler, LinkedList<GenData> datas, out int dataNums, LinkedList<IExportData> exports, out int exportNums)
        {
            dataNums = 0;
            exportNums = 0;
            string title = "Parse Data...";
            int rows = m_UseTab.Rows.Count - m_StartRow - 3;
            //List<JObject> datas = new List<JObject>();
            LinkedListNode<GenData> node = null;
            for (int i = 0; i < rows; i++)
            {
                int r = i + m_StartRow + 3;
                var row = m_UseTab.Rows[r];
                var entity = new GenData(ClassMod);
                bool skip = false;
                for (int j = 0; j < ClassMod.PropertyCount; j++)
                {
                    var property = ClassMod.GetProperty(j);
                    if (property.Ignore)
                        continue;
                    string dt = Utils.GetString(row[j + m_StartCol]);
                    if (property.IsID && string.IsNullOrEmpty(dt))
                    {
                        skip = true;
                        break;
                    }
                    entity.SetProperty(property.Index, dt, Utils.GetCellName(r, j + m_StartCol));
                    var comment = string.Format("{0}[{1}]", UsedTableName, property.Name);
                    IExportData exp = property.GenType.ExportData(dt, comment);
                    if (exp != null)
                    {
                        exports.AddLast(exp);
                        exportNums++;
                    }
                }
                if (skip)
                    break;
                dataNums++;
                if (node == null)
                {
                    node = datas.AddFirst(entity);
                }
                else
                {
                    var id = node.Value.id;
                    if (id == entity.id)
                        throw new DuplexIdException(id.ToString());
                    if (id < entity.id && !InsertAfter(datas, ref node, entity))
                        throw new DuplexIdException(id.ToString());
                    else if (id > entity.id && !InsertBefore(datas, ref node, entity))
                        throw new DuplexIdException(id.ToString());
                }
                progressHandler?.Invoke(i * 99 / rows, title);
            }
        }

        void WriteFile(LinkedList<GenData> datas, int count, Action<int, string> progressHandler)
        {
            string str;
            int n = 0;
            if (Config.ActiveData.DataMode == EDataMode.dictionary)
            {
                JObject dic = new JObject();
                foreach (var dt in datas)
                {
                    dic[dt.id.ToString()] = dt.GetFormatData(Config.ActiveData);
                    progressHandler?.Invoke(n++ * 95 / count, "Write File...");
                }
                str = dic.ToString();
            }
            else
            {
                JArray arr = new JArray();
                var dat = datas.First;
                while (dat != null)
                {
                    var current = dat.Value;
                    dat = dat.Next;
                    arr.Add(current.GetFormatData(Config.ActiveData));
                    progressHandler?.Invoke(n++ * 95 / count, "Write File...");
                }
                str = arr.ToString();
            }
            string folder = Config.ActiveData.DataFolder;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string file = DataPath;
            byte[] data = Encoding.UTF8.GetBytes(str);
            File.WriteAllBytes(file, data);
            progressHandler?.Invoke(99, "Write File...");
        }

        void ExportData(Action<int, string> progressHandler, int count, LinkedList<IExportData> exports)
        {
            int n = 0;
            Dictionary<string, GTStatus> status = new Dictionary<string, GTStatus>();
            foreach (var export in exports)
            {
                GTStatus gts;
                if (!status.TryGetValue(export.ExportExcelFile, out gts))
                {
                    gts = GetTemporary();
                    status[export.ExportExcelFile] = gts;
                    if (Utils.IsAbsolutePath(export.ExportExcelFile))
                        gts.OpenFile(export.ExportExcelFile);
                    else
                        gts.OpenFile(Path.Combine(FileFolder, export.ExportExcelFile));
                }
                gts.UseTable(export.ExportExcelSheet, export.ExcelStartRow, export.ExcelStartCol);
                gts.UpdateData(export.ExportData);
                progressHandler?.Invoke(n++ * 95 / count, "Export Data...");
            }
            foreach (var gts in status.Values)
            {
                gts.ExportDirty();
            }
        }

        void OutputModify(Action<int, string> processHandler, LinkedList<GenData> datas, IDataModify output, ref int nums)
        {
            var node = datas.First;
            GenData previours = null;
            while(node != null)
            {
                var dat = node.Value;
                var next = node.Next;
                var modify = output.FixOutputData(Config.ActiveCategory, dat, previours, next == null ? null : next.Value);
                if(modify != null)
                {
                    var tmp = node;
                    for (int i = 0; i < modify.Length; i++)
                    {
                        var newdat = modify[i];
                        if (newdat != null)
                        {
                            tmp = datas.AddAfter(tmp, newdat);
                            nums++;
                        }
                    }
                }
                datas.Remove(node);
                nums--;
                node = next;
                previours = dat;
            }

        }

        public void GenerateData(Action<int, string> progressHandler, bool writeFile = true, bool export = true)
        {
            if (m_UseTab == null || ClassMod == null)
                throw new NoTableException();
            if (string.IsNullOrEmpty(Config.ActiveData.DataFolder))
                throw new DirectoryNotFoundException();
            if (!ClassMod.IsIdDefined)
                throw new NoIdDefineException(ClassMod.ClassName);
            //int rows = m_UseTab.Rows.Count - m_StartRow - 3;
            //List<JObject> datas = new List<JObject>();
            LinkedList<GenData> datas = new LinkedList<GenData>();
            LinkedList<IExportData> exports = new LinkedList<IExportData>();
            int dataNums;
            int exportNums;
            GenerateData(progressHandler, datas, out dataNums, exports, out exportNums);
            var modify = Config.ActiveData.PrepareTableModify(this, Config.ActiveCategory, UsedTableName);
            if(modify != null)
                OutputModify(progressHandler, datas, modify, ref dataNums);
            if(writeFile)
                WriteFile(datas, ++dataNums, progressHandler);
            if(export) 
                ExportData(progressHandler, ++exportNums, exports);
            progressHandler?.Invoke(100, "Complish");
        }

        //public void ExportColumn(string column, Action<int, string> processHandler)
        //{
        //    if (m_UseTab == null || ClassMod == null)
        //        throw new NoTableException();
        //    if (string.IsNullOrEmpty(Config.ActiveData.DataFolder))
        //        throw new DirectoryNotFoundException();
        //    if (!ClassMod.IsIdDefined)
        //        throw new NoIdDefineException(ClassMod.ClassName);
        //    ClassModel.Property p;
        //    if (!ClassMod.GetProperty(column, out p))
        //        throw new Exception(string.Format("Column[{0}] doesn't exist.", column));
        //    int rows = m_UseTab.Rows.Count - m_StartRow - 3;
        //}
    }
}
