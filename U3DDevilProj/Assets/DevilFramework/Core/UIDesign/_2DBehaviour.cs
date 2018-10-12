using UnityEngine;

namespace Devil.UI
{

    [RequireComponent(typeof(RectTransform))]
    public class _2DBehaviour : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        RectTransform mRectTrans;

        public RectTransform SelfRect
        {
            get
            {
                if (mRectTrans == null)
                    mRectTrans = GetComponent<RectTransform>();
                return mRectTrans;
            }
        }
    }
}