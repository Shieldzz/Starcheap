using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PieceSettings : ScriptableObject
{
    public GameObject m_cPrefabModel = null;
    public GameObject m_cPrefabHologram = null;

    public Sprite[] m_acSpritesUI = new Sprite[4];

    public int m_iWeight = 1;
    public float m_fSlopeCoeff = 0.02f;

    public int m_iNomberOfPiece = 1;

    public int m_iIndex = 0;

    [HideInInspector]
    public int m_iSizeList = 0;
    [HideInInspector]
    [SerializeField]
    public List<PieceRecipeComponent> m_lcPieceRecipeCompoment = new List<PieceRecipeComponent>();
}

