using UnityEngine;

public class TableEntity : Entity
{   [SerializeField] private MeshFilter meshFilter;
    private TableCustomData customeData;
    public override void ReadCustomData(string data)
    {
        //customeData = JsonUtility.FromJson<TableCustomData>(data);
        //Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
        //meshFilter.transform.localScale = new Vector3(customeData.w / meshSize.x, transform.localScale.y, customeData.d / meshSize.z);
    }

    public override void WriteCustomData(string data) 
    {

    }
}
