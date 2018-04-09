using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[BTSharedType]
[BTComposite(Title = "测试行为树", Detail = "hi收到附件是浪费深刻搭街坊")]
public class TestTask : BTTaskBase // BTServiceBase, BTConditionBase, BTNodeBase
{
    [BTVariable( Name = "INFO." , DefaultVallue = "Default Info")]
    string mInformation;

    BTBlackboardSetter<Vector3> mTargetSetter;

    bool mAbort;
    public TestTask(int id) : base(id) { }

    public override void OnAbort(BehaviourTreeRunner btree)
    {
        mAbort = true;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mTargetSetter = btree.Blackboard.Setter<Vector3>("test");
        mInformation = obj.Value<string>("INFO.");
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        mAbort = false;
        Debug.Log("Start Test Task: " + mInformation);
        mTargetSetter.SetValue(new Vector3(10, 201, 10));
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        if (mAbort)
            return EBTTaskState.faild;
        return EBTTaskState.running;
    }
}
