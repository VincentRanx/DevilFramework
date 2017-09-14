using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using ExcelBook = Microsoft.Office.Interop.Excel.Workbook;
using ExcelSheet = Microsoft.Office.Interop.Excel.Worksheet;
using ExcelRange = Microsoft.Office.Interop.Excel.Range;
using System.Configuration;
using System.Collections.Generic;

namespace TableGenerater
{

    public partial class TableGenerater : Form
    {
        private ExcelApp app;
        private ExcelBook book;
        private ExcelSheet sheet;
        private string openedExcel;
        private string csharpOutput;
        private string jsonOutput;
        private string nameSpace;
        private ClassModel classModel;

        public TableGenerater()
        {
            InitializeComponent();
            InitSavedData();  
        }

        private void InitSavedData()
        {
            DurableCfg.Init();
            nameSpace = ConfigurationManager.AppSettings["namespace"];
            namespaceValue.Text = nameSpace + (ClassModel.IsValidNamespace(nameSpace) ? "" : " (不合法)");
            for (int i = 0; i < DurableCfg.Cfg.excelFiles.Count; i++)
            {
                excelFileList.Items.Add(DurableCfg.Cfg.excelFiles[i]);
            }
            excelFileList.Items.Add("-- 浏览文件夹 --");
            for (int i = 0; i < DurableCfg.Cfg.csharpOutput.Count; i++)
            {
                csharpOutputList.Items.Add(DurableCfg.Cfg.csharpOutput[i]);
            }
            csharpOutputList.Items.Add("-- 浏览文件夹 --");
            if (DurableCfg.Cfg.csharpOutput.Count > 0)
            {
                csharpOutputList.SelectedIndex = 0;
                csharpOutput = DurableCfg.Cfg.csharpOutput[0];
            }
            for (int i = 0; i < DurableCfg.Cfg.jsonOutput.Count; i++)
            {
                jsonOutputList.Items.Add(DurableCfg.Cfg.jsonOutput[i]);
            }
            jsonOutputList.Items.Add("-- 浏览文件夹 --");
            if (DurableCfg.Cfg.jsonOutput.Count > 0)
            {
                jsonOutput = DurableCfg.Cfg.jsonOutput[0];
                jsonOutputList.SelectedIndex = 0;
            }
        }

        private void btnEditExcelFile_Click(object sender, EventArgs e)
        {
            if (app != null && !string.IsNullOrEmpty(openedExcel) && File.Exists(openedExcel))
            {
                if(app.Workbooks.Count == 0)
                {
                    book = app.Workbooks.Open(excelFileList.Text);
                    sheet = book.Worksheets.Item[1];
                }
                app.Visible = true;
                excelFileList.SelectedIndex = -1;
                openedExcel = null;
            }
        }

        private void SelectExcele()
        {
            DialogResult result = selectExcelFile.ShowDialog();
            if (result == DialogResult.OK && selectExcelFile.FileName != excelFileList.Text)
            {
                UpdateFileList(selectExcelFile.FileName, excelFileList, 10, DurableCfg.Cfg.excelFiles);
            }
            else
            {
                excelFileList.SelectedIndex = string.IsNullOrEmpty(openedExcel) ? -1 : excelFileList.Items.IndexOf(openedExcel);
            }
        }

        private string SelectFolder()
        {
            DialogResult result = selectOutputFolder.ShowDialog();
            if (result == DialogResult.OK)
                return selectOutputFolder.SelectedPath;
            else
                return null;
        }

        private void UpdateFileList(string newItem, ComboBox box, int maxLen, List<string> list)
        {
            if (string.IsNullOrEmpty(newItem))
                return;
            int n = box.Items.IndexOf(newItem);
            if (n != 0)
            {
                box.Items.Remove(newItem);
                box.Items.Insert(0, newItem);
                while (box.Items.Count > maxLen)
                {
                    box.Items.RemoveAt(box.Items.Count - 1);
                }
                box.SelectedIndex = 0;
                list.Clear();
                for (int i = 0; i < box.Items.Count - 1; i++)
                {
                    list.Add(box.Items[i].ToString());
                }
            }
        }

