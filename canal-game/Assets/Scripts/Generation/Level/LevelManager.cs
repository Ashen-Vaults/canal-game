using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [SerializeField]
    int _rooms;


    [SerializeField]
    [MinMaxRange(1,100)]
    MinMax _radius;

    [SerializeField]
    Color _circleColor;

    [SerializeField]
    List<Vector2> _points;

    List<GameObject> _tiles;

    [SerializeField]
    GameObject _prefab;

    [SerializeField]
    [MinMaxRange(2,15)]
    MinMax _tileSize;


    void Start()
    {
        CreateRandomPoints();
    }

    void OnCreatedGrid()
    {
        DestroyTiles(_tiles);
    }


    void DestroyTiles(List<GameObject> tiles)
    {
        if (tiles != null)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Destroy(tiles[i].gameObject);
            }
        }
    }
    

    IEnumerator Wait(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        if(callback != null)
        {
            callback();
        }
    }


    Vector2 GetRandomPointInCircle(float radius)
    { 
        return (UnityEngine.Random.insideUnitCircle * radius) + GetPosition(this.transform);
    }

    Vector2 GetPosition(Transform t)
    {
        return new Vector2(t.position.x, t.position.y);
    }

    List<Vector2> CreateRandomPointsInCircle(int numberOfPoints, float radius)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            points.Add(GetRandomPointInCircle(radius));
        }

        return points;
    }


    void OnCenter(Vector2 pos)
    {
        this.transform.position = pos;
    }


    [ContextMenu("Create Point")]
    void TestGetRandomPoint()
    {
        _points.Add(GetRandomPointInCircle(_radius.maxValue));
    }

    [ContextMenu("Create Points")]
    void CreateRandomPoints()
    {
        _points.AddRange(CreateRandomPointsInCircle(_rooms,_radius.maxValue));
        _tiles = CreateGameObjectsAtPoint();

        StartCoroutine(Wait(2.5f, ()=> { EventManager.instance.QueueEvent(new Events.RequestCreateGrid(OnCreatedGrid)); }));
      // EventManager.instance.QueueEvent(new Events.SetWalkableInGrid( ));
    }

    List<GameObject> CreateGameObjectsAtPoint()
    {
        System.Random rand = new System.Random();
        List<GameObject> gameObjects = new List<GameObject>();
        if (_prefab != null)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                GameObject collider = Instantiate(_prefab);
                collider.transform.position = _points[i];
                collider.transform.localScale = new Vector2(rand.Next((int)_tileSize.minValue, (int)_tileSize.maxValue), rand.Next((int)_tileSize.minValue, (int)_tileSize.maxValue));
                gameObjects.Add(collider);
            }
        }
        return gameObjects;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = _circleColor;
        Gizmos.DrawWireSphere(this.transform.position, _radius.maxValue);

        if(_points != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _points.Count; i++)
            {
                Gizmos.DrawSphere(_points[i], 0.1f);
            }
        }
    }
}

public struct Room
{
    public readonly string _id;
    public readonly Vector2 _position;
    public readonly Vector2 _size;

    public Room(string id, Vector2 position, Vector2 size)
    {
        _id = id;
        _position = position;
        _size = size;
    }

}