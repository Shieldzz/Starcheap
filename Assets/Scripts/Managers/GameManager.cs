using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class GameManager : MonoBehaviour
{
	private static GameManager m_cInstance = null;
	public static GameManager Instance
	{
		get { return m_cInstance; }
	}

    private CameraOrientation m_cCameraOrientation = null;
    private CameraTargetGroup m_cCameraTargetGroup = null;

	[SerializeField]
	private DestructionManager m_cDestructionManager = null;

	[SerializeField]
	private List<string> m_lsSceneNamesAlive = new List<string>();

	[SerializeField]
	private string m_sGameSceneName = "";
	public string GameSceneName
	{
		get { return m_sGameSceneName; }
	}

	[SerializeField]
	private string m_sTutorialSceneName = "";
	public string TutorialSceneName
	{
		get { return m_sTutorialSceneName; }
	}

	[SerializeField]
	private string m_sSecretLevelName = "";

	[Header("Players")]
	[SerializeField]
	private Player[] m_acPlayers = new Player[4];
	public Player[] Players
	{
		get { return m_acPlayers; }
	}

	private int[] m_aiPlayersCharacterChoice = new int[4];
	public int[] PlayersCharacterChoice
	{
		get { return m_aiPlayersCharacterChoice; }
	}

	[SerializeField]
	private PlayerModel[] m_acCharactersModelPrefab = new PlayerModel[4];

	[SerializeField]
	private Color[] m_acPlayersColor = new Color[4];
	public Color[] PlayersColor
	{
		get { return m_acPlayersColor; }
	}

	private ScoreManager m_cScoreManager = null;

	[Header("UI")]
	[SerializeField]
	private UIManager m_cUIManager = null;

    private Transform m_cPlatformTransform = null;
    public Transform PlatformTransform
    {
        get { return m_cPlatformTransform; }
    }

    private Platform m_cPlatform = null;
    public Platform Platform
    {
        get { return m_cPlatform; }
    }

    private Spaceship m_cSpaceship = null;
    public Spaceship Spaceship
    {
        get { return m_cSpaceship; }
    }

    private Transform m_cSpaceshipFocusCamera = null;

    [SerializeField]
    private float m_fWinTimerShowScoringScreen = 8.0f;

    [SerializeField]
    private float m_fLoseTimerShowScoringScreen = 6.0f;

    [Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cLevelMusic = null;

	[SerializeField]
	private StudioEventEmitter m_cWinJingle = null;
	[SerializeField]
	private StudioEventEmitter m_cLoseJingle = null;

	[SerializeField]
	private StudioEventEmitter m_cDefaultSnapshot = null;
	[SerializeField]
	private StudioEventEmitter m_cPauseSnapshot = null;

	private Controller[] m_acControllers = new Controller[4];
	public Controller[] Controllers
	{
		get { return m_acControllers; }
	}

	private int m_iDeadPlayerNumber = 0;

	private bool m_bIsGamePaused = false;
    public bool IsGamePaused
    {
        get { return m_bIsGamePaused; }
    }
    private bool m_bIsGameFinish = false;
    public bool IsGameFinish
    {
        get { return m_bIsGameFinish; }
    }

	private bool m_bInTutorial = false;

	#region	DevDebug
#if UNITY_EDITOR
	[Header("Dev Debug")]
	[SerializeField]
	private GameObject m_cSceneLoadingPrefab = null;
	[SerializeField]
	private LocalizationManager m_cLocalizationManagerPrefab = null;
	[SerializeField]
	private Controller m_cControllerPrefab = null;
#endif
	#endregion


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

		SceneManager.sceneLoaded += OnSceneChange;

		m_cPlatformTransform = GameObject.FindGameObjectWithTag("Platform").transform;

		m_aiPlayersCharacterChoice = new int[4] { 0, 1, 2, 3 };

        InitPlayers();
		InitControllers();

		m_cUIManager.GameTimer.OnTimerEnd += Lose;
		m_cUIManager.GameTimer.OnPhaseChange += ChangeLevelMusicPhase;

		m_cDefaultSnapshot.Play();
	}
	#endregion

	public void Pause()
	{
        if (!m_bIsGameFinish)
        {
            m_bIsGamePaused = !m_bIsGamePaused;

            if (m_bIsGamePaused)
            {
                Time.timeScale = 0f;

				m_cDefaultSnapshot.Stop();
				m_cPauseSnapshot.Play();

				foreach (Controller controller in m_acControllers)
					controller.PauseVibration();
            }
            else
            {
                Time.timeScale = 1f;

				m_cPauseSnapshot.Stop();
				m_cDefaultSnapshot.Play();

				foreach (Controller controller in m_acControllers)
					controller.ResumeVibration();
			}

            for (int controllerIdx = 0; controllerIdx < m_acControllers.Length; ++controllerIdx)
            {
                Controller controller = m_acControllers[controllerIdx];
                
                if (controller != null)
                {
                    if (m_bIsGamePaused)
                        controller.LockGameInputs = m_bIsGamePaused;
                    else
                        controller.LockGameInputs = controller.BlockStateBeforePause;
                }
            }

            m_cUIManager.SetPauseMenuVisible(m_bIsGamePaused);
        }
	}

	#region Initialisation
	private void InitControllers()
	{
		m_acControllers = FindObjectsOfType<Controller>();

#if UNITY_EDITOR
		if (m_acControllers.Length != 4)
			DevDebug();
#endif

		Array.Sort(m_acControllers, delegate (Controller a, Controller b)
		{
			return a.PlayerNumber.CompareTo(b.PlayerNumber);
		});

		for (int playerIdx = 0; playerIdx < m_acPlayers.Length; ++playerIdx)
		{
			Player currPlayer = m_acPlayers[m_aiPlayersCharacterChoice[playerIdx]];
			currPlayer.m_iIndex = playerIdx;
			currPlayer.OnDeath += PlayerDied;
			currPlayer.OnRespawn += PlayerRespawn;
			currPlayer.OnEnterSpaceship += PlayerEnterSpaceship;

			m_acControllers[playerIdx].m_cPlayer = currPlayer;

			// TODO : Call this when camera cinematic has finished
			m_acControllers[playerIdx].LockInputs = false;
		}
	}

	private void InitPlayers()
	{
		m_acPlayers = FindObjectsOfType<Player>();

		Array.Sort(m_acPlayers, delegate (Player a, Player b)
		{
			return a.BindedCharacter.CompareTo(b.BindedCharacter);
		});
	}

	private void InitGame()
	{
		m_bInTutorial = false;

		InitPlayers();
		SetCharactersToPlayers();
		InitControllers();

		m_iDeadPlayerNumber = 0;

		m_cDestructionManager = GameObject.FindGameObjectWithTag("DestructionManager").GetComponent<DestructionManager>();
		m_cDestructionManager.StartDeterioration();

		m_cScoreManager = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
		m_cPlatformTransform = GameObject.FindGameObjectWithTag("Platform").transform;
        m_cPlatform = GameObject.FindGameObjectWithTag("Platform").GetComponent<Platform>();
        m_cCameraOrientation = GameObject.FindGameObjectWithTag("CameraOrientation").GetComponent<CameraOrientation>();
        m_cCameraTargetGroup = GameObject.FindGameObjectWithTag("CameraTargetGroup").GetComponent<CameraTargetGroup>();

		m_cSpaceship = GameObject.FindGameObjectWithTag("Spaceship").GetComponent<Spaceship>();
        m_cSpaceshipFocusCamera = GameObject.FindGameObjectWithTag("SpaceshipFocusCamera").transform;
		m_cSpaceship.OnWinEvent += Win;
        m_cSpaceship.FollowSpline.OnFinishSpline += SpaceshipFinishFlying;
        m_cSpaceship.OnLaunchPlatform += DisableGame;
        m_cSpaceship.OnLaunchSpaceship += LaunchSpaceship;
        m_cSpaceship.FollowSpline.OnAlmostFinished += m_cSpaceship.StartZooming;

        m_cPlatform.OnFinishResetPlatform += m_cSpaceship.LaunchSpaceship;

        // m_cSpaceship.OnCompleteSpaceshipEvent += FinishSpaceship;
		//m_cSpaceship.OnLaunchTimerToEnterSpaceship += LaunchTimerToEnterSpaceship;
        //m_cSpaceship.OnFinishTakeoff += DisableGame;

        m_cUIManager.StartGame();
		m_cUIManager.EnterSpaceshipTimer.OnTimerEnd -= LaunchSpaceship;
		m_cUIManager.EnterSpaceshipTimer.OnTimerEnd += LaunchSpaceship;

		ChangeLevelMusicPhase(0);
		m_cLevelMusic.Play();

		m_bIsGameFinish = false;

		SetLockPlayersInputs(false);
		SetLockPlayersInteraction(false);
	}

	private void InitTuto()
	{
		m_bInTutorial = true;

		InitPlayers();
		SetCharactersToPlayers();
		InitControllers();

		m_iDeadPlayerNumber = 0;

		m_cPlatformTransform = GameObject.FindGameObjectWithTag("Platform").transform;
		m_cPlatform = GameObject.FindGameObjectWithTag("Platform").GetComponent<Platform>();
		m_cCameraOrientation = GameObject.FindGameObjectWithTag("CameraOrientation").GetComponent<CameraOrientation>();
		m_cCameraTargetGroup = GameObject.FindGameObjectWithTag("CameraTargetGroup").GetComponent<CameraTargetGroup>();

		m_cSpaceship = GameObject.FindGameObjectWithTag("Spaceship").GetComponent<Spaceship>();
		m_cSpaceship.OnLaunchSpaceship += TutoEnd;
	}
    #endregion

    #region PlayerManagement
	public void SetLockPlayersInputs(bool lockInputs)
	{
		foreach (Controller controller in m_acControllers)
			controller.LockInputs = lockInputs;
	}

    public void SetLockPlayersInteraction(bool lockInteraction)
	{
		foreach (Controller controller in m_acControllers)
			controller.m_bLockInteraction = lockInteraction;
	}

	public void SetLockPlayerInteraction(bool lockInteraction, int playerIndex)
	{
		foreach (Controller controller in m_acControllers)
		{
			if (controller.PlayerNumber == playerIndex)
				controller.m_bLockInteraction = lockInteraction;
		}
	}

	public void SetControllersVibration(ControllerVibrationData vibrationData)
	{
		foreach (Controller controller in m_acControllers)
			controller.PlayVibration(vibrationData);
	}

	private void SetCharactersToPlayers()
	{
		for (int playerIdx = 0; playerIdx < m_acPlayers.Length; ++playerIdx)
		{
			int playerCharacterIndex = PlayerPrefs.GetInt("P" + (playerIdx + 1) + "Character");

			Player currPlayer = m_acPlayers[playerCharacterIndex];

			m_aiPlayersCharacterChoice[playerIdx] = playerCharacterIndex;
			PlayerModel currModel = Instantiate(m_acCharactersModelPrefab[playerCharacterIndex], currPlayer.Transform);
			currModel.transform.localPosition = Vector3.zero;
			currModel.SetAntennaColor(m_acPlayersColor[playerIdx]);

			currPlayer.m_cPlayerModel = currModel;
		}
	}

	private void PlayerDied(int deadPlayerIdx)
	{
		if (!m_bInTutorial)
		{
			++m_iDeadPlayerNumber;

			if (m_iDeadPlayerNumber == 4)
				Lose();
		}
	}

	private void PlayerRespawn()
	{
		--m_iDeadPlayerNumber;
	}

    private void PlayerEnterSpaceship(int playerIdx)
    {
        m_acControllers[playerIdx].LockInputs = true;
    }
	#endregion

	#region UI
	public void Lose()
	{
        if (!m_bIsGameFinish)
        {
            DisableGameInput();

            m_cUIManager.GameLose();

            m_cLevelMusic.Stop();
            m_cLoseJingle.Play();

            Timer gameTimer = m_cUIManager.GameTimer;
            m_cScoreManager.ComputeTimerScore(gameTimer.CurrTime, gameTimer.TimerInSeconds);

            m_cCameraTargetGroup.Clear();
            m_cCameraOrientation.DisableFocusPlatform();
            m_bIsGameFinish = true;

            DisableGame();

            m_cScoreManager.ComputeGlobalScore();

            m_cPlatform.DisablePlatformLose();
            m_cDestructionManager.LaunchDestruction();
        }
    }

    public void Win()
    {
        if (!m_bIsGameFinish)
        {
            m_cUIManager.GameWin();

            Timer gameTimer = m_cUIManager.GameTimer;
            m_cScoreManager.ComputeTimerScore(gameTimer.CurrTime, gameTimer.TimerInSeconds);

            m_cScoreManager.ComputePlayerEscapedScore(m_cSpaceship.PlayerInSpaceship);
            m_cScoreManager.ComputeGlobalScore();

            m_bIsGameFinish = true;

            m_cUIManager.LaunchTimerToEnterSpaceship();
        }
    }

    private void LaunchSpaceship()
    {
        DisableGameInput();

		m_cLevelMusic.Stop();
		m_cWinJingle.Play();

        m_cUIManager.StopTimerToEnterSpaceship();

        //m_cSpaceship.LaunchSpaceship();
        m_cCameraOrientation.enabled = false;

        m_cCameraTargetGroup.SetSpaceshipTarget(m_cSpaceshipFocusCamera);

		m_cDestructionManager.LaunchDestruction();
        m_cPlatform.DisablePlatformWin();
        DisableGame();
	}

    private void DisableGame()
    {
        foreach (Player player in m_acPlayers)
            player.DisableEvent();

		RandomEventManager randomEventManager = RandomEventManager.Instance;
		randomEventManager.StopCurrentEvent();
		randomEventManager.enabled = false;

        m_iDeadPlayerNumber = 0;
    }

    private void DisableGameInput()
    {
        for (int controllerIdx = 0; controllerIdx < m_acControllers.Length; ++controllerIdx)
        {
            m_acControllers[controllerIdx].LockGameInputs = true;
            m_acPlayers[controllerIdx].DisableVelocity();
        }
    }

    private void SpaceshipFinishFlying()
    {
        m_cCameraTargetGroup.SetCameraScoringScreen(m_cSpaceship.Transform);
		Invoke("ShowScoreScreen", m_fWinTimerShowScoringScreen);
    }

	private void ShowScoreScreen()
	{
        m_cUIManager.ShowScore();

		m_cSpaceship.PlayScoringAnim();
	}
    #endregion

    public void KillSounds()
    {
		m_cWinJingle.Stop();
		m_cLoseJingle.Stop();

		Bus masterFMOD = FMODUnity.RuntimeManager.GetBus("Bus:/");
        masterFMOD.stopAllEvents(STOP_MODE.IMMEDIATE);
    }

    public void Restart()
	{
        KillSounds();
        UIEndGame.Instance.RestartUI();

		CancelInvoke();

		SceneManager.LoadScene("Game", LoadSceneMode.Single);

		UIManager.Instance.Init();
	}

	public void Quit()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	private void TutoEnd()
	{
		SceneLoading.Instance.LoadScene(m_sGameSceneName);
	}

	private void ChangeLevelMusicPhase(int phaseIndex)
	{
		m_cLevelMusic.SetParameter("Music_Variation", phaseIndex);
	}

	private void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
		if (!m_lsSceneNamesAlive.Contains(scene.name))
			Delete();
		else if (scene.name == m_sGameSceneName)
			InitGame();
		else if (scene.name == m_sTutorialSceneName)
			InitTuto();
		else if (scene.name == m_sSecretLevelName)
			InitGame();
	}

	private void Delete()
	{
		SceneManager.sceneLoaded -= OnSceneChange;

        m_cUIManager.Delete();
        Destroy(m_cUIManager.gameObject);
		Destroy(gameObject);
	}

	#region DevDebug
#if UNITY_EDITOR
	private void DevDebug()
	{
		GameObject sceneLoading = Instantiate(m_cSceneLoadingPrefab);
		DontDestroyOnLoad(sceneLoading);

		LocalizationManager localizationManager = Instantiate(m_cLocalizationManagerPrefab);
		localizationManager.LoadLocalizationFile("en");

		m_acControllers = new Controller[4];

		for (int idx = 0; idx < 4; ++idx)
		{
			Controller controller = Instantiate(m_cControllerPrefab);
			controller.LockInputs = true;
			controller.Init(idx);

			m_acControllers[idx] = controller;

			DontDestroyOnLoad(controller);
		}

		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("P1Character", 0);
		PlayerPrefs.SetInt("P2Character", 1);
		PlayerPrefs.SetInt("P3Character", 2);
		PlayerPrefs.SetInt("P4Character", 3);
		PlayerPrefs.Save();
	}
#endif
	#endregion
}
