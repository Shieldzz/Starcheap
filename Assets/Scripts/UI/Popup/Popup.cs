using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class Popup : MonoBehaviour
{
	[SerializeField]
	private Canvas m_cCanvas = null;

	[SerializeField]
	private LocalizedText m_cPopupText = null;

	[SerializeField]
	private GameObject m_cYesButton = null;

	private event Action OnAccept;
	private event Action OnCancel;


	public void Init(string textLocalizationKey, Action acceptFuntion, Action cancelFunction)
	{
		EventSystem.current.SetSelectedGameObject(m_cYesButton, new BaseEventData(EventSystem.current));

		OnAccept = acceptFuntion;
		OnCancel = cancelFunction;

		m_cPopupText.UpdateTextWithKey(textLocalizationKey);

		m_cCanvas.enabled = true;
	}

	public void Accept()
	{
		if (OnAccept != null)
		{
			OnAccept();

			OnAccept = null;
			OnCancel = null;
		}

		m_cCanvas.enabled = false;

        if (GameManager.Instance)
            GameManager.Instance.SetLockPlayersInputs(false);
	}

	public void Cancel()
	{
		if (OnCancel != null)
		{
			OnCancel();

			OnAccept = null;
			OnCancel = null;
		}

		m_cCanvas.enabled = false;

        if (GameManager.Instance)
            GameManager.Instance.SetLockPlayersInputs(false);
	}

	public void SetVisible(bool visible)
	{
		m_cCanvas.enabled = visible;
	}

    public bool CanvasActive()
    {
        return m_cCanvas.isActiveAndEnabled;
    }
}
