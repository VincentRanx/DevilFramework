using UnityEngine;
using UnityEngine.UI;

namespace Devil.UI
{
    public class AdvanceImage : Image
    {

        [Range(0, 1)]
        [SerializeField]
        float m_Saturation = 1;

        int m_PropertyId;

        public float Staturation
        {
            get
            {
                return m_Saturation;
            }
            set
            {
                if (m_Saturation != value)
                {
                    m_Saturation = value;
                    ValidateSaturation();
                }
            }
        }

        void ValidateSaturation()
        {
            Material mat = this.materialForRendering;
            if (mat)
            {
                mat.SetFloat(m_PropertyId, m_Saturation);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_PropertyId = Shader.PropertyToID("_Saturation");
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            m_PropertyId = Shader.PropertyToID("_Saturation");
            ValidateSaturation();
        }
#endif

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();
            ValidateSaturation();
        }
    }
}