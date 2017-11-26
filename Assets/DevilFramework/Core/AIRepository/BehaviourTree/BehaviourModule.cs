using DevilTeam.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    public class BehaviourModule : MonoBehaviour
    {

        public virtual IBTNode GetBehaviourNode(BehaviourGraph.BehaviourNode node)
        {
#if UNITY_EDITOR
            try
            {
#endif
                switch (node.BehaviourType)
                {
                    case EBTNode.queue:
                        return new BTQueueExec(node);
                    case EBTNode.sequence:
                        return new BTSequenceExec(node);
                    case EBTNode.selector:
                        return new BTSelectorExec(node);
                    case EBTNode.patrol:
                        BTPatrolExec patrol = new BTPatrolExec(node);
                        patrol.OnBehaviourTick = Delegate.CreateDelegate(typeof(BehaviourTick), this, node.ModuleName) as BehaviourTick;
                        return patrol;
                    case EBTNode.behaviour:
                        BTBehaviourExec behav = new BTBehaviourExec(node);
                        behav.OnBehaviourTick = Delegate.CreateDelegate(typeof(BehaviourTick), this, node.ModuleName) as BehaviourTick;
                        return behav;
                    case EBTNode.condition:
                        BTConditionExec becon = new BTConditionExec(node);
                        becon.IsOnCondition = Delegate.CreateDelegate(typeof(ConditionTick), this, node.ModuleName) as ConditionTick;
                        return becon;
                    default:
                        return GetCustomBehaviourNode(node);
                }
#if UNITY_EDITOR
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("Faild to bind behaviour module: {0}.{1}\n[Exception] {2}",
                    GetType().Name, node.ModuleName, exception), gameObject);
                return null;
            }
#endif
        }

        protected virtual IBTNode GetCustomBehaviourNode(BehaviourGraph.BehaviourNode node)
        {
            return null;
        }

#if UNITY_EDITOR

        public virtual void __getBehaviourModules(ICollection<string> modules)
        {
            Type[] argType = new Type[] { typeof(BTCustomData) };
            object[] mtds = Ref.GetMethodsWithParams(GetType(), typeof(EBTState), argType);
            foreach (var mtd in mtds)
            {
                System.Reflection.MethodInfo info = mtd as System.Reflection.MethodInfo;
                modules.Add(info.Name);
            }
        }

        public virtual void __getConditionModules(ICollection<string> modules)
        {
            Type[] argType = new Type[] { typeof(BTCustomData) };
            object[] mtds = Ref.GetMethodsWithParams(GetType(), typeof(bool), argType);
            foreach (var mtd in mtds)
            {
                System.Reflection.MethodInfo info = mtd as System.Reflection.MethodInfo;
                modules.Add(info.Name);
            }
        }

        public virtual void __getCustomModules(ICollection<string> modules)
        {

        }

#endif
    }
}