using FMOD.Studio;
using FMODUnity;
using UnityEngine;


public class UINavigationSounds : MonoBehaviour
{
	[EventRef]
	[Header("Button Clic")]
	[SerializeField]
	private string m_sButtonClic = "";
	[SerializeField]
	private bool m_bAllowFadeoutButtonClic = true;
	private EventInstance m_cButtonClicEvent;

	[EventRef]
	[Header("Button Hover")]
	[SerializeField]
	private string m_sButtonHover = "";
	[SerializeField]
	private bool m_bAllowFadeoutButtonHover = true;
	private EventInstance m_cButtonHoverEvent;

	private bool m_bFirstSelection = true;


	#region MonoBehavior
	private void Awake()
	{
		m_cButtonClicEvent = RuntimeManager.CreateInstance(m_sButtonClic);
		m_cButtonHoverEvent = RuntimeManager.CreateInstance(m_sButtonHover);
	}
	#endregion

	public void PlayButtonClic()
	{
		m_cButtonClicEvent.stop((m_bAllowFadeoutButtonClic) ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
		m_cButtonClicEvent.start();
	}

	public void PlayButtonHover()
	{
		if (!m_bFirstSelection)
		{
			m_cButtonHoverEvent.stop((m_bAllowFadeoutButtonHover) ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
			m_cButtonHoverEvent.start();
		}
		else
			m_bFirstSelection = false;
	}
}
