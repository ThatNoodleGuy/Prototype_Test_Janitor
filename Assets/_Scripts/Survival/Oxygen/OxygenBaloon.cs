using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenBaloon : MonoBehaviour
{
    public int myValue;
    OxygenSpawner oxygenSpawner;

    private void Awake()
    {

        #if UNITY_6000_0_OR_NEWER
			oxygenSpawner = FindAnyObjectByType<OxygenSpawner>();
		#else
            oxygenSpawner = FindObjectOfType<OxygenSpawner>();
		#endif

        int r = Random.Range(3, 5);
        myValue = r;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "TrashBox")
        {
            oxygenSpawner.pointsTarget -= myValue;
            Destroy(gameObject, 0.3f);
        }
    }
}
