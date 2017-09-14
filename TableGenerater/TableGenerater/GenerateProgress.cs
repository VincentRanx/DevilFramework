using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        ExcelSheet sheet;
        int totalNum;
        Thread t;
        string output;
        HashSet<string> ids;

        string currentId;

        public GenerateProgress(ClassModel model, ExcelSheet sheet, string output)
        {
            InitializeComponent();
            ids = new HashSet<string>();
            this.output = output;
            genProgress.Value = 0;
            this.model = model;
            this.sheet = sheet;
            totalNum = Math.Max(1, sheet.UsedRange.Rows.Count);
            t = new Thread(OnGenJson);
            t.Start();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            t.Abort();
            Close();
            Dispose();
        }

        void OnGenJson()
        {
            int col = model.Fields.Count;
            ExcelRange range;
            StringBuilder fstr = new StringBuilder();
            int row = 4;
            while (true)
            {
                range = sheet.Cells[row, 2];
                if (string.IsNullOrEmpty(range.Text))
                    break;
                Action refresh = () =>
                {
                    genProgress.Value = Math.Min(totalNum, row ) * 100 / totalNum;
                };
                Invoke(refresh);

                fstr.Append("{");
                ClassModel.Field f = model.Fields[0];
                fstr.Append("\"").Append(f.name).Append("\":");
                fstr.Append(f.GetJsonValue(range.Text));
                if(f.name == "id" && !ids.Add(range.Text))
                {
                   
                    currentId = range.Text;
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
                    range = sheet.Cells[row, i + 2];
                    fstr.Append("\"").Append(f.name).Append("\":");
                    fstr.Append(f.GetJsonValue(range.Text));
                }
                fstr.Append("}\n");
                row++;
            }
            File.WriteAllText(Path.Combine( output, model.ClassName+".txt"), fstr.ToString());
            Action act = () =>
            {
                Close();
                Dispose();
                MessageBox.Show("生成数据已完成");
            };
            Invoke(act);
        }

    }
}
