using Devil.Utility;
using System.Collections.Generic;

namespace Devil.GamePlay
{

    public class SitcomCmdSequence : SitcomFile, ISitcomExec
    {
        public static readonly int EndMarkId = StringUtil.IgnoreCaseToHash("end");

        SitcomCmd mCmd;
        int mNextMarkId;
        bool mExecuting;
        bool mFinish;
        bool mIsReading;
        public ISitcomResult Result { get { return mCmd.Result; } }

        public SitcomCmdSequence()
        {
            mCmd = new SitcomCmd();
        }

        public override bool BeginRead()
        {
            var ret = base.BeginRead();
            if (ret)
                mIsReading = true;
            return ret;
        }

        public void SetNextMark(int mark)
        {
            mNextMarkId = mark;
        }

        public void SetNextMark(string mark)
        {
            mNextMarkId = StringUtil.IgnoreCaseToHash(mark);
        }

        public bool SelectMark(string mark)
        {
            var id = StringUtil.IgnoreCaseToHash(mark);
            if (mExecuting)
            {
                mNextMarkId = id;
                return SelectNextMark();
            }
            mNextMarkId = 0;
            BeginRead();
            mIsReading = true;
            while (!Eof && NextMark('#'))
            {
                if (NextKeywords() && keyword.id == id)
                    return true;
            }
            return false;
        }

        bool SelectNextMark()
        {
            if (mNextMarkId == EndMarkId)
                return false;
            if (mNextMarkId == 0)
                return true;
            var line = PresentLine;
            while (!Eof && NextMark('#'))
            {
                if (NextKeywords() && keyword.id == mNextMarkId)
                {
                    mNextMarkId = 0;
                    return true;
                }
            }
            BeginRead();
            while (PresentLine < line && NextMark('#'))
            {
                if (NextKeywords() && keyword.id == mNextMarkId)
                {
                    mNextMarkId = 0;
                    return true;
                }
            }
            mNextMarkId = 0;
            return false;
        }

        public void OnExecute(SitcomContext runtime)
        {
            if(!mExecuting)
            {
                mExecuting = true;
                mFinish = false;
                if(!mIsReading)
                    BeginRead();
                runtime.Heap.BeginStack();
            }
            if(SelectNextMark() && mCmd.Read(this))
            {
                runtime.Push(mCmd);
            }
            else
            {
                mFinish = true;
            }
        }

        public void OnStop(SitcomContext runtime)
        {
            if (!mFinish)
            {
                runtime.Push(this);
            }
            else
            {
                mExecuting = false;
                mFinish = false;
                mIsReading = false;
                runtime.Heap.EndStack();
            }
        }
    }
}