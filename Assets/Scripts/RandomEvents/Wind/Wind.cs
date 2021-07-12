using UnityEngine;
using FMODUnity;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Wind : RandomEvent
{
	[Header("Wind")]
	[SerializeField]
	private float m_fMinWindPower = 1f;
	[SerializeField]
	private float m_fMaxWindPower = 2f;

	[SerializeField]
	private int m_iMinDustCloudNumber = 1;
	[SerializeField]
	private int m_iMaxDustCloudNumber = 10;

	[SerializeField]
	private float m_fDustCloudsDestinationSpread = 1f;

	[SerializeField]
	private float m_fDustCloudProjectionAngle = 40f;

	[SerializeField]
	private Vector3 m_vSpawnPosition = Vector3.zero;

	private Vector3 m_vWindDirection = new Vector3(1f, 0f, 0f);

	private float m_fCurrentWindPower;
	private int m_iDustCloudToSpawn;

	private float m_fSpawnFrequency;
	private float m_fSpawnTimer;

	[Header("FX")]
	[SerializeField]
	private ParticleSystem m_cWindFX = null;
	private GameObject m_cWindFXGameObject;

	[Header("Prefab")]
	[SerializeField]
	private GameObject m_cDustCloudPrefab = null;
	private int m_iPoolIDDustCloud;

	private Platform m_cPlatform;

	[Header("Sounds")]
    [SerializeField]
    private StudioEventEmitter m_cAmbianceSound = null;


    #region MonoBehavior
    protected override void Awake()
	{
		base.Awake();

		m_fDustCloudProjectionAngle *= Mathf.Deg2Rad;

		m_cWindFXGameObject = m_cWindFX.gameObject;
		m_cWindFXGameObject.SetActive(false);

		m_iPoolIDDustCloud = PoolManager.Preload(m_cDustCloudPrefab, m_cTransform, m_iMaxDustCloudNumber);

		m_cPlatform = GameObject.FindGameObjectWithTag("Platform").GetComponent<Platform>();
	}
	#endregion

	override public void Launch()
	{
		base.Launch();

		m_fCurrentWindPower = Random.Range(m_fMinWindPower, m_fMaxWindPower);
		m_iDustCloudToSpawn = Random.Range(m_iMinDustCloudNumber, m_iMaxDustCloudNumber);

		m_fSpawnFrequency = m_fDuration / m_iDustCloudToSpawn;
		m_fSpawnTimer = m_fSpawnFrequency;

		m_cPlatform.m_vWindDirection = m_vWindDirection * m_fCurrentWindPower;

		m_cWindFXGameObject.SetActive(true);
        m_cAmbianceSound.Play();
	}

	override protected void EventUpdate()
	{
		base.EventUpdate();

		m_fSpawnTimer += Time.deltaTime;

		if (m_fSpawnTimer >= m_fSpawnFrequency)
		{
			m_fSpawnTimer = 0f;

			SpawnDustCloud();
		}
	}

	override public void Stop()
	{
		base.Stop();

		m_cPlatform.m_vWindDirection = Vector3.zero;

        m_cAmbianceSound.Stop();
		m_cWindFXGameObject.SetActive(false);
    }

	private void SpawnDustCloud()
	{
		Vector2 randomPlatformPoint = Random.insideUnitCircle * m_fDustCloudsDestinationSpread;

		Vector3 targetPosition = new Vector3(randomPlatformPoint.x, 1f, randomPlatformPoint.y);

		GameObject dustCloudObject = PoolManager.Spawn(m_iPoolIDDustCloud, m_cTransform.position + m_vSpawnPosition, Quaternion.identity, m_cTransform);

		DustCloud newDustCloud = dustCloudObject.GetComponent<DustCloud>();
		newDustCloud.Init(m_cTransform.position + m_vSpawnPosition, targetPosition, m_fDustCloudProjectionAngle);
	}

#if UNITY_EDITOR
	#region Gizmos
	private void OnDrawGizmosSelected()
	{
		Handles.color = Color.red;
		Handles.DrawWireDisc(Vector3.zero, Vector3.up, m_fDustCloudsDestinationSpread);

		Gizmos.color = Color.blue;
		Gizmos.DrawCube(transform.position + m_vSpawnPosition, Vector3.one);
	}
	#endregion
#endif
}
