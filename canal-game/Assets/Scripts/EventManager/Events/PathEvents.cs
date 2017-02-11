using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class Events
{


    public struct RequestToMoveToNodeEvent : IGameEvent
    {
        public readonly Vector3 _currentPos;
        public readonly Vector3 _requestedPos;
        public readonly DistanceHeuristic _heuristic;
        public readonly bool _simplifyPath;
        public readonly Action<Vector3[], bool> _callback;

        public RequestToMoveToNodeEvent(Vector3 currentPos, Vector3 requestedPos, DistanceHeuristic heurstic, bool simplify, Action<Vector3[], bool> callback)
        {
            _currentPos = currentPos;
            _requestedPos = requestedPos;
            _heuristic = heurstic;
            _simplifyPath = simplify;
            _callback = callback;
        }
    }


}
                      
