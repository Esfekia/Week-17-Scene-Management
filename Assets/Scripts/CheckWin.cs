using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckWin : MonoBehaviour
{
    public GameObject winMessage;
    
    // Start is called before the first frame update
    void Start()
    {
        winMessage.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            // win!
            if (!winMessage.activeInHierarchy)
            {
                winMessage.SetActive(true);
            }
            
        }
    }
}
