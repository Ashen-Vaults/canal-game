using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PathFinder
{
    public class PathManager : MonoBehaviour
    {
        Queue<PathRequest> PathQueue = new Queue<PathRequest>();
        PathRequest currentPath;


        public PathAlgorithm myAlgorithm;
        bool processing;


        private void Awake()
        {
            Subscribe();
        }

        void Subscribe()
        {
            EventManager.instance.AddListener<Events.RequestToMoveToNode>(OnRequestToMoveToNode);
        }

        void OnRequestToMoveToNode(Events.RequestToMoveToNode @event)
        {
            if(this.myAlgorithm.myProperties.myGraph.grid != null)
                RequestPath(@event._currentPos, @event._requestedPos, @event._heuristic, @event._simplifyPath, @event._callback);
        }

        public void RequestPath(Vector3 _start, Vector3 _end, DistanceHeuristic _distanceType, bool _simplifiedPath, Action<Vector3[], bool> callback)
        {
            PathRequest newRequest = new PathRequest(_start, _end, _distanceType, _simplifiedPath, callback);
            PathQueue.Enqueue(newRequest);
            ProcessNext();
        }
        

        void ProcessNext()
        {
            if (!processing && PathQueue.Count > 0)
            {
                currentPath = PathQueue.Dequeue();
                processing = true;
                myAlgorithm.StartFindPath(currentPath.start, currentPath.end, currentPath.distanceType, currentPath.simplified);
            }
        }

        public void DoneProcessing(Vector3[] path, bool passed)
        {
            currentPath.callback(path, passed);
            processing = false;
            ProcessNext();
        }

        struct PathRequest
        {
            public Vector3 start, end;
            public Action<Vector3[], bool> callback;
            public DistanceHeuristic distanceType;
            public bool simplified;

            public PathRequest(Vector3 _start, Vector3 _end, DistanceHeuristic _distanceType, bool _simplified, Action<Vector3[], bool> _callback)
            {
                start = _start;
                end = _end;
                distanceType = _distanceType;
                simplified = _simplified;
                callback = _callback;
            }
        }  
    }

}