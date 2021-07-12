#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class MeteorShower : RandomEvent
{
    private GameManager m_cGameManager;

	[Header("Meteor Shower")]
	[SerializeField]
	private GameObject m_cMeteorPrefab = null;

	[SerializeField]
	private int m_iThemeNumber = 3;
	private int m_iCurrentTheme;

	[SerializeField]
	private int m_iMinMeteorNumber = 1;
	[SerializeField]
	private int m_iMaxMeteorNumber = 10;

	private int m_iMeteorsToSpawnNumber = 1;

	[SerializeField]
	private float m_fMinMeteorSpeed = 1f;
	[SerializeField]
	private float m_fMaxMeteorSpeed = 2f;

	[SerializeField]
	private float m_fShowerSize = 1f;
	[SerializeField]
	private float m_fMeteorSpreadSize = 1f;

	[SerializeField, Range(0, 100)]
	private int m_iChanceResourcePerMeteor = 1;

	[SerializeField]
	private GameObject[] m_acResourceObjectsInsideMeteors;

	[Header("Effects")]
	[SerializeField]
	private GameObject m_cMeteorCloud = null;

	[Header("Camera Shake")]
	[SerializeField]
	private CameraEffects.CameraShakeData m_cCameraShakeMeteorImpact;
	private CameraEffects m_cCameraEffects = null;

	[Header("Controller Vibration")]
	[SerializeField]
	private ControllerVibrationData m_cImpactControllerVibration;

	private float m_fMaxSpawnFrequency = 1f;
	private float m_fSpawnTimer = 0f;

    private int m_iPoolIDMeteor = 0;

    #region MonoBehavior
    protected override void Awake()
	{
        base.Awake();
        m_cGameManager = GameManager.Instance;
        m_iPoolIDMeteor = PoolManager.Preload(m_cMeteorPrefab, m_cTransform, m_iMaxMeteorNumber);

		m_cCameraEffects = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CameraEffects>();

		m_cMeteorCloud.SetActive(false);
	}
	#endregion

	#region RandomEvent
	override public void Launch()
	{
		base.Launch();

		m_iCurrentTheme = Random.Range(0, m_iThemeNumber);
		m_iMeteorsToSpawnNumber = Random.Range(m_iMinMeteorNumber, m_iMaxMeteorNumber + 1);

		m_fMaxSpawnFrequency = m_fDuration / m_iMeteorsToSpawnNumber;
		m_fSpawnTimer = m_fMaxSpawnFrequency;

		m_cMeteorCloud.SetActive(true);
	}

	override protected void EventUpdate()
	{
		base.EventUpdate();

		m_fSpawnTimer += Time.deltaTime;

		if (m_fSpawnTimer >= m_fMaxSpawnFrequency)
		{
			m_fSpawnTimer = 0f;

			Vector2 randomUp = Random.insideUnitCircle * m_fShowerSize;
			Vector2 randomDown = Random.insideUnitCircle * m_fMeteorSpreadSize;

			Vector3 meteorPositionUp = new Vector3(randomUp.x, 0f, randomUp.y);
			Vector3 meteorPositionDown = new Vector3(randomDown.x, -m_cTransform.position.y, randomDown.y);

			Vector3 meteorDir = (meteorPositionDown - meteorPositionUp).normalized;
			float meteorSpeed = Random.Range(m_fMinMeteorSpeed, m_fMaxMeteorSpeed);

			bool resourceInMeteor = Random.Range(1, 101) <= m_iChanceResourcePerMeteor;

			SpawnMeteor(meteorPositionUp, meteorDir, meteorSpeed, resourceInMeteor);
		}
	}

	override public void Stop()
	{
		base.Stop();

		m_cMeteorCloud.SetActive(false);
	}
	#endregion

	#region MeteorShower
	private void SpawnMeteor(Vector3 position, Vector3 direction, float speed, bool containsResource)
	{
		GameObject meteorObject = PoolManager.Spawn(m_iPoolIDMeteor, position, Quaternion.identity, m_cTransform);

		Meteor newMeteor = meteorObject.GetComponent<Meteor>();

		newMeteor.OnImpact += MeteorImpact;

		if (containsResource)
			newMeteor.OnImpactPlatform += SpawnResourceInsideMeteor;

		newMeteor.OnDespawn += DespawnMeteor;
		newMeteor.Init(direction, speed, m_iCurrentTheme);
	}

	private void DespawnMeteor(GameObject meteorToDespawn)
	{
		PoolManager.Despawn(meteorToDespawn);
	}

	private void MeteorImpact()
	{
		m_cCameraEffects.PlayCameraShake(m_cCameraShakeMeteorImpact);

		m_cGameManager.SetControllersVibration(m_cImpactControllerVibration);
	}

	private void SpawnResourceInsideMeteor(Vector3 impactPosition)
	{
        ResourceObjectManager.Instance.Spawn(RandomEventManager.Instance.GetRandomGeneratedResource(), impactPosition, m_cGameManager.PlatformTransform.rotation);
	}
	#endregion

#if UNITY_EDITOR
	#region Gizmos
	private void OnDrawGizmosSelected()
	{
		Handles.color = Color.red;
		Handles.DrawWireDisc(transform.position, Vector3.up, m_fShowerSize);
		Handles.DrawWireDisc(Vector3.zero, Vector3.up, m_fMeteorSpreadSize);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position + new Vector3(m_fShowerSize, 0f, 0f), new Vector3(m_fMeteorSpreadSize, 0f, 0f));
		Gizmos.DrawLine(transform.position + new Vector3(-m_fShowerSize, 0f, 0f), new Vector3(-m_fMeteorSpreadSize, 0f, 0f));
		Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, m_fShowerSize), new Vector3(0f, 0f, m_fMeteorSpreadSize));
		Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, -m_fShowerSize), new Vector3(0f, 0f, -m_fMeteorSpreadSize));
	}
	#endregion
#endif
}
