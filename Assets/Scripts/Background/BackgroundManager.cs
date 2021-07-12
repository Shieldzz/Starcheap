using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


public class BackgroundManager : MonoBehaviour
{
	[Header("Background")]
	[SerializeField]
	private MeshRenderer m_cGround = null;

	[Header("Transition")]
	[SerializeField]
	private GameObject m_cBiomeTransition = null;

	[SerializeField]
	private float m_fStartTransitionPositionX = -4f;
	[SerializeField]
	private float m_fEndTransitionPositionX = 4f;

	//[SerializeField]
	//private float m_fTransitionDuration = 5f;
	//private float m_fTransitionTimer = 0f;

	[SerializeField]
	private float m_fTransitionSpeed = 3f;
	private bool m_bTansition = false;

	[Header("Biome")]
	[SerializeField]
	private Vector3 m_vBiomeSpawn = Vector3.zero;

	[SerializeField]
	private float m_fBiomeZoneSize = 10f;

	[SerializeField]
	private float m_fBiomeZoneOffset = 0f;

	[SerializeField]
	private Biome[] m_acBiomes = new Biome[1];

	[Header("Biomes parameters")]
	[SerializeField]
	private float m_fScrollingSpeed = 1f;
	[SerializeField]
	private float m_fMinBiomeDuration = 10f;
	[SerializeField]
	private float m_fMaxBiomeDuration = 30f;
	private float m_fCurrentBiomeTimer = 0f;

	private Biome m_cCurrentBiome;
    private int m_iCurrentBiomeIdx = -1;
	private List<BiomeElement> m_lcSpawnedBiomeElements = new List<BiomeElement>();

	private float m_fFrequency = 0f;
	private float m_fGenerationTimer = 0f;

	private int m_iLayerBiomeElement;

    private int[][] m_aaiPoolsID;

	private Transform m_cTransform = null;

    #region MonoBehavior
    private void Awake()
	{
		m_iLayerBiomeElement = LayerMask.NameToLayer("BiomeElement");

		m_cTransform = GetComponent<Transform>();

		PreloadBiomeEnvironment();

        m_cCurrentBiome = m_acBiomes[0];
        m_cCurrentBiome.m_cAmbianceSound.Play();

        ChangeBiome();
    }

	private void Update()
	{
		if (!m_bTansition)
		{
			m_fCurrentBiomeTimer -= Time.deltaTime;

			if (m_fCurrentBiomeTimer <= 0f)
				ChangeBiome();
		}

		BiomeGeneration();
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject otherGameObject = other.gameObject;

		if (otherGameObject.layer == m_iLayerBiomeElement)
		{
			m_lcSpawnedBiomeElements.Remove(otherGameObject.GetComponent<BiomeElement>());
			PoolManager.Despawn(otherGameObject);
		}
	}
	#endregion

	#region Initialisation
	private void PreloadBiomeEnvironment()
	{
		int biomesSize = m_acBiomes.Length;
		m_aaiPoolsID = new int[biomesSize][];

		int environmentSize;

		for (int biomeIdx = 0; biomeIdx < biomesSize; ++biomeIdx)
		{
			Biome biome = m_acBiomes[biomeIdx];
			BiomeElement[] biomeElements = biome.m_acEnvironmentPrefabs;

			environmentSize = biomeElements.Length;

			m_aaiPoolsID[biomeIdx] = new int[environmentSize];

			for (int environmentIdx = 0; environmentIdx < environmentSize; ++environmentIdx)
				m_aaiPoolsID[biomeIdx][environmentIdx] = PoolManager.Preload(biomeElements[environmentIdx].gameObject, m_cTransform, (int)(100f / biome.m_fDensity));
		}
	}
	#endregion

	#region Biome
	private void ChangeBiome()
	{
		StartCoroutine(BiomeTransition());

        if (m_iCurrentBiomeIdx != -1)
            m_cCurrentBiome.m_cAmbianceSound.Stop();

        m_iCurrentBiomeIdx = Random.Range(0, m_acBiomes.Length);
        m_cCurrentBiome = m_acBiomes[m_iCurrentBiomeIdx];

		m_fFrequency = 100f / m_cCurrentBiome.m_fDensity;
		m_fGenerationTimer = m_fFrequency;
	}

	private IEnumerator BiomeTransition()
	{
		m_bTansition = true;

		Transform biomeTransitionTransform = m_cBiomeTransition.transform;

		Vector3 position = biomeTransitionTransform.localPosition;
		position.x = m_fStartTransitionPositionX;
		biomeTransitionTransform.localPosition = position;

		while (biomeTransitionTransform.localPosition.x <= 0f)
		{
			position = biomeTransitionTransform.localPosition;
			position.x += m_fTransitionSpeed * Time.deltaTime;
			biomeTransitionTransform.localPosition = position;

			yield return null;
		}

		//m_fTransitionTimer = m_fTransitionDuration;
		m_cGround.material = m_cCurrentBiome.m_cGroundMaterial;

		foreach (BiomeElement biomeElement in m_lcSpawnedBiomeElements)
			PoolManager.Despawn(biomeElement.gameObject);

		m_lcSpawnedBiomeElements.Clear();

		//while (m_fTransitionTimer >= 0f)
		//{
		//	m_fTransitionTimer -= Time.deltaTime;
		//	yield return null;
		//}

		while (biomeTransitionTransform.localPosition.x <= m_fEndTransitionPositionX)
		{
			position = biomeTransitionTransform.localPosition;
			position.x += m_fTransitionSpeed * Time.deltaTime;
			biomeTransitionTransform.localPosition = position;

			yield return null;
		}

		m_fCurrentBiomeTimer = Random.Range(m_fMinBiomeDuration, m_fMaxBiomeDuration);

        ChangeSoundBiome();

		m_bTansition = false;
	}

	private void BiomeGeneration()
	{
		m_fGenerationTimer -= Time.deltaTime;

		if (m_fGenerationTimer <= 0f)
		{
			m_fGenerationTimer = m_fFrequency;

			Vector3 biomeElementPosition = new Vector3(m_vBiomeSpawn.x, m_vBiomeSpawn.y, Random.Range(-m_fBiomeZoneSize / 2f, m_fBiomeZoneSize / 2f) + m_fBiomeZoneOffset + transform.position.z);
			float biomeElementYRotation = Random.Range(0f, 359f);

            int random = Random.Range(0, m_cCurrentBiome.m_acEnvironmentPrefabs.Length);
            //BiomeElement biomeElementPrefab = m_cCurrentBiome.m_acEnvironmentPrefabs[random];

			GameObject newObject = PoolManager.Spawn(m_aaiPoolsID[m_iCurrentBiomeIdx][random], biomeElementPosition, Quaternion.Euler(0f, biomeElementYRotation, 0f), transform, true);
			BiomeElement newBiomeElement = newObject.GetComponent<BiomeElement>();
			newBiomeElement.Init(m_fScrollingSpeed);

			m_lcSpawnedBiomeElements.Add(newBiomeElement);
		}
	}
	#endregion

    private void ChangeSoundBiome()
    {
        m_cCurrentBiome.m_cAmbianceSound.Play();
    }

	#region Gizmos
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		if (m_cGround != null)
			Gizmos.DrawWireCube(transform.position + new Vector3(0f, 0f, m_fBiomeZoneOffset), new Vector3(m_cGround.transform.lossyScale.x * 10f, 1f, m_fBiomeZoneSize));

		Gizmos.color = Color.blue;
		Gizmos.DrawCube(m_vBiomeSpawn, new Vector3(5f, 5f, 5f));
	}
	#endregion
}
