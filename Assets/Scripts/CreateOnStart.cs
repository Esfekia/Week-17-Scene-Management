using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOnStart : MonoBehaviour
{

    GameObject go;
    // Start is called before the first frame update
    void Start()
    {
        go = new GameObject();
        go.name = "Hello World";
        go.SetActive(true);
    }

}
