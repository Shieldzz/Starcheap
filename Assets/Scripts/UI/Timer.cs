using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI m_cTimerLabel = null;

	[SerializeField]
	protected float m_fTimerInSeconds = 120f;
	public float TimerInSeconds
	{
		get { return m_fTimerInSeconds; }
    }

	private GameObject m_cGameObject;

    protected float m_fCurrTime = 0f;
	public float CurrTime
	{
		get { return m_fCurrTime; }
	}

	public event System.Action OnTimerEnd = null;

	private bool m_bIsPlaying = false;


	#region MonoBehavior
	protected virtual void Awake()
	{
		m_fCurrTime = m_fTimerInSeconds;

		m_cGameObject = gameObject;
	}

	protected virtual void Update()
	{
		if (m_bIsPlaying)
			RefreshTimer();
	}

	protected virtual void OnDestroy()
	{
		OnTimerEnd = null;
	}
	#endregion

	public virtual void Launch()
    {
        m_bIsPlaying = true;

		m_fCurrTime = m_fTimerInSeconds;

	}

    public void Stop()
	{
		m_bIsPlaying = false;
	}

	private void RefreshTimer()
	{
		m_fCurrTime -= Time.deltaTime;

		if (m_fCurrTime <= 0f)
		{
			m_bIsPlaying = false;

			if (OnTimerEnd != null)
				OnTimerEnd();

			return;
		}

        SetFormeTimer();
	}

    public void SetVisible(bool visible)
    {
        m_cGameObject.SetActive(visible);
    }

    protected virtual void SetFormeTimer()
    {

    }

}
