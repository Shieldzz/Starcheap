using UnityEngine;


public class TutorialTaskActionUnlockController : TutorialTaskAction
{

	protected override void SetTaskAction()
	{
		TaskAction += () => {
			foreach (Controller controller in GameManager.Instance.Controllers)
				controller.LockGameInputs = false;
		};
	}
}
