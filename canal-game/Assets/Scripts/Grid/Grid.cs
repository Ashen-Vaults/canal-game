using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFinder;
using System;

public class Grid : MonoBehaviour
{
    #region Properties

    #region Grid Options
    [Header("Grid Options")]
    [SerializeField]
    Vector2 _gridWorldSize;

    [SerializeField]
    Grid _envGrid;

    [SerializeField]
    Grid _movementGrid;

    [SerializeField]
    LayerMask _unwalkableMask;

    [SerializeField]
    float _nodeRadius;

    [SerializeField]
    bool _isMovementGrid;

    bool collisionSet = false;

    [SerializeField]
    bool _allowDiagonalAdjacents;


    #endregion

    #region Misc
    [Header("Debug Info")][ReadOnly][SerializeField]
    public int gridSizeX;

    [ReadOnly][SerializeField]
     public int gridSizeY;

    [ReadOnly][SerializeField]
    List<Node> _myNodes;

    [ReadOnly][SerializeField]
    List<Node> _unwalkableNodes = new List<Node>();

    [ReadOnly][SerializeField]
    List<Node> _wallNodes = new List<Node>();

    Node[,] _grid;

    Vector3 _worldBottomLeft;

    List<Node> _neighbours = new List<Node>();

    Node _target;


    #endregion

    #region Debug Options
    [Header("Debug Options")]
    [SerializeField]
    bool _showGrid;

    [SerializeField]
    bool _editGrid;

    [SerializeField]
    bool _ReadOnlyColliders;

    [SerializeField]
    bool _showLabels;

    [SerializeField]
    bool _randomObstacles;

    [SerializeField]
    bool _createOnInit;

    [SerializeField][Range(0,1)]
    float _timeBetweenNodeCreation;
    #endregion

    #region Debug Visuals
    [Header("Debug Visuals")]

    [SerializeField]
    Color _walkableColor;

    [SerializeField]
    Color _unWalkableColor;

    [SerializeField]
    Color _targetColor;


    #endregion

    #region Debug Styles
    [Header("Debug Styles")]
    [SerializeField]
    GUIStyle _gridPosStyle;

    [SerializeField]
    GUIStyle _gStyle;

    [SerializeField]
    GUIStyle _hStyle;

    [SerializeField]
    GUIStyle _fStyle;

    [SerializeField]
    GUIStyle _targetStyle;
    #endregion

    #endregion


    #region Unity Implementation
    void Awake()
    {
        if (_createOnInit)
        {
            //this.CreateGrid(()=> { Debug.Log("Created Grid"); });
            StartCoroutine(CreateNodes(_timeBetweenNodeCreation));
        }

        if (_randomObstacles)
        {
            CreateObstacles();
        }

        Subscribe();
    }

    private void OnDisable()
    {
     //   UnSubscribe();
    }

