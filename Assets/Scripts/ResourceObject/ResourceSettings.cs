using UnityEngine;


[System.Serializable]
public class ResourceSettings : ScriptableObject
{
	public Sprite m_cSpriteUI;
    public GameObject m_cPrefab;
    public Color m_cColor;

    public int m_iIndex = 0;
}
