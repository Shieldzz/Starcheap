using UnityEngine;


public class TutorialTaskActionActiveGameObject : TutorialTaskAction
{
	[SerializeField]
	private GameObject m_cTaskGameObject = null;

	[SerializeField]
	private bool m_bActive = false;


	protected override void SetTaskAction()
	{
		TaskAction += () => { m_cTaskGameObject.SetActive(m_bActive); };
	}
}
