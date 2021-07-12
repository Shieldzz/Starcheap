using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpaceshipeRecipeComponent
{
    public PieceSettings m_cPieceSettings;
    public int m_iNumber;
}

[System.Serializable]
public class PieceRecipeComponent
{
    public ResourceSettings m_cResourceSettings;
    public int m_iNumber;
}