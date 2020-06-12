// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    public class AccuracyModifier : IGazeModifier
    {
        private readonly IGazeModifierSettings _settings;
        private readonly MetricsForPercentile _metricsForPercentile;

        public AccuracyModifier(IGazeModifierSettings settings)
        {
            _settings = settings;
            _metricsForPercentile = new MetricsForPercentile(settings.Repository);
        }

        private Vector3 Modify(Vector3 direction, Vector3 forward)
        {
            var reference = forward;
            var rotation = Quaternion.FromToRotation(reference, direction);
            var diff = Vector3.Angle(reference, direction);

            if (Math.Abs(diff) < .001f) return direction;
            var angle = _metricsForPercentile.Accuracy(_settings.SelectedPercentileIndex,diff);
            var rotate = Quaternion.Lerp(Quaternion.identity, rotation, angle / diff);
            var result = rotate * direction;
            var a = angle;
            while (a > diff)
            {
                a -= diff;
                rotate = Quaternion.Lerp(Quaternion.identity, rotation, a / diff);
                result = rotate * result;
            }
            return result;
        }

        public void Modify(TobiiXR_EyeTrackingData data, Vector3 forward)
        {
            data.GazeRay.Direction = Modify(data.GazeRay.Direction, forward);
        }
    }
}

