using Devil.Utility;

namespace Devil.AsyncTask
{
    public class ThreadPoolState
    {
        public string name;
        public bool isAlive;

        public override string ToString()
        {
            return StringUtil.Concat(name, isAlive ? " is alive." : " is dead.");
        }
    }
}