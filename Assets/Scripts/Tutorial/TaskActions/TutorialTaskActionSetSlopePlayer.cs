using UnityEngine;


public class TutorialTaskActionSetSlopePlayer : TutorialTaskAction
{
	[SerializeField]
	private bool m_bSlope = false;


	protected override void SetTaskAction()
	{
		TaskAction += () => 
		{
			Player[] players = GameManager.Instance.Players;
			
			foreach (Player player in players)
				player.m_bActivateSlope = m_bSlope;
		};
	}
}
