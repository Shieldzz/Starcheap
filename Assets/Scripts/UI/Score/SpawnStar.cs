using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnStar : MonoBehaviour {

    private GameObject m_cGameObject = null;
    private Transform m_cTransform = null;

    private int m_iNbOfStar = 0;

    [SerializeField]
    private float m_fShowerSizeX = 0.5f;
    [SerializeField]
    private float m_fShowerSizeY = 0.3f;

    private bool m_bIsLauchStar = false;
    private bool m_bIsWaitingToLauchStar = false;

    private int m_iIndex = 0;

    [SerializeField]
    private float m_fMaxSpawnFrequency = 1.0f;
    private float m_fSpawnTimer = 0.0f;

    private float m_fWaitToLaunchStarTimer = 1.0f;


    private GameObject[] m_lcStar;

    private float m_fForce = 100.0f;

    private int m_iPoolID;

    private Vector3 m_cRotation = new Vector3(90.0f, 0.0f, 0.0f);

    private void Awake()
    {
        m_cGameObject = gameObject;
        m_cTransform = GetComponent<Transform>();
    }
	
    private void Update()
    {
        if (m_bIsWaitingToLauchStar)
        {
            m_fSpawnTimer += Time.deltaTime;
            if (m_fSpawnTimer > m_fWaitToLaunchStarTimer)
            {
                m_bIsWaitingToLauchStar = false;
                m_fSpawnTimer = 0.0f;
                m_bIsLauchStar = true;
            }
        }

        if (m_bIsLauchStar)
        {
            m_fSpawnTimer += Time.deltaTime;
            if (m_fSpawnTimer > m_fMaxSpawnFrequency && m_iIndex <= m_iNbOfStar)
            {
                m_fSpawnTimer = 0;
                StarFall();
                m_iIndex++;
            }
        }
    }

    public void Init(int nbStar, int poolID, float TimerToWait)
    {
        m_bIsWaitingToLauchStar = true;
        m_fWaitToLaunchStarTimer = TimerToWait;
        m_fSpawnTimer = 0.0f;
        m_iNbOfStar = nbStar;
        m_iPoolID = poolID;
        m_lcStar = new GameObject[nbStar + 1];
        m_lcStar = PoolManager.Spawns(m_iPoolID, m_cTransform.position, Quaternion.identity, m_cTransform, nbStar + 1);
        foreach (GameObject star in m_lcStar)
            star.SetActive(false);
    }

    private void StarFall()
    {
        float randomX = Random.Range(-m_fShowerSizeX, m_fShowerSizeX);
        float randomY = Random.Range(-m_fShowerSizeY, m_fShowerSizeY);

        Vector3 position = Vector3.zero;
        position.x += randomX;
        position.y += randomY;

        GameObject star = m_lcStar[m_iIndex];
        star.transform.position = m_cTransform.position + position;
        star.transform.rotation = Random.rotation;
        star.SetActive(true);

        Rigidbody starRigidbody = star.GetComponent<Rigidbody>();
        starRigidbody.AddForce(m_cTransform.forward * m_fForce);
    }

    public void Disable()
    {
        PoolManager.Despawn(m_lcStar);
        m_iIndex = 0;
    }
}
