using Devil.AI;
using GameExt;

public class BehaviourLib : BehaviourLibrary
{
    public BehaviourLib() : base()
    {

    }

    protected override void OnInit()
    {
        mTasks["BTCoolDownTask"] = () => new BTCoolDownTask();
        mConditions["BTCondtionTrue"] = () => new BTCondtionTrue();
        mServices["BTFindTargetArroundPlayerService"] = () => new BTFindTargetArroundPlayerService();
        mServices["BTFindTargetService"] = () => new BTFindTargetService();
        mControllers["BTRandom"] = (id) => new BTRandom(id);
        mControllers["BTParralel"] = (id) => new BTParralel(id);
        mControllers["BTSelector"] = (id) => new BTSelector(id);
        mControllers["BTSequence"] = (id) => new BTSequence(id);

    }
}