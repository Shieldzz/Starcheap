using System;
using UnityEngine;

public class BendDeformer : MonoBehaviour
{
    public event Action OnFinishBending = null;

    [SerializeField]
    private AnimationCurve m_cAnimationCurve = null; 

    private Mesh m_cMesh = null;
    private Mesh m_cMeshCopy = null;

    protected float m_fTimer = 0.0f;

    private Matrix4x4 m_m4x4Axis;
    private Matrix4x4 m_m4x4AxisInverse;

    private Vector3[] m_vVertices;
    private Vector3[] m_vVerticesBase;
    private int m_iVertexCount = 0;

    [Header("Animation")]
    [SerializeField, Range(0.0f, 1.0f)]
    private float m_fAngle = 0.0f;
    [SerializeField]
    protected float m_fDuration = 5.0f;

    [SerializeField]
    protected Vector3 m_vAxisBlend;

    [SerializeField, Range(float.Epsilon, float.MaxValue)]
    private float m_fCoeff = 1.0f;

    private float m_fMinHeight = float.MaxValue;
    private float m_fMaxHeight = float.MinValue;

    private float m_fOneOverHeight = 0.0f;

    [SerializeField]
    protected bool m_bIsBending = false;

    #region MonoBehaviour
    protected virtual void Awake()
    {
        m_cMesh = GetComponent<MeshFilter>().mesh;
    }

    protected virtual void Start()
    {
        InitAnimationCurve();

        m_cMeshCopy = Instantiate(m_cMesh);
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = m_cMeshCopy;

        InitalizeMinMaxHeight();
    }

    protected virtual void Update()
    {
        if (m_bIsBending)
            if (m_fTimer < m_fDuration)
            {
                m_fTimer += Time.deltaTime;
                float coeff = m_fTimer / m_fDuration;
                UpdateAnimationCurve(coeff, 1.0f, 0.0f);
                CurvePlaque();

                //m_fTimer += Time.deltaTime * (1.0f / m_fSpeedBending);
            }
            else
            {
                m_bIsBending = false;
                OnFinishBending();
            }

	}

    protected virtual void FixedUpdate()
    {
    }

    #endregion

    public void StartBend()
    {
        m_bIsBending = true;
    }

    private void InitAnimationCurve()
    {
        m_cAnimationCurve = null;
        m_cAnimationCurve = new AnimationCurve();
        m_cAnimationCurve.AddKey(new Keyframe(0f,0f));
        m_cAnimationCurve.AddKey(new Keyframe(1f, 0f));

        m_iVertexCount = m_cMesh.vertexCount;
        m_vVertices = m_cMesh.vertices;
        m_vVerticesBase = m_cMesh.vertices;

        Quaternion rotation = Quaternion.Euler(m_vAxisBlend);
        m_m4x4Axis = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(rotation), Vector3.one);
        m_m4x4AxisInverse = m_m4x4Axis.inverse;
    }

    private void UpdateAnimationCurve(float value, float weightKey0, float weightKey1)
    {
        Keyframe key1 = m_cAnimationCurve.keys[1];
        m_cAnimationCurve.RemoveKey(1);
        key1.value = value;
        key1.inWeight = weightKey1;
        key1.weightedMode = WeightedMode.In;

        Keyframe key0 = m_cAnimationCurve.keys[0];
        m_cAnimationCurve.RemoveKey(0);
        key0.outWeight = weightKey0;
        key0.weightedMode = WeightedMode.Out;

        m_cAnimationCurve.AddKey(key0);
        m_cAnimationCurve.AddKey(key1);
    }

    private void InitalizeMinMaxHeight()
    {
        // Find the min/max height.
        for (int i = 0; i < m_iVertexCount; i++)
        {
            var position = m_m4x4Axis.MultiplyPoint3x4(m_vVerticesBase[i]);
            if (position.z > m_fMaxHeight)
                m_fMaxHeight = position.z;
            if (position.z < m_fMinHeight)
                m_fMinHeight = position.z;
        }
        m_fOneOverHeight = 1f / (m_fMaxHeight - m_fMinHeight);
    }

    private void CurvePlaque() 
    {
        for (int i = 0; i < m_iVertexCount; i++)
        {
            var position = m_m4x4Axis.MultiplyPoint3x4(m_vVerticesBase[i]);

            var normalizedHeight = (position.z - m_fMinHeight) * m_fOneOverHeight;

            position.y += m_cAnimationCurve.Evaluate(normalizedHeight) * m_fCoeff;

            m_vVertices[i] = m_m4x4AxisInverse.MultiplyPoint3x4(position);
        }

        m_cMeshCopy.vertices = m_vVertices;
    }
    /*
    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_cMesh.vertexCount; i++)
        {
            if ((i + 1) != m_cMesh.vertexCount)
                Gizmos.DrawLine(m_cMesh.vertices[i], m_cMesh.vertices[i+1]);
        }
    }*/
}
