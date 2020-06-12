// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    public class GetValueForAngle : ILerpValue
    {
        private Vector2 _max;
        private readonly List<Vector2> _values = new List<Vector2>();

        public float Evaluate(float key)
        {
            var value = Mathf.Clamp(key, 0, _max.x);
            return Evaluate(_values, value);

        }

        public void SetValues(IEnumerable<Vector2> keyValuepairs)
        {
            _values.Clear();
            var valuePairs = keyValuepairs.ToList();
            if (valuePairs.Any())
            {
                _max = valuePairs.OrderBy(n => n.x).Last();
                if (valuePairs.Count(f => f.x == 0) == 0)
                {
                    _values.Add(new Vector2(0,0));
                }
                _values.AddRange(valuePairs);
            }
        }

        public bool HasValues
        {
            get { return _values.Count > 0; }
        }
      
        private float Evaluate(IList<Vector2> target, float val)
        {
            var count = target.Count;
            if (count == 0) return 0;
            if (target.Count == 1) return target.First().y;

            var wp = target
                .OrderBy(p => p.x)
                .SelectWithPrevious((previous, current) => new { low = previous, high = current })
                .ToList();

            var inRange = wp.LastOrDefault(w => val > w.low.x);

            if (inRange == null) return wp.First().low.y;

            return Mathf.Lerp(inRange.low.y, inRange.high.y, (val - inRange.low.x) / (inRange.high.x - inRange.low.x));
        }
    }
}
