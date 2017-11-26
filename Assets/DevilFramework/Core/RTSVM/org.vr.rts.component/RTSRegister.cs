using System.Collections.Generic;

namespace org.vr.rts.component
{

    public class RTSRegister
    {

        private Dictionary<string, object> mVars;
        public Dictionary<string,object> Vars { get { return mVars; } }

        public RTSRegister()
        {
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
                mVars = new Dictionary<string, object>();
            mVars[varName] = var;
        }

        public void clearVars()
        {
            if (mVars != null)
            {
                mVars.Clear();
            }
        }

    }
}
