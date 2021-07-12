using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpaceshipSettings : ScriptableObject
{
    [HideInInspector]
    public int m_iSizeList = 0;
    [HideInInspector]
    [SerializeField]
    public List<SpaceshipeRecipeComponent> m_lcSpaceshipRecipeCompoment = new List<SpaceshipeRecipeComponent>();
}
