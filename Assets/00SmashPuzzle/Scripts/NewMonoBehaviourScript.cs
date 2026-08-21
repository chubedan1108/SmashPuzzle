using DG.Tweening;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject g;
    public Transform root; // uh dung roi
    private GameObject game;
    int i = 0;
    private void Start()
    {
        game = Instantiate(g,root);
        game.transform.position = new Vector3(0,10,0); 
        i++;
        Debug.Log("Start" + i);
        Debug.Log(game.GetComponentInChildren<Rigidbody>().position);
        Debug.Log(game.transform.position); 
        //DOVirtual.DelayedCall(1f, AAA);
    }

    private void FixedUpdate()
    {
        i++;
        Debug.Log("Fix update" + i);
        Debug.Log(game.GetComponentInChildren<Rigidbody>().position); 
        Debug.Log(game.transform.position);
    }

    private void Update()
    {
        i++;
        Debug.Log("Update" + i);
        Debug.Log(game.GetComponentInChildren<Rigidbody>().position); 
        Debug.Log(game.transform.position); 
    }
    private void AAA()
    {
        Physics.SyncTransforms();
    }

}