    void Update()
    {
        if (_showGrid && _editGrid)
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Node node;

            if (Input.GetMouseButton(0))
            {
                node = GetNode(position);

                if (CanEditNode(node, false))
                {
                    node.Walkable = !node.Walkable;
                }
            }

            else if (Input.GetMouseButton(1))
            {
                node = GetNode(position);

                if (CanEditNode(node, true))
                {
                    node.Walkable = !node.Walkable;
                }
            }

        }
    }

    Vector2 GetCenterOfGrid()
    {
        float x = Mathf.RoundToInt(gridSizeX / 2);
        float y = Mathf.RoundToInt(gridSizeY / 2);
        return new Vector2(x, y);
    }

    bool IsCenter(Vector2 pos)
    {
        Vector2 center = GetCenterOfGrid();
        if (pos.x == center.x && pos.y == center.y)
        {
            return true;
        }
        return false;
    }

    List<Node> GetNodesFromGameObject(GameObject obj)
    {
        List<Node> nodes = new List<Node>();

        Node centerNode = GetNode(obj.transform.position);

        nodes.Add(centerNode);

        return nodes;
    }

    void SetNodesWalkable(List<GameObject> walkableObjects)
    {
        if (walkableObjects  != null)
        {
            for (int i = 0; i < walkableObjects.Count; i++)
            {
                Node n = GetNode(walkableObjects[i].transform.position);
                SetNodeWalkable(n, true);

                List<Node> nodes = GetAdjacents(n);
                for (int j = 0; j < nodes.Count; j++)
                {
                    SetNodeWalkable(nodes[j], true);
                }
            }
        }
    }

    [ContextMenu("Invert Nodes")]
    void InvertNodeStates()
    {
        for (int i = 0; i < _myNodes.Count; i++)
        {
            if (_myNodes[i].Walkable)
            {
                _myNodes[i].Walkable = false;
            }
            else
            {
                _myNodes[i].Walkable = true;
            }
        }
    }

    float GetNodeDiameter(float radius)
    {
        return radius * 2;
    }

    List<Node> GetWallNodes()
    {
        return _wallNodes;
    }

    bool CanEditNode(Node node, bool walkingState)
    {
        if (node != null && !IsNodeWall(node) && node.Walkable == walkingState)
        {
            return true;
        }
        return false;
    }

    #endregion


    #region Grid Creation    
    /// <summary>
    /// Creates the grid.
    /// </summary>
    [ContextMenu("Create Grid")]
    void CreateGrid(Action callback)
    {
        gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / GetNodeDiameter(_nodeRadius));
        gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / GetNodeDiameter(_nodeRadius));

        _myNodes.Clear();

        _grid = new Node[gridSizeX, gridSizeY];

        Vector3 bottomLeftCorner = transform.position;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = Vector3.zero;

                //worldPoint = bottomLeftCorner + Vector3.right * (x * NodeRadius + NodeRadius) + Vector3.up * (y * NodeRadius + gridSizeY);
                worldPoint = bottomLeftCorner + Vector3.right * (x * _nodeRadius) + Vector3.up * (y * _nodeRadius);
                if (x == 0 && y == 0)
                    Debug.Log("FirstNode " + worldPoint);

                bool walkable = (Physics2D.OverlapCircle(worldPoint, _nodeRadius, this._unwalkableMask));

                bool isWall = false;

                if (x == 0)
                    isWall = true;

                if (y == 0)
                    isWall = true;

                if (x == this.gridSizeX - 1)
                    isWall = true;

                if (y == this.gridSizeY - 1)
                    isWall = true;

                bool isCenter = false;

                if (IsCenter(worldPoint))
                    isCenter = true;

                _grid[x, y] = new Node(walkable, worldPoint, x, y, isWall, isCenter);
                if (_grid[x, y].Walkable != true)
                    _grid[x, y].occupierCount += 1;
                _myNodes.Add(_grid[x, y]);


                if (isWall)
                    _wallNodes.Add(_grid[x, y]);

                if (walkable == false)
                    _unwalkableNodes.Add(_grid[x, y]);
            }
        }

        if (!this._isMovementGrid && this._movementGrid != null)
            this._movementGrid.CreateGrid(callback);

        if (callback != null)
        {
            callback();
        }
    }

    /// <summary>
    /// Creates the nodes.
    /// </summary>
    /// <returns></returns>
    IEnumerator CreateNodes(float waitTime)
    {
        gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / GetNodeDiameter(_nodeRadius));
        gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / GetNodeDiameter(_nodeRadius));

        //Debug.Log("gridSizeX/Y: " + gridSizeX + "," + gridSizeY);


        _myNodes.Clear();



        _grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeftCorner = transform.position;

        //Debug.Log("bottomLeftCorner " + bottomLeftCorner);


        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = Vector3.zero;

                //worldPoint = bottomLeftCorner + Vector3.right * (x * NodeRadius + NodeRadius) + Vector3.up * (y * NodeRadius + gridSizeY);
                worldPoint = bottomLeftCorner + Vector3.right * (x * _nodeRadius) + Vector3.up * (y * _nodeRadius);
                if (x == 0 && y == 0)
                    Debug.Log("FirstNode " + worldPoint);

                bool walkable = (Physics2D.OverlapCircle(worldPoint, _nodeRadius, this._unwalkableMask));

                bool isWall = false;

                if (x == 0)
                    isWall = true;
                if (y == 0)
                    isWall = true;

                if (x == this.gridSizeX - 1)
                    isWall = true;
                if (y == this.gridSizeY - 1)
                    isWall = true;

                bool isCenter = false;

                if (IsCenter(worldPoint))
                    isCenter = true;

                _grid[x, y] = new Node(walkable, worldPoint, x, y, isWall, isCenter);
                if (_grid[x, y].Walkable != true)
                    _grid[x, y].occupierCount += 1;
                _myNodes.Add(_grid[x, y]);


                if (isWall)
                    _wallNodes.Add(_grid[x, y]);

                if (walkable == false)
                {
                    _unwalkableNodes.Add(_grid[x, y]);
                }

                yield return new WaitForSeconds(waitTime);
            }
        }
    }

    [ContextMenu("Create Colliders")]
    void CreateColliders()
    {
        if (_envGrid != null)
        {
            //go thru each node in the env 
            for (int i = 0; i < _envGrid._unwalkableNodes.Count; i++)
            {
                Node node = this.GetNode(_envGrid._unwalkableNodes[i].myWorldPosition);
                node.Walkable = false;
                List<Node> nodes = this.GetAdjacents(node);
                for (int j = 0; j < nodes.Count; j++)
                {
                    nodes[j].Walkable = false;
                }
            }

            for (int i = 0; i < _envGrid._wallNodes.Count; i++)
            {
                Node node = this.GetNode(_envGrid._wallNodes[i].myWorldPosition);
                List<Node> nodes = this.GetAdjacents(node);
            }
        }
        collisionSet = true;
    }

    void SetNodeWalkable(Node node, bool state)
    {
        if (node != null)
        {
            node.Walkable = state;
            if (state == false)
                this._unwalkableNodes.Add(node);
            else
            {
                if (this._unwalkableNodes.Contains(node))
                    this._unwalkableNodes.Remove(node);
            }
        }
    }
    #endregion


    #region Grid Querying    
    /// <summary>
    /// Gets the adjacent nodes from
    /// the one provided.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns></returns>
    public List<Node> GetAdjacents(Node node)
    {
        _neighbours.Clear();

        if (_allowDiagonalAdjacents)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        _neighbours.Add(_grid[checkX, checkY]);
                    }
                }
            }
        }
        else
        {
            if (node.gridX - 1 >= 0 && node.gridX - 1 < gridSizeX)
            {
                _neighbours.Add(_grid[node.gridX - 1, node.gridY]);
            }

            if (node.gridX + 1 >= 0 && node.gridX + 1 < gridSizeX)
            {
                _neighbours.Add(_grid[node.gridX + 1, node.gridY]);
            }

            if (node.gridY - 1 >= 0 && node.gridY - 1 < gridSizeY)
            {
                _neighbours.Add(_grid[node.gridX, node.gridY - 1]);
            }

            if (node.gridY + 1 >= 0 && node.gridY + 1 < gridSizeY)
            {
                _neighbours.Add(_grid[node.gridX, node.gridY + 1]);
            }
        }

        return _neighbours;
    }

    /// <summary>
    /// Gets the node at the world position passed thru
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <returns></returns>
    public Node GetNode(Vector3 worldPosition)
    {
        Vector3 roundedPos = new Vector3(Mathf.CeilToInt(worldPosition.x), Mathf.CeilToInt(worldPosition.y), Mathf.CeilToInt(worldPosition.z));
        float minDist = float.PositiveInfinity;
        Node closestNode = null;
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                float sqrDist = (_grid[i, j].myWorldPosition - worldPosition).sqrMagnitude;
                if (sqrDist < minDist)
                {
                    minDist = sqrDist;
                    closestNode = _grid[i, j];
                }
            }
        }
        _target = closestNode;
        return closestNode;
    }

    bool IsNodeWall(Node node)
    {
        if(GetWallNodes() != null)
        {
            if (GetWallNodes().Contains(node))
            {
                return true;
            }
        }
        return false;
    }

    #endregion


    #region IObserver

    void Subscribe()
    {
        EventManager.instance.AddListener<Events.RequestCenterOfGrid>(OnRequestCenterOfGrid);
        EventManager.instance.AddListener<Events.SetWalkableInGrid>(OnSetWalkablePointsInGrid);
        EventManager.instance.AddListener<Events.RequestCreateGrid>(OnRequestCreateGrid);
    }

    void UnSubscribe()
    {
        EventManager.instance.RemoveListener<Events.RequestCenterOfGrid>(OnRequestCenterOfGrid);
        EventManager.instance.RemoveListener<Events.SetWalkableInGrid>(OnSetWalkablePointsInGrid);
        EventManager.instance.RemoveListener<Events.RequestCreateGrid>(OnRequestCreateGrid);
    }

    #endregion


    #region Events

    void OnRequestCenterOfGrid(Events.RequestCenterOfGrid @event)
    {
        Vector2 center = GetCenterOfGrid();

        Node node = null;
        if (_grid[(int)center.x, (int)center.y] != null)
        {
            node = _grid[(int)center.x, (int)center.y];
        }
        if (@event._callback != null)
        {
            @event._callback(node.myWorldPosition);
        }
    }


    void OnSetWalkablePointsInGrid(Events.SetWalkableInGrid @event)
    {
        SetNodesWalkable(@event._points);
    }

    void OnRequestCreateGrid(Events.RequestCreateGrid @event)
    {
        CreateGrid(@event._callback);
    }

    #endregion


    #region Debugging
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        //   Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, _gridWorldSize.y,1));
        if (_grid != null && _showGrid)
        {

            foreach (Node n in this._myNodes)
            {
                if (!_ReadOnlyColliders)
                {
                    if (n.Walkable)
                    {
                        Gizmos.color = _walkableColor;
                        Gizmos.DrawCube(n.myWorldPosition, Vector3.one * (GetNodeDiameter(_nodeRadius) / 2));
                        //(n.myWorldPosition + (Vector3.up), n.gridX + " - " + n.gridY, _gridPosStyle);

                        if (_showLabels)
                        {
                            UnityEditor.Handles.Label(n.myWorldPosition + (Vector3.up * 0.25f), "G: " + n.gScore.ToString(), _gStyle);
                            UnityEditor.Handles.Label(n.myWorldPosition, "H: " + n.hScore.ToString(), _hStyle);
                            UnityEditor.Handles.Label(n.myWorldPosition + (Vector3.down * 0.25f), "F: " + n.fScore.ToString(), _fStyle);
                        }
                    }

                    else
                    {
                        Gizmos.color = _unWalkableColor;
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (GetNodeDiameter(_nodeRadius) / 2));
                    }


                    if (n.onWall)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(n.myWorldPosition, Vector3.one * (GetNodeDiameter(_nodeRadius) / 2));
                    }
                }
                else
                {
                    if (!n.Walkable)
                    {
                        Gizmos.color = _unWalkableColor;
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (GetNodeDiameter(_nodeRadius) / 2));
                    }

                    if (_showLabels && n.centerNode)
                    {
                        UnityEditor.Handles.Label(n.myWorldPosition, n.gridX.ToString() + " -  " + n.gridY.ToString());
                    }
                }

                /*
                foreach (Node n in neighbours)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(n.myWorldPosition, Vector3.one * (_nodeDiameter - .1f));
                }
                */


                if (_target != null)
                {

                    // GetAdjacents(_target);
                }
            }

        }
    }
#endif



    [ContextMenu("Show Node Gizmo Size")]
    void ShowNodeGizmoSize()
    {
        Debug.Log(this + "  " + Vector3.one * GetNodeDiameter(_nodeRadius) / 2);
    }



    #endregion


    #region Obstacle Placement

    void CreateRandomObstacles(Action createObstacle)
    {
        if (createObstacle != null)
        {
            createObstacle();
        }
    }

    void ObstaclesRandom()
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < this._myNodes.Count; i++)
        {
            if (rand.Next() % 3 == 0)
            {
                this._myNodes[i].Walkable = false;
            }
        }
    }

    [ContextMenu("Create Random Obstacles")]
    void CreateObstacles()
    {
        CreateRandomObstacles(ObstaclesRandom);
    }

    #endregion

}