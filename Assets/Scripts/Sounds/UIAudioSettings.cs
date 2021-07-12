using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIAudioSettings : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField]
    private Slider m_cMasterSlider = null;
    [SerializeField]
    private Slider m_cMusicsSlider = null;
    [SerializeField]
    private Slider m_cFXSlider = null;

    [Header("Emitter")]
    [SerializeField]
	private StudioEventEmitter m_cSliderSound = null;

    private FMOD.Studio.Bus m_cMasterFMOD;
    private FMOD.Studio.Bus m_cMusicsFMOD;
    private FMOD.Studio.Bus m_cGamePlayFXFMOD;
    private FMOD.Studio.Bus m_cUIFXFMOD;


    [Range(0.0f, 1.0f)]
    private float m_fMasterVolume = 0.5f;
    [Range(0.0f, 1.0f)]
    private float m_fMusicsVolume = 0.5f;
    [Range(0.0f, 1.0f)]
    private float m_fFXVolume = 0.5f;

    private bool m_bFirstValueChangeMaster = true;
	private bool m_bFirstValueChangeMusics = true;
	private bool m_bFirstValueChangeFX = true;


	#region MonoBehavior
	private void Start()
    {
	    m_cMasterFMOD = FMODUnity.RuntimeManager.GetBus("Bus:/");
        m_cMusicsFMOD = FMODUnity.RuntimeManager.GetBus("Bus:/Music_ALL");
        m_cGamePlayFXFMOD = FMODUnity.RuntimeManager.GetBus("Bus:/GamePlay_SFX");
        m_cUIFXFMOD = FMODUnity.RuntimeManager.GetBus("Bus:/UI_SFX");
        InitSliderVolume();
    }
	#endregion

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);

		if (active)
		{
			EventSystem.current.SetSelectedGameObject(m_cMasterSlider.gameObject);
			m_cMasterSlider.OnSelect(null);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			m_cMasterSlider.OnDeselect(null);
		}

		InitSliderVolume();
	}

	private void InitSliderVolume()
    {
        float temp;
        m_cMasterFMOD.getVolume(out m_fMasterVolume, out temp);
        m_cMasterSlider.value = m_fMasterVolume;

        m_cMusicsFMOD.getVolume(out m_fMusicsVolume, out temp);
        m_cMusicsSlider.value = m_fMusicsVolume;

        m_cGamePlayFXFMOD.getVolume(out m_fFXVolume, out temp);
        m_cFXSlider.value = m_fFXVolume;
    }

    public void UpdateMasterVolume(float newVolume)
    {
        m_fMasterVolume = newVolume;
        m_cMasterFMOD.setVolume(newVolume);

		if (!m_bFirstValueChangeMaster)
			m_cSliderSound.Play();
		else
			m_bFirstValueChangeMaster = false;
    }

    public void UpdateMusicsVolume(float newVolume)
    {
        m_fMusicsVolume = newVolume;
        m_cMusicsFMOD.setVolume(newVolume);

        if (!m_bFirstValueChangeMusics)
            m_cSliderSound.Play();
        else
            m_bFirstValueChangeMusics = false;
    }

    public void UpdateFXVolume(float newVolume)
    {
        m_fFXVolume = newVolume;
        m_cGamePlayFXFMOD.setVolume(newVolume);
        m_cUIFXFMOD.setVolume(newVolume);

        if (!m_bFirstValueChangeFX)
            m_cSliderSound.Play();
        else
            m_bFirstValueChangeFX = false;
    }
}
