using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Xml;

namespace TableCore.Plugin
{
    public class DecimalDataFormater : IGenFormater
    {
        StringBuilder buffer;

        public void Init(XmlElement element)
        {
            buffer = new StringBuilder(30);
        }

        public bool IsValid(string input)
        {
            return true;
        }

        public JToken Format(string input, GTOutputCfg category)
        {
            decimal d = Decimal.Parse(input, System.Globalization.NumberStyles.Float);// Convert.ToDecimal(input.ToString());
            buffer.Clear();
            long l = (long)d;// long.Parse(input, System.Globalization.NumberStyles.AllowExponent);
            bool nig = l < 0;
            if(nig)
            {
                l = -l;
            }
            while (l > 0)
            {
                buffer.Insert(0, l % 10);
                l /= 10;
            }
            if (nig)
                buffer.Insert(0, '-');
            else if (buffer.Length == 0)
                buffer.Append(0);
            return buffer.ToString();
        }
        
        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
