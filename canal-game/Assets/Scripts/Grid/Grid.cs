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

    Grid MovementGrid
    {
        get { return _movementGrid; }
    }

    [SerializeField]
    LayerMask _unwalkableMask;

    [SerializeField]
    float _nodeRadius;
    float NodeRadius
    {
        get { return _nodeRadius; }
    }

    [SerializeField]
    bool _isMovementGrid;

    bool IsMovementGrid
    {
        get { return _isMovementGrid; }
    }

    bool collisionSet = false;

    [SerializeField]
    bool _allowDiagonalAdjacents;

    #endregion



    #region Misc
    [Header("Debug Info")]
    [ReadOnly]
    public int gridSizeX;
    [ReadOnly]
    public int gridSizeY;

    [ReadOnly][SerializeField]
    List<Node> myNodes;

    [ReadOnly]
    List<Node> _unwalkableNodes = new List<Node>();

    List<Node> _wallNodes = new List<Node>();

    float _nodeDiameter;

    public Node[,] grid;

    Vector3 _worldBottomLeft;

    List<Node> neighbours = new List<Node>();

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

    string areaInLevel;
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
            this.CreateGrid(()=> { Debug.Log("Created Grid"); });
        }

        if (_randomObstacles)
        {
            CreateObstacles();
        }

        Subscribe();
    }

    void OnDestroy()
    {
        UnSubscribe();
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
                if (node != null && node.Walkable)
                    node.Walkable = !node.Walkable;
            }
            else if (Input.GetMouseButton(1))
            {
                node = GetNode(position);
                if (node != null)
                    node.Walkable = true;
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
        for (int i = 0; i < myNodes.Count; i++)
        {
            if (myNodes[i].Walkable)
            {
                myNodes[i].Walkable = false;
            }
            else
            {
                myNodes[i].Walkable = true;
            }
        }
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
        if (grid[(int)center.x, (int)center.y] != null)
        {
            node = grid[(int)center.x, (int)center.y];
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

       
    #region Grid Creation    
    /// <summary>
    /// Creates the grid.
    /// </summary>
    [ContextMenu("Create Grid")]
    void CreateGrid(Action callback)
    {
        _nodeDiameter = _nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter);
        gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter);

        //Debug.Log("gridSizeX/Y: " + gridSizeX + "," + gridSizeY);


        myNodes.Clear();



        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeftCorner = transform.position;

        //Debug.Log("bottomLeftCorner " + bottomLeftCorner);


        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = Vector3.zero;

                //worldPoint = bottomLeftCorner + Vector3.right * (x * NodeRadius + NodeRadius) + Vector3.up * (y * NodeRadius + gridSizeY);
                worldPoint = bottomLeftCorner + Vector3.right * (x * NodeRadius) + Vector3.up * (y * NodeRadius);
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

                grid[x, y] = new Node(walkable, worldPoint, x, y, isWall, isCenter);
                if (grid[x, y].Walkable != true)
                    grid[x, y].occupierCount += 1;
                myNodes.Add(grid[x, y]);


                if (isWall)
                    _wallNodes.Add(grid[x, y]);

                if (walkable == false)
                    _unwalkableNodes.Add(grid[x, y]);
            }
        }

        if (!this.IsMovementGrid && this._movementGrid != null)
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
    IEnumerator CreateNodes()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = gridSizeY - 1; y >= 0; y--)
            {
                Vector3 worldPoint = _worldBottomLeft + Vector3.right * (x * _nodeDiameter + _nodeRadius) + Vector3.up * (y * _nodeDiameter + _nodeRadius / 2);
                bool walkable = (Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkableMask));
                bool isWall = false;

                if (x == 0)
                    isWall = true;
                if (y == 0)
                    isWall = true;
                if (x == this.gridSizeX)
                    isWall = true;
                if (y == this.gridSizeY)
                    isWall = true;

                bool isCenter = false;

                if (x == Mathf.RoundToInt(gridSizeX / 2) && y == Mathf.RoundToInt(gridSizeY / 2))
                    isCenter = true;

                grid[x, y] = new Node(walkable, worldPoint, x, y, isWall, isCenter);
                if (grid[x, y].Walkable != true)
                    grid[x, y].occupierCount += 1;
                myNodes.Add(grid[x, y]);
                yield return new WaitForSeconds(0);
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
                for (int j = 0; j < nodes.Count; j++)
                {
                    nodes[j].onWall = true;
                }
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
        neighbours.Clear();

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
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }
        }
        else
        {
            if (node.gridX - 1 >= 0 && node.gridX - 1 < gridSizeX)
            {
                neighbours.Add(grid[node.gridX - 1, node.gridY]);
            }

            if (node.gridX + 1 >= 0 && node.gridX + 1 < gridSizeX)
            {
                neighbours.Add(grid[node.gridX + 1, node.gridY]);
            }

            if (node.gridY - 1 >= 0 && node.gridY - 1 < gridSizeY)
            {
                neighbours.Add(grid[node.gridX, node.gridY - 1]);
            }

            if (node.gridY + 1 >= 0 && node.gridY + 1 < gridSizeY)
            {
                neighbours.Add(grid[node.gridX, node.gridY + 1]);
            }
        }

        return neighbours;
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
                float sqrDist = (grid[i, j].myWorldPosition - worldPosition).sqrMagnitude;
                if (sqrDist < minDist)
                {
                    minDist = sqrDist;
                    closestNode = grid[i, j];
                }
            }
        }
        _target = closestNode;
        return closestNode;
    }


    #endregion




    #region Debugging
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        //   Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, _gridWorldSize.y,1));
        if (grid != null && _showGrid)
        {

            foreach (Node n in this.myNodes)
            {
                if (!_ReadOnlyColliders)
                {
                    if (n.Walkable)
                    {
                        Gizmos.color = _walkableColor;
                        Gizmos.DrawCube(n.myWorldPosition, Vector3.one * (_nodeDiameter / 2));
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
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (_nodeDiameter / 2));
                    }


                    if (n.onWall)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(n.myWorldPosition, Vector3.one * (_nodeDiameter / 2));
                    }
                }
                else
                {
                    if (!n.Walkable)
                    {
                        Gizmos.color = _unWalkableColor;
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (_nodeDiameter / 2));
                    }

                    if (_showLabels && n.centerNode)
                    {
                        UnityEditor.Handles.Label(n.myWorldPosition, this.areaInLevel);
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
        Debug.Log(this + "  " + Vector3.one * _nodeDiameter / 2);
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
        for (int i = 0; i < this.myNodes.Count; i++)
        {
            if (rand.Next() % 3 == 0)
            {
                this.myNodes[i].Walkable = false;
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