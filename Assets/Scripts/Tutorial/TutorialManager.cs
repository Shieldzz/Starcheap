using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private TutorialTask[] m_acTutorialTasks;

	private int m_iCurrentTaskIndex = 0;
    private int m_iTutorialTaskLength = 0;

	private int m_iGroupTaskNumber = 0;


	#region MonoBehavior
	private void Awake()
	{
        m_iTutorialTaskLength = m_acTutorialTasks.Length;
	}

	private void Start()
	{
		m_iCurrentTaskIndex = 0;

		TutorialTask firstTask = m_acTutorialTasks[m_iCurrentTaskIndex];

		if (firstTask.TaskType == TutorialTask.E_TASK_TYPE.Single)
			LaunchSingleTask(firstTask);
		else
			LaunchGroupTask();
	}
	#endregion

	private void ProceedToNextTask(TutorialTask completedTask)
	{
		completedTask.StopTask();

		++m_iCurrentTaskIndex;

		if (m_iCurrentTaskIndex < m_iTutorialTaskLength)
		{
			TutorialTask nextTask = m_acTutorialTasks[m_iCurrentTaskIndex];

			if (nextTask.TaskType == TutorialTask.E_TASK_TYPE.Single)
				LaunchSingleTask(nextTask);
			else
				LaunchGroupTask();
		}
	}

	private void LaunchSingleTask(TutorialTask task)
	{
		task.OnTaskCompleted += ProceedToNextTask;
		task.LaunchTask();
	}

	private void LaunchGroupTask()
	{
		TutorialTask nextTask = m_acTutorialTasks[m_iCurrentTaskIndex];

		int groupTaskStartIndex = m_iCurrentTaskIndex;

		do
		{
			nextTask.OnTaskCompleted += TaskInGroupComplete;
			nextTask.LaunchTask();

			++m_iGroupTaskNumber;

			m_iCurrentTaskIndex = (m_iCurrentTaskIndex + 1) % m_iTutorialTaskLength;
			nextTask = m_acTutorialTasks[m_iCurrentTaskIndex];

		} while (nextTask.TaskType == TutorialTask.E_TASK_TYPE.Group && groupTaskStartIndex != m_iCurrentTaskIndex);

		m_iCurrentTaskIndex = (m_iCurrentTaskIndex + m_iTutorialTaskLength - 1) % m_iTutorialTaskLength;
	}

	private void TaskInGroupComplete(TutorialTask completedTask)
	{
		--m_iGroupTaskNumber;

		if (m_iGroupTaskNumber != 0)
			completedTask.StopTask();
		else
			ProceedToNextTask(completedTask);
	}

	#region DevDebug
#if UNITY_EDITOR
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Minus) && SceneManager.GetActiveScene().name == GameManager.Instance.TutorialSceneName)
			SceneLoading.Instance.LoadScene(GameManager.Instance.GameSceneName);
	}
#endif
	#endregion
}
