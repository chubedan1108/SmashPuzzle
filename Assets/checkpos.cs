using UnityEngine;

public class checkpos : MonoBehaviour
{
    private float time = 2f;
    private float time2 = 0f;
    public Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // transform.position = new Vector3(0, 5, 0);
       // rb.position = new Vector3(0, 5, 0);
    }
    //void Update()
    //{
    //    time2 += Time.deltaTime;
    //    if (time2 >= time)
    //    {
    //        time2 = 0f;
    //    }
    //    else
    //    {
    //        transform.position += 5f * Vector3.forward * Time.deltaTime;
    //    }
    //}
}
