using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFinder;

public abstract class Graph : MonoBehaviour
{
    //  public Transform levelFloor;

    public Vector3 GraphSize;

    //  public Vector3 GraphSize
    // {
    //   get { return levelFloor.localScale; }
    // }

    public int sizeX, sizeY;
    public int Size { get { return sizeX * sizeY; } }
    protected virtual void CreateGraph() { }
    public abstract Node GetNode(Vector3 _pos);
    public abstract List<Node> GetAdjacents(Node _t);
}