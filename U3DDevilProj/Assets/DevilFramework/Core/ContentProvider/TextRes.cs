using Newtonsoft.Json.Linq;
namespace Devil.ContentProvider
{
    public class TextRes : TableBase
	{
		public string text {get; private set;}
		public string voice {get; private set;}
		public override void Init(JObject obj)
		{
			base.Init(obj);
			text = obj.Value<string>("text");
			voice = obj.Value<string>("voice");
		}
	}
}