using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGlowRange : MonoBehaviour
{
    public GameObject glowy;
    public bool secondTime = false;

    // Start is called before the first frame update
    void Start()
    {
        glowy.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MainCamera") && secondTime == false)
        {
            if (!glowy.activeInHierarchy)
            {
                glowy.SetActive(true);
            }
            secondTime = true;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            glowy.SetActive(false);
        }
    }
}
