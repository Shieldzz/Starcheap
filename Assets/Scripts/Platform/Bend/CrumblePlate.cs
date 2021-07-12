using UnityEngine;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;

public class CrumblePlate : BendDeformer
{
    private Transform m_cTransform = null;

    [SerializeField]
    private float m_fDurability = 1.0f;

    private float m_fDistCenterToPlaque;
    public float DistCenterToPlaque { get { return m_fDistCenterToPlaque; } }

    [Header("Collider")]
    [SerializeField]
    private PlateCollider m_cPlateCollider = null;

    [Header("Impulse")]
    private float m_fTimerToImpule = 0.0f;
    private float m_fCurrTimerToImpulse = 0.0f;

    [SerializeField]
    private float m_fSpeed = 2.0f;

    private bool m_bIsImpulse = false;
    private bool m_bIsDurabilityNull = true;

    private Vector3 m_vAxisMove;


    private MeshRenderer m_cMeshRenderer = null;
    private Color m_cMainColorFeedback = new Color(1f, 0f, 0f, 1f);
    private Color m_cMainColor = new Color(1f, 0f, 0f, 1f);

    private float m_fFeedbackCurrInterval = 0.0f;

    [Header("FeedBack")]
    [SerializeField]
    private float m_fFeedbackSpeedInterval = 1.0f;
    [SerializeField]
    private float m_fFeedbackMinInterval = 0.01f;
    private float m_fFeedbackMaxInterval = 0.0f;

    private AnimationCurve m_cFeedbackAnimationCurve = null;

    [Header("Sounds")]
    [SerializeField]
    private StudioEventEmitter m_cCreakingSound = null;
    [SerializeField]
    private StudioEventEmitter m_cDisconnectSound = null;

    #region MonoBehaviour
    protected override void Awake()
    {
        base.Awake();

        m_cTransform = gameObject.transform;

        m_cMeshRenderer = GetComponent<MeshRenderer>();
        m_cMainColor = m_cMeshRenderer.material.GetColor("_Albedo_BlendColor");
    }

    protected override void Start()
    {
        base.Start();
        OnFinishBending += StartImpule;
        m_fCurrTimerToImpulse = m_fTimerToImpule;
    }

    protected override void Update()
    {
        base.Update();

        UpdateDurability();

        if (m_bIsBending)
        {
            //float coeff = (m_fTimer / m_fDuration) * m_fFeedbackSpeedInterval;
            float coeff = m_cFeedbackAnimationCurve.Evaluate( (m_fTimer / m_fDuration) ) * m_fFeedbackSpeedInterval;
            m_fFeedbackCurrInterval = (m_fFeedbackMaxInterval - coeff) + m_fFeedbackMinInterval;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (m_bIsImpulse)
        {
            if (m_fCurrTimerToImpulse > 0.0f)
            {
                m_fCurrTimerToImpulse -= Time.deltaTime;
            }
            else
            { 
                m_cPlateCollider.EnablePhysics(m_vAxisMove);
                m_bIsImpulse = false;
            }
        }
    }

    #endregion

    public void Init(Vector3 centerPlatform, float feedBackDuration, float timerToImpule, Color color, AnimationCurve animationCurve)
    {
        Vector3 dir = m_cTransform.position - centerPlatform;
        m_vAxisMove = dir.normalized;
        m_fDistCenterToPlaque = dir.magnitude;

        m_fTimerToImpule = timerToImpule;
        m_cMainColorFeedback = color;

        m_cFeedbackAnimationCurve = animationCurve;
        int curveLength = m_cFeedbackAnimationCurve.length;
        m_fFeedbackMaxInterval = m_cFeedbackAnimationCurve.keys[curveLength - 1].value;
        m_fFeedbackCurrInterval = m_fFeedbackMaxInterval;
    }

    public void CalculeDurability(float timer, int nbOfPlate)
    {
        m_fDurability = timer - ((m_fDistCenterToPlaque * timer) / (nbOfPlate / 2) * 2.0f);

		m_bIsDurabilityNull = false;
    }

    public void StartImpule()
    {
        m_bIsImpulse = true;
        m_cDisconnectSound.Play();
    }

    private void UpdateDurability()
    {
        if (!m_bIsDurabilityNull)
        {
            if (m_fDurability > 0.0f)
                m_fDurability -= Time.deltaTime;
            else
                StartBending();
        }
    }

    private void StartBending()
    {
	    StartBend();
        m_cCreakingSound.Play();
        m_bIsDurabilityNull = true;
        StartCoroutine(FeedbackPlateBending());
    }

    private IEnumerator FeedbackPlateBending()
    {
        while (m_bIsBending)
        {
            m_cMeshRenderer.material.SetColor("_Albedo_BlendColor", m_cMainColorFeedback);

            yield return new WaitForSeconds(m_fFeedbackCurrInterval);

            m_cMeshRenderer.material.SetColor("_Albedo_BlendColor", m_cMainColor);

            yield return new WaitForSeconds(m_fFeedbackCurrInterval);

        }

        m_cMeshRenderer.material.SetColor("_Albedo_BlendColor", m_cMainColor);
    }
};
