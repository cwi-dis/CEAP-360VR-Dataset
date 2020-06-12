// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    public class PercentileData
    {
        public int Percentile;
        public float Precision;
        public float Angle;
        public float Accuracy;
        public float Trackability;

        private static readonly ILerpValue _precisionFactors = new GetValueForAngle();

        public PercentileData()
        {
        }

        public PercentileData(int percent, float angle, float accuracy, float trackability)
        {
            _precisionFactors.SetValues(new List<Vector2>() { new Vector2(0, .45f), new Vector2(10, .45f), new Vector2(20, .4f), new Vector2(25, .35f) });
            Percentile = percent;
            Angle = angle;
            Accuracy = accuracy;
            Precision = _precisionFactors.Evaluate(angle) * accuracy;
            Trackability = trackability;
        }
    }
}