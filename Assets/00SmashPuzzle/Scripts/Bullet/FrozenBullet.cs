using System.Collections;
using UnityEngine;

public class FrozenBullet: Bullet
{
    private Rigidbody rb;
    private void Awake()
    {
        rb.GetComponent<Rigidbody>();
    }

    public override void Launch(Vector3 initialVelocity)
    {
        base.Launch(initialVelocity);
    }

    public override void ResetBullet()
    {
        base.ResetBullet();
    }

    

}