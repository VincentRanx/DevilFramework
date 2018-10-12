using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TableCore.Plugin
{
    public class JsonDataFormater : IGenFormater
    {
        public IExportData ExportData(string data, string comment)
        {
            return null;
        }

        public JToken Format(string data, GTOutputCfg catgory)
        {
            return JsonConvert.DeserializeObject<JToken>(data);
        }

        public void Init(XmlElement element)
        {
            
        }

        public bool IsValid(string data)
        {
            return true;
        }
    }
}
