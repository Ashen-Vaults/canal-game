using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{


    #region Properties

    #region Movement
    [Header("Movement")]
    [SerializeField][MinMaxRange(0,20)]
    MinMax _distance;

    [SerializeField]
    float _maxSpeed;
    #endregion

    #region References
    Coroutine _move;
    Camera _camera;
    #endregion

    #region Gizmos
    [Header("Gizmos")]
    [SerializeField][ReadOnly]
    Transform _gizmo_target;

    [SerializeField]
    float _minSize;

    [SerializeField]
    float _ScreenEdgeBuffer;

    [SerializeField]
    float _dampTime;
    #endregion

    #endregion

    void Awake()
    {
        _camera = GetCamera(OnError);
        _gizmo_target = FindTarget();
    }

    void FixedUpdate()
    {
        if (_gizmo_target != null)
        {
            if (Vector3.Distance(transform.position, _gizmo_target.transform.position) > _distance.maxValue)
            {
                Zoom(FindTargets(OnError));
                MoveToTarget(_gizmo_target, OnError);
            }
        }
    }

    Camera GetCamera(Action<string> onFail, int numberOfTries=0)
    {
        int maxTries = 5;
        if(this.GetComponent<Camera>() != null)
        {
            return this.GetComponent<Camera>();
        }
        if(onFail != null)
        {
            onFail("No Camera found on this game object - Adding now and retrying");
            this.gameObject.AddComponent<Camera>();

            if(numberOfTries < maxTries)
            {
                GetCamera(onFail, numberOfTries++);
            }
            else
            {
                onFail("Coud not add camera, number of tries exceded: " + maxTries);
            }
           
        }
        return null;
    }

    //TEMP: obviously temporary, find target
    //will most likely be triggered by an event that sends the target
    //information and it'll just use that instead of .Find
    //.Find is in just for testing purposes (calm down)
    Transform FindTarget()
    {
        if (GameObject.Find("Target") != null)
        {
            return  GameObject.Find("Target").transform;
        }
        return null;
    }

    Transform[] FindTargets(Action<string> onError)
    {
        Transform[] targets = null;
        if (GameObject.FindObjectsOfType<CameraTarget>() != null)
        {
            CameraTarget[] camTargs = GameObject.FindObjectsOfType<CameraTarget>();
            targets = new Transform[camTargs.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = camTargs[i].transform;
            }
        }
        else
        {
            if(onError != null)
            {
                onError("No CameraTargets in the Scene");
            }
        }
        return targets;
    }

    void MoveToTarget(Transform target, Action<string> onFail)
    {
        if(_move != null)
        {
            StopCoroutine(_move);
        }
        _move = StartCoroutine(Move(target, _distance.minValue, _maxSpeed, onFail));
    }

    IEnumerator Move(Transform target, float minDistance, float speed, Action<string> onFail)
    {
        if (target)
        {
            Vector3 point = CalculatePoint(target);
            Vector3 destination = CalculateDestination(target);
            while (Vector3.Distance(transform.position, destination) > minDistance)
            {
                transform.position = Vector3.Lerp(transform.position, destination, speed);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            if (onFail != null)
            {
                onFail("MoveToTarget:  No Target");
            }
        }
        yield return new WaitForEndOfFrame();
    }


    void Zoom(Transform[] targets)
    {
        // Find the required size based on the desired position and smoothly transition to that size.
        float requiredSize = FindRequiredSize(targets);
        GetCamera(OnError).orthographicSize = Mathf.Lerp(GetCamera(OnError).orthographicSize, requiredSize, _dampTime);
    }


    float FindRequiredSize(Transform[] targets)
    {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(CalculateDestination(targets[0]));

        float size = 0f;

        for (int i = 0; i < targets.Length; i++)
        {
            Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].position);

            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / GetCamera(OnError).aspect);
        }

        size += _ScreenEdgeBuffer;

        size = Mathf.Max(size, _minSize);

        return size;
    }


    #region Utility

    Vector3 CalculatePoint(Transform target)
    {
        return GetCamera(OnError).WorldToViewportPoint(target.position);
    }

    Vector3 CalculateDelta(Transform target)
    {
        return target.position - GetCamera(OnError).ViewportToWorldPoint(new Vector3(0.5f, 0.5f, CalculatePoint(target).z));
    }

    Vector3 CalculateDestination(Transform target)
    {
        return transform.position + CalculateDelta(target);
    }

    //TODO: implement
    List<Vector2> GetLines()
    {
        List<Vector2> lines = new List<Vector2>();

        return lines;
    }

    void OnError(string errorMessage)
    {
        Debug.Log("ERROR: " + this.ToString() + " -- <color=#FF0000>" + errorMessage + "</color>");
    }

    #endregion


    #region Gizmos

    void OnDrawGizmos()
    {
        DrawLines(Color.white, OnError); 
    }


    void DrawLines(Color color, Action<string> onFail)
    {
        List<Vector2> lines = GetLines();
        if (lines != null)
        {
            if (lines.Count < 1)
            {
                if (onFail != null)
                {
                    onFail("DrawLines - Failure: lines is 0");
                }
            }
            else
            {
                for (int i = lines.Count; i > -1; i--)
                {
                    Gizmos.color = color;
                    Gizmos.DrawLine(lines[i - 1], lines[i]);
                }
            }
        }
        else
        {
            if(onFail != null)
            {
                onFail("DrawLines - Failure: lines is null");
            }
        }
    }
    #endregion


    #region Test

    [ContextMenu("Test - Move To Target")]
    void TestMoveToTarget()
    {
        Transform target = FindTarget();
        if (target != null)
        {
            MoveToTarget(target, OnError);
        }
    }

    #endregion

}
