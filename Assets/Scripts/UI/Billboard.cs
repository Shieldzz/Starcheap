using UnityEngine;


public class Billboard : MonoBehaviour
{
	[SerializeField]
	private Camera m_cCamera = null;

	private Transform m_cTransform = null;
	public Transform Transform
	{
		get { return m_cTransform; }
	}


	#region MonoBehavior
	private void Awake()
	{
		m_cTransform = GetComponent<Transform>();
	}

	private void Update()
	{
		m_cTransform.LookAt(m_cCamera.transform.position, Vector3.up);
	}
	#endregion
}
