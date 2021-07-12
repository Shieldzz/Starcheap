using UnityEngine;


public class DollyTrackEnd : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cGameCamera = null;

	[SerializeField]
	private GameObject m_cTrailerCamera = null;


	private void Trigger()
	{
		m_cGameCamera.SetActive(true);
		m_cTrailerCamera.SetActive(false);
	}
}
