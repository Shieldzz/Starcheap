using UnityEngine;


public class TutorialTaskActionBalancePlatform : TutorialTaskAction
{
	private Transform m_cPlatformTransform;

	private bool m_bBalance = false;


	#region MonoBehavior
	private void Start()
	{
		m_cPlatformTransform = GameManager.Instance.PlatformTransform;
	}

	private void FixedUpdate()
	{
		if (m_bBalance)
		{
			m_cPlatformTransform.rotation = Quaternion.Euler(m_cPlatformTransform.rotation.eulerAngles + new Vector3(0f, 0f, 4f * Time.fixedDeltaTime));

			if (m_cPlatformTransform.rotation.eulerAngles.z <= 0.5f)
				m_bBalance = false;
		}
	}
	#endregion

	protected override void SetTaskAction()
	{
		TaskAction += () => { m_bBalance = true; };
	}
}
