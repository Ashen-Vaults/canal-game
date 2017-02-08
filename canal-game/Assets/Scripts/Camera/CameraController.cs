using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    #region Properties

    Camera _camera;

    [SerializeField]
    float _dampTime;

    [SerializeField]
    Vector3 _velocity;

    #region Gizmos
    [SerializeField][ReadOnly]
    Transform _gizmo_target;
    #endregion


    #endregion

    void Awake()
    {
        _camera = GetCamera(OnError);
    }

    Camera GetCamera(Action<string> onFail)
    {
        if(this.GetComponent<Camera>() != null)
        {
            return this.GetComponent<Camera>();
        }
        if(onFail != null)
        {
            onFail("No Camera found on this game object");
        }
        return null;
    }


    void MoveToTarget(Transform target, Action<string> onFailure)
    {
        if (target)
        {
            Vector3 point = _camera.WorldToViewportPoint(target.position);
            Vector3 delta = target.position - _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.3f, point.z));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, 0.2F);
        }
        else
        {
            if(onFailure != null)
            {
                onFailure("MoveToTarget:  No Target");
            }
        }
    }


    #region Utility

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
        if (GameObject.Find("Target") != null)
        {
            Transform target = GameObject.Find("Target").transform;
            if (target != null)
            {
                MoveToTarget(target, OnError);
            }
        }
    }

    #endregion

}
