using UnityEngine;

public class Ground : MonoBehaviour
{
    [SerializeField] private GameObject ballHitEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Bullet>(out _))
        {
            if (collision.contactCount > 0 && ballHitEffect != null)
            {
                ContactPoint contact = collision.GetContact(0);
                Quaternion effectRotation = Quaternion.FromToRotation(Vector3.up, contact.normal) * ballHitEffect.transform.rotation;
                GameObject effect = Instantiate(ballHitEffect, contact.point, effectRotation);
                Destroy(effect, 2f);
            }
        }
    }
}
