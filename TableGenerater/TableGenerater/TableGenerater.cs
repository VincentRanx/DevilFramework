using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

using System.Configuration;
using System.Collections.Generic;

namespace TableGenerater
{

    public partial class TableGenerater : Form
    {
        private string csharpOutput;
        private string jsonOutput;
        private string nameSpace;
        private ClassModel classModel;
        private ExcelReader excel;

        public TableGenerater()
        {
            InitializeComponent();
            excel = new ExcelReader();
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
            excel.Visible = true;
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
                excelFileList.SelectedIndex = string.IsNullOrEmpty(excel.CurrentExcel) ? -1 : excelFileList.Items.IndexOf(excel.CurrentExcel);
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
            Errors error = excel.OpenExcel(filePath);
            if(error != 0)
            {
                DisplayError(error);
                return;
            }
            dataPreviewGroup.Text = excel.FileName + (ClassModel.IsValidName(excel.FileName) ? "" : " (不合法)");
            classModel = new ClassModel(nameSpace, excel.FileName, excel);
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

        private void TableGenerater_FormClosing(object sender, FormClosingEventArgs e)
        {
            DurableCfg.Save();
            excel.Close();
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

        private void btnGenerateCsharp_Click(object sender, EventArgs e)
        {
            if (classModel != null)
            {
                csharpOutput = csharpOutputList.Text ?? "";
                Errors error = classModel.GenerateCSharp(csharpOutput);
                if (error != 0)
                {
                    DisplayError(error);
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

        private void btnGenerateJson_Click(object sender, EventArgs e)
        {
            if(classModel == null || !classModel.IsVaild)
            {
                DisplayError(Errors.no_file | (classModel == null ? 0 : classModel.Error));
            }
            else
            {
                Errors error = excel.Validate();
                jsonOutput = jsonOutputList.Text;
                if (!Directory.Exists(jsonOutput))
                {
                    error |= Errors.no_folder;
                }
                if (error != 0)
                {
                    DisplayError(error);
                    return;
                }
                GenerateProgress genWin = new GenerateProgress(classModel, excel, jsonOutput);
                DialogResult result = genWin.ShowDialog(this);
                if(result == DialogResult.OK)
                {
                    MessageBox.Show("生成数据已完成");
                }
            }
           
        }

        public void DisplayError(Errors error)
        {
            MessageBox.Show("Error: " + error.ToString("0000X"));
        }
    }
}
