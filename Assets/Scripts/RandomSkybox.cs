using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSkybox : MonoBehaviour
{
    public Material[] skyBox; //where we set&keep the skybox materials

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void ChangeSky()
    {
        RenderSettings.skybox = skyBox[Random.Range(0, skyBox.Length)];
    }
}
