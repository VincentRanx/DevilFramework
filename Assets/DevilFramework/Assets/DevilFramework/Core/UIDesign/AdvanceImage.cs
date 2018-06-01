using UnityEngine;
using UnityEngine.UI;

namespace Devil.UI
{
    public class AdvanceImage : Image
    {

        [Range(0, 1)]
        [SerializeField]
        float m_Saturation = 1;

        [SerializeField]
        bool m_InstanceMaterial = true;

        int m_PropertyId;
        Material mMaterialInstance;

        public override Material materialForRendering
        {
            get
            {
                if (m_Material == null)
                {
                    return base.materialForRendering;
                }
                if (mMaterialInstance == null)
                    mMaterialInstance = m_InstanceMaterial ? new Material(m_Material) : m_Material;
                return mMaterialInstance;
            }
        }

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
                    SetMaterialDirty();
                    //ValidateSaturation();
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