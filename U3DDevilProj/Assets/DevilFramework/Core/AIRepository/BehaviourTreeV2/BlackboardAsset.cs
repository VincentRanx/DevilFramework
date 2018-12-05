using UnityEngine;

namespace Devil.AI
{
    [CreateAssetMenu(fileName = "Blackboard", menuName = "AI/Blackboard")]
    public class BlackboardAsset : ScriptableObject
    {
        [System.Serializable]
        public class KeyValue
        {
            public string m_Key;
            public string m_Value;
            public string m_Comment;

            public KeyValue()
            {
                m_Key = "varName";
                m_Value = typeof(int).FullName;
                m_Comment = "";
            }
        }

        [HideInInspector]
        public KeyValue[] m_Properties = new KeyValue[0];

        public bool HasKey(string key)
        {
            for(int i = 0; i < m_Properties.Length; i++)
            {
                if (m_Properties[i].m_Key == key)
                    return true;
            }
            return false;
        }
    }
}