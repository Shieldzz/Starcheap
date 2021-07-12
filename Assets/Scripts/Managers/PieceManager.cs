using System;
using System.Collections.Generic;
using UnityEngine;


public class PieceManager : MonoBehaviour
{
	private static PieceManager m_cInstance = null;
	public static PieceManager Instance
	{
		get { return m_cInstance; }
	}

	public enum E_PIECE_COLOR
	{
		Blue,
		Green,
		Orange,
		Pink
	}

	[SerializeField]
	private Material[] m_acBlueprintMaterials = new Material[4];

	[SerializeField]
	private Material m_cBaseBlueprintMaterial = null;

	private List<E_PIECE_COLOR> m_lePieceColorAvailable = new List<E_PIECE_COLOR>(){ E_PIECE_COLOR.Blue, E_PIECE_COLOR.Green, E_PIECE_COLOR.Orange, E_PIECE_COLOR.Pink };

	[SerializeField]
    private GameObject m_cPiecePrefab = null;

    [SerializeField]
    private PieceSettings[] m_acPieceSettings;
    private int m_iPieceSettingsCount;
    public int PieceCount
    {
        get { return m_iPieceSettingsCount; }
    }

    private int[] m_liPoolID;

	private int[] m_aiResourcesNeededNumber;
	public int[] ResourcesNeededNumber
	{
		get { return m_aiResourcesNeededNumber; }
	}

	private int m_iTotalResourcesNeeded = 0;
	public int TotalResourcesNeeded
	{
		get { return m_iTotalResourcesNeeded; }
	}

	public event Action<Piece> OnBlueprintDespawn = null;
	public event Action<Piece> OnPieceDestroy = null;

    //private List<Sprite> m_lcSprite = new List<Sprite>();


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

        m_iPieceSettingsCount = m_acPieceSettings.Length;

        m_liPoolID = new int[m_iPieceSettingsCount];

        PreloadAllPieces();
    }

	private void Start()
	{
		ResourceSettings[] resourceSettings = ResourceObjectManager.Instance.ResourceSettings;

		int resourceSettingsSize = resourceSettings.Length;

		m_aiResourcesNeededNumber = new int[resourceSettingsSize];
	}

	private void OnDestroy()
	{
		OnBlueprintDespawn = null;
	}
	#endregion

	private void PreloadAllPieces()
    {
        for (int i = 0; i < m_iPieceSettingsCount; i++)
        {
            PieceSettings piece = m_acPieceSettings[i];
            if (piece.m_cPrefabModel)
               m_liPoolID[i] = PoolManager.Preload(piece.m_cPrefabModel, transform, piece.m_iNomberOfPiece);
        }
    }

    public GameObject[] GetPieces()
    {
        GameObject[] pieces = new GameObject[m_iPieceSettingsCount];

        for (int i = 0; i < m_iPieceSettingsCount; i++)
        {
            pieces[i] = Instantiate(m_cPiecePrefab);
            /*GameObject model = */Instantiate(m_acPieceSettings[i].m_cPrefabModel, pieces[i].transform);
            //Hologram hologram = pieces[i].GetComponent<Hologram>();
            //hologram.SetModels(model);
            //hologram.EnableCollider(false);
        }

        return pieces;
    }

    public GameObject[] GetHologram(Transform parent)
    {
        GameObject[] hologram = new GameObject[m_iPieceSettingsCount];

        for (int i = 0; i < m_iPieceSettingsCount; i++)
        {
            hologram[i] = Instantiate(m_acPieceSettings[i].m_cPrefabHologram, parent);
        }

        return hologram;
    }

    public Piece SpawnObject(int index)
    {
		PieceSettings pieceSettings = m_acPieceSettings[index];

		GameObject gameObject = PoolManager.Spawn(m_liPoolID[index], Vector3.zero, Quaternion.identity, transform);

        Piece piece = gameObject.GetComponent<Piece>();

		List<PieceRecipeComponent> pieceRecipe = pieceSettings.m_lcPieceRecipeCompoment;

		piece.SetRecipe(pieceRecipe, pieceSettings.m_iIndex);

		for (int pieceRecipeIdx = 0; pieceRecipeIdx < pieceRecipe.Count; ++pieceRecipeIdx)
		{
			PieceRecipeComponent pieceRecipeComponent = pieceRecipe[pieceRecipeIdx];

			int resourcesNumber = pieceRecipeComponent.m_iNumber;

			m_aiResourcesNeededNumber[pieceRecipeComponent.m_cResourceSettings.m_iIndex] += resourcesNumber;
			m_iTotalResourcesNeeded += resourcesNumber;
		}

		E_PIECE_COLOR pieceColor = m_lePieceColorAvailable[0];
		piece.Init(pieceSettings.m_acSpritesUI, pieceSettings.m_iWeight, pieceSettings.m_fSlopeCoeff, pieceColor, m_acBlueprintMaterials[(int)pieceColor]);
		m_lePieceColorAvailable.RemoveAt(0);

		piece.OnPiecePlaced += UIManager.Instance.ShowNewRecipe;
		piece.OnResourceAdded += RemoveResourceNeeded;
		piece.OnPieceCompleted += PieceComplete;

        return piece;
    }

    public void DespawnObject(GameObject objectToDespawn, bool destroy)
    {
		Piece piece = objectToDespawn.GetComponent<Piece>();

		if (piece.InteractEnum == Blueprints.E_INTERACT.Blueprint)
		{
			PieceComplete(piece);

			PieceRecipeComponent[] pieceRecipe = piece.PieceRecipeComponents;

			for (int pieceRecipeIdx = 0; pieceRecipeIdx < pieceRecipe.Length; ++pieceRecipeIdx)
			{
				PieceRecipeComponent pieceRecipeComponent = pieceRecipe[pieceRecipeIdx];

				int resourcesNumber = pieceRecipeComponent.m_iNumber;

				m_aiResourcesNeededNumber[pieceRecipeComponent.m_cResourceSettings.m_iIndex] -= resourcesNumber;
				m_iTotalResourcesNeeded -= resourcesNumber;
			}
		}

		if (piece.InteractEnum != Blueprints.E_INTERACT.End)
			RaiseOnBlueprintDespawnEvent(piece);

		if (OnPieceDestroy != null && destroy)
			OnPieceDestroy(piece);

		piece.SetMaterial(m_cBaseBlueprintMaterial);

		piece.ReturnToPool();

        PoolManager.Despawn(objectToDespawn);
    }

	private void RemoveResourceNeeded(int resourceIndex)
	{
		--m_aiResourcesNeededNumber[resourceIndex];
		--m_iTotalResourcesNeeded;
	}

	private void PieceComplete(Piece piece)
	{
		if (piece.InteractEnum != Blueprints.E_INTERACT.End)
			UIManager.Instance.HideRecipeCompleted(piece);

		m_lePieceColorAvailable.Add(piece.PieceColor);
	}

	private void RaiseOnBlueprintDespawnEvent(Piece piece)
	{
		if (OnBlueprintDespawn != null)
			OnBlueprintDespawn(piece);
	}
}
