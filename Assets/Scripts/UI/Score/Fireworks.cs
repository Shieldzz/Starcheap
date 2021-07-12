using UnityEngine;
using FMODUnity;

public class Fireworks : MonoBehaviour
{
    [SerializeField]
    private float m_fTimerLaunchSound = 2.0f;
    private float m_fCurrTimerLaunchSound = 0.0f;

    [SerializeField]
    private StudioEventEmitter m_cFireworkSound = null;

    private ParticleSystem m_cFireworkFX = null;

    #region MonoBehaviour
    private void Awake()
    {
        m_cFireworkFX = GetComponent<ParticleSystem>();
    }

    private void OnDisable()
    {
        Init();
    }

    private void Update()
    {
        if (m_cFireworkFX.isEmitting)
        {
            m_fCurrTimerLaunchSound += Time.deltaTime;
            if (m_fCurrTimerLaunchSound >= m_fTimerLaunchSound)
            {
                m_cFireworkSound.Play();
                m_fCurrTimerLaunchSound = 0.0f;
            }
        }
        else
            m_fCurrTimerLaunchSound = 0.0f;
    }
    #endregion

    private void Init()
    {
        m_fCurrTimerLaunchSound = 0.0f;
    }

}
