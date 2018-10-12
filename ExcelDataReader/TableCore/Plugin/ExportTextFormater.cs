using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace TableCore.Plugin
{
    public class ExportTextFormater : IGenFormater
    {
        readonly string voice_pattern = @"^\[voice:[a-zA-Z0-9_]+\]";

        public class ExportText : IExportData
        {
            public string ExportExcelFile { get; set; }

            public string ExportExcelSheet { get; set; }

            public int ExcelStartRow { get; set; }

            public int ExcelStartCol { get; set; }

            public JObject ExportData { get; set; }
            

            public ExportText(JObject data, string excelFile, string sheet,int startRow, int startCol)
            {
                ExportData = data;
                ExportExcelFile = excelFile;
                ExportExcelSheet = sheet;
                ExcelStartRow = startRow;
                ExcelStartCol = startCol;
            }

            public ExportText()
            {

            }
        }

        int mStartRow = 0;
        int mStartCol = 1;
        string mFile = "TextRes.xlsx";
        string mSheet = "TextRes_cn";

        public void Init(XmlElement element)
        {
            string s;
            mFile = element.GetAttribute("file");
            s = element.GetAttribute("sheet");
            mSheet = s;
            s = element.GetAttribute("row");
            if (!string.IsNullOrEmpty(s))
                mStartRow = int.Parse(s);
            s = element.GetAttribute("col");
            if (!string.IsNullOrEmpty(s))
                mStartCol = int.Parse(s);
        }

        public bool IsValid(string input)
        {
            return true;
        }

        public JToken Format(string input, GTOutputCfg category)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            else
            {
                Match mat = Regex.Match(input, voice_pattern);
                if (mat == null || mat.Length <= 8)
                    return input.GetHashCode();
                else
                    return input.Substring(mat.Length).GetHashCode();
            }
        }

        public IExportData ExportData(string input, string comment)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            Match mat = Regex.Match(input, voice_pattern);
            string txt;
            string voice;
            if (mat == null || !mat.Success)
            {
                txt = input;
                voice = "";
            }
            else
            {
                txt = input.Substring(mat.Length);
                voice = input.Substring(7, mat.Length - 8);
            }
            JObject data = new JObject();
            data["comment"] = comment;
            data["id"] = input.GetHashCode();
            data["text"] = txt;
            data["voice"] = voice;
            return new ExportText(data, mFile, mSheet, mStartRow, mStartCol);
        }
    }
}
