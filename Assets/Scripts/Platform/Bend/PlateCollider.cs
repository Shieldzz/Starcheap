using System.Collections.Generic;
using UnityEngine;


public class PlateCollider : MonoBehaviour
{
    private List<Transform> m_lcGameObjectCollide = new List<Transform>();

    private Transform m_cTransform = null;

    [SerializeField]
    private Rigidbody m_cRigidbody = null;
    private BoxCollider m_cBoxCollider = null;

    [SerializeField]
    private Vector3 m_vColliderAngle = new Vector3(0f, 0f, 90f);
    private Quaternion m_qColliderRotation = Quaternion.identity;

    [SerializeField]
    private float m_fForce = 7.0f;
    [SerializeField]
    private float m_fForceRotation = 5.0f;

    [SerializeField]
    private float m_fHeightMax = 1.0f;
    [SerializeField]
    private float m_fHeightMin = 0.4f;

    private Vector3 m_vDir = Vector3.zero;

    private bool m_bIsPhysicsEnable = false;

    private int m_iLayerResourceObject;
    private int m_iLayerPiece;
    private int m_iLayerPlayer;
	private int m_iLayerLiftedObject;

    private int m_iLayerMaskGrab;
	private int m_iLayerMaskGrabExit;


    #region MonoBehaviour
    private void Awake()
    {
        m_cTransform = GetComponent<Transform>();
        m_cBoxCollider = GetComponent<BoxCollider>();

        m_qColliderRotation = Quaternion.Euler(m_vColliderAngle);
    }

    private void Start()
    {
        m_iLayerResourceObject = LayerMask.NameToLayer("ResourceObject");
        m_iLayerPiece = LayerMask.NameToLayer("Piece");
        m_iLayerPlayer = LayerMask.NameToLayer("Player");
		m_iLayerLiftedObject = LayerMask.NameToLayer("LiftedObject");

        m_iLayerMaskGrab = (1 << m_iLayerPiece) | (1 << m_iLayerResourceObject) | (1 << m_iLayerPlayer);
		m_iLayerMaskGrabExit = m_iLayerMaskGrab | (1 << m_iLayerLiftedObject);
    }

    private void FixedUpdate()
    {
        if (m_bIsPhysicsEnable)
        {
            foreach (Transform otherGameObject in m_lcGameObjectCollide)
            {
                if (otherGameObject != null)
                {
                    Rigidbody rigibody = otherGameObject.GetComponent<Rigidbody>();
                    if (rigibody != null)
                        rigibody.velocity = m_cRigidbody.velocity;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

		if (((1 << otherLayer) & m_iLayerMaskGrab) != 0)
            m_lcGameObjectCollide.Add((otherLayer == m_iLayerPlayer) ? other.transform.parent : other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
		int otherLayer = other.gameObject.layer;

		if (((1 << otherLayer) & m_iLayerMaskGrabExit) != 0)
            m_lcGameObjectCollide.Remove((otherLayer == m_iLayerPlayer) ? other.transform.parent : other.transform);
    }

    public void Destroy()
    {
        m_lcGameObjectCollide.Clear();
        m_cTransform.parent.gameObject.SetActive(true);
    }
    #endregion

    public void EnablePhysics(Vector3 dir)
    {
        m_bIsPhysicsEnable = true;
        m_cTransform.rotation = m_qColliderRotation;

        float rand = Random.Range(m_fHeightMin, m_fHeightMax);

        if (dir.x >= 0.0f)
            m_vDir.x = 1.0f;
        else
            m_vDir.x = -1.0f;

        m_vDir.y = rand;

        if (m_cRigidbody)
        {
            m_cRigidbody.AddForce(m_vDir * m_fForce, ForceMode.Impulse);
            m_cRigidbody.AddTorque(m_vDir * m_fForceRotation, ForceMode.Impulse);
            m_cRigidbody.useGravity = true;
        }
    }
}
