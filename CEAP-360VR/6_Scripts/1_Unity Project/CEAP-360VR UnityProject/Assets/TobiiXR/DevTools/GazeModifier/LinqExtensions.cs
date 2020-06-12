// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    public static class LinqExtension
    {
        public static IEnumerable<TResult> SelectWithPrevious<TSource, TResult>
        (this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {

                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return projection(previous, iterator.Current);
                    previous = iterator.Current;
                }
            }
        }

        public static IEnumerable<T> Trim<T>(this IEnumerable<T> source, float percentage)
        {
            var s = source.ToList();
            var total = s.Count;
            var toTake  = Mathf.RoundToInt(total * percentage);
            var skipFirst = Mathf.RoundToInt((total - toTake) / 2);
            return s.Skip(skipFirst).Take(toTake);
        }
    }
}

