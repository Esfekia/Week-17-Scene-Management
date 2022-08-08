using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever_Lights : MonoBehaviour
{
    public Transform startOrientation = null;
    public Transform endOrientation = null;

    MeshRenderer meshRenderer = null;

    public GameObject lightEffects;
    bool isLightOn = false;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

    }

    public void OnLeverPullStart()
    {
        if (isLightOn)
        {
            lightEffects.SetActive(false);
            isLightOn = false;
        }
        else
        {
            lightEffects.SetActive(true);
            isLightOn = true;
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
