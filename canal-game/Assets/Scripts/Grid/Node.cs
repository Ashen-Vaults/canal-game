using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PathFinder
{
    [Serializable]
    public class Node : IQueueable
    {

        public bool occupied;

        public int occupierCount = 0;

        public bool onWall;

        [SerializeField]
        private bool _walkable;
        public bool Walkable
        {
            get { return _walkable; }
            set
            {
                _walkable = value;
                /*
                if(value == false)
                {
                    this.gScore = 10000000;
                    this.hScore = 10000000;
                }
                */
            }
        }
        //public Vector2 myPosition2D;
        public Vector3 myWorldPosition;

        public Transform myParentTransform;

        //public Vector3 MyDisplayPosition;
  

        public Vector3 mySize;

        #region GridProperties
        public int gridX, gridY; //Positions in the grid
        public int gScore = 0; // Cost it took to get to this tile from the start
        public int hScore = 0; //is a heuristic estimate of the cost to get from n to any goal node.                
        public int fScore  // Estimated total cost from start to goal through; determines which tile gets put into the closed list.
        {
            get { return gScore + hScore; }
        }

        public Node myParent;

        public bool centerNode;
        #endregion GridProperties;


        #region IQueueable Properties

        int priority;
        public int Priority
        {
            get { return fScore; }
        }

        int queueIndex;
        public int QueueIndex
        {
            get { return queueIndex; }
            set { queueIndex = value; }
        }

        int insertionIndex;
        public int InsertionIndex
        {
            get { return insertionIndex; }
            set { insertionIndex = value; }
        }


        public int CompareTo(Node _t)
        {
            int value = fScore.CompareTo(_t.fScore);
            if (value == 0) value = hScore.CompareTo(_t.hScore);
            return -value;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Node)obj);
        }
        #endregion IQueueable Properties


        public Node(bool _walkable, Vector3 _pos)
        {
            this._walkable = _walkable;
            this.myWorldPosition = _pos;
        }

        /// <summary>
        /// The main Constructor which is called
        /// </summary>
        /// <param name="_walkable">Whether or not it will be a considered tile when a path is created</param>
        /// <param name="_pos">the world position</param>
        /// <param name="_x">the x position in the grid</param>
        /// <param name="_y">the y position in the grid</param>
        public Node(bool _walkable, Vector3 _pos, int _x, int _y, bool isWall, bool isCenter)
        {
            this._walkable = _walkable;
            this.myWorldPosition = _pos;
            gridX = _x;
            gridY = _y;
            this.onWall = isWall;
            this.centerNode = isCenter;

        }

        /// <summary>
        /// The main Constructor which is called
        /// </summary>
        /// <param name="_walkable">Whether or not it will be a considered tile when a path is created</param>
        /// <param name="_pos">the world position</param>
        /// <param name="_x">the x position in the grid</param>
        /// <param name="_y">the y position in the grid</param>
        public Node(bool _walkable, Vector3 _pos, int _x, int _y, Vector3 _size)
        {
            this._walkable = _walkable;
            this.myWorldPosition = _pos;
            gridX = _x;
            gridY = _y;
            this.mySize = _size;

        }

        /// <summary>
        /// The main Constructor which is called
        /// </summary>
        /// <param name="_walkable">Whether or not it will be a considered tile when a path is created</param>
        /// <param name="_pos">the world position</param>
        /// <param name="_x">the x position in the grid</param>
        /// <param name="_y">the y position in the grid</param>
        public Node(bool _walkable, Transform _parent, Vector3 _localPos, int _x, int _y, Vector3 _size)
        {
            this._walkable = _walkable;
            this.myParentTransform = _parent;
            this.myWorldPosition = _localPos;
            gridX = _x;
            gridY = _y;
            this.mySize = _size;

        }


        public override string ToString()
        {
            return this.myWorldPosition + " " + _walkable + " " + gridX + " " + gridY;
        }
    }
}
