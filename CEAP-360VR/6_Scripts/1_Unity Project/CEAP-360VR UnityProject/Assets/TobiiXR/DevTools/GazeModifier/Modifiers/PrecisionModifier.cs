// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    public class PrecisionModifier : IGazeModifier
    {
        private readonly IGazeModifierSettings _settings;
        private int _currentPercentileIndex;
        private IList<Vector2>[] _precisions;
        private readonly MetricsForPercentile _metricsForPercentile;

        public PrecisionModifier(IGazeModifierSettings settings)
        {
            _settings = settings;
            _metricsForPercentile = new MetricsForPercentile(settings.Repository);
        }
        
        private Vector3 Modify(Vector3 direction, Vector3 forward)
        {
            var maxAngleDegrees = GetMaxAngle(direction, forward);
            var randomX = Random.Range(-maxAngleDegrees, maxAngleDegrees);
            var randomY = Random.Range(-maxAngleDegrees, maxAngleDegrees);

            var ret = Quaternion.Euler(randomY, randomX, 0) * direction;
            return ret;
        }

        public void Modify(TobiiXR_EyeTrackingData data, Vector3 forward)
        {
            data.GazeRay.Direction = Modify(data.GazeRay.Direction, forward);
        }

        public float GetMaxAngle(Vector3 direction, Vector3 forward)
        {
            var angle = Vector3.Angle(forward, direction);
            return _metricsForPercentile.Precision(_settings.SelectedPercentileIndex, angle);
        }
    }
}
