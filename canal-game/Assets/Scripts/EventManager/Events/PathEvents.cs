using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class Events
{


    public struct RequestToMoveToNode : IGameEvent
    {
        public readonly Vector3 _currentPos;
        public readonly Vector3 _requestedPos;
        public readonly DistanceHeuristic _heuristic;
        public readonly bool _simplifyPath;
        public readonly Action<Vector3[], bool> _callback;

        public RequestToMoveToNode(Vector3 currentPos, Vector3 requestedPos, DistanceHeuristic heurstic, bool simplify, Action<Vector3[], bool> callback)
        {
            _currentPos = currentPos;
            _requestedPos = requestedPos;
            _heuristic = heurstic;
            _simplifyPath = simplify;
            _callback = callback;
        }
    }

    public struct SetWalkableInGrid : IGameEvent
    {
        public readonly List<GameObject> _points;
        public SetWalkableInGrid(List<GameObject> points)
        {
            _points = points;
        }
    }


    public struct RequestCenterOfGrid : IGameEvent
    {
        public readonly Action<Vector2> _callback;
        public RequestCenterOfGrid(Action<Vector2> callback)
        {
            _callback = callback;
        }
    }

    public struct RequestCreateGrid : IGameEvent
    {
        public readonly Action _callback;
        public RequestCreateGrid(Action callback)
        {
            _callback = callback;
        }
    }

    public struct SendCenterOfGrid : IGameEvent
    {
        public readonly Vector2 _center;
        public SendCenterOfGrid(Vector2 center)
        {
            _center = center;
        }
    }
}


