using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public enum InputMask
    {
        attack_A = 1,
        attack_B = 2,
        attack_C = 4,
        attack_D = 8,
        defend = 0x10,
        dodge = 0x20,
        jump = 0x40,
        act_A = 0x100,
        act_B = 0x200,
        act_C = 0x400,
        act_D = 0x800,
    }

    public enum InputCombine
    {
        just_all = 0,
        contains_all = 1,
        contains_any = 2,
    }
    
    [System.Serializable]
    public class KeyMap
    {
        public InputCombine m_Combination;
        [MaskField]
        public InputMask m_Mask;

        public int UniqueId { get { return ((int)m_Combination << 16) | (int)m_Mask; } }

        public bool MatchKey(InputMask mask)
        {
            if (m_Combination == InputCombine.contains_all)
                return (mask & m_Mask) == m_Mask;
            else if (m_Combination == InputCombine.contains_any)
                return (mask & m_Mask) != 0;
            else if (m_Combination == InputCombine.just_all)
                return mask == m_Mask;
            else
                return false;
        }
    }
}