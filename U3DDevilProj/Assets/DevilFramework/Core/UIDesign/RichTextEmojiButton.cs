using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Devil.UI
{
    [RequireComponent(typeof(RichText))]
    public class RichTextEmojiButton : MonoBehaviour, IPointerClickHandler
    {
        [System.Serializable]
        public class EmojiEvent : UnityEvent<int>
        {

        }

        RichText mTxt;
        public RichText richText
        {
            get
            {
                if (mTxt == null)
                    mTxt = GetComponent<RichText>();
                return mTxt;
            }
        }

        public EmojiEvent m_ClickEvent = new EmojiEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            var btn = richText.GetBtnId(eventData.pointerCurrentRaycast.screenPosition);
            if(btn != 0)
            {
                m_ClickEvent.Invoke(btn);
            }
        }
    }
}
