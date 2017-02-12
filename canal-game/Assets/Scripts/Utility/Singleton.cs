using UnityEngine;
using System.Collections;

/// <summary>
/// This is a utility script.
/// Anyone who needs to be a singlton should derive from this object
/// When they do, they will automatically be a singleton.
/// Used for convenience.
/// </summary>
public class Singleton<T> where T : Singleton<T>, new()
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T(); //makes the concrete implemenation a singleton
            }
            return _instance;
        }
    }
}


public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T)); //makes the concrete implemenation a singleton
            }
            return _instance;
        }
    }
}
