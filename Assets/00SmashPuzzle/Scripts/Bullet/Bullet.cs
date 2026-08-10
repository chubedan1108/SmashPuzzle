using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 initialVelocity)
    {
        rb.isKinematic = false;
        rb.linearVelocity = initialVelocity;
    }

    public void ResetBullet()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = Vector3.zero; // Reset position to origin or any desired position
        transform.rotation = Quaternion.identity; // Reset rotation
    }
}
