using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever_Fire : MonoBehaviour
{
    public Transform startOrientation = null;
    public Transform endOrientation = null;

    MeshRenderer meshRenderer = null;

    public GameObject fireEffects;
    bool isFireOn = false;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

    }

    public void OnLeverPullStart()
    {
        if (isFireOn)
        {
            fireEffects.SetActive(false);
            isFireOn = false;
        }
        else
        {
            fireEffects.SetActive(true);
            isFireOn = true;
        }


    }

    public void onLeverPullStop()
    {
        //
    }

    public void UpdateLever(float percent)
    {
        transform.rotation = Quaternion.Slerp(startOrientation.rotation, endOrientation.rotation, percent);
    }
}
