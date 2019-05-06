using UnityEngine;

namespace Devil.GamePlay
{
    [System.Serializable]
    public struct AnimParam : System.IEquatable<AnimParam>
    {
        [SerializeField]
        string _name;
        [SerializeField]
        int _id;

        public AnimatorControllerParameterType type;

        [SerializeField]
        float _value;


        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                _id = value == null ? 0 : Animator.StringToHash(value);
            }
        }
        public int id { get { return _id; } }

        public int intValue { get { return (int)_value; } set { _value = value; } }
        public float floatValue { get { return _value; } set { _value = value; } }
        public bool boolValue { get { return _value > 0.2f; } set { _value = value ? 1 : 0; } }

        public void ApplyTrigger(Animator anim)
        {
            anim.SetTrigger(_id);
        }

        public void ApplyFloat(Animator anim)
        {
            anim.SetFloat(_id, _value);
        }

        public void ApplyBool(Animator anim)
        {
            anim.SetBool(_id, boolValue);
        }

        public void ApplyInt(Animator anim)
        {
            anim.SetInteger(_id, intValue);
        }

        public void Apply(Animator anim)
        {
            if (anim == null || _id == 0)
                return;
            if (type == AnimatorControllerParameterType.Trigger)
                anim.SetTrigger(_id);
            else if (type == AnimatorControllerParameterType.Float)
                anim.SetFloat(_id, floatValue);
            else if (type == AnimatorControllerParameterType.Bool)
                anim.SetBool(_id, boolValue);
            else if (type == AnimatorControllerParameterType.Int)
                anim.SetInteger(_id, intValue);
        }

        public static AnimParam ReferenceTo(AnimatorControllerParameter aparam)
        {
            AnimParam p;
            p.type = aparam.type;
            p._name = aparam.name;
            p._id = aparam.nameHash;
            p._value = 0;
            return p;
        }

        public static AnimParam ReferenceTo(string name, AnimatorControllerParameterType type)
        {
            AnimParam p;
            p.type = type;
            p._name = name;
            p._id = name == null ? 0 : Animator.StringToHash(name);
            p._value = 0;
            return p;
        }

        public bool Equals(AnimParam other)
        {
            return _id == other._id && type == other.type && _value == other._value;
        }

        public static implicit operator int (AnimParam aparam)
        {
            return aparam._id;
        }

        public static explicit operator string(AnimParam aparam)
        {
            return aparam._name;
        }
    }
}