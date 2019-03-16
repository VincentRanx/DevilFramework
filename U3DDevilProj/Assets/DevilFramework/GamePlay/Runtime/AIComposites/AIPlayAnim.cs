using Devil.AI;
using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay
{
    [BTComposite(Title = "动画控制(A)", HotKey = KeyCode.A)]
    public class AIPlayAnim : BTTaskAsset
    {
        public AnimParam m_AnimParam = AnimParam.ReferenceTo("", AnimatorControllerParameterType.Trigger);
        public float m_Duration = 1;
        public AnimationCurve m_NormalizedCurve;

        bool mContainsParam;
        Animator mAnim;
        float mTime;
        float mTimeNormalize;

        bool mBool;
        int mInt;
        float mFloat;

        public override string DisplayContent
        {
            get
            {
                return string.Format("[{0}] {1}", m_AnimParam.type, m_AnimParam.name);
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mAnim = binder.GetComponent<Animator>();
            Init();
        }

        void Init()
        {
            mContainsParam = false;
            if (mAnim != null)
            {
                for (int i = 0; i < mAnim.parameterCount; i++)
                {
                    if (mAnim.GetParameter(i).nameHash == m_AnimParam.id)
                    {
                        mContainsParam = true;
                        break;
                    }
                }
            }
            mTimeNormalize = m_Duration > 0 ? (1 / m_Duration) : 1f;
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            if (!mContainsParam)
                return EBTState.failed;
            mTime = 0;
            if (m_AnimParam.type == AnimatorControllerParameterType.Trigger)
                mBool = false;
            else if (m_AnimParam.type == AnimatorControllerParameterType.Bool)
                mBool = mAnim.GetBool(m_AnimParam.id);
            else if (m_AnimParam.type == AnimatorControllerParameterType.Int)
                mInt = mAnim.GetInteger(m_AnimParam.id);
            else if (m_AnimParam.type == AnimatorControllerParameterType.Float)
                mFloat = mAnim.GetFloat(m_AnimParam.id);
            return EBTState.running;
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            mTime += deltaTime;
            if (m_AnimParam.type == AnimatorControllerParameterType.Trigger)
            {
                if (!mBool)
                    mAnim.SetTrigger(m_AnimParam.id);
                mBool = true;
            }
            else if (m_AnimParam.type == AnimatorControllerParameterType.Bool)
            {
                var b = m_AnimParam.boolValue;
                if (b ^ mBool)
                    mAnim.SetBool(m_AnimParam.id, b);
                mBool = b;
            }
            else if (m_AnimParam.type == AnimatorControllerParameterType.Int)
            {
                mInt = m_AnimParam.intValue;
                mAnim.SetInteger(m_AnimParam.id, mInt);
            }
            else if (m_AnimParam.type == AnimatorControllerParameterType.Float)
            {
                mFloat = m_AnimParam.floatValue;
                if (GlobalUtil.IsValid(m_NormalizedCurve))
                    mFloat *= GlobalUtil.GetNormalizedValue(m_NormalizedCurve, Mathf.Clamp01(mTime * mTimeNormalize));
                mAnim.SetFloat(m_AnimParam.id, mFloat);
            }
            return mTime >= m_Duration ? EBTState.success : EBTState.running;
        }

        public override void OnStop()
        {
        }
    }
}