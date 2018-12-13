using System.Collections.Generic;

namespace Devil.AI
{
    public enum ELogic
    {
        And,
        Or,
    }

    [System.Serializable]
    public struct BoolLogic: ICondition , System.IEquatable<BoolLogic>
    {
        public bool isTrue;
        public BoolLogic(bool value)
        {
            this.isTrue = value;
        }

        public bool IsSuccess { get { return isTrue; } }

        public bool Equals(BoolLogic other)
        {
            return isTrue == other.isTrue;
        }

        public static implicit operator BoolLogic(bool value)
        {
            return new BoolLogic(value);
        }
        public static implicit operator bool(BoolLogic value)
        {
            return value.isTrue;
        }
    }

    public struct AndLogic : ICondition , System.IEquatable<AndLogic>
    {
        public ICollection<ICondition> combines;

        public bool IsSuccess
        {
            get
            {
                if (combines == null || combines.Count == 0)
                    return true;
                foreach (var t in combines)
                {
                    if (t != null && !t.IsSuccess)
                        return false;
                }
                return true;
            }
        }

        public bool Equals(AndLogic other)
        {
            return IsSuccess == other.IsSuccess;
        }
    }

    public struct OrLogic : ICondition , System.IEquatable<OrLogic>
    {
        public ICollection<ICondition> combines;

        public bool IsSuccess
        {
            get
            {
                if (combines == null || combines.Count == 0)
                    return false;
                foreach (var t in combines)
                {
                    if (t != null && t.IsSuccess)
                        return true;
                }
                return false;
            }
        }

        public bool Equals(OrLogic other)
        {
            return IsSuccess == other.IsSuccess;
        }
    }

    public struct NotLogic: ICondition, System.IEquatable<NotLogic>
    {
        public ICondition target;
        public bool IsSuccess { get { return target == null ? true : !target.IsSuccess; } }

        public bool Equals(NotLogic other)
        {
            return IsSuccess == other.IsSuccess;
        }
    }
}