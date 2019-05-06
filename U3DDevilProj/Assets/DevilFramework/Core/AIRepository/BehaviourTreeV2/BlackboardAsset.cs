using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [CreateAssetMenu(fileName = "Blackboard", menuName = "AI/Blackboard")]
    public class BlackboardAsset : ScriptableObject
    {
       
        [System.Serializable]
        public struct VariableDefine
        {
            public string name;
            public string typeDef;
            public bool isList;
            public string comment;
        }
        
        [SerializeField]
        VariableDefine[] m_Properties = new VariableDefine[0];

        public int Length { get { return m_Properties.Length; } }
        public VariableDefine this[int index] { get { return m_Properties[index]; } }

        public bool HasKey(string key)
        {
            for (int i = 0; i < m_Properties.Length; i++)
            {
                if (m_Properties[i].name == key)
                    return true;
            }
            return false;
        }
    }

}