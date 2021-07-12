using UnityEngine;


public class ScoreManager : MonoBehaviour
{
	private static ScoreManager m_cInstance = null;
	public static ScoreManager Instance
	{
		get { return m_cInstance; }
	}

	[Header("Scoring system")]
	[SerializeField]
	private int m_iResourceAddedOnPieceScore = 1;
	[SerializeField]
	private int m_iPieceCompletedScore = 1;
	[SerializeField]
	private int m_iPieceAddedOnSpaceshipScore = 1;
	[SerializeField]
	private int m_iPlayerDeathScore = 1;
	[SerializeField]
	private int m_iTimerSecondsScore = 1;
	[SerializeField]
	private int m_iPerPlayerEscapedScore = 1;

    [SerializeField]
    private int m_iFirstStarScore = 2000;
    public int FirstStarScore
    {
        get { return m_iFirstStarScore; }
    }
    [SerializeField]
    private int m_iSecondStarScore = 4000;
    public int SecondStarScore
    {
        get { return m_iSecondStarScore; }
    }
    [SerializeField]
    private int m_iThirdStarScore = 6000;
    public int ThirdStarScore
    {
        get { return m_iThirdStarScore; }
    }

    [SerializeField]
    private int m_iMaxPlayerScore = 300;
    public int MaxPlayerScore
    {
        get { return m_iMaxPlayerScore; }
    }

    [SerializeField]
	private Player[] m_acPlayers = new Player[4];

	private int[] m_aiPlayersScore = new int[4];
	public int[] PlayersScore
	{
		get { return m_aiPlayersScore; }
	}

	private int m_iGlobalScore = 0;
	public int GlobalScore
	{
		get { return m_iGlobalScore; }
	}


	#region MonoBehavior
	private void Awake()
	{
		if (m_cInstance == null)
			m_cInstance = this;
		else
		{
			Destroy(gameObject);
			return;
		}


		for (int playerIdx = 0; playerIdx < 4; ++playerIdx)
		{
			Player currPlayer = m_acPlayers[playerIdx];
			currPlayer.OnDeath += PlayerDied;
			currPlayer.OnResourceAddedInPiece += ResourceAddedOnPiece;
			currPlayer.OnPieceCompleted += PieceCompleted;
			currPlayer.OnPieceAddedInSpaceship += PieceAddedOnSpaceship;

			m_aiPlayersScore[playerIdx] = 0;
			m_iGlobalScore = 0;
		}
	}
	#endregion

	#region Score calculation
	public void ComputeGlobalScore()
	{
		foreach (int score in m_aiPlayersScore)
			m_iGlobalScore += score;
	}

	public void ComputeTimerScore(float currentTime, float maxTime)
	{
		float ratio = currentTime / maxTime;

		m_iGlobalScore += (int)(m_iTimerSecondsScore * ratio);
	}

	public void ComputePlayerEscapedScore(int numberEscaped)
	{
		m_iGlobalScore += numberEscaped * m_iPerPlayerEscapedScore;
	}

	private void ResourceAddedOnPiece(int playerIdx)
	{
		m_aiPlayersScore[playerIdx] += m_iResourceAddedOnPieceScore;
	}

	private void PieceCompleted(int playerIdx)
	{
		m_aiPlayersScore[playerIdx] += m_iPieceCompletedScore;
	}

	private void PieceAddedOnSpaceship(int playerIdx)
	{
		m_aiPlayersScore[playerIdx] += m_iPieceAddedOnSpaceshipScore;
	}

	private void PlayerDied(int deadPlayerIdx)
	{
		m_aiPlayersScore[deadPlayerIdx] = (int)Mathf.Max(0f, m_aiPlayersScore[deadPlayerIdx] - m_iPlayerDeathScore);
	}
	#endregion
}
