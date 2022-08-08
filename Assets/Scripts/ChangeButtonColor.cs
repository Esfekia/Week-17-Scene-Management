using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeButtonColor : MonoBehaviour
{
    MeshRenderer m_meshRenderer = null;

    private void Awake()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ChangeColor()
    {
        m_meshRenderer.material.SetColor("_Color", Random.ColorHSV(0, 1.0f, .9f, 1.0f, 0.9f, 1.0f));
    }

    public void LogInteractionStarted()
    {
        Debug.Log("Interacton Started");
    }

    public void LogInteractionEnded()
    {
        Debug.Log("Interaction Ended");
    }

}
