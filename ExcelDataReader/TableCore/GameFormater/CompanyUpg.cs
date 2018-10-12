using System;
using System.Text.RegularExpressions;
using System.Xml;
using TableCore.Plugin;

namespace TableCore.GameFormater
{
    public class CompanyUpg : IDataModify
    {
        int mStage = 10000;
        int mMin = 1;
        int mMax = 100;
        string mTablePattern = "^$";

        ClassModel mMod;
       
        public static HugeNumber Income(int lv, float a, HugeNumber b,  HugeNumber f)
        {
            HugeNumber num = lv;
            num.Multiply(f);
            return num;
        }

        public static HugeNumber Investment(int lv, float a, HugeNumber b,  HugeNumber f)
        {
            if (b != 0)
            {
                HugeNumber pow = new HugeNumber();
                pow.SetValue(a.ToString(), 7);
                pow.Pow(lv - 1, 7);
                pow.Multiply(b);
                pow.MoveRight(7);
                return pow;
            }
            else
            {
                return 0;
            }
        }

        public bool PrepareTable(GTStatus status, string catgory, string tableName)
        {
            if (Regex.IsMatch(tableName, mTablePattern))
            {
                var properties = new ClassModel.Property[5];
                var cfg = status.Config.ActiveClass;
                properties[0] = new ClassModel.Property("id", cfg.GetGTType("int"), "id");
                properties[1] = new ClassModel.Property("income", cfg.GetGTType("decimal"), "income");
                properties[2] = new ClassModel.Property("time", cfg.GetGTType("float"), "time");
                properties[3] = new ClassModel.Property("price", cfg.GetGTType("decimal"), "price");
                properties[4] = new ClassModel.Property("reward", cfg.GetGTType("int"), "reward");
                mMod = new ClassModel("Company_Upgrade", properties);
                return true;
            }
            else
            {
                return false;
            }
        }

        public GenData[] FixOutputData(string catgory, GenData data, GenData previours, GenData next)
        {
            int stage = data.id / mStage;
            float a;
            HugeNumber b;
            HugeNumber f;
            a = float.Parse(data.GetProperty("a"));
            b = new HugeNumber(data.GetProperty("b"));
            f = new HugeNumber(data.GetProperty("f"));

            int id0, id1;
            if (previours == null || previours.id / mStage != stage)
                id0 = mMin;
            else
                id0 = data.id % mStage;
            if (next == null || next.id / mStage != stage)
                id1 = Math.Max(data.id % mStage, mMax);
            else
                id1 = next.id % mStage - 1;
            var datas = new GenData[id1 - id0 + 1];
            for (int i = 0; i < datas.Length; i++)
            {
                var dt =// new GenData(mMod);
                          data.Copy(i + id0 + stage * mStage);
                //dt.id = i + id0 + stage * mStage;
                //dt.SetProperty("income", Income(id0 + i, a, b, f).ToString(), "A" + dt.id);
                //dt.SetProperty("time", data.GetProperty("time"), "B" + dt.id);
                //dt.SetProperty("price", Investment(id0 + i, a, b, f).ToString(), "C" + dt.id);
                //dt.SetProperty("reward", data.GetProperty("feedback"), "D" + dt.id);
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

    }
}
