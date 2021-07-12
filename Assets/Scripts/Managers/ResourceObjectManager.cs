using UnityEngine;


public class ResourceObjectManager : MonoBehaviour
{
	private static ResourceObjectManager m_cInstance = null;
	public static ResourceObjectManager Instance
	{
		get { return m_cInstance; }
	}

	[SerializeField]
	private ResourceSettings[] m_acResourceSettings = null;
	public ResourceSettings[] ResourceSettings
	{
		get { return m_acResourceSettings; }
	}

	private int m_iResourceQuantity = 10;

    private int[] m_liPoolID;

	private float[] m_afResourcesInPoolRatio;
	public float[] ResourcesInPoolRatio
	{
		get { return m_afResourcesInPoolRatio; }
	}


	#region MonoBehavior
	private void Awake()
	{
		if (m_cInstance == null)
			m_cInstance = this;
		else
			Destroy(gameObject);

		int resourceSettingsSize = m_acResourceSettings.Length;

		m_liPoolID = new int[resourceSettingsSize];
		m_afResourcesInPoolRatio = new float[resourceSettingsSize];
		for (int idx = 0; idx < resourceSettingsSize; idx++)
		{
			m_liPoolID[idx] = PoolManager.Preload(m_acResourceSettings[idx].m_cPrefab, transform, m_iResourceQuantity);
			m_afResourcesInPoolRatio[idx] = 1f;
		}
	}
	#endregion

	public GameObject Spawn(GameObject resource, Vector3 position, Quaternion quaternion, Transform parent = null, bool inWorldPos = true, bool withSlope = true)
	{
        int resourceIdx = Find(resource);
		int poolID = m_liPoolID[resourceIdx];

		GameObject spawnedObject = PoolManager.Spawn(poolID, position, quaternion, parent ?? transform, inWorldPos);
		spawnedObject.GetComponent<WeightedObject>().m_bActivateSlope = withSlope;

		RefreshResourceRatio(resourceIdx, poolID);

		return spawnedObject;
	}

	public void Destroy(GameObject resourceObject)
	{
		int resourceIdx = Find(resourceObject);

		MovableObject movableObject = resourceObject.GetComponent<MovableObject>();
		movableObject.EnablePhysics();
		movableObject.Rigidbody.velocity = Vector3.zero;

		movableObject.ReturnToPool();

		PoolManager.Despawn(resourceObject);

		RefreshResourceRatio(resourceIdx, m_liPoolID[resourceIdx]);
	}

	private int Find(GameObject resource)
	{
		for (int i = 0; i < m_acResourceSettings.Length; i++)
		{
			if (m_acResourceSettings[i].m_cPrefab == resource)
				return i;
		}

		return 0;
	}

	private void RefreshResourceRatio(int resourceIdx, int poolID)
	{
		m_afResourcesInPoolRatio[resourceIdx] = (float)PoolManager.GetPoolSize(poolID) / (float)m_iResourceQuantity;
	}
}
