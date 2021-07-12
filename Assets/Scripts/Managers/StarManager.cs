using UnityEngine;

public class StarManager : MonoBehaviour
{
    private static StarManager m_cInstance = null;
    public static StarManager Instance
    {
        get { return m_cInstance; }
    }

    private Transform m_cTransform = null;

    [SerializeField]
    private GameObject m_cStar = null;

    [SerializeField]
    private int m_iNbOfStarMax = 50;

    [SerializeField]
    private float m_fTimerToSpawn = 2.0f;

    private int m_iPoolID = 0;

    [SerializeField]
    private SpawnStar[] m_lcStarGameObject = new SpawnStar[4];

    #region MonoBehaviour
    private void Awake()
    {
        #region Instance
        if (m_cInstance == null)
            m_cInstance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        #endregion

        m_cTransform = GetComponent<Transform>();
        m_iPoolID = PoolManager.Preload(m_cStar, m_cTransform, m_iNbOfStarMax * 4);
    }
    #endregion

    public void Init()
    {
        Physics.gravity = new Vector3(0.0f, 0.0f, -9.81f);
    }

    public void LaunchStars(int score, int idx)
    {
        int maxPlayerScore = ScoreManager.Instance.MaxPlayerScore;

        int nb = 0;
        if (score >= maxPlayerScore)
            nb = (maxPlayerScore * m_iNbOfStarMax) / maxPlayerScore;
        else if (score > 0)
            nb = (score * m_iNbOfStarMax) / maxPlayerScore;

        m_lcStarGameObject[idx].Init(nb, m_iPoolID, m_fTimerToSpawn);
    }

    public void Disable()
    {
        Physics.gravity = new Vector3(0.0f, -9.81f, 0.0f);

        foreach (SpawnStar spawnStar in m_lcStarGameObject)
            spawnStar.Disable();
    }
}
