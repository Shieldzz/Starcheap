using UnityEngine;
using UnityEngine.UI;


public class UIRecipeResource : MonoBehaviour
{
	[SerializeField]
	private Image m_cResourceImage = null;

	[SerializeField]
	private GameObject m_cCheckImage = null;

	private GameObject m_cGameObject;

	private int m_iResourceIndex;
	public int ResourceIndex
	{
		get { return m_iResourceIndex; }
	}


	#region MonoBehavior
	private void Awake()
	{
		m_cCheckImage.SetActive(false);

		m_cGameObject = gameObject;
	}
	#endregion

	public void Init(ResourceSettings newResource)
	{
		m_cResourceImage.sprite = newResource.m_cSpriteUI;

		m_iResourceIndex = newResource.m_iIndex;
	}

	public void Done()
	{
		//m_cCheckImage.SetActive(true);

		m_cGameObject.SetActive(false);
	}
}
