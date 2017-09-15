using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using ExcelSheet = Microsoft.Office.Interop.Excel.Worksheet;
using ExcelRange = Microsoft.Office.Interop.Excel.Range;
using System.Text;
using System.IO;

namespace TableGenerater
{
    partial class GenerateProgress : Form
    {
        ClassModel model;
        ExcelReader excel;
        int totalNum;
        Thread t;
        string output;
        HashSet<string> ids;

        string currentId;

        public GenerateProgress(ClassModel model, ExcelReader excel, string output)
        {
            InitializeComponent();
            ids = new HashSet<string>();
            this.output = output;
            genProgress.Value = 0;
            this.model = model;
            this.excel = excel;
            totalNum = Math.Max(1, excel.Rows);
            t = new Thread(OnGenJson);
            t.Start();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            t.Abort();
            Close();
            Dispose();
        }

        void OnGenJson()
        {
            int col = model.Fields.Count;
            string range;
            StringBuilder fstr = new StringBuilder();
            int row = 4;
            while (true)
            {
                range = excel.GetCell(row, 2);
                if (string.IsNullOrEmpty(range))
                    break;
                Action refresh = () =>
                {
                    genProgress.Value = Math.Min(totalNum, row ) * 100 / totalNum;
                };
                Invoke(refresh);

                fstr.Append("{");
                ClassModel.Field f = model.Fields[0];
                fstr.Append("\"").Append(f.name).Append("\":");
                fstr.Append(f.GetJsonValue(range));
                if(f.name == "id" && !ids.Add(range))
                {
                   
                    currentId = range;
                    Action box = () => 
                    {
                        MessageBox.Show("重复的id " + currentId, "错误");
                        t.Abort();
                        Close();
                        Dispose();
                    };
                    Invoke(box);
                    return;
                }
                for (int i = 1; i < col; i++)
                {
                    fstr.Append(",");
                    f = model.Fields[i];
                    range = excel.GetCell(row, i + 2);
                    fstr.Append("\"").Append(f.name).Append("\":");
                    fstr.Append(f.GetJsonValue(range));
                }
                fstr.Append("}\n");
                row++;
            }
            File.WriteAllText(Path.Combine( output, model.ClassName+".txt"), fstr.ToString());
            Action act = () =>
            {
                Close();
                Dispose();
                DialogResult = DialogResult.OK;
            };
            Invoke(act);
        }

    }
}
