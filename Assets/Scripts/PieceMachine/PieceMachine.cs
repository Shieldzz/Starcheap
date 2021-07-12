using UnityEngine;


public class PieceMachine : MonoBehaviour
{
    private PieceManager m_cPieceManager;

    [SerializeField]
    private GameObject m_cPieceSpawn = null;

    private int m_iPiecesCount = 0;
	private int[] m_aiNeededPieces;
    private GameObject[] m_acHolograms;

    [SerializeField]
    private float m_fTimeBetweenTwoPiece = 1.5f;
    private float m_fTimer = 0.0f;

	[SerializeField]
	private int m_iMaxBlueprints = 4;
	private int m_iBlueprintsNumber = 0;

	[SerializeField]
	private SpaceshipSettings m_cSpaceshipSettings = null;

	private bool m_bLockMachine = false;

    private int m_iIndexPiece = 0;

	[Header("Animations")]
	[SerializeField]
	private Animator m_cAnimator = null;


	#region MonoBehavior
	private void Start()
    {
        m_cPieceManager = PieceManager.Instance;
        m_iPiecesCount = m_cPieceManager.PieceCount;
		m_cPieceManager.OnBlueprintDespawn += OnPieceCompleted;
		m_cPieceManager.OnPieceDestroy += OnPieceDestroyed;

		m_aiNeededPieces = new int[m_iPiecesCount];
		m_acHolograms = new GameObject[m_iPiecesCount];

		SpaceshipeRecipeComponent[] spaceshipRecipeComponents = m_cSpaceshipSettings.m_lcSpaceshipRecipeCompoment.ToArray();
		for (int idx = 0; idx < m_iPiecesCount; ++idx)
		{
			SpaceshipeRecipeComponent recipeComponent = spaceshipRecipeComponents[idx];
			Debug.Log("Piece " + recipeComponent.m_cPieceSettings.m_cPrefabHologram.name + " with index " + recipeComponent.m_cPieceSettings.m_iIndex + "\nNeed " + recipeComponent.m_iNumber);
			int pieceIndex = recipeComponent.m_cPieceSettings.m_iIndex;
			m_aiNeededPieces[pieceIndex] = recipeComponent.m_iNumber;

			GameObject hologram = Instantiate(recipeComponent.m_cPieceSettings.m_cPrefabHologram, m_cPieceSpawn.transform);
			hologram.SetActive(false);
			m_acHolograms[pieceIndex] = hologram;
		}

		m_acHolograms[0].SetActive(true);

		ResetTimer();

		m_iBlueprintsNumber = 0;
		m_iIndexPiece = 0;

		m_cAnimator.SetBool("isActive", true);
	}
	
	private void Update()
    {
		if (!m_bLockMachine)
		{
			m_fTimer -= Time.deltaTime;

			if (m_fTimer < 0)
			{
				SwitchPiece();
				ResetTimer();
			}
		}
	}
	#endregion

	public Piece GetPieceInMachine()
	{
		if (m_iBlueprintsNumber < m_iMaxBlueprints && ContainsNeededPieces())
		{
			++m_iBlueprintsNumber;

			Piece piece = m_cPieceManager.SpawnObject(m_iIndexPiece);
			piece.OnPieceCompleted += OnPieceCompleted;

			--m_aiNeededPieces[m_iIndexPiece];

			if (m_iBlueprintsNumber == m_iMaxBlueprints || !ContainsNeededPieces())
				LockMachine();
			else
			{
				SwitchPiece();
				ResetTimer();
			}

			return piece;
		}

		return null;
	}

	private void SwitchPiece()
    {
		m_acHolograms[m_iIndexPiece].SetActive(false);

		do
			m_iIndexPiece = (m_iIndexPiece + 1) % m_iPiecesCount;
		while (m_aiNeededPieces[m_iIndexPiece] == 0);

		m_acHolograms[m_iIndexPiece].SetActive(true);
    }

	private void LockMachine()
	{
		m_bLockMachine = true;
		m_acHolograms[m_iIndexPiece].SetActive(false);
		ResetTimer();

		m_cAnimator.SetBool("isActive", false);
	}

	private void UnlockMachine()
	{
		m_bLockMachine = false;

		SwitchPiece();

		m_cAnimator.SetBool("isActive", true);
	}

	private void ResetTimer()
	{
		m_fTimer = m_fTimeBetweenTwoPiece;
	}

	private bool ContainsNeededPieces()
	{
		for (int idx = 0; idx < m_iPiecesCount; ++idx)
		{
			if (m_aiNeededPieces[idx] > 0)
				return true;
		}

		return false;
	}

	private void OnPieceCompleted(Piece piece)
	{
		--m_iBlueprintsNumber;

		if (ContainsNeededPieces())
			UnlockMachine();
	}

	private void OnPieceDestroyed(Piece piece)
	{
		++m_aiNeededPieces[piece.IndexPieceRecipeComponent];

		if (m_iBlueprintsNumber < m_iMaxBlueprints && m_bLockMachine)
			UnlockMachine();
	}
}
