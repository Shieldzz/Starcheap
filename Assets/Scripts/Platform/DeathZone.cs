using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class DeathZone : MonoBehaviour
{
	private int m_iLayerPlayer;
	private int m_iLayerResourceObject;
	private int m_iLayerBlueprintObject;
	private int m_iLayerPiece;
	private int m_iLayerLiftedObject;
	private int m_iLayerNoCollision;
	private int m_iLayerMeteor;
	private int m_iLayerPlate;
	private int m_iLayerPlatformBorder;
	private int m_iLayerDustCloud;


	#region MonoBehavior
	private void Awake()
	{
		m_iLayerPlayer = LayerMask.NameToLayer("Player");
		m_iLayerResourceObject = LayerMask.NameToLayer("ResourceObject");
		m_iLayerBlueprintObject = LayerMask.NameToLayer("BlueprintObject");
		m_iLayerPiece = LayerMask.NameToLayer("Piece");
		m_iLayerLiftedObject = LayerMask.NameToLayer("LiftedObject");
		m_iLayerNoCollision = LayerMask.NameToLayer("NoCollision");
		m_iLayerMeteor = LayerMask.NameToLayer("Meteor");
		m_iLayerPlate = LayerMask.NameToLayer("Plate");
		m_iLayerPlatformBorder = LayerMask.NameToLayer("PlatformBorder");
		m_iLayerDustCloud = LayerMask.NameToLayer("DustCloud");
	}

	private void OnTriggerEnter(Collider other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer != 0)
		{
			if (otherLayer == m_iLayerPlayer)
			{
				other.GetComponentInParent<Player>().Die();
			}
			else if (otherLayer == m_iLayerResourceObject)
			{
				ResourceObjectManager.Instance.Destroy(other.gameObject);
			}
			else if (otherLayer == m_iLayerBlueprintObject || otherLayer == m_iLayerPiece)
			{
				PieceManager.Instance.DespawnObject(other.gameObject, true);
			}
			else if (otherLayer == m_iLayerLiftedObject)
			{
				if (other.GetComponent<MovableObject>().ObjectType == MovableObject.E_OBJECT_TYPE.Resource)
					ResourceObjectManager.Instance.Destroy(other.gameObject);
				else
					PieceManager.Instance.DespawnObject(other.gameObject, true);
			}
			else if (otherLayer == m_iLayerNoCollision)
			{
				MovableObject movableObject = other.GetComponent<MovableObject>();
				if (movableObject != null)
				{
					if (movableObject.ObjectType == MovableObject.E_OBJECT_TYPE.Resource)
						ResourceObjectManager.Instance.Destroy(other.gameObject);
					else
						PieceManager.Instance.DespawnObject(other.gameObject, true);
				}
				else
					other.transform.parent.gameObject.SetActive(false);
			}
			else if (otherLayer == m_iLayerMeteor)
			{
				Meteor meteor = other.GetComponent<Meteor>();
				meteor.HideMeteor();
				meteor.Despawn();
			}
			else if (otherLayer == m_iLayerPlate || otherLayer == m_iLayerPlatformBorder)
			{
				PlateCollider plateCollider = other.GetComponent<PlateCollider>();

				if (plateCollider != null)
					plateCollider.Destroy();
			}
            else if (otherLayer == m_iLayerPlatformBorder)
            {
                other.transform.parent.gameObject.SetActive(false);
            }
			else if (otherLayer == m_iLayerDustCloud)
			{
				PoolManager.Despawn(other.gameObject);
			}
		}
	}
	#endregion
}
