using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AStarNode
{
    public Vector3 position;
    public float g = 0.0f;
    public float h = 0.0f;
    public float f = 0.0f;
    public int predecessorID = -1;
    public List<int> adjacentPointIDs = new List<int>();
}
