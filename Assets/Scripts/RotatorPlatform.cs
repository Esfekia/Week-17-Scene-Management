using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorPlatform : MonoBehaviour
{
    [Header("Rotation Controls")]

    [Tooltip("The axis around which the gameObject is rotating")]
    [SerializeField]
    private Vector3 rotationAxis = Vector3.up;

    [Tooltip("The speed with which the gameObject is rotating. Measured in degrees/frame")]
    [SerializeField]
    private float rotationSpeed = 0.2f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed);
    }
}
