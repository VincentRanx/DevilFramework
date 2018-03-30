using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "运行行为树", Detail = "以其他行为树作为任务")]
    public class BTSubTreeTask : BTTaskBase
    {
        [BTVariable(Name = "asset")]
        string mAssetPath;

        BehaviourLooper mLooper;
        public BTSubTreeTask(int id) : base(id) { }
        
        public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            mAssetPath = obj.Value<string>("asset");

            BehaviourTreeAsset asset = Resources.Load<BehaviourTreeAsset>(mAssetPath);
            if (!asset)
            {
                Debug.LogError("Can't load behaviour asset: " + mAssetPath);
                return;
            }
            BTNodeBase tree = asset.CreateBehaviourTree(btree);
            mLooper = new BehaviourLooper(tree);
        }

        public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
        {
            if (mLooper == null)
            {
                return EBTTaskState.faild;
            }
            else
            {
                mLooper.Reset();
                return EBTTaskState.running;
            }
        }

        public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
        {
            if (mLooper != null)
            {
                mLooper.Update(btree, deltaTime);
                if (mLooper.IsComplate)
                {
                    return mLooper.State;
                }
                else
                {
                    return EBTTaskState.running;
                }
            }
            else
            {
                return EBTTaskState.faild;
            }
        }

        public override void OnAbort(BehaviourTreeRunner btree)
        {
            if(mLooper != null && mLooper.RuntimeNode != null)
            {
                mLooper.RuntimeNode.Abort(btree);
            }
        }
        
    }
}