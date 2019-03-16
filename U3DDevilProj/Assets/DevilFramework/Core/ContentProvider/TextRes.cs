using LitJson;
namespace Devil.ContentProvider
{
    public class TextRes : TableBase
	{
		public string text {get; private set;}
		public string voice {get; private set;}
		public override void Init(JsonData obj)
		{
			base.Init(obj);
			text = (string)obj["text"];
			voice = (string)obj["voice"];
		}
	}
}