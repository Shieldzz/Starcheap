using UnityEngine;


public class ButtonSelection : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer m_cButton = null;

	[SerializeField]
	private Material m_cSelectedMaterial = null;

	private Material m_cOriginalMaterial;


	#region MonoBehavior
	private void Awake()
	{
		m_cOriginalMaterial = m_cButton.material;
	}
	#endregion

	public void Selected()
	{
		m_cButton.material = m_cSelectedMaterial;
	}

	public void Unselected()
	{
		m_cButton.material = m_cOriginalMaterial;
	}
}
