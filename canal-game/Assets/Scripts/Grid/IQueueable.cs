using UnityEngine;
using System.Collections;
using System;

public interface IQueueable : IComparable
{
    int Priority {get;}
    int QueueIndex { get; set; }

    int InsertionIndex { get; set; }
 
}
