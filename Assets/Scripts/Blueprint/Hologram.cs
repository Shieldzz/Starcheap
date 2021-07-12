using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hologram : MonoBehaviour {

    private GameObject m_cModel;
    private BoxCollider m_cBoxCollider;

    public void SetModels(GameObject model)
    {
        m_cModel = model;
        m_cBoxCollider = m_cModel.GetComponent<BoxCollider>();
    }

    public void SwitchModelToHologram()
    {
        m_cModel.SetActive(false);
    }

    public void SwitchHologramToModel()
    {
        m_cModel.SetActive(true);
    }

    public void EnableModel(bool enable)
    {
        m_cModel.SetActive(enable);
    }

    public void EnableCollider(bool enable)
    {
        m_cBoxCollider.enabled = enable;
    }
}
