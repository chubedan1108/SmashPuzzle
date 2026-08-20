using UnityEngine;

public abstract class Entity : MonoBehaviour
{
   //Doc cac tham so tuy bien cua entity data
    public virtual void ReadCustomData(string data)
    {
       
    }

    //Ghi lai lam so tuy bien
    public virtual void WriteCustomData(string data)
    {
        
    }
}