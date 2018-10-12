using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "冷却", Detail = "等待{time}秒后重新执行", HideProperty = true)]
    public class BTCoolDown : BTConditionBase
    {
        [BTVariable(Name = "time", DefaultVallue = "1")]
        float mCoolTime;
        float mTime;

        public BTCoolDown(int id) : base(id) { }

        public override bool IsTaskOnCondition(BehaviourTreeRunner btree)
        {
            mTime = Time.time;
            return true;
        }

        public override bool IsTaskRunnable(BehaviourTreeRunner btree)
        {
            if(mTime + mCoolTime < Time.time)
            {
                mTime = Time.time;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            mCoolTime = obj.Value<float>("time");
            mTime = Time.time - mCoolTime;
        }

        public override void OnClearData(BehaviourTreeRunner btree)
        {
        }
    }
}