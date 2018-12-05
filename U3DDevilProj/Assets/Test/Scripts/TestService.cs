using Devil.AI;
using UnityEngine;

namespace GameLogic
{
    public class TestService : BTServiceAsset
    {
        public override void OnStart()
        {
            Debug.Log("Test Service Started");
        }

        public override void OnStop()
        {
            Debug.Log("Test Service Stoped");
        }

        public override void OnUpdate(float deltaTime)
        {
        }
    }
}