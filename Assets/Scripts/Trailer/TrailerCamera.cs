using UnityEngine;


public class TrailerCamera : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cGameCamera = null;

	[SerializeField]
	private GameObject m_cTrailerCamera = null;

	[SerializeField]
	private Animator m_cAnimator = null;



	#region MonoBehavior
	private void Awake()
	{
		m_cTrailerCamera.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T))
		{
			m_cGameCamera.SetActive(false);
			m_cTrailerCamera.SetActive(true);

			m_cAnimator.SetTrigger("LaunchDolly");
		}
	}
	#endregion
}
