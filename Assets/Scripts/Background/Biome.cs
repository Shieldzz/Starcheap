using UnityEngine;
using FMODUnity;

[System.Serializable]
struct Biome
{
	public BiomeElement[] m_acEnvironmentPrefabs;
	public Material m_cGroundMaterial;
	public float m_fDensity;
    public StudioEventEmitter m_cAmbianceSound;
}
