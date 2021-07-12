using FMODUnity;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Dinosaure : RandomEvent
{
	[Header("Dinosaure")]
	[SerializeField]
	private GameObject m_cDinosaure = null;

	[SerializeField]
	private Vector3 m_vSpawnPosition = Vector3.zero;

	[SerializeField]
	private Transform m_cMouth = null;

	[SerializeField]
	private float m_fSpeed = 2f;

	[SerializeField]
	private float m_fRotationSpeed = 0.5f;

	[SerializeField]
	private int m_iResourcesNumber = 6;

	[SerializeField]
	private float m_fSpitDistance = 1f;

	[SerializeField]
	private float m_fSpitAngle = 40f;

	private float m_fFireRate;
	private float m_fFireRateTimer;

	[Header("Waypoints")]
	[SerializeField]
	private Vector3[] m_avWaypoints = new Vector3[1];

	[SerializeField]
	private float m_fWaypointRadius = 0.3f;

	private int m_iCurrentWaypointIndex = -1;
	private Vector3 m_vCurrentWaypoint;

	private Vector3 m_vMoveDirection;

	[Header("Animations")]
	[SerializeField]
	private Animator m_cAnimator = null;

	[Header("FX")]
	[SerializeField]
	private ParticleSystem m_cExplosionFX = null;
	private float m_fFXDuration;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cWalkSound = null;
	[SerializeField]
	private StudioEventEmitter m_cThrowResourceSound = null;
	[SerializeField]
	private StudioEventEmitter m_cExplosionSound = null;

	[Header("Camera shake")]
	[SerializeField]
	private CameraEffects.CameraShakeData m_cDropOnPlatformCameraShake;
	private CameraEffects m_cCameraEffects = null;

	[Header("Controller vibration")]
	[SerializeField]
	private ControllerVibrationData m_cDropOnPlatformVibration;

	private bool m_bFirstLanding = false;

	private Rigidbody m_cRigidbody;
	private CapsuleCollider m_cCollider;


	#region MonoBehavior
	protected override void Awake()
	{
		base.Awake();

		m_cRigidbody = GetComponent<Rigidbody>();
		m_cCollider = GetComponent<CapsuleCollider>();

		m_cGameObject.SetActive(false);

		m_fSpitAngle *= Mathf.Deg2Rad;

		m_fFireRate = m_fDuration / m_iResourcesNumber;

		ParticleSystem.MainModule explosionFXMainModule = m_cExplosionFX.main;
		m_fFXDuration = explosionFXMainModule.duration + explosionFXMainModule.startLifetime.constant;

		m_cCameraEffects = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CameraEffects>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!m_bFirstLanding)
		{
			m_bFirstLanding = true;

			m_cCameraEffects.PlayCameraShake(m_cDropOnPlatformCameraShake);

			GameManager.Instance.SetControllersVibration(m_cDropOnPlatformVibration);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CancelInvoke();
	}
	#endregion

	#region RandomEvent
	public override void Launch()
	{
		base.Launch();

		m_cTransform.position = m_vSpawnPosition;

		m_cGameObject.SetActive(true);

		m_cCollider.enabled = true;
		m_cRigidbody.useGravity = true;
		m_cDinosaure.SetActive(true);

		m_cAnimator.SetBool("isWalking", true);

		m_iCurrentWaypointIndex = -1;
		ProceedToNextWaypoint();

		m_fFireRateTimer = m_fFireRate;

		m_cWalkSound.Play();
	}

	protected override void EventUpdate()
	{
		base.EventUpdate();

		MoveTo();

		m_fFireRateTimer -= Time.deltaTime;

		if (m_fFireRateTimer <= 0f)
		{
			m_fFireRateTimer = m_fFireRate;

			SpitResource();
		}
	}

	public override void Stop()
	{
		base.Stop();

		m_bFirstLanding = false;

		m_cExplosionFX.Play(true);
		m_cAnimator.SetBool("isWalking", false);

		m_cCollider.enabled = false;
		m_cRigidbody.useGravity = false;
		m_cDinosaure.SetActive(false);

		m_cWalkSound.Stop();
		m_cExplosionSound.Play();

		Invoke("Disable", m_fFXDuration);
	}
	#endregion

	#region Dinosaure
	private void SpitResource()
	{
		GameObject resource = ResourceObjectManager.Instance.Spawn(RandomEventManager.Instance.GetRandomGeneratedResource(), m_cMouth.position, Quaternion.identity);

		Rigidbody rigidbody = resource.GetComponent<Rigidbody>();

		Vector3 target = m_cMouth.position + m_cMouth.forward * m_fSpitDistance;

		m_cTransform.LookAt(target);

		float velocity = Mathf.Sqrt(m_fSpitDistance * -Physics.gravity.y / (Mathf.Sin(m_fSpitAngle * 2f)));
		float yVelocity = Mathf.Sin(m_fSpitAngle) * velocity;
		float zVelocity = Mathf.Cos(m_fSpitAngle) * velocity;

		Vector3 globalVelocity = m_cMouth.TransformVector(new Vector3(0f, yVelocity, zVelocity));

		rigidbody.velocity = globalVelocity;

		m_cAnimator.SetTrigger("spitResource");

		m_cThrowResourceSound.Play();
	}

	private void MoveTo()
	{
		m_cTransform.position += m_vMoveDirection * m_fSpeed * Time.deltaTime;

		m_cTransform.rotation = Quaternion.Slerp(m_cTransform.rotation, Quaternion.LookRotation(m_vMoveDirection), m_fRotationSpeed);

		if (IsAtWaypoint())
			ProceedToNextWaypoint();
	}

	private void ProceedToNextWaypoint()
	{
		int nextWaypointIndex;

		do
			nextWaypointIndex = Random.Range(0, m_avWaypoints.Length);
		while (nextWaypointIndex == m_iCurrentWaypointIndex);

		m_iCurrentWaypointIndex = nextWaypointIndex;
		m_vCurrentWaypoint = m_avWaypoints[m_iCurrentWaypointIndex];

		Vector3 dinosaurePosition = m_cTransform.position;
		dinosaurePosition.y = 0f;

		m_vMoveDirection = (m_vCurrentWaypoint - dinosaurePosition).normalized;
	}

	private bool IsAtWaypoint()
	{
		Vector3 waypoint = m_vCurrentWaypoint;
		waypoint.y = m_cTransform.position.y;

		return Vector3.Distance(waypoint, m_cTransform.position) < m_fWaypointRadius;
	}

	private void Disable()
	{
		m_cGameObject.SetActive(false);
	}
	#endregion

#if UNITY_EDITOR
	#region Gizmos
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(m_vSpawnPosition, Vector3.one);

		Gizmos.color = Color.green;

		foreach (Vector3 waypoint in m_avWaypoints)
			Gizmos.DrawSphere(waypoint, m_fWaypointRadius);
	}
	#endregion
#endif
}
