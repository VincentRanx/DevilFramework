using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchTask : BTTaskBase
{
    [BTVariable(DefaultVallue = "3")]
    float mSearchTime;
    float mTimer;
    bool mAbort;
    float mSign;

    public SearchTask(int id) : base(id) { }

    public override void OnAbort(BehaviourTreeRunner btree)
    {
        mAbort = true;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mSearchTime = obj.Value<float>("mSearchTime");
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        mAbort = false;
        mTimer = 0;
        mSign = Mathf.Sign(Random.value - 0.5f);
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        if (mAbort)
            return EBTTaskState.success;
        mTimer += deltaTime;
        if (mTimer > mSearchTime)
            return EBTTaskState.success;
        btree.transform.Rotate(Vector3.up, mSign * 100 * deltaTime);
        return EBTTaskState.running;
    }
}