        private void OpenExcel(string filePath)
        {
            if (openedExcel == filePath)
            {
                return;
            }
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在！");
                excelFileList.Items.Remove(filePath);
                DurableCfg.Cfg.excelFiles.Remove(filePath);
                return;
            }
            if (app == null)
            {
                app = new ExcelApp();
            }
            openedExcel = filePath;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            dataPreviewGroup.Text = fileName + (ClassModel.IsValidName(fileName)?"":" (不合法)");
            app.Workbooks.Close();
            book = app.Workbooks.Open(excelFileList.Text);
            sheet = book.Worksheets.Item[1];
            classModel = new ClassModel(nameSpace, fileName, sheet);
            if (classModel.IsVaild)
            {
                tableFields.Rows.Clear();
                for (int i = 0; i < classModel.Fields.Count; i++)
                {
                    ClassModel.Field f = classModel.Fields[i];
                    DataRow row = tableFields.NewRow();
                    row["field"] = f.name;
                    row["type"] = f.primType;
                    row["comment"] = f.comment;
                    tableFields.Rows.Add(row);
                }
            }
        }

        void InitClassModel()
        {

        }

        private void TableGenerater_FormClosing(object sender, FormClosingEventArgs e)
        {
            DurableCfg.Save();
            if (app != null)
            {
                app.Workbooks.Close();
                app.Quit();
            }
        }

        private void excelFileList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (excelFileList.SelectedIndex == excelFileList.Items.Count - 1)
            {
                SelectExcele();
            }
            else if(excelFileList.SelectedIndex >= 0)
            {
                OpenExcel(excelFileList.Text);
                UpdateFileList(excelFileList.Text, excelFileList,10, DurableCfg.Cfg.excelFiles);
            }
        }

        private void btnGenerateCsharp_Click(object sender, EventArgs e)
        {
            if(classModel != null)
            {
                csharpOutput = csharpOutputList.Text ?? "";
                ClassModel.Errors error = classModel.GenerateCSharp(csharpOutput);
                if (error != 0)
                {
                    if ((error & ClassModel.Errors.no_folder) != 0)
                    {
                        DurableCfg.Cfg.csharpOutput.Remove(csharpOutput);
                        csharpOutputList.Items.Remove(csharpOutput);
                    }
                    MessageBox.Show("出错: " + ((int)error).ToString("0000x"), "错误");
                }
                else
                {
                    MessageBox.Show("生成C#代码已完成");
                }
            }
            else
            {
                MessageBox.Show("没有任何Excel文件打开.", "错误");
            }
        }

        private void csharpOutputList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (csharpOutputList.SelectedIndex == csharpOutputList.Items.Count - 1)
            {
                csharpOutput = SelectFolder() ?? csharpOutput;
                if (!string.IsNullOrEmpty(csharpOutput))
                {
                    UpdateFileList(csharpOutput, csharpOutputList, 5, DurableCfg.Cfg.csharpOutput);
                    csharpOutputList.SelectedIndex = 0;
                }
                else
                {
                    csharpOutputList.SelectedIndex = -1;
                }
            }
            else
            {
                UpdateFileList(csharpOutputList.Text, csharpOutputList, 5, DurableCfg.Cfg.csharpOutput);
            }
        }

        private void jsonOutputList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (jsonOutputList.SelectedIndex == jsonOutputList.Items.Count - 1)
            {
                jsonOutput = SelectFolder() ?? jsonOutput;
                if (!string.IsNullOrEmpty(jsonOutput))
                {
                    UpdateFileList(jsonOutput, jsonOutputList, 5, DurableCfg.Cfg.jsonOutput);
                    jsonOutputList.SelectedIndex = 0;
                }
                else
                {
                    jsonOutputList.SelectedIndex = -1;
                }
            }
            else
            {
                UpdateFileList(jsonOutputList.Text, jsonOutputList, 5, DurableCfg.Cfg.jsonOutput);
            }
        }

        private void btnGenerateJson_Click(object sender, EventArgs e)
        {
            if(classModel == null || !classModel.IsVaild)
            {
                MessageBox.Show("配表格式不正确", "错误");
            }
            else if (app != null && File.Exists(openedExcel ?? "") && File.Exists(jsonOutputList.Text ?? ""))
            {
                if (app.Workbooks.Count == 0)
                {
                    book = app.Workbooks.Open(excelFileList.Text);
                    sheet = book.Worksheets.Item[1];
                }
                jsonOutput = jsonOutputList.Text;
                GenerateProgress genWin = new GenerateProgress(classModel, sheet, jsonOutput);
                genWin.ShowDialog(this);
            }
            else
            {
                MessageBox.Show("无法打开文件", "错误");
            }
        }
    }
}
