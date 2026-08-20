using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int Version;
    public int MoveCount;
    public int Difficulty;
    public int BackgroundIndex;
    public List<EntityData> Entities;
}

[Serializable]
public class EntityData
{
    public string Id;
    public Vector3 Position;
    public Quaternion Rotation;
    public TableCustomData Custom;
}

[Serializable]
public class TableCustomData
{
    public float w;
    public float d;
}