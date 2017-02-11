using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace PathFinder
{
    public abstract class PathAlgorithm : MonoBehaviour
    {
        public PathProps myProperties;


        void Awake()
        {
            //      this.myProperties.myGraph = GetComponent<Grid>();
        }

        /// <summary>
        /// Gets the path for an AI to follow
        /// Concrete implementations use different algorithms
        /// such as A*, Concurrent Dijkstra, etc..
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="targetPos"></param>
        public abstract IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, DistanceHeuristic distanceType, bool simplified);

        /// <summary>
        /// Creates the final path after it is found
        /// </summary>
        /// <param name="startTile"></param>
        /// <param name="endTile"></param>
        public abstract Vector3[] CreatePath(Node startTile, Node endTile, bool simplified);

        public abstract void StartFindPath(Vector3 startPos, Vector3 targetPos, DistanceHeuristic distanceType, bool simplified);

        public float DisplayTiming()
        {
            float timer = Time.deltaTime;
            return timer;
        }

    }

    [Serializable]
    public class PathProps
    {
        public Grid myGraph;
        public bool debug;
        public PathManager myManager;
        public int tentativeGScore;
        public Stopwatch sw;
    }
}
