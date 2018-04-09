using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;

[BTComposite(Title = "移动", Detail = "朝着黑板中的 targetPos 位置移动")]
public class MoveToTask : BTTaskBase
{
    [BTVariable(DefaultVallue = "")]
    string mBlackboardTarget;
    float mUpdateInterval = .1f;
    [BTVariable(DefaultVallue = "2")]
    float mStopDistance = 2;
    [BTVariable(DefaultVallue = "0.6")]
    float mSpeedPercentage = 2;

    float mTimer;
    bool mAbort;
    BTBlackboardGetter<Vector3> mTarget;
    NavMeshPath mPath;
    Vector3 mTargetPos;
    PlayerController mPlayer;
    int mCornnerPtr;
    BTBlackboardGetter<Transform> mTargetTrans;

    public MoveToTask(int id) : base(id) { }

    public override void OnAbort(BehaviourTreeRunner btree)
    {
        mAbort = true;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mTarget = btree.Blackboard.Getter<Vector3>("targetPos");
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mStopDistance = obj.Value<float>("mStopDistance");
        mSpeedPercentage = obj.Value<float>("mSpeedPercentage");
        mBlackboardTarget = obj.Value<string>("mBlackboardTarget");
        if (!string.IsNullOrEmpty(mBlackboardTarget))
            mTargetTrans = btree.Blackboard.Getter<Transform>(mBlackboardTarget);
        mPlayer = btree.GetComponent<PlayerController>();
        mPath = new NavMeshPath();
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        if (!mTarget.IsSet && mTargetTrans == null)
            return EBTTaskState.faild;
        mAbort = false;
        mTimer = mUpdateInterval;
        Debug.Log("move to " + mTarget.Value);
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        if (mAbort)
        {
            return EBTTaskState.faild;
        }
        mTimer += deltaTime;
        if (mTimer >= mUpdateInterval)
        {
            if (mTargetTrans != null)
            {
                Transform trans = mTargetTrans.Value;
                if (trans == null)
                    return EBTTaskState.faild;
                mTargetPos = trans.position;
            }
            else
            {
                mTargetPos = mTarget.Value;
            }
            mPath.ClearCorners();
            NavMesh.CalculatePath(mPlayer.transform.position, mTargetPos, 1, mPath);
            mCornnerPtr = 1;
            mTimer = 0;
        }
        Vector3[] points = mPath.corners;
        if (mCornnerPtr < points.Length)
        {
            Vector3 p = points[mCornnerPtr];
            Vector3 dir = p - mPlayer.transform.position;
            mPlayer.Move(dir.normalized * mSpeedPercentage * Mathf.Min(1, dir.magnitude * 3));
            float rad;
            if (mCornnerPtr == points.Length - 1 && mPath.status == NavMeshPathStatus.PathComplete )
            {
                rad = mStopDistance * mStopDistance;
            }
            else
            {
                rad = 0.009f;
            }
            if(dir.sqrMagnitude < rad)
            {
                mCornnerPtr++;
            }
            return EBTTaskState.running;
        }
        else
        {
            if(mPath.status == NavMeshPathStatus.PathPartial)
            {
                mPlayer.Move(mPlayer.transform.forward * mSpeedPercentage);
            }
            return mPath.status == NavMeshPathStatus.PathComplete ? EBTTaskState.success : EBTTaskState.running;
        }
    }
}
