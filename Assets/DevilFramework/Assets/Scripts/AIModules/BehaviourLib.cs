using Devil.AI;

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
        mControllers["BTSubTreeTask"] = (id) => new BTSubTreeTask(id);
        mConditions["BTFoundTarget"] = (id) => new BTFoundTarget(id);
        mConditions["BTBlackboardSetCondtion"] = (id) => new BTBlackboardSetCondtion(id);
        mConditions["BTCoolDownCondition"] = (id) => new BTCoolDownCondition(id);
        mServices["BTFindTargetService"] = (id) => new BTFindTargetService(id);
        mTasks["BTWalkAway"] = (id) => new BTWalkAway(id);
        mTasks["BTFindTargetTask"] = (id) => new BTFindTargetTask(id);
        mTasks["BTMoveTo"] = (id) => new BTMoveTo(id);

    }
}