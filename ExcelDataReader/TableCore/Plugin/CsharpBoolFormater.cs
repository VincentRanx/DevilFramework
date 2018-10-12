using Newtonsoft.Json.Linq;
using System.Xml;

namespace TableCore.Plugin
{
    public class CsharpBoolFormater : IGenFormater
    {
        public void Init(XmlElement element)
        {

        }

        public bool IsValid(string input)
        {
            string str = input.ToLower();
            return str == "true" || str == "false";
        }

        public JToken Format( string input, GTOutputCfg category)
        {
            return input.ToLower() == "true" ? true : false;
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
