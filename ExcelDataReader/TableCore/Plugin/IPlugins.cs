using Newtonsoft.Json.Linq;
using System.Xml;

namespace TableCore.Plugin
{
    // 数据导出
    public interface IExportData
    {
        string ExportExcelFile { get; }
        string ExportExcelSheet { get; }
        int ExcelStartRow { get; }
        int ExcelStartCol { get; }
        JObject ExportData { get; }
    }

    // 数据类型输出格式化
    public interface IGenFormater
    {
        void Init(XmlElement element);
        bool IsValid(string data);
        IExportData ExportData(string data, string comment);
        JToken Format(string data, GTOutputCfg catgory);
    }

    // 数据输出时格式化
    public interface IDataModify
    {
        void Init(XmlElement element);

        bool PrepareTable(GTStatus status, string catgory, string tableName);

        GenData[] FixOutputData(string catgory, GenData data, GenData previours, GenData next);
    }
    
}
