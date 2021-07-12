using System;
using UnityEngine;


public class CharacterSelection : MonoBehaviour
{
	public enum E_CHARACTER
	{
		Raccoon,
		Giraffe,
		Lizard,
		Penguin
	}

	[SerializeField]
	private CharacterSelector[] m_acCharacterSelectors = new CharacterSelector[4];

	[SerializeField]
	private Player[] m_acPlayers = new Player[4];

	[SerializeField]
	private PlayerModel[] m_acPlayersModel = new PlayerModel[4];

	private E_CHARACTER[] m_aePlayerCharacters = new E_CHARACTER[4];

	private int m_iPlayerReadyNum = 0;

	private bool m_bAllPlayersReady = false;


	#region MonoBehavior
	private void Start()
	{
		for (int idx = 0; idx < 4; ++idx)
		{
			m_acCharacterSelectors[idx].OnStateChange += PlayerReadyChange;

			Color[] playersColor = GameManager.Instance.PlayersColor;
			m_acPlayersModel[idx].SetAntennaColor(playersColor[idx]);
		}

		GameManager.Instance.SetLockPlayersInputs(false);
		GameManager.Instance.SetLockPlayersInteraction(false);
	}


	private void Update()
	{
		if (m_bAllPlayersReady)
		{
			foreach (Player player in m_acPlayers)
			{
				if (!player.CharacterSelected)
					return;
			}

			m_bAllPlayersReady = false;
			AllPlayersReady();
		}

		#region DevDebug
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			m_aePlayerCharacters[0] = E_CHARACTER.Raccoon;
			m_aePlayerCharacters[1] = E_CHARACTER.Giraffe;
			m_aePlayerCharacters[2] = E_CHARACTER.Lizard;
			m_aePlayerCharacters[3] = E_CHARACTER.Penguin;

			AllPlayersReady();
		}
#endif
		#endregion
	}

	#endregion

	private void PlayerReadyChange(int playerIdx, E_CHARACTER character)
	{
		if (playerIdx != -1)
		{
			++m_iPlayerReadyNum;

			m_aePlayerCharacters[playerIdx] = character;
		}
		else
			--m_iPlayerReadyNum;

		m_bAllPlayersReady = m_iPlayerReadyNum == 4;
	}

	private void SavePlayersSelection()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("P1Character", (int)m_aePlayerCharacters[0]);
		PlayerPrefs.SetInt("P2Character", (int)m_aePlayerCharacters[1]);
		PlayerPrefs.SetInt("P3Character", (int)m_aePlayerCharacters[2]);
		PlayerPrefs.SetInt("P4Character", (int)m_aePlayerCharacters[3]);
		PlayerPrefs.Save();
	}

	private void AllPlayersReady()
	{
		SavePlayersSelection();

		UIManager.Instance.ShowTutorialPopup();
	}
}
