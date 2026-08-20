using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject g;
    public Transform root; // uh dung roi

    private void Start()
    {
        GameObject game = Instantiate(g,root); // setposition o day luon ah
        game.transform.position = new Vector3(0,2,0); 
       
    }
}
