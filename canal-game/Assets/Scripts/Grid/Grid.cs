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
    public Vector2 _gridWorldSize;

    [SerializeField]
    private Grid _envGrid;

    [SerializeField]
    private Grid _movementGrid;

    public Grid MovementGrid
    {
        get { return _movementGrid; }
    }

    [SerializeField]
    private LayerMask _unwalkableMask;

    [SerializeField]
    private float _nodeRadius;
    public float NodeRadius
    {
        get { return _nodeRadius; }
    }

    [SerializeField]
    private bool _isMovementGrid;

    public bool IsMovementGrid
    {
        get { return _isMovementGrid; }
    }

    public bool collisionSet = false;

    [SerializeField]
    private bool _allowDiagonalAdjacents;
    #endregion



    #region Misc
    [Header("Debug Info")]
    [ReadOnly]
    public int gridSizeX;
    [ReadOnly]
    public int gridSizeY;

    [ReadOnly]
    public List<Node> myNodes;

    [ReadOnly]
    public List<Node> _unwalkableNodes = new List<Node>();

    public List<Node> _wallNodes = new List<Node>();

    float _nodeDiameter;

    public Node[,] grid;

    Vector3 _worldBottomLeft;

    List<Node> neighbours = new List<Node>();

    private Node _target;


    #endregion



    #region Debug Options
    [Header("Debug Options")]
    [SerializeField]
    private bool _showGrid;

    [SerializeField]
    private bool _editGrid;

    [SerializeField]
    private bool _ReadOnlyColliders;

    [Flags]
    public enum GridDisplayOptions
    {
        NONE = 0,
        ENV_WALKABLE = 1 << 0,
        ENV_COLLISION = 1 << 1,
        ENV_COLLISION_WALKABLE = ENV_WALKABLE | ENV_COLLISION,
        ENV_WALLS = 1 << 2,
        ENV_COLLISION_WALKABLE_WALLS = ENV_COLLISION_WALKABLE | ENV_WALLS,
        MOVE_WALKABLE = 1 << 3,
        MOVE_COLLISION = 1 << 4,
        MOVE_COLLISION_WALKABLE = MOVE_WALKABLE | MOVE_COLLISION,
        MOVE_WALLS = 1 << 5,
        MOVE_COLLISION_WALKABLE_WALLS = MOVE_COLLISION_WALKABLE | MOVE_WALLS
    }

    public GridDisplayOptions gridDisplayOptions;


    [SerializeField]
    public bool _showLabels;

    [SerializeField]
    private bool _randomObstacles;

    [SerializeField]
    private bool _createOnInit;

    public string areaInLevel;
    #endregion



    #region Debug Visuals
    [Header("Debug Visuals")]

    [SerializeField]
    private Color _walkableColor;

    [SerializeField]
    private Color _unWalkableColor;

    [SerializeField]
    private Color _targetColor;


    #endregion



    #region Debug Styles
    [Header("Debug Styles")]
    [SerializeField]
    private GUIStyle _gridPosStyle;

    [SerializeField]
    private GUIStyle _gStyle;

    [SerializeField]
    private GUIStyle _hStyle;

    [SerializeField]
    private GUIStyle _fStyle;

    [SerializeField]
    private GUIStyle _targetStyle;
    #endregion

    #endregion




    #region Unity Implementation
    void Awake()
    {
        if (_createOnInit)
        {
            this.CreateGrid();
        }

        if (_randomObstacles)
        {
            this.CreateRandomObstacles();
        }
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
    #endregion




    #region Grid Creation    
    /// <summary>
    /// Creates the grid.
    /// </summary>
    [ContextMenu("Create Grid")]
    public void CreateGrid()
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

                bool walkable = !(Physics2D.OverlapCircle(worldPoint, _nodeRadius, this._unwalkableMask));
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

                if (x == Mathf.RoundToInt(gridSizeX / 2) && y == Mathf.RoundToInt(gridSizeY / 2))
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

        /*
        grid = new Node[gridSizeX, gridSizeY];
       _worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.up * _gridWorldSize.y / 2;

       for (int x = 0; x < gridSizeX; x++)
       {
           for (int y = gridSizeY - 1; y >= 0; y--)
           {
               Vector3 worldPoint = _worldBottomLeft + Vector3.right * (x * _nodeDiameter + _nodeRadius) + Vector3.up * (y * _nodeDiameter + _nodeRadius / 2);
               bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkableMask));


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

               if (x == Mathf.RoundToInt(gridSizeX / 2) && y == Mathf.RoundToInt(gridSizeY / 2))
                   isCenter = true;

               grid[x, y] = new Node(walkable, worldPoint, x, y, isWall, isCenter);
               myNodes.Add(grid[x, y]);


               if (isWall)
                   _wallNodes.Add(grid[x, y]);

               if (walkable == false)
                   _unwalkableNodes.Add(grid[x, y]);
           }
       }
       */
        // StartCoroutine(CreateNodes());


        if (!this.IsMovementGrid && this._movementGrid != null)
            this._movementGrid.CreateGrid();

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
                bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkableMask));
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
    public void CreateColliders()
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


    public void SetNodeWalkable(Node node, bool state)
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
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (_nodeDiameter/2));
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
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (_nodeDiameter/2));
                    }


                    if (n.onWall)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (_nodeDiameter/2));
                    }
                }
                else
                {
                    if (!n.Walkable)
                    {
                        Gizmos.color = _unWalkableColor;
                        Gizmos.DrawWireCube(n.myWorldPosition, Vector3.one * (_nodeDiameter/2));
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

    [ContextMenu("Create Random Obstacles")]
    private void CreateRandomObstacles()
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

    [ContextMenu("Show Node Gizmo Size")]
    private void ShowNodeGizmoSize()
    {
        Debug.Log(this + "  " + Vector3.one * _nodeDiameter/2);
    }


    public static GridDisplayOptions SetFlag(GridDisplayOptions a, GridDisplayOptions b)
    {
        return a | b;
    }

    public static GridDisplayOptions UnsetFlag(GridDisplayOptions a, GridDisplayOptions b)
    {
        return a & (~b);
    }

    // Works with "None" as well
    public static bool HasFlag(GridDisplayOptions a, GridDisplayOptions b)
    {
        return (a & b) == b;
    }

    public static GridDisplayOptions ToogleFlag(GridDisplayOptions a, GridDisplayOptions b)
    {
        return a ^ b;
    }

    #endregion


}