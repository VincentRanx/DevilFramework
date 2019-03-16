using Devil.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Devil.GamePlay
{
    [RequireComponent(typeof(Text))]
    public class FpsCounter : MonoBehaviour
    {

        [Range(0.1f, 2f)]
        public float m_UpdateRate = 0.5f;
        float frames;
        float time;
        protected float fps;
        protected Text mText;

        protected virtual void Start()
        {
            frames = 0;
            time = 0;
            fps = 0;
            mText = GetComponent<Text>();
        }

        void CountFps()
        {
            frames++;
            time += Time.unscaledDeltaTime;
            fps = frames / time;
        }

        protected virtual void UpdateContent()
        {
            mText.text = StringUtil.Concat(Mathf.Round(fps), "FPS");
        }

        void Update()
        {
            CountFps();
            if (time >= m_UpdateRate)
            {
                UpdateContent();
                frames = 0;
                time = 0;
            }
        }

    }
}