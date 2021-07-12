using System;
using UnityEngine;
using UnityEngine.UI;


public class GameTimer : Timer
{
	public event Action<int> OnPhaseChange = null;

	[Header("Game Timer")]
	[SerializeField]
	private Animator m_cTimerAnimator;

	[SerializeField]
	private Image m_cTimerFill = null;

	[SerializeField]
	private Transform m_cClockHand = null;

	[SerializeField, Range(1f, 100f)]
	private float m_fCloseToEndTimerPercentage = 8f;
	private float m_fCloseToEndTime;

	private Vector3 m_vClockHandRotation = Vector3.zero;

	private float m_fTimerValue;
	private float m_fClockHandRotationValue;

	private float m_fSecondPhaseTime;
	private float m_fThirdPhaseTime;

	private bool m_bSecondPhase = false;
	private bool m_bThirdPhase = false;
	private bool m_bCloseToEnd = false;

	private Transform m_cTransform;


	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		m_fCloseToEndTime = m_fTimerInSeconds * m_fCloseToEndTimerPercentage / 100f;
		m_fTimerValue = 1f / m_fTimerInSeconds;
		m_fClockHandRotationValue = 360f / m_fTimerInSeconds;

		m_cTransform = transform;
	}

	protected override void Update()
    {
        base.Update();

		if (!m_bSecondPhase && m_fCurrTime <= m_fSecondPhaseTime && OnPhaseChange != null)
		{
			m_bSecondPhase = true;

			OnPhaseChange(1);
		}

		if (!m_bThirdPhase && m_fCurrTime <= m_fThirdPhaseTime && OnPhaseChange != null)
		{
			m_bThirdPhase = true;

			OnPhaseChange(2);
		}

		if (!m_bCloseToEnd && m_fCurrTime <= m_fCloseToEndTime)
		{
			m_bCloseToEnd = true;

			m_cTimerAnimator.SetTrigger("CloseToEnd");
		}
    }

	protected override void OnDestroy()
	{
		base.OnDestroy();

		OnPhaseChange = null;
	}
	#endregion

	public override void Launch()
	{
		base.Launch();

		Init();
	}

	protected override void SetFormeTimer()
    {
		//string minutes = Mathf.Floor(m_fCurrTime / 60f).ToString("00");
		//string seconds = Mathf.Floor(m_fCurrTime % 60f).ToString("00");
		//m_cTimerLabel.text = minutes + ":" + seconds;

		m_cTimerFill.fillAmount += m_fTimerValue * Time.deltaTime;
		m_vClockHandRotation.z -= m_fClockHandRotationValue * Time.deltaTime;
		m_cClockHand.localRotation = Quaternion.Euler(m_vClockHandRotation);
	}

	private void Init()
	{
		m_fSecondPhaseTime = m_fTimerInSeconds / 3f * 2f;
		m_fThirdPhaseTime = m_fTimerInSeconds / 3f;

		m_bSecondPhase = false;
		m_bThirdPhase = false;
		m_bCloseToEnd = false;

		m_cTimerFill.fillAmount = 0f;
		m_vClockHandRotation = Vector3.zero;
		m_cClockHand.localRotation = Quaternion.Euler(m_vClockHandRotation);

		m_cTransform.localScale = new Vector3(1f, 1f, 1f);
	}
}
