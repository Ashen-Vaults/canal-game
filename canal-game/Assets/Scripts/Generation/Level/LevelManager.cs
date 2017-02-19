using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region Properties

    [SerializeField]
    string _seed;

    [SerializeField]
    int _rooms;


    [SerializeField]
    [Range(1,100)]
    float _radius;

    [SerializeField]
    Color _circleColor;

    [SerializeField]
    List<Point> _points;

    List<GameObject> _tiles;

    [SerializeField]
    GameObject _prefab;

    [SerializeField]
    [MinMaxRange(2,15)]
    MinMax _tileSize;

    #endregion

    [Serializable]
    struct Point
    {
        public Vector3 position;
        public Color color;

        public Point(Vector3 position, Color color)
        {
            this.position = position;
            this.color = color;
        }
    } 

    void Start()
    {
        EventManager.instance.QueueEvent(new Events.RequestCenterOfGrid(OnCenterFound));
    }

    void CreateNewLevel()
    {
        UnityEngine.Random.InitState(GetSeed());

        //CreateLevel(CreatePointInSpiral, _radius, _rooms, _seed, CreateNormalSize);

        CreateLevel(CreateRandomPointsInCircle, _radius, _rooms, _seed, CreateRandomSize);
    }

    void CreateLevel(Func<float,int, List<Point>> createPoints, float radius, int numberOfRooms, string seed, Func<System.Random, Vector2> onSetLocalScale)
    {
        if(createPoints != null)
        {
            _points = createPoints(radius, numberOfRooms);

            _tiles = CreateGameObjectsAtPoint(_points, GetSeed(), onSetLocalScale);
        }

        StartCoroutine(Wait(GetTimeForRoomSteering(radius, numberOfRooms), () =>
        {
            EventManager.instance.QueueEvent(new Events.RequestCreateGrid(OnCreatedGrid));
        }));
    }

    List<GameObject> CreateGameObjectsAtPoint(List<Point> points, int seed, Func<System.Random, Vector2> onSetLocalScale)
    {
        System.Random rand = new System.Random(seed);
        List<GameObject> gameObjects = new List<GameObject>();
        if (_prefab != null)
        {
            for (int i = 0; i < points.Count; i++)
            {
                GameObject collider = Instantiate(_prefab);
                collider.transform.position = points[i].position;

                if(onSetLocalScale != null)
                {
                    collider.transform.localScale =  onSetLocalScale(rand);
                }      

                gameObjects.Add(collider);
            }
        }
        return gameObjects;
    }

    Vector2 CreateRandomSize(System.Random rand)
    {
        return new Vector2(rand.Next((int)_tileSize.minValue, (int)_tileSize.maxValue), rand.Next((int)_tileSize.minValue, (int)_tileSize.maxValue));
    }

    Vector2 CreateNormalSize(System.Random rand)
    {
        return new Vector2(5, 5);
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

    #region Level Types

    List<Point> CreateRandomPointsInCircle(float radius, int numberOfPoints)
    {
        List<Point> points = new List<Point>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            points.Add(new Point(GetRandomPointInCircle(radius), Color.green));
        }

        return points;
    }

    List<Point> CreatePointInSpiral(float radius, int numberOfRooms)
    {
        List<Point> points = new List<Point>();
        float c = (radius * .01f);
        float hu;
        for  (int i = 0; i < numberOfRooms; i++)
        {
            float a = i * ((137.1f) * ((Mathf.PI / 180)));
            float r = c * Mathf.Sqrt(i);
            float x = r * Mathf.Cos(a);
            float y = r * Mathf.Sin(a);
            hu =( r )/ 3.0f % 1.5f;

            Vector3 position = (new Vector2(x, y) * radius) + GetPosition(this.transform);
            points.Add(new Point(position, Color.HSVToRGB(hu,255,255)));
        }

        return points;
    }

    #endregion


    #region Callbacks
    void OnCreatedGrid()
    {
        DestroyTiles(_tiles);
    }

    void OnCenterFound(Vector2 pos)
    {
        this.transform.position = pos;

        CreateNewLevel();
    }
    #endregion


    #region Utility
    IEnumerator Wait(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        if (callback != null)
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

    float GetRadius()
    {
        return _radius;
    }

    int GetSeed()
    {
        return _seed.GetHashCode();
    }

    float GetTimeForRoomSteering(float radiusSize, int numberOfRooms)
    {
        float time = ((numberOfRooms / radiusSize) * 0.1f) * Time.deltaTime;
        Debug.Log("Get Time For Steering:  " + time);
        return time;
    }
    #endregion


    #region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, _radius);

        if (_points != null)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                Gizmos.color = _points[i].color;
                Gizmos.DrawSphere(_points[i].position, 0.25f);
            }
        }
    }
    #endregion
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