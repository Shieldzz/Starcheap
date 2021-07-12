using System;
using UnityEngine;


public abstract class TutorialTaskAction : MonoBehaviour
{
	protected event Action TaskAction = null;

	[SerializeField]
	private bool m_bExecuteOnTaskStart = false;
	public bool ExecuteOnTaskStart
	{
		get { return m_bExecuteOnTaskStart; }
	}

	[SerializeField]
	private bool m_bExecuteOnTaskEnd = false;
	public bool ExecuteOnTaskEnd
	{
		get { return m_bExecuteOnTaskEnd; }
	}

	protected bool m_bExecuted = false;


	#region MonoBehavior
	private void Awake()
	{
		SetTaskAction();
	}

	private void OnDestroy()
	{
		TaskAction = null;
	}
	#endregion

	public void Execute()
	{
		if (TaskAction != null)
			TaskAction();

		m_bExecuted = true;
	}

	public virtual void StopExecution()
	{
		m_bExecuted = false;
	}

	protected abstract void SetTaskAction();
}
