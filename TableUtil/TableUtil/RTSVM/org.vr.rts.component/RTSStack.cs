using System.Collections.Generic;

namespace org.vr.rts.component
{

    /**
     * 运行时作用域
     * 
     * @author Administrator
     * 
     */
    public class RTSStack : IRTSStack
    {

        private IRTSThread mThread;
        private RTSStack mSuper;// 所属作用域
        private Dictionary<string, object> mVars;
        private int mId;

        public int getId()
        {
            return mId;
        }

        private RTSStack(IRTSThread thread)
        {
            mThread = thread;
        }

        private RTSStack(IRTSThread thread, int id)
        {
            this.mId = id;
            mThread = thread;
        }

        public IRTSThread getThread()
        {
            return mThread;
        }

        public IRTSStack getSuper()
        {
            return mSuper;
        }

        public IRTSStack makeChild(int id)
        {
            RTSStack scope = Factory.getStack(mThread, id);
            scope.mSuper = this;
            return scope;
        }

        public void onRemoved()
        {
            Factory.cacheStack(this);
        }

        public object getVar(string varName)
        {
            object ret;
            if (mVars != null && mVars.TryGetValue(varName, out ret))
                return ret;
            else
                return null;
        }

        public bool containsVar(string varName)
        {
            if (mVars != null && mVars.ContainsKey(varName))
                return true;
            else
                return false;
        }

        public void removeVar(string varName)
        {
            if (mVars != null)
            {
                mVars.Remove(varName);
            }
        }

        public void addVar(string varName, object var)
        {
            if (mVars == null)
                mVars = new Dictionary<string, object>(RTSCfg.CALL_STACK_INIT_SIZE);
            mVars[varName] = var;
        }

        public void clearVars()
        {
            if (mVars != null)
            {
                mVars.Clear();
            }
        }

        public class Factory
        {
            private static Queue<RTSStack> sStacks;

            public static RTSStack getStack(IRTSThread thread, int id)
            {
                if(sStacks == null)
                {
                    sStacks = new Queue<RTSStack>(RTSCfg.CALL_STACK_CACHE);
                }
                RTSStack sec;
                if (sStacks.Count > 0)
                {
                    sec = sStacks.Dequeue();
                    sec.mThread = thread;
                    sec.mId = id;
                }
                else
                {
                    sec = new RTSStack(thread, id);
                }
                return sec;
            }

            public static void cacheStack(RTSStack stack)
            {
                stack.clearVars();
                sStacks.Enqueue(stack);
            }
        }
    }
}
