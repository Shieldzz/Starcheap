using UnityEngine;


[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class BiomeElement : MonoBehaviour
{
	private float m_fScrollSpeed = 1f;

	private Transform m_cTransform = null;


	#region MonoBehavior
	private void Awake()
	{
		m_cTransform = GetComponent<Transform>();
	}

	private void Update()
	{
		Scroll();
	}
	#endregion

	public void Init(float scrollSpeed)
	{
		m_fScrollSpeed = scrollSpeed;
	}

	private void Scroll()
	{
		Vector3 position = m_cTransform.position;
		position.x += m_fScrollSpeed * Time.deltaTime;
		m_cTransform.position = position;
	}
}
