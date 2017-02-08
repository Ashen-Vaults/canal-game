using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventManager : Singleton<EventManager>
{

    #region Properties
    /// <summary>
    /// Limit the number of events that will processed at once.
    /// </summary>
    public bool limitProcessing = false;

    /// <summary>
    /// True = it will print out when an event happens.
    /// </summary>
    [SerializeField]
    private bool _debug;

    /// <summary>
    /// The queue process time
    /// The amount of time we will wait between each process.
    /// </summary>
    [Range(0, 0.5f)]
    [SerializeField]
    private float queueProcessTime = 0.0f;

    /// <summary>
    /// The event queue
    /// A queue of events, first in first out to be broadcasted to listeners.
    /// </summary>
    private Queue<IGameEvent> _eventQueue = new Queue<IGameEvent>();

    /// <summary>
    /// Delegate for IGameEvents
    /// to be pointed to
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="event">The event.</param>
    public delegate void EventDelegate<T>(T @event) where T : IGameEvent;
    private delegate void EventDelegate(IGameEvent @event);

    /// <summary>
    /// A dictionary where the key is the IGameEvent and the value
    /// is the delegate that we want to associate with this IGameEvent class object
    /// </summary>
    private Dictionary<Type, EventDelegate> _delegates = new Dictionary<Type, EventDelegate>();

    /// <summary>
    /// The temporary delegate
    /// that we add to the delegate dictionary
    /// </summary>
    private EventDelegate _tempDelegate;

    private event EventDelegate TempDelegate
    {
        add
        {
            _tempDelegate -= value;
            _tempDelegate += value;
        }
        remove
        {
            _tempDelegate -= value;
        }
    }

    /// <summary>
    /// Allows you to find specific delegates for a IGameEvent 
    /// These are for delegates which stay put
    /// </summary>
    private Dictionary<Delegate, EventDelegate> _delegateLookup = new Dictionary<Delegate, EventDelegate>();

    /// <summary>
    /// Allows you to find specific delegates for a IGameEvent
    /// These are for delegates which are used once
    /// </summary>
    private Dictionary<Delegate, Delegate> _onceLookups = new Dictionary<Delegate, Delegate>();

    /// <summary>
    /// Stores the components and which events they are listening too,
    /// that way unsubscribing it automated by a method call,
    /// instead of manually having to unsubscribing.
    /// </summary>
    private Dictionary<System.Object, List<Delegate>> _componentLookups = new Dictionary<System.Object, List<Delegate>>();
    #endregion

    #region Implementation


    #region Adding
    /// <summary>
    /// Adds the delegate to the dictionary of delegates
    /// THis is used so we can invoke the non-generic delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="delegate">The delegate.</param>
    /// <returns></returns>
    private EventDelegate AddDelegate<T>(EventDelegate<T> @delegate) where T : IGameEvent
    {
        // Early-out if we've already registered this delegate
        if (_delegateLookup.ContainsKey(@delegate))
            return null;

        // Create a new non-generic delegate which calls our generic one.
        // This is the delegate we actually invoke.


        EventDelegate internalDelegate = (@event) => @delegate((T)@event);
        _delegateLookup[@delegate] = internalDelegate;

        if (_delegates.TryGetValue(typeof(T), out _tempDelegate))
        {
            _delegates[typeof(T)] = _tempDelegate += internalDelegate;
        }
        else
        {
            _delegates[typeof(T)] = internalDelegate;
        }
        return internalDelegate;
    }

    /// <summary>
    /// Adds the listener.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="delegate">The delegate.</param>
    public void AddListener<T>(EventDelegate<T> @delegate) where T : IGameEvent
    {
        if (_debug)
            Debug.Log("Adding Listener: " + @delegate + @delegate.Target);

        if (_componentLookups.ContainsKey(@delegate.Target))
            _componentLookups[@delegate.Target].Add(@delegate);

        else
            _componentLookups.Add(@delegate.Target, new List<Delegate> { @delegate });

        //in case a duplicate was added, remove it
        RemoveListener(@delegate);
        AddDelegate<T>(@delegate);
    }


    /// <summary>
    /// Adds the listener once and immediately
    /// removes the listener once its destroyed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="delegate">The delegate.</param>
    public void AddListenerOnce<T>(EventDelegate<T> @delegate) where T : IGameEvent
    {
        EventDelegate result = AddDelegate<T>(@delegate);

        if (result != null)
        {
            // remember this is only called once
            _onceLookups[result] = @delegate;
        }
    }

    #endregion

    #region Remove
    /// <summary>
    /// Removes the listener.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="delegate">The delegate.</param>
    public void RemoveListener<T>(EventDelegate<T> @delegate) where T : IGameEvent
    {
        EventDelegate internalDelegate;

        //We need to check to see if this delegate has an internal delegate
        if (_delegateLookup.TryGetValue(@delegate, out internalDelegate))
        {
            EventDelegate tempDel;

            //get all of its values
            if (_delegates.TryGetValue(typeof(T), out tempDel))
            {
                tempDel -= internalDelegate;

                //get rid of the listeners
                if (tempDel == null)
                {
                    _delegates.Remove(typeof(T));
                }

                else
                {
                    _delegates[typeof(T)] = tempDel;
                }
            }
            //remove the delegate
            _delegateLookup.Remove(@delegate);
        }
    }

    /// <summary>
    /// Clears all of the dictionaries
    /// </summary>
    public void RemoveAll()
    {
        _delegates.Clear();
        _delegateLookup.Clear();
        _onceLookups.Clear();
    }
    #endregion

    #region Triggering
    /// <summary>
    /// Trigger events are sent to
    /// its listeners immediately.
    /// NOTE: This should be used
    /// sparingly, as it can result
    /// in FPS drops if a lot of events are triggered
    /// at once.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="_timeToWaitBeforeCallback">an optional parameter, should the event wait some time before we invoke it?s</param>
    public void TriggerEvent(IGameEvent @event, float _timeToWaitBeforeCallback = 0)
    {
        EventDelegate @delegate;

        //We need to check to see if the event has any listeners before proceeding.
        if (_delegates.TryGetValue(@event.GetType(), out @delegate))
        {
            @delegate.Invoke(@event);

            //should we print out the event that happened to the log?
            if (this._debug)
                Debug.Log("Event :\t" + @event + " " + Time.deltaTime);

            if (_delegates.ContainsKey(@event.GetType()))
            {
                // remove listeners which should only be called once
                foreach (EventDelegate key in _delegates[@event.GetType()].GetInvocationList())
                {
                    if (_onceLookups.ContainsKey(key))
                    {
                        _delegates[@event.GetType()] -= key;

                        if (_delegates[@event.GetType()] == null)
                        {
                            _delegates.Remove(@event.GetType());
                        }

                        _delegateLookup.Remove(_onceLookups[key]);
                        _onceLookups.Remove(key);
                    }
                }
            }
        }

        //we couldnt find listeners for this event
        else
        {
            Debug.LogWarning("Event: " + @event.GetType() + " has no listeners");
        }
    }


    /// <summary>
    /// Triggers the event after seconds.
    /// </summary>
    /// <param name="_delegateToInvoke">The delegate to invoke.</param>
    /// <param name="event">The event.</param>
    /// <param name="_secondsToWait">The seconds to wait.</param>
    /// <returns></returns>
    IEnumerator TriggerEventAfterSeconds(EventDelegate _delegateToInvoke, IGameEvent @event, float _secondsToWait)
    {
        yield return new WaitForSeconds(_secondsToWait);
        _delegateToInvoke.Invoke(@event);

    }

    /// <summary>
    /// Inserts the event into the current queue.
    /// </summary>
    /// <param name="@event"></param>
    /// <returns></returns>
    public bool QueueEvent(IGameEvent @event)
    {
        if (!_delegates.ContainsKey(@event.GetType()))
        {
            //Debug.LogWarning("EventManager: QueueEvent failed due to no listeners for event: " + @event.GetType());
            return false;
        }

        _eventQueue.Enqueue(@event);
        return true;
    }


    /// <summary>
    /// TODO
    /// Aborts the event.
    /// Finds the next instance of the event
    /// and removes it from queue, so that it is
    /// not triggered.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="_removeAll">if set to <c>true</c> [remove all instances of the event type].</param>
    /// <returns></returns>
    public bool AbortEvent(IGameEvent @event, bool _removeAll = false)
    {
        bool pass = false;

        if (_eventQueue.Peek() == @event)
        {
            // while(eventQueue.Dequeue!=nu)
        }

        return pass;
    }

    /// <summary>
    /// Every update cycle the queue is processed, if the queue processing is limited,
    /// a maximum processing time per update can be set after which the events will have
    /// to be processed next update loop.
    /// </summary>
    void Update()
    {
        float timer = 0.0f;
        while (_eventQueue.Count > 0)
        {
            if (limitProcessing)
            {
                if (timer > queueProcessTime)
                    return;
            }

            IGameEvent @event = _eventQueue.Dequeue() as IGameEvent;

            //Broadcast the event to all listeners
            TriggerEvent(@event);

            if (limitProcessing)
                timer += Time.deltaTime;
        }
    }
    #endregion

    #region Utility
    /// <summary>
    /// Determines whether the specified delegate has listener.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="delegate">The delegate.</param>
    /// <returns>
    ///   <c>true</c> if the specified delegate has listener; otherwise, <c>false</c>.
    /// </returns>
    public bool HasListener<T>(EventDelegate<T> @delegate) where T : IGameEvent
    {
        return _delegateLookup.ContainsKey(@delegate);
    }

    /// <summary>
    /// TEMP: We may use this so that
    /// other managers dont have to be monobehavours
    /// and allow them to still use coroutines.
    /// Contains pointers to 
    /// coroutines that should
    /// be run from non-monobehaviours
    /// </summary>
    /// <returns></returns>
    public delegate IEnumerator CoroutineMethod();

    /// <summary>
    /// Runs a coroutine sent from
    /// a non-mono behavior
    /// </summary>
    /// <param name="coroutineMethod"></param>
    /// <returns></returns>
    IEnumerator RunCoroutine(CoroutineMethod coroutineMethod)
    {
        return coroutineMethod();
    }

    /// <summary>
    /// Unsubscribes all listeners for
    /// the object
    /// </summary>
    /// <param name="obj">The object.</param>
    public void RemoveListeners(System.Object obj)
    {
        if (this._componentLookups.ContainsKey(obj))
        {
            foreach (KeyValuePair<System.Object, List<Delegate>> entry in this._componentLookups)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    //this.RemoveListener<>
                }
            }
        }
    }

    /// <summary>
    /// Called when the game quits
    /// We need to remove all events and
    /// clear the queue before shutting down
    /// </summary>
    public void OnApplicationQuit()
    {
        RemoveAll();
        _eventQueue.Clear();
    }
    #endregion


    #endregion

}

