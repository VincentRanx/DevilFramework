
namespace Devil.Utility
{
    public struct JDateTime
    {
        //java t0: 62135596800000 millies
        //delta millis
        System.DateTime mTime;

        public JDateTime(System.DateTime time)
        {
            mTime = time;
        }

        public JDateTime(long millies)
        {
            mTime = new System.DateTime((millies + 62135596800000) * 10000, System.DateTimeKind.Utc);
        }

        public static JDateTime Now
        {
            get
            {
                JDateTime dt;
                dt.mTime = System.DateTime.Now.ToUniversalTime();
                return dt;
            }
        }

        public static long NowMillies
        {
            get
            {
                return System.DateTime.Now.ToUniversalTime().Ticks / 10000 - 62135596800000;
            }
        }

        public System.DateTime Time { get { return mTime; } set { mTime = value; } }
        public System.DateTime LocalTime { get { return mTime.ToLocalTime(); } set { mTime = value.ToUniversalTime(); } }

        public static string FormatLocaltime(long time, string format = null)
        {
            JDateTime t = new JDateTime(time);
            if (format == null)
                return t.LocalTime.ToShortTimeString();
            else
                return t.LocalTime.ToString(format);
        }

        public long JMilliseconds
        {
            get { return mTime.Ticks / 10000 - 62135596800000; }
            set { mTime = new System.DateTime((value + 62135596800000) * 10000, System.DateTimeKind.Utc); }
        }

        public override string ToString()
        {
            return LocalTime.ToString();
        }
    }
}