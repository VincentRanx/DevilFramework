using UnityEngine;
using UnityEngine.UI;

namespace Devil.Utility
{

    [RequireComponent(typeof(Text))]
    public class FpsCounter : MonoBehaviour
    {

        [Range(0.1f, 2f)]
        public float m_UpdateRate = 0.5f;
        float frames;
        float time;
        float fps;
        Text mText;

        void Start()
        {
            frames = 0;
            time = 0;
            fps = 0;
            mText = GetComponent<Text>();
        }

        void Update()
        {
            frames++;
            time += Time.unscaledDeltaTime;
            if (time >= m_UpdateRate)
            {
                fps = frames / time;
                frames = 0;
                time = 0;
                mText.text = Mathf.Round(fps) + " FPS";
            }
        }

    }
}