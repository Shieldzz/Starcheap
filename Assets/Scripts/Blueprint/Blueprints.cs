using UnityEngine;
using System.Collections.Generic;


public abstract class Blueprints : MovableObject
{
    public enum E_INTERACT
    {
        Idle = 0,
        Blueprint,
        End
    };

    protected E_INTERACT m_eInteractEnum = E_INTERACT.Idle;
	public E_INTERACT InteractEnum
	{
		get { return m_eInteractEnum; }
	}

	[Header("Recipe")]
    public List<int> m_liNumberPiece = new List<int>();

    protected int m_iMeshDrawIndex = 0;
    protected int m_iMeshDrawCount;

    [Header("Material")]
	[SerializeField]
    protected Material m_cHologramMaterial = null;
	protected Color m_cMainColorHologram;
	protected Color m_cSecondaryColorHologram;
    [SerializeField]
    protected Material m_cShipMaterial = null;

    [Header("Renderer")]
    [SerializeField]
    protected List<MeshRenderer> m_cMeshRenderer = new List<MeshRenderer>();


	#region MonoBehavior
	protected new virtual void Awake()
    {
		base.Awake();

        m_cTransform = gameObject.transform;
    }
	#endregion

	virtual public bool AddObject(GameObject resourceObject)
    {
        if (m_iMeshDrawIndex < m_iMeshDrawCount)
            m_iMeshDrawIndex++;
        if (m_iMeshDrawIndex == m_iMeshDrawCount)
            m_eInteractEnum = E_INTERACT.End;

        return true;
    }

    protected void SwitchMaterial(MeshRenderer mesh, Material material)
    {
        mesh.material = material;
    }

    public void SetMaterial(Material material)
    {
        foreach (MeshRenderer meshRenderer in m_cMeshRenderer)
            SwitchMaterial(meshRenderer, material);
    }

	public abstract void BlueprintFinish();
}
