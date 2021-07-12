using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RandomEventManager : MonoBehaviour
{
	private static RandomEventManager m_cInstance = null;
	public static RandomEventManager Instance
	{
		get { return m_cInstance; }
	}

	[SerializeField]
	private float m_fEventInterval = 1f;
	private float m_fEventIntervalTimer = 0f;

	[SerializeField]
	private RandomEvent[] m_acRandomEventsPrefab;

    private RandomEvent[] m_acRandomEvents;
    private int m_iRandomEventsLength = 0;

	[SerializeField, Range(50f, 100f)]
	private float m_fAccurateResourcePercentage;

	private int m_iResourcesNumber;

	private GameObject[] m_acResourcesPrefab;

	private RandomEvent m_cCurrentRandomEvent;

	private Dictionary<GameObject, float> m_dcfResourcesPercentage = new Dictionary<GameObject, float>();
	private List<KeyValuePair<GameObject, float>> m_lcfResourcesPercentageList;

	private List<GameObject> m_lcNeedlessResources = new List<GameObject>();

	private float m_fDefaultPercentage;

	private PostProcessProfileManager m_cPostProcessProfileManager;


	#region MonoBehavior
	private void Awake()
	{
		if (m_cInstance == null)
			m_cInstance = this;
		else
			Destroy(gameObject);

		m_iRandomEventsLength = m_acRandomEventsPrefab.Length;

        m_acRandomEvents = new RandomEvent[m_iRandomEventsLength];
		for (int eventIdx = 0; eventIdx < m_iRandomEventsLength; ++eventIdx)
		{
			RandomEvent newRandomEvent = Instantiate(m_acRandomEventsPrefab[eventIdx], transform);
			newRandomEvent.OnEventEnd += EventEnd;
            m_acRandomEvents[eventIdx] = newRandomEvent;
		}
	}

	private void Start()
	{
		ResourceSettings[] resourceSettings = ResourceObjectManager.Instance.ResourceSettings;
		m_iResourcesNumber = resourceSettings.Length;

		m_acResourcesPrefab = new GameObject[m_iResourcesNumber];

		m_fDefaultPercentage = 100f / m_iResourcesNumber;

		for (int resourceIdx = 0; resourceIdx < m_iResourcesNumber; ++resourceIdx)
		{
			GameObject prefab = resourceSettings[resourceIdx].m_cPrefab;
			m_acResourcesPrefab[resourceIdx] = prefab;
			m_dcfResourcesPercentage[prefab] = m_fDefaultPercentage;
		}

		m_cPostProcessProfileManager = GameObject.FindGameObjectWithTag("GameCamera").GetComponent<PostProcessProfileManager>();
	}

	private void Update()
	{
		m_fEventIntervalTimer += Time.deltaTime;

		if (m_fEventIntervalTimer >= m_fEventInterval)
		{
			m_fEventIntervalTimer = 0f;

			LaunchEvent();
		}
	}
	#endregion

	public GameObject GetRandomGeneratedResource()
	{
		RefreshResourcesPercentage();

		float randomNumber = UnityEngine.Random.Range(1f, 100f);

		float totalPercentage = 0;

		foreach (KeyValuePair<GameObject, float> resourcePercentage in m_lcfResourcesPercentageList)
		{
			if (totalPercentage < randomNumber && randomNumber <= totalPercentage + resourcePercentage.Value)
				return resourcePercentage.Key;

			totalPercentage += resourcePercentage.Value;
		}

		return null;
	}

	public void StopCurrentEvent()
	{
		if (m_cCurrentRandomEvent != null)
			m_cCurrentRandomEvent.Stop();
	}

	private void LaunchEvent()
	{
		int randomEventIdx = UnityEngine.Random.Range(0, m_iRandomEventsLength);

		m_cCurrentRandomEvent = m_acRandomEvents[randomEventIdx];
		m_cCurrentRandomEvent.Launch();

		m_cPostProcessProfileManager.SwitchProfile(m_cCurrentRandomEvent.PostProcessProfile);
	}

	private void EventEnd()
	{
		m_cPostProcessProfileManager.SwitchProfile();

		m_cCurrentRandomEvent = null;
	}

	private void RefreshResourcesPercentage()
	{
		PieceManager pieceManager = PieceManager.Instance;

		int totalResourcesNeeded = pieceManager.TotalResourcesNeeded;

		if (totalResourcesNeeded != 0)
		{
			float percentagePerResource = m_fAccurateResourcePercentage / totalResourcesNeeded;

			float percentageUse = 0f;

			m_lcNeedlessResources.Clear();

			int[] resourcesNeededNumber = pieceManager.ResourcesNeededNumber;
			float[] resourcesInPoolRatio = ResourceObjectManager.Instance.ResourcesInPoolRatio;

			for (int resourceIdx = 0; resourceIdx < m_iResourcesNumber; ++resourceIdx)
			{
				float resourcePercentage = percentagePerResource * resourcesNeededNumber[resourceIdx] * resourcesInPoolRatio[resourceIdx];

				if (resourcePercentage != 0f)
				{
					percentageUse += resourcePercentage;

					m_dcfResourcesPercentage[m_acResourcesPrefab[resourceIdx]] = resourcePercentage;
				}
				else
					m_lcNeedlessResources.Add(m_acResourcesPrefab[resourceIdx]);
			}

			float percentageLeftover = 100f - percentageUse;

			percentagePerResource = percentageLeftover / m_lcNeedlessResources.Count;

			foreach (GameObject resource in m_lcNeedlessResources)
				m_dcfResourcesPercentage[resource] = percentagePerResource;

			m_lcfResourcesPercentageList = m_dcfResourcesPercentage.ToList();

			m_lcfResourcesPercentageList.Sort((x, y) => y.Value.CompareTo(x.Value));
		}
		else
		{
			foreach (GameObject key in m_dcfResourcesPercentage.Keys.ToList())
				m_dcfResourcesPercentage[key] = m_fDefaultPercentage;

			m_lcfResourcesPercentageList = m_dcfResourcesPercentage.ToList();
		}
	}
}
