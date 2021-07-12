using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIRecipe : MonoBehaviour
{
	[SerializeField]
	private UIRecipeElement m_cRecipeElementPrefab = null;

	private List<UIRecipeElement> m_lcRecipeElements = new List<UIRecipeElement>();


	public void Clear()
	{
		foreach (UIRecipeElement recipeElement in m_lcRecipeElements)
			Destroy(recipeElement.gameObject);

		m_lcRecipeElements.Clear();
	}

	public UIRecipeElement AddRecipeElement(Piece piece)
	{
		UIRecipeElement newRecipeElement = Instantiate(m_cRecipeElementPrefab, transform);

		newRecipeElement.Init(piece);
		newRecipeElement.OnHidden += RemoveRecipeElement;

		m_lcRecipeElements.Add(newRecipeElement);

		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

		return newRecipeElement;
	}

	public void RemoveRecipeElement(Piece piece)
	{
		UIRecipeElement recipeElementToRemove = m_lcRecipeElements.Find(recipeElement => recipeElement.Piece == piece);

		if (recipeElementToRemove != null)
			RemoveRecipeElement(recipeElementToRemove);
	}

	private void RemoveRecipeElement(UIRecipeElement recipeElementToRemove)
	{
		m_lcRecipeElements.Remove(recipeElementToRemove);

		Destroy(recipeElementToRemove.gameObject);
	}
}
