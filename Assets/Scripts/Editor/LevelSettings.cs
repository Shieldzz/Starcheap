using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LevelSettings : ScriptableObject
{
	public GameObject m_cCellPrefab = null;

	public Vector2 m_vOvalSize = new Vector2(11f, 8f);

	public Vector2 m_vPivotPoint = new Vector2(5.5f, -5.5f);

	public int m_iHorizontalCells = 5;
	public int m_iVerticalCells = 5;

	[HideInInspector]
	public List<int> m_liForceGrid = new List<int>();
}
