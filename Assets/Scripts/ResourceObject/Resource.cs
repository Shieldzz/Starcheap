using UnityEngine;


public class Resource : MovableObject
{
    [SerializeField]
    private ResourceSettings m_cResourceSettings;
    [HideInInspector]
    public int m_iIndex;


    protected new void Awake()
    {
		base.Awake();

        m_iIndex = m_cResourceSettings.m_iIndex;
    }

	public override void ReturnToPool()
	{
		base.ReturnToPool();

		m_cGameObject.layer = m_iLayerResourceObject;
	}
}
