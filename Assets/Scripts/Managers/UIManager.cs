using UnityEngine;
using UnityEngine.EventSystems;


public class UIManager : MonoBehaviour
{
	private static UIManager m_cInstance = null;
	public static UIManager Instance
	{
		get { return m_cInstance; }
	}

	[SerializeField]
	private PauseMenu m_cPauseMenu = null;
 
	[SerializeField]
	private GameTimer m_cGameTimer = null;
	public GameTimer GameTimer
	{
		get { return m_cGameTimer; }
	}

    [SerializeField]
	private GameObject m_cLoseUI = null;

	[SerializeField]
	private GameObject m_cWinUI = null;

	[SerializeField]
	private UIRecipe m_cRecipeInterface = null;

	[SerializeField]
	private Popup m_cPopup = null;

	[SerializeField]
	private UIEndGame m_cUIEndGame = null;

    [SerializeField]
    private ConsoleDebug m_cDebugUI = null;
    private bool m_bIsDebugUIShow = false;

	[Header("FX")]
	[SerializeField]
	private Color[] m_acParticleColors = new Color[4];

    private CountDown m_cEnterSpaceshipTimer = null;
    public CountDown EnterSpaceshipTimer
    {
        get { return m_cEnterSpaceshipTimer; }
    }


	#region MonoBehavior
	private void Awake()
	{
		if (m_cInstance == null)
		{
			m_cInstance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}
    }

	private void Start()
	{
		Init();
	}

	private void Update()
    {
#if UNITY_DEBUG || UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            if (!m_bIsDebugUIShow)
                ShowDebug(true);
            else
                ShowDebug(false);
        }
#endif
    }
#endregion

    public void Init()
	{
		m_cPauseMenu.SetActive(false);
		m_cGameTimer.SetVisible(false);
		m_cLoseUI.SetActive(false);
		m_cWinUI.SetActive(false);
		m_cPopup.SetVisible(false);
        m_cUIEndGame.SetVisible(false);

        StarManager.Instance.Disable();

		m_cRecipeInterface.Clear();
	}

	public void StartGame()
	{
		Init();

        m_cEnterSpaceshipTimer = GameObject.FindGameObjectWithTag("SpaceshipTimer").GetComponent<CountDown>();
        if (m_cEnterSpaceshipTimer)
            m_cEnterSpaceshipTimer.SetVisible(false);

		m_cGameTimer.SetVisible(true);
		m_cGameTimer.Launch();
	}

	public void SetPauseMenuVisible(bool visible)
	{
		m_cPauseMenu.SetActive(visible);
	}

	public void GameLose()
	{
		m_cGameTimer.Stop();
        m_cGameTimer.SetVisible(false);
        m_cLoseUI.SetActive(false);
        m_cEnterSpaceshipTimer.SetVisible(false);

        m_cUIEndGame.ShowLoseUI();

        m_cRecipeInterface.Clear();
    }

	public void GameWin()
	{
		m_cGameTimer.Stop();
        m_cGameTimer.SetVisible(false);
        m_cWinUI.SetActive(false);
        //m_cEnterSpaceshipTimer.SetVisible(false);

        m_cRecipeInterface.Clear();
    }

    public void LaunchTimerToEnterSpaceship()
    {
        m_cEnterSpaceshipTimer.SetVisible(true);
        m_cEnterSpaceshipTimer.Launch();
    }

    public void StopTimerToEnterSpaceship()
    {
        m_cEnterSpaceshipTimer.Stop();
        m_cEnterSpaceshipTimer.SetVisible(false);
    }

	public void ShowScore()
	{
        m_cUIEndGame.ShowWinUI();
	}

	public void ShowQuitPopup()
	{
		m_cPauseMenu.SetActive(false);

		GameManager.Instance.SetLockPlayersInputs(true);

		m_cPopup.Init("QuitPopup", 
			() => { GameManager.Instance.Quit(); }, 
			() => { m_cPauseMenu.SetActive(true); });
	}

	public void ShowTutorialPopup()
	{
		GameManager.Instance.SetLockPlayersInputs(true);

		m_cPopup.Init("TutoPopup", 
			() => { SceneLoading.Instance.LoadScene(GameManager.Instance.TutorialSceneName); }, 
			() => { SceneLoading.Instance.LoadScene(GameManager.Instance.GameSceneName); });
	}

    private void ShowDebug(bool visible)
    {
        m_bIsDebugUIShow = visible;
        m_cDebugUI.SetActive(visible);
    }

	public void ShowNewRecipe(Piece piece)
	{
		Vector3 fxPosition = m_cRecipeInterface.AddRecipeElement(piece).transform.position;

		piece.SetParticleTrailDestination(fxPosition, m_acParticleColors[(int)piece.PieceColor]);
	}

	public void HideRecipeCompleted(Piece piece)
	{
		m_cRecipeInterface.RemoveRecipeElement(piece);
	}

    public void Delete()
    {
        Destroy(m_cUIEndGame.gameObject);
    }
}
