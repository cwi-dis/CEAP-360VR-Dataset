// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    public class TrackabilityModifier : IGazeModifier
    {
        private readonly IGazeModifierSettings _settings;

        private TobiiXR_EyeTrackingData _latestValidData = new TobiiXR_EyeTrackingData();
        private int _cacheSize = 100;
        private int _invalidCount;
        private Queue<float> _queue = new Queue<float>();
        private IList<Vector2>[] _trackabilities;
        private readonly MetricsForPercentile _metricsForPercentile;

        public TrackabilityModifier(IGazeModifierSettings settings)
        {
            _metricsForPercentile = new MetricsForPercentile(settings.Repository);
            _settings = settings;

            for (int i = 0; i < _cacheSize; i++)
            {
                _queue.Enqueue(1);
            }
            _settings = settings;
        }

        public void Modify(TobiiXR_EyeTrackingData data, Vector3 forward)
        {
            var trackability = GetTrackability(data.GazeRay.Direction, forward);
            _queue.Enqueue(data.GazeRay.IsValid ? 1f : 0f);
            _queue.Dequeue();
            var currentTrackability = _queue.Average();

            if (data.GazeRay.IsValid)
            {
                var r = Random.Range(0f, 1f);
                var shouldSetToInvalid = r > trackability;

                if (shouldSetToInvalid)
                {
                    if (currentTrackability > trackability)
                    {
                        if (_invalidCount > 0)
                        {
                            _invalidCount--;
                        }
                        else
                        {
                            data.GazeRay.IsValid = false;
                            data.GazeRay.Origin = _latestValidData.GazeRay.Origin;
                            data.GazeRay.Direction = _latestValidData.GazeRay.Direction;

                            data.IsLeftEyeBlinking = _latestValidData.IsLeftEyeBlinking;
                            data.IsRightEyeBlinking = _latestValidData.IsRightEyeBlinking;
                            data.ConvergenceDistance = _latestValidData.ConvergenceDistance;
                            data.ConvergenceDistanceIsValid = _latestValidData.ConvergenceDistanceIsValid;
                        }
                    }
                }
                else
                {
                    _latestValidData = data;
                }
            }
            else
            {
                if (_invalidCount < _cacheSize)
                    _invalidCount++;
            }
        }

        private float GetTrackability(Vector3 direction, Vector3 forward)
        {
            var angle = Mathf.Abs(Vector3.Angle(direction, forward));
            return _metricsForPercentile.Trackability(_settings.SelectedPercentileIndex,angle);
        }
    }
}