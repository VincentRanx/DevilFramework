using UnityEngine;
namespace DevilTeam.Utility
{
    [System.Serializable]
    public struct DragAmount
    {
        public float drag;
        public float limitSpeed;
        public float sensitive;
        public float devition;

        float factor;
        Vector3 velocity;
        Vector3 pushDelta;
        bool pushing;
        bool toTarget;

        public void Push(Vector2 delta)
        {
            pushDelta = delta * sensitive;
            factor = 0.2f;
            pushing = true;
            toTarget = false;
        }

        public void PushTo(Vector3 targetPos, float strength)
        {
            pushing = false;
            toTarget = true;
            pushDelta = targetPos;
            factor = strength > 0 ? Mathf.Clamp01(strength) : 1f;
        }

        public bool OnDragProcess(Transform trans)
        {
            float t = Mathf.Min(Time.deltaTime, 0.04f);
            float limit = limitSpeed > 0 ? limitSpeed * t : 0;
            float distance;
            Vector3 delta;
            if (toTarget && trans.localPosition != pushDelta)
            {
                Vector3 v = trans.localPosition;
                Vector3 v2 = GlobalUtil.CloseTo(v, pushDelta, factor, devition);
                float dis = Vector3.Distance(v, v2);
                if (limit > 0 && dis > limit)
                {
                    v2 = Vector3.Lerp(v, v2, limit / dis);
                }
                trans.localPosition = v2;
                return true;
            }
            else if (pushing)
            {
                pushing = false;
                delta = GlobalUtil.CloseTo(Vector3.zero, pushDelta, factor, devition);
                distance = delta.magnitude;
                if (limit > 0 && distance > limit)
                {
                    delta = Vector3.Lerp(Vector3.zero, delta, limit / distance);
                }
                velocity = delta / Time.deltaTime;
                trans.localPosition += delta;
                return true;
            }
            else if (drag > 0 && velocity != Vector3.zero)
            {
                distance = velocity.magnitude;
                velocity = GlobalUtil.CloseTo(velocity, Vector3.zero, Mathf.Clamp(drag * Time.deltaTime, 0, distance) / distance, devition);
                delta = velocity * Time.deltaTime;
                trans.localPosition += delta;
                return true;
            }
            else
            {
                toTarget = false;
                pushing = false;
                velocity = Vector3.zero;
                return false;
            }
        }
    }
}