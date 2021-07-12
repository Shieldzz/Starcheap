using System;
using UnityEngine;


public class CharacterViewer : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cReadyVisual = null;
	[SerializeField]
	private GameObject m_cConnectControllerVisual = null;

	[SerializeField]
	private Animator[] m_cCharacterAnimators = new Animator[4];

	public event Action<bool> OnReadyPressed = null;
	public event Action<MainMenu.E_MENU_STATE> OnReturnPressed = null;

	private int m_iCurrCharacterIndex = 0;
	public int CurrCharacterIndex
	{
		get { return m_iCurrCharacterIndex; }
	}

	private bool m_bIsReady = false;


	public void Init()
	{
		m_cReadyVisual.SetActive(false);
		m_bIsReady = false;

		m_iCurrCharacterIndex = 0;

		for (int idx = 0; idx < 4; ++idx)
			m_cCharacterAnimators[idx].SetBool("Visible", (idx == m_iCurrCharacterIndex));
	}

	public void ChangeControllerConnected(bool isConnected)
	{
		m_cConnectControllerVisual.SetActive(!isConnected);

		if (!isConnected && m_bIsReady)
			CancelSelection();
	}

	public void ChangeCharacter(Vector2 leftStickValue)
	{
		if (!m_bIsReady)
		{
			if (leftStickValue.x > 0f)
			{
				// right
				int prevIndex = (m_iCurrCharacterIndex + 3) % 4;

				ScrollRight(prevIndex);

				m_iCurrCharacterIndex = prevIndex;
			}
			else
			{
				// left
				int nextIndex = (m_iCurrCharacterIndex + 1) % 4;

				ScrollLeft(nextIndex);

				m_iCurrCharacterIndex = nextIndex;
			}
		}
	}

	public void SelectCharacter()
	{
		if (!m_bIsReady)
		{
			m_cReadyVisual.SetActive(true);
			m_bIsReady = true;

			if (OnReadyPressed != null)
				OnReadyPressed(true);
		}
	}

	public void CancelSelection()
	{
		if (m_bIsReady)
		{
			m_cReadyVisual.SetActive(false);
			m_bIsReady = false;

			if (OnReadyPressed != null)
				OnReadyPressed(false);
		}
		else if (OnReturnPressed != null)
			OnReturnPressed(MainMenu.E_MENU_STATE.TitleScreen);
	}

	private void ScrollRight(int prevIndex)
	{
		m_cCharacterAnimators[m_iCurrCharacterIndex].SetTrigger("CenterToRight");
		m_cCharacterAnimators[m_iCurrCharacterIndex].SetBool("Visible", false);

		m_cCharacterAnimators[prevIndex].SetTrigger("LeftToCenter");
		m_cCharacterAnimators[prevIndex].SetBool("Visible", true);
	}
	private void ScrollLeft(int nextIndex)
	{
		m_cCharacterAnimators[m_iCurrCharacterIndex].SetTrigger("CenterToLeft");
		m_cCharacterAnimators[m_iCurrCharacterIndex].SetBool("Visible", false);

		m_cCharacterAnimators[nextIndex].SetTrigger("RightToCenter");
		m_cCharacterAnimators[nextIndex].SetBool("Visible", true);
	}

	private void OnDestroy()
	{
		OnReadyPressed = null;
		OnReturnPressed = null;
	}
}
