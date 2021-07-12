using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class PostProcessProfileManager : MonoBehaviour
{
	[SerializeField]
	private PostProcessVolume m_cDefaultVolume = null;

	[SerializeField]
	private PostProcessVolume m_cRandomEventVolume = null;

	[SerializeField]
	private float m_fSwitchWeightSpeed = 1f;

	private bool m_bTransition = false;
	private bool m_bIsDefault = true;


	#region MonoBehavior
	private void Awake()
	{
		m_cDefaultVolume.weight = 1f;
		m_cRandomEventVolume.weight = 0f;
	}

	private void Update()
	{
		if (m_bTransition)
		{
			if (m_bIsDefault)
			{
				m_cDefaultVolume.weight += Time.deltaTime * m_fSwitchWeightSpeed;
				m_cRandomEventVolume.weight -= Time.deltaTime * m_fSwitchWeightSpeed;

				m_bTransition = m_cDefaultVolume.weight < 1f;
			}
			else
			{
				m_cDefaultVolume.weight -= Time.deltaTime * m_fSwitchWeightSpeed;
				m_cRandomEventVolume.weight += Time.deltaTime * m_fSwitchWeightSpeed;

				m_bTransition = m_cDefaultVolume.weight > 0f;
			}
		}
	}
	#endregion

	public void SwitchProfile(PostProcessProfile profile = null)
	{
		m_bTransition = true;
		m_bIsDefault = !m_bIsDefault;

		if (profile != null)
			m_cRandomEventVolume.profile = profile;
	}
}
