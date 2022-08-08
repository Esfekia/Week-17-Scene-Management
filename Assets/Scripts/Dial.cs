using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dial : MonoBehaviour
{
    Vector3 m_startRotation;

    MeshRenderer m_MeshRenderer = null;


    private void Start()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
    }
    public void StartTurn()
    {
        m_startRotation = transform.localEulerAngles;
        m_MeshRenderer.material.SetColor("_Color", Color.red);
    }

    public void StopTurn()
    {
        m_MeshRenderer.material.SetColor("_Color", Color.white);
    }
    public void DialUpdate(float angle)
    {
        Vector3 angles = m_startRotation;

        // add the incoming angle to its z-axis
        angles.y += angle;

        // set that back to our local euler angles directly
        transform.localEulerAngles = angles;
    }
}
