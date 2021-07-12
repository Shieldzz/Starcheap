using System;
using System.Collections.Generic;
using UnityEngine;


public class TutorialTask : MonoBehaviour
{
	private enum E_TASK
	{
		Take,
		Drop,
		MoveTo,
		Place
	}

	[SerializeField]
	private E_TASK m_eTask = E_TASK.Take;

	public enum E_TASK_TYPE
	{
		Single,
		Group
	}

	[SerializeField]
	private E_TASK_TYPE m_eTaskType = E_TASK_TYPE.Single;
	public E_TASK_TYPE TaskType
	{
		get { return m_eTaskType; }
	}

	private enum E_REQUIRED_PLAYER
	{
		One,
		All
	}

	[SerializeField]
	private E_REQUIRED_PLAYER m_eRequiredPlayers = E_REQUIRED_PLAYER.One;

	public event Action<TutorialTask> OnTaskCompleted = null;

	[SerializeField]
	private bool m_bConstraintPlayerInteraction = false;
	[SerializeField]
	private bool m_bConstraintPlayerController = false;

	[SerializeField]
	private TutorialTaskAction[] m_acTaskActions = new TutorialTaskAction[0];

	[SerializeField]
	private Canvas m_cIndicatorCanvas = null;

	[SerializeField]
	private Canvas m_cInteractionButtonCanvas = null;

	[SerializeField]
	private GameObject m_cFXIndicator = null;

	[SerializeField]
	private SphereCollider m_cCollider = null;

	private List<int> m_liPlayersNearbyIndex = new List<int>();

	private bool m_bTaskRunning = false;

	private int m_iLayerPlayer;


	#region MonoBehavior
	private void Awake()
	{
		SetInteractionButtonVisible(false);
		SetIndicatorVisible(false);
		m_cFXIndicator.SetActive(false);

		m_cCollider.enabled = false;

		m_iLayerPlayer = LayerMask.NameToLayer("Player");
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject otherGameObject = other.gameObject;

		if (m_bTaskRunning && otherGameObject.layer == m_iLayerPlayer)
		{
			Player player = otherGameObject.GetComponentInParent<Player>();

			m_liPlayersNearbyIndex.Add(player.m_iIndex);

			if (m_eRequiredPlayers == E_REQUIRED_PLAYER.All
				&& m_eTask == E_TASK.MoveTo
				&& m_liPlayersNearbyIndex.Count == 4)
				RaiseTaskCompletedEvent(0);

			if (m_bConstraintPlayerInteraction
				&& player.HasObjectInHand
				&& m_eTask == E_TASK.Drop)
			{
				GameManager.Instance.SetLockPlayerInteraction(false, player.m_iIndex);

				SetIndicatorVisible(false);
				SetInteractionButtonVisible(true);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		GameObject otherGameObject = other.gameObject;

		if (m_bTaskRunning && otherGameObject.layer == m_iLayerPlayer)
		{
			Player player = otherGameObject.GetComponentInParent<Player>();

			m_liPlayersNearbyIndex.Remove(player.m_iIndex);

			if (m_eTask == E_TASK.Take && m_liPlayersNearbyIndex.Count == 0)
			{
				SetIndicatorVisible(true);
				SetInteractionButtonVisible(false);
			}
			else if (m_eTask != E_TASK.MoveTo && player.HasObjectInHand)
			{
				SetIndicatorVisible(true);
				SetInteractionButtonVisible(false);
			}

			if (m_bConstraintPlayerInteraction)
				GameManager.Instance.SetLockPlayerInteraction(true, player.m_iIndex);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		GameObject otherGameObject = other.gameObject;

		if (m_bTaskRunning && otherGameObject.layer == m_iLayerPlayer)
		{
			Player player = otherGameObject.GetComponentInParent<Player>();

			if ((m_eTask == E_TASK.Take && !player.HasObjectInHand) || (m_eTask == E_TASK.Place && player.HasObjectInHand))
			{
				if (player.CanInteractWithObject)
				{
					SetInteractionButtonVisible(true);
					SetIndicatorVisible(false);

					if (m_bConstraintPlayerInteraction)
						GameManager.Instance.SetLockPlayerInteraction(false, player.m_iIndex);
				}
				else
				{
					SetInteractionButtonVisible(false);
					SetIndicatorVisible(true);

					if (m_bConstraintPlayerInteraction)
						GameManager.Instance.SetLockPlayerInteraction(true, player.m_iIndex);
				}
			}
		}
	}

	private void OnDestroy()
	{
		OnTaskCompleted = null;
	}
#endregion

	public void LaunchTask()
	{
		m_bTaskRunning = true;

		m_cCollider.enabled = true;

		switch (m_eTask)
		{
			case E_TASK.Take:
			case E_TASK.Drop:
			case E_TASK.Place:
				Player[] players = GameManager.Instance.Players;
				foreach (Player player in players)
					player.OnInteractSuccessful += RaiseTaskCompletedEvent;

				break;
			case E_TASK.MoveTo:
				break;
			default: break;
		}

		foreach (TutorialTaskAction taskAction in m_acTaskActions)
		{
			if (taskAction.ExecuteOnTaskStart)
				taskAction.Execute();
		}

		SetIndicatorVisible(true);
		m_cFXIndicator.SetActive(true);

		if (m_bConstraintPlayerInteraction)
			GameManager.Instance.SetLockPlayersInteraction(true);
	}

	public void StopTask()
	{
		m_bTaskRunning = false;

		m_cCollider.enabled = false;

		SetInteractionButtonVisible(false);
		SetIndicatorVisible(false);
		m_cFXIndicator.SetActive(false);

		foreach (TutorialTaskAction taskAction in m_acTaskActions)
		{
			if (taskAction.ExecuteOnTaskEnd)
			{
				taskAction.Execute();
				taskAction.StopExecution();
			}
		}

		if (m_bConstraintPlayerInteraction)
			GameManager.Instance.SetLockPlayersInteraction(false);

		OnTaskCompleted = null;
	}

	private void RaiseTaskCompletedEvent(int playerIndex)
	{
		if (m_liPlayersNearbyIndex.Contains(playerIndex))
		{
			if (m_bConstraintPlayerController)
			{
				GameManager.Instance.Controllers[playerIndex].LockGameInputs = true;
				GameManager.Instance.Players[playerIndex].DisableVelocity();
			}

			m_liPlayersNearbyIndex.Clear();

			if (OnTaskCompleted != null)
				OnTaskCompleted(this);

			if (m_eTask != E_TASK.MoveTo)
			{
				Player[] players = GameManager.Instance.Players;
				foreach (Player player in players)
					player.OnInteractSuccessful -= RaiseTaskCompletedEvent;
			}
		}
	}

	private void SetIndicatorVisible(bool visible)
	{
		m_cIndicatorCanvas.enabled = visible;
	}

	private void SetInteractionButtonVisible(bool visible)
	{
		m_cInteractionButtonCanvas.enabled = visible;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (GetComponent<BoxCollider>() == null
			&& GetComponent<SphereCollider>() == null
			&& GetComponent<CapsuleCollider>() == null)
		{
			Debug.LogError("TutorialTask " + gameObject.name + " need a box/sphere/capsule collider");
		}
	}
#endif
}
