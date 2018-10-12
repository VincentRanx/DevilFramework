using System;
using System.Text.RegularExpressions;
using System.Xml;
using TableCore.Plugin;

namespace TableCore.GameFormater
{
    public class AutoFillID : IDataModify
    {
        int mStage = 10000;
        int mMin = 1;
        int mMax = 100;
        string mTablePattern = "^$";

        public GenData[] FixOutputData(string catgory, GenData data, GenData previours, GenData next)
        {
            int stage = data.id / mStage;
            int id0, id1;
            if (previours == null || previours.id / mStage != stage)
                id0 = mMin;
            else
                id0 = previours.id % mStage;
            if (next == null || next.id / mStage != stage)
                id1 = Math.Max(data.id % mStage, mMax);
            else
                id1 = next.id % mStage - 1;
            var datas = new GenData[id1 - id0 + 1];
            for (int i = 0; i < datas.Length; i++)
            {
                var dt = data.Copy(stage * mStage + i + id0);
                datas[i] = dt;
            }
            return datas;
        }

        public void Init(XmlElement element)
        {
            var attr = GTConfig.GetChildElement(element, "stage");
            if(attr != null)
                mStage = int.Parse(attr.InnerText);
            attr = GTConfig.GetChildElement(element, "min");
            if (attr != null)
                mMin = int.Parse(attr.InnerText);
            attr = GTConfig.GetChildElement(element, "max");
            if (attr != null)
                mMax = int.Parse(attr.InnerText);
            attr = GTConfig.GetChildElement(element, "table");
            if (attr != null)
                mTablePattern = attr.InnerText;
        }

        public bool PrepareTable(GTStatus status, string catgory, string tableName)
        {
            return Regex.IsMatch(tableName, mTablePattern);
        }
    }
}
