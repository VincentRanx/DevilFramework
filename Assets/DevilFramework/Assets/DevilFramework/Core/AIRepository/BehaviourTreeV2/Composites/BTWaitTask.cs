using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Devil.AI
{
    [BTComposite(Title = "等待", Detail = "等待{timeForWait}秒钟\n始终返回成功：{alwaysSuccess}", HideProperty = true)]
    public class BTWaitTask : BTTaskBase
    {
        [BTVariable(Name = "timeForWait", DefaultVallue = "1")]
        float mWaitTime = 1;
        [BTVariable(Name = "alwaysSuccess", DefaultVallue = "false")]
        bool mAlwaysSucces;

        bool mAbort;

        public BTWaitTask(int id) : base(id) { }

        public override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }

        public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            mWaitTime = obj.Value<float>("timeForWait");
            mAlwaysSucces = obj.Value<bool>("alwaysSuccess");
        }

        public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
        {
            mAbort = false;
            return EBTTaskState.running;
        }

        public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
        {
            if (mAbort)
                return mAlwaysSucces ? EBTTaskState.success : EBTTaskState.faild;
            return btree.TaskTime >= mWaitTime ? EBTTaskState.success : EBTTaskState.running;
        }
    }
}