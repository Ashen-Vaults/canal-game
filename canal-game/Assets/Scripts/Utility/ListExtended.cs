using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An extended class for lists
/// </summary>
public static class ListExtended
{
    private static readonly Random rnd = new Random();

    /// <summary>
    /// Randomizes a list: based off of
    /// Fisher-Yates shuffling.
    /// 
    /// Source: https://stackoverflow.com/revisions/1262619/1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = (rnd.Next(0, n) % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
