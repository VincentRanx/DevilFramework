using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Devil.AI
{

    [CreateAssetMenu(fileName = "ReflectionBaseBehaviourAsset", menuName = "AI/Reflection Behaviour Tree Asset")]
    public class ReflectionBehaviourTreeAsset : BehaviourTreeAsset
    {

        public override void GetDecoratorNames(ICollection<string> decorators)
        {
            object[] mthds = Ref.GetMethodsWithParams(GetType(), typeof(IBTDecorator), null);
            if (mthds != null)
            {
                for (int i = 0; i < mthds.Length; i++)
                {
                    decorators.Add((mthds[i] as MethodInfo).Name);
                }
            }
        }

        public override IBTDecorator GetDecoratorByName(string decorator)
        {
            MethodInfo mtd = GetType().GetMethod(decorator);
            if (mtd != null)
            {
                return mtd.Invoke(this, null) as IBTDecorator;
            }
            else
            {
                return null;
            }
        }

        public override void GetServiceNames(ICollection<string> services)
        {
            object[] mthds = Ref.GetMethodsWithParams(GetType(), typeof(IBTService), null);
            if (mthds != null)
            {
                for (int i = 0; i < mthds.Length; i++)
                {
                    services.Add((mthds[i] as MethodInfo).Name);
                }
            }
        }

        public override IBTService GetServiceByName(string service)
        {
            MethodInfo mtd = GetType().GetMethod(service);
            if (mtd != null)
            {
                return mtd.Invoke(this, null) as IBTService;
            }
            else
            {
                return null;
            }
        }

        public override void GetTaskNames(ICollection<string> tasks)
        {
            object[] mthds = Ref.GetMethodsWithParams(GetType(), typeof(IBTTask), null);
            if (mthds != null)
            {
                for (int i = 0; i < mthds.Length; i++)
                {
                    tasks.Add((mthds[i] as MethodInfo).Name);
                }
            }
        }

        public override IBTTask GetTaskByName(string taskName)
        {
            MethodInfo mtd = GetType().GetMethod(taskName);
            if (mtd != null)
            {
                return mtd.Invoke(this, null) as IBTTask;
            }
            else
            {
                return null;
            }
        }


        // Test

        public IBTDecorator Success()
        {
            return new Decorator(true);
        }

        public IBTDecorator Faild()
        {
            return new Decorator(false);
        }

        public IBTService ServiceDemo()
        {
            return null;
        }
        
        public IBTTask Wait1Sec()
        {
            return new TaskCoolDown(1);
        }
        public IBTTask Wait2Sec()
        {
            return new TaskCoolDown(2);
        }

        public IBTTask Wait3Sec()
        {
            return new TaskCoolDown(3);
        }

        public IBTTask Wait4Sec()
        {
            return new TaskCoolDown(4);
        }

        public IBTTask Wait5Sec()
        {
            return new TaskCoolDown(5); ;
        }

        // test
        public IBTDecorator DecoratorA()
        {
            return null;
        }
        public IBTDecorator DecoratorB()
        {
            return null;
        }
        public IBTDecorator DecoratorC()
        {
            return null;
        }
        public IBTDecorator DecoratorD()
        {
            return null;
        }
        public IBTDecorator DecoratorE()
        {
            return null;
        }
        public IBTDecorator DecoratorF()
        {
            return null;
        }
        public IBTDecorator DecoratorG()
        {
            return null;
        }
        public IBTService ServiceA()
        {
            return null;
        }
        public IBTService ServiceB()
        {
            return null;
        }
        public IBTService ServiceC()
        {
            return null;
        }
        public IBTService ServiceD()
        {
            return null;
        }
        public IBTService ServiceE()
        {
            return null;
        }
        public IBTService ServiceF()
        {
            return null;
        }
        public IBTService ServiceG()
        {
            return null;
        }
    }

}