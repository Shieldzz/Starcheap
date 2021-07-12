using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


abstract public class RandomEvent : MonoBehaviour
{
	[SerializeField]
	protected float m_fDuration = 1f;
	private float m_fTimer = 0f;

	[SerializeField]
	private PostProcessProfile m_cPostProcessProfile = null;
	public PostProcessProfile PostProcessProfile
	{
		get { return m_cPostProcessProfile; }
	}

	private bool m_bIsOngoing = false;

	public event System.Action OnEventEnd = null;

    protected Transform m_cTransform = null;
    public Transform Transform { get { return m_cTransform; } }

	protected GameObject m_cGameObject = null;


    #region MonoBehavior
    protected virtual void Awake()
    {
        m_cTransform = GetComponent<Transform>();
		m_cGameObject = gameObject;
    }

    private void Update()
	{
		if (m_bIsOngoing)
			EventUpdate();
	}

	protected virtual void OnDestroy()
	{
		OnEventEnd = null;
	}
	#endregion

	virtual public void Launch()
	{
		m_fTimer = 0f;
		m_bIsOngoing = true;
	}

	virtual protected void EventUpdate()
	{
		m_fTimer += Time.deltaTime;

		if (m_fTimer >= m_fDuration)
			Stop();
	}

	virtual public void Stop()
	{
		m_bIsOngoing = false;

		if (OnEventEnd != null)
			OnEventEnd();
	}
}
