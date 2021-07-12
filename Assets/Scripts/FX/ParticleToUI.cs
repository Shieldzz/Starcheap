using UnityEngine;


public class ParticleToUI : MonoBehaviour
{
	[SerializeField]
	private float m_fSpeed = 5f;

	[SerializeField]
	private float m_fLastParticleTravelTime = 1f;
	private float m_fLastParticleTravalTimer;

	private bool m_bAtDestination = false;

	private GameObject m_cGameObject = null;
	private Transform m_cTransform = null;
	private ParticleSystem[] m_acParticleSystems = null;

	private Camera m_cGameCamera = null;
	private Transform m_cGameCameraTransform = null;

	private Vector3 m_vDestinationPosition = Vector3.zero;

	private bool m_bInit = false;


	#region MonoBehavior
	private void Start()
	{
		m_cGameObject = gameObject;
		m_cTransform = transform;
		m_acParticleSystems = GetComponentsInChildren<ParticleSystem>();

		m_cGameCamera = GameObject.FindGameObjectWithTag("GameCamera").GetComponent<Camera>();
		m_cGameCameraTransform = m_cGameCamera.transform;

		Hide();
	}

	private void Update()
	{
		if (m_bInit)
		{
			Vector3 UIPosToViewport = m_cGameCamera.ScreenToViewportPoint(m_vDestinationPosition);

			Vector3 viewportPosToWorld = m_cGameCamera.ViewportToWorldPoint(new Vector3(UIPosToViewport.x, UIPosToViewport.y, -m_cGameCameraTransform.position.z));

			Vector3 position = m_cTransform.position;

			Vector3 dir = viewportPosToWorld - position;

			if (dir.magnitude > 0.1f)
			{
				position += dir.normalized * m_fSpeed * Time.deltaTime;
				m_cTransform.position = position;
			}
			else if (!m_bAtDestination)
			{
				m_bAtDestination = true;

				m_fLastParticleTravalTimer = m_fLastParticleTravelTime;
			}

			m_fLastParticleTravalTimer -= Time.deltaTime;

			if (m_bAtDestination && m_fLastParticleTravalTimer <= 0f)
			{
				m_bInit = false;

				Hide();
			}
		}
	}
	#endregion

	public void Init(Vector3 UIPosition, Color color)
	{
		m_vDestinationPosition = UIPosition;
		m_vDestinationPosition.y += 30f;

		foreach (ParticleSystem particle in m_acParticleSystems)
		{
			ParticleSystem.MainModule module = particle.main;
			module.startColor = color;
		}

		m_bAtDestination = false;
		m_bInit = true;
	}

	private void Hide()
	{
		m_cGameObject.SetActive(false);
	}
}
