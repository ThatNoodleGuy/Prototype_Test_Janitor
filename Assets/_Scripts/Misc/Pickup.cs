using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public GameObject heldObject;
    public float pickupRange = 5;
    public float moveForce = 250;
    public Transform pickupDestination;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (heldObject == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange))
                {
                    PickupObject(hit.transform.gameObject);
                }
            }
            else
            {
                DropObject();
            }
        }

        if (heldObject != null)
        {
            MoveObject();
        }
    }

    void PickupObject(GameObject pickupObject)
    {
        if (pickupObject.GetComponent<Rigidbody>())
        {
            Rigidbody objectRB = pickupObject.GetComponent<Rigidbody>();
            objectRB.useGravity = false;
            objectRB.linearDamping = 10;

            //objectRB.transform.parent = pickupDestination;
            heldObject = pickupObject;
        }
    }

    void MoveObject()
    {
        if (Vector3.Distance(heldObject.transform.position, pickupDestination.position) > 0.1f)
        {
            Vector3 moveDirection = (pickupDestination.position - heldObject.transform.position);
            heldObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);
        }
    }

    void DropObject()
    {
        Rigidbody objectRB = heldObject.GetComponent<Rigidbody>();
        objectRB.useGravity = true;
        objectRB.linearDamping = 1;
        objectRB.transform.parent = null;
        heldObject = null;
    }
}
