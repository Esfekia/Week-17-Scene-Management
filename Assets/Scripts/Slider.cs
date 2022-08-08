using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
    public Transform startPosition = null;
    public Transform endPosition = null;

    MeshRenderer meshRenderer = null;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

    }

    public void OnSlideStart()
    {
        meshRenderer.material.SetColor("_Color", Color.red);

    }

    public void onSlideStop()
    {
        meshRenderer.material.SetColor("_Color", Color.white);
    }

    public void UpdateSlider(float percent)
    {
        transform.position = Vector3.Lerp(startPosition.position, endPosition.position, percent);
    }
}
