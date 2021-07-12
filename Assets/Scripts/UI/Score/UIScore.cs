using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIScore : MonoBehaviour
{
    private Transform m_cTransform = null;
    private GameObject m_cGameObject = null;

    [SerializeField]
    private ScoringUIModel[] m_acCharactersPrefab = new ScoringUIModel[4];

    [SerializeField]
    private Transform[] m_acPlayersPos = new Transform[4];

    [SerializeField]
    private TextMeshProUGUI[] m_acPlayersScoreLabel = new TextMeshProUGUI[4];

    [SerializeField]
    private TextMeshProUGUI m_cGlobalScoreLabel = null;

    [SerializeField]
    private GameObject m_cFirstButtonSelected = null;

    private Camera m_cCamera = null;

    [Header("Scoring")]
    [SerializeField]
    private MeshRenderer[] m_acStar = new MeshRenderer[3];
    [SerializeField]
    private Material m_cStarMaterial = null;

    [Header("FX")]
    [SerializeField]
    private GameObject m_cFireworksBlueFX = null;
    [SerializeField]
    private GameObject m_cFireworksPinkFX = null;

    [SerializeField]
    private float m_fTimerToStartFX = 1.0f;

    [SerializeField]
    private float m_fTimerBtwnFX = 1.0f;
    private float m_fCurrTimerFX = 0.0f;

    private bool m_bIsScoreShow = false;
    private bool m_bIsFXStart = false;

    #region MonoBehavior
    private void Awake()
    {
        m_cTransform = GetComponent<Transform>();
        m_cGameObject = gameObject;

        m_cFireworksBlueFX.SetActive(false);
        m_cFireworksPinkFX.SetActive(false);
    }

    private void Update()
    {
        if (m_bIsScoreShow)
        {
            m_fCurrTimerFX += Time.deltaTime;
            if (m_fCurrTimerFX > m_fTimerToStartFX)
            {
                m_bIsScoreShow = false;
                m_bIsFXStart = true;
                m_fCurrTimerFX = 0.0f;
                m_cFireworksBlueFX.SetActive(true);
            }
        }

        if (m_bIsFXStart)
        {
            m_fCurrTimerFX += Time.deltaTime;
            if (m_fCurrTimerFX > m_fTimerBtwnFX)
            {
                m_cFireworksPinkFX.SetActive(true);
                m_bIsFXStart = false;
            }
        }
    }
    #endregion

    private void OnBecameVisible()
    {
        EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected, new BaseEventData(EventSystem.current));
    }

    public void SetScores(Transform spaceship)
    {
		GameManager gameMgr = GameManager.Instance;
        StarManager starMgr = StarManager.Instance;

		int[] playersCharacterChoice = gameMgr.PlayersCharacterChoice;
        int[] playersScore = ScoreManager.Instance.PlayersScore;

        m_cTransform.position = spaceship.position;
        starMgr.Init();

        for (int idx = 0; idx < 4; ++idx)
        {
			ScoringUIModel model = m_acCharactersPrefab[playersCharacterChoice[idx]];

            model.Transform.position = m_acPlayersPos[idx].position;
            model.Win();
			model.GetComponent<PlayerModel>().SetAntennaColor(gameMgr.PlayersColor[idx]);

            starMgr.LaunchStars(playersScore[idx],idx);
            m_acPlayersScoreLabel[idx].text = playersScore[idx].ToString();
        }

        ShowStarGlobalScore();
        m_bIsScoreShow = true;
    }

    private void ShowStarGlobalScore()
    {
        ScoreManager scoreManager = ScoreManager.Instance;
        int globalScore = scoreManager.GlobalScore;
        m_cGlobalScoreLabel.text = globalScore.ToString();

        if (globalScore > scoreManager.FirstStarScore)
        {
            int idx = 1;
            if (globalScore > scoreManager.SecondStarScore)
            {
                idx++;
                if (globalScore > scoreManager.ThirdStarScore)
                    idx++;
            }

            for (int i = 0; i < idx; ++i)
                m_acStar[i].material = m_cStarMaterial;
        }
    }

    public void SetActive(bool active)
    {
        m_cGameObject.SetActive(active);
        m_cFireworksBlueFX.SetActive(!active);
        m_cFireworksPinkFX.SetActive(!active);

    }
}
