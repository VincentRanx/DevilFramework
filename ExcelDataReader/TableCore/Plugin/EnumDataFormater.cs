using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class EnumDataFormater : IGenFormater
    {
        string[] mEnums;
        int[] mValues;
        string mNumPattern = @"^(\-|\+)?\d+$";

        public void Init(XmlElement element)
        {
            string arg = element.GetAttribute("enums");
            if (string.IsNullOrEmpty(arg))
                mEnums = new string[0];
            else
                mEnums = arg.Split(',');
            arg = element.GetAttribute("values");
            if (string.IsNullOrEmpty(arg))
            {
                mValues = new int[mEnums.Length];
                for (int i = 0; i < mValues.Length; i++)
                    mValues[i] = i;
            }
            else
            {
                mValues = new int[mEnums.Length];
                string[] values = arg.Split(',');
                int n = 0;
                for(int i = 0; i < mValues.Length; i++)
                {
                    if (i < values.Length)
                        n = int.Parse(values[i]);
                    else
                        n++;
                    mValues[i] = n;
                }
            }
        }

        public bool IsValid(string input)
        {
            if (Regex.IsMatch(input, mNumPattern))
                return true;
            for (int i = 0; i < mEnums.Length; i++)
            {
                if (EqualV(mEnums[i], input))
                    return true;
            }
            return false;
        }

        public JToken Format(string input, GTOutputCfg category)
        {
            int num;
            if (int.TryParse(input, out num))
                return num;
            for(int i = 0; i < mEnums.Length; i++)
            {
                if (EqualV(mEnums[i], input))
                    return mValues[i];
            }
            return 0;
        }

        bool EqualV(string a, string b)
        {
            return a.ToLower() == b.ToLower();
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
