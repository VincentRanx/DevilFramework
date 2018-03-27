using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Devil.AI
{
    public class BTSubTreeTask : BTNodeBase
    {
        string mAssetPath;
        BehaviourLooper mLooper;
        public BTSubTreeTask(int id) : base(id) { }

        public override void InitData(BehaviourTreeRunner btree, string jsonData)
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

        public override BTNodeBase ChildForVisit { get { return null; } }

        protected override void OnReturnWithState(EBTTaskState state)
        {
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mLooper != null)
            {
                mLooper.Update(behaviourTree, deltaTime);
                if (mLooper.IsComplate)
                {
                    State = mLooper.State;
                }
            }
            else
            {
                State = EBTTaskState.faild;
            }
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            if(mLooper != null)
            {
                mLooper.Reset();
            }
            State = EBTTaskState.running;
        }
    }
}