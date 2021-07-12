using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class Cell : MonoBehaviour
{
	public int m_iImportance = 0;

	private List<WeightedObject> m_lcObjectsInCell = new List<WeightedObject>();

	private Transform m_cTransform = null;
	public Transform Transform
	{
		get { return m_cTransform; }
	}

	private int m_iMaskWeightedObject = 0;

#if UNITY_EDITOR
	public bool m_bShowDebug = false;
#endif


	#region MonoBehavior
	private void Awake()
	{
		m_cTransform = GetComponent<Transform>();

		m_iMaskWeightedObject = (1 << LayerMask.NameToLayer("ResourceObject")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Piece")) | (1 << LayerMask.NameToLayer("Meteor"));
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & m_iMaskWeightedObject) != 0)
		{
			WeightedObject otherObject = other.GetComponentInParent<WeightedObject>();

			if (otherObject.m_cCurrCell != null)
			{
				otherObject.m_acPrevCells.Add(otherObject.m_cCurrCell);
				otherObject.m_cCurrCell.RemoveWeightedObject(otherObject);
			}

			AddWeightedObject(otherObject);
			otherObject.m_cCurrCell = this;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (((1 << other.gameObject.layer) & m_iMaskWeightedObject) != 0)
		{
			WeightedObject otherObject = other.GetComponentInParent<WeightedObject>();

			if (otherObject.m_cCurrCell == this)
			{
				if (otherObject.m_acPrevCells.Count != 0)
				{
					otherObject.m_cCurrCell = otherObject.m_acPrevCells[0];
					otherObject.m_acPrevCells.RemoveAt(0);

					otherObject.m_cCurrCell.AddWeightedObject(otherObject);
				}
				else
					otherObject.m_cCurrCell = null;
			}

			otherObject.m_acPrevCells.Remove(this);

			m_lcObjectsInCell.Remove(otherObject);
		}
	}
	#endregion

	public Vector3 ComputeForce(Vector3 center, Vector3 platformSlope)
	{
		if (m_lcObjectsInCell.Count == 0)
			return Vector3.zero;

		int force = 0;

		WeightedObject currWeightedObject;

		for (int idx = 0; idx < m_lcObjectsInCell.Count; ++idx)
		{
			currWeightedObject = m_lcObjectsInCell[idx];

			if (currWeightedObject.IsOnGround)
			{
				force += m_iImportance * currWeightedObject.Weight;
				currWeightedObject.m_vSlope = platformSlope;
			}
			else
				currWeightedObject.m_vSlope = Vector3.zero;
		}

		Vector3 forceVector = (m_cTransform.position - center).normalized;
		forceVector.y = 0f;
		return forceVector * force;
	}

	public void AddWeightedObject(WeightedObject objectToAdd)
	{
		m_lcObjectsInCell.Add(objectToAdd);
	}

	public void RemoveWeightedObject(WeightedObject objectToRemove)
	{
		m_lcObjectsInCell.Remove(objectToRemove);
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (m_bShowDebug)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(transform.position, transform.lossyScale);

			GUIStyle textStyle = new GUIStyle
			{
				fontSize = 24,
				contentOffset = new Vector2(-12f, -12f)
			};
			Handles.Label(transform.position, m_iImportance.ToString(), textStyle);
		}
	}
#endif
}
