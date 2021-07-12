using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIRecipeElement : MonoBehaviour
{
	public event System.Action<UIRecipeElement> OnHidden;

	[Header("Recipe")]
	[SerializeField]
	private Image m_cRecipeImage = null;

	[SerializeField]
	private Image m_cPieceImage = null;

	[SerializeField]
	private Image m_cPieceSpriteImage = null;

	[Header("Resources")]
	[SerializeField]
	private UIRecipeResource m_cRecipeResourcePrefab = null;

	[SerializeField]
	private Transform m_cResourcesParent = null;

	[SerializeField]
	private Sprite[] m_acRecipePieceSprites = new Sprite[4];

	[SerializeField]
	private Sprite[] m_acRecipeThreeResourcesSprites = new Sprite[4];

	[Header("Animations")]
	[SerializeField]
	private Animator m_cAnimator = null;

	private List<UIRecipeResource> m_lcRecipeResources = new List<UIRecipeResource>();

	private Piece m_cPiece = null;
	public Piece Piece
	{
		get { return m_cPiece; }
	}

	private bool m_bPlayAnim = true;
	private int m_iCurrentAnimStep = 0;


	#region MonoBehavior
	private void OnDestroy()
	{
		OnHidden = null;
	}
	#endregion

	public void Init(Piece piece)
	{
		m_cPiece = piece;

		int pieceColorIdx = (int)piece.PieceColor;
		m_cRecipeImage.sprite = m_acRecipeThreeResourcesSprites[pieceColorIdx];

		m_cPieceImage.sprite = m_acRecipePieceSprites[pieceColorIdx];

		m_cPieceSpriteImage.sprite = m_cPiece.UISprites[pieceColorIdx];

        foreach (PieceRecipeComponent pieceRecipe in Piece.PieceRecipeComponents)
		{
			for (int idx = 0; idx < pieceRecipe.m_iNumber; ++idx)
			{
				UIRecipeResource newResourceImage = Instantiate(m_cRecipeResourcePrefab, m_cResourcesParent);

				newResourceImage.Init(pieceRecipe.m_cResourceSettings);

				m_lcRecipeResources.Add(newResourceImage);
			}
		}

		piece.OnResourceAdded += ResourceAdded;

		m_cAnimator.Play("Show");
	}

	private void ResourceAdded(int resourceIndex)
	{
		foreach (UIRecipeResource recipeResource in m_lcRecipeResources)
		{
			if (recipeResource.ResourceIndex == resourceIndex)
			{
				recipeResource.Done();
				m_cAnimator.speed = 1f;

				++m_iCurrentAnimStep;

				if (m_bPlayAnim)
				{
					m_cAnimator.Play("RessourceDone");

					m_bPlayAnim = false;
				}
				return;
			}
		}
	}

	private void ResourceAddedStep1()
	{
		if (m_iCurrentAnimStep == 1)
			m_cAnimator.speed = 0f;
	}

	private void ResourceAddedStep2()
	{
		if (m_iCurrentAnimStep == 2)
			m_cAnimator.speed = 0f;
	}

	private void ResourceAddedAnimationEnd()
	{
		if (OnHidden != null)
			OnHidden(this);
	}
}
