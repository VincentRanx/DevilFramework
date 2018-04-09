using UnityEngine;
using Devil.AI;


[DefaultExecutionOrder(-100)]
public class BehaviourLib : BehaviourLibrary
{
    public BehaviourLib() : base()
    {

    }

    protected override void OnInit()
    {
        mControllers["BTParralel"] = (id) => new BTParralel(id);
        mControllers["BTSelector"] = (id) => new BTSelector(id);
        mControllers["BTSequence"] = (id) => new BTSequence(id);
        mConditions["BTBlackboardSetCondtion"] = (id) => new BTBlackboardSetCondtion(id);
        mConditions["CoolDownCondition"] = (id) => new CoolDownCondition(id);
        mServices["FindTargetService"] = (id) => new FindTargetService(id);
        mTasks["PatrolTask"] = (id) => new PatrolTask(id);
        mTasks["SearchTask"] = (id) => new SearchTask(id);
        mTasks["TestTask"] = (id) => new TestTask(id);
        mTasks["DisplayTask"] = (id) => new DisplayTask(id);
        mTasks["CheerupTask"] = (id) => new CheerupTask(id);
        mTasks["LookAtTask"] = (id) => new LookAtTask(id);
        mTasks["MoveToTask"] = (id) => new MoveToTask(id);
        mTasks["BTSubTreeTask"] = (id) => new BTSubTreeTask(id);
        mTasks["BTWaitTask"] = (id) => new BTWaitTask(id);

    }
}