using Devil.AI;
using GameExt;

public class BehaviourLib : BehaviourLibrary
{
    public BehaviourLib() : base()
    {

    }

    protected override void OnInit()
    {
        mConditions["BTCondtionTrue"] = () => new BTCondtionTrue();
        mTasks["BTCoolDownTask"] = () => new BTCoolDownTask();
        mServices["BTFindTargetArroundPlayerService"] = () => new BTFindTargetArroundPlayerService();
        mServices["BTFindTargetService"] = () => new BTFindTargetService();
        mControllers["BTRandom"] = (id) => new BTRandom(id);
        mControllers["BTSelector"] = (id) => new BTSelector(id);
        mControllers["BTSequence"] = (id) => new BTSequence(id);

    }
}