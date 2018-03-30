using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Devil.AI
{
    [BTComposite(Title = "黑板", Detail = "黑板设置了属性 {property}", IconPath = "Assets/DevilFramework/Editor/Icons/blackboard.png")]
    public class BTBlackboardSetCondtion : BTConditionBase
    {
        [BTVariable(Name = "property", TypePattern = "name")]
        string mBlackboardVar;
        int mBlackboardId;

        public BTBlackboardSetCondtion(int id) : base(id) { }

        public override bool IsTaskOnCondition(BehaviourTreeRunner btree)
        {
            return btree.Blackboard.IsPropertySet(mBlackboardId);
        }

        public override bool IsTaskRunnable(BehaviourTreeRunner btree)
        {
            return btree.Blackboard.IsPropertySet(mBlackboardId);
        }

        public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            mBlackboardVar = obj.Value<string>("property");
            mBlackboardId = btree.Blackboard.GetPropertyId(mBlackboardVar);
        }
    }
}