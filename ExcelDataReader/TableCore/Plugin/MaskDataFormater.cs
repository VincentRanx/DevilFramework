using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class MaskDataFormater : IGenFormater
    {
        string all = "abcdefghigklmnopqrstuvwxyz";

        public void Init(XmlElement element)
        {

        }

        public bool IsValid(string input)
        {
            return true;
        }

        public string FormatInput(string input)
        {
            if (input == "[ALL]")
                return all;
            return input == null ? "" : input.ToLower();
        }

        public JToken Format(string input, GTOutputCfg category)
        {
            int num;
            if (int.TryParse(input, out num))
                return num;
            int n = 0;
            input = FormatInput(input);
            int len = input == null ? 0 : input.Length;
            for(int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                n |= 1 << (c - 'a');
            }
            return n;
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
