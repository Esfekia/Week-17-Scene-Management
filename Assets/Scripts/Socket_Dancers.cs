using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Socket_Dancers : MonoBehaviour
{
        
    public GameObject socketDancers;
    bool isDancing = false;

    private void Start()
    {
        
    }

    public void OnFuseSocketed()
    {
        socketDancers.SetActive(true);
            
    }

    public void onFuseRemoved()
    {
        socketDancers.SetActive(false);
    }

    
}
