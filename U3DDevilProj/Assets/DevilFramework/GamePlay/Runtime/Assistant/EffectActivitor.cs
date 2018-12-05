using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    [ExecuteInEditMode]
    public class EffectActivitor : MonoBehaviour, IManagedEffect
    {
        ParticleSystem mSelfPartcle;

        [SerializeField]
        bool m_JustActiveParticles = false;
        [SerializeField]
        bool m_ActiveChildParticle = true;

        public float lifeTime { get; set; }
        public bool isAlive { get; private set; }
        public int poolId { get; set; }

        float mLife;

        private void Awake()
        {
            if(m_JustActiveParticles)
                mSelfPartcle = GetComponent<ParticleSystem>();
        }

        [ContextMenu("Reactive")]
        public virtual void Reactive()
        {
            if (!m_JustActiveParticles)
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
            else if (mSelfPartcle != null)
            {
                mSelfPartcle.Play(m_ActiveChildParticle);
            }
            mLife = lifeTime;
            isAlive = true;
        }

        public virtual void Deactive()
        {
            if (isAlive)
            {
                if (!m_JustActiveParticles)
                {
                    if (gameObject.activeSelf)
                        gameObject.SetActive(false);
                }
                else if (mSelfPartcle != null)
                {
                    mSelfPartcle.Stop(m_ActiveChildParticle);
                }
                isAlive = false;
            }
        }

        private void Update()
        {
            if(mLife >= 0 && isAlive)
            {
                mLife -= Time.deltaTime;
                if(mLife <= 0)
                {
                    isAlive = false;
                    EffectsManager.UnSpawnEffect(this);
                }
            }
        }
    }
}