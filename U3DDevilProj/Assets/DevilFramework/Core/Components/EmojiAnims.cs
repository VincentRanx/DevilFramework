using UnityEngine;

namespace Devil
{
    [CreateAssetMenu(fileName = "EmojiAnims", menuName = "Emoji Animation")]
    public class EmojiAnims : ScriptableObject
    {

        [System.Serializable]
        public class Frame
        {
            public string m_Sprite;
            public float m_Rotate;
        }

        [System.Serializable]
        public class Anim
        {
            public string m_Name;
            public float m_Duration = 0.3f;
            //public string[] m_Sprs;
            //public float[] m_Rotates;
            public Frame[] m_Frames = new Frame[0];

            public bool IsVaild { get { return m_Duration > 0 && m_Frames != null && m_Frames.Length > 0; } }

            public int GetFrameIndex(float time)
            {
                return Mathf.FloorToInt(time / m_Duration) % m_Frames.Length;
            }

            public Frame GetFrame(float time)
            {
                return m_Frames[GetFrameIndex(time)];
            }
        }

        public Anim[] m_Anims = new Anim[0];

        public Anim GetAnim(string animName)
        {
            for(int i = 0; i < m_Anims.Length; i++)
            {
                var anim = m_Anims[i];
                if (anim.m_Name == animName && anim.IsVaild)
                    return anim;
            }
            return null;
        }
    }  
}
