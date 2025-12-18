using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxpoints : MonoBehaviour
{


    private void Start()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Test")
        {
            Destroy(gameObject);
        }
    }
}
