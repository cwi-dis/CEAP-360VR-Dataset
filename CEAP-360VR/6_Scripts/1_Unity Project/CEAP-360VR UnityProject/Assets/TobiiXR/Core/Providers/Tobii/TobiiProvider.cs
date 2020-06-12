// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Uses Tobii's Stream Engine library to provide eye tracking data to TobiiXR
    /// </summary>
    [ProviderDisplayName("Tobii")]
    public class TobiiProvider : IEyeTrackingProvider
    {
        private StreamEngineTracker _streamEngineTracker;
        private HmdToWorldTransformer _hmdToWorldTransformer;
        private TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
        private Matrix4x4 _localToWorldMatrix;

        public Matrix4x4 LocalToWorldMatrix
        {
            get { return _localToWorldMatrix; }
        }

        public TobiiXR_EyeTrackingData EyeTrackingDataLocal
        {
            get { return _eyeTrackingDataLocal; }
        }

        public bool Initialize(FieldOfUse fieldOfUse)
        {
            return Initialize(fieldOfUse, null);
        }

        public bool Initialize(FieldOfUse fieldOfUse, StreamEngineTracker streamEngineTracker)
        {
            try
            {
                if (streamEngineTracker == null)
                {
                    _streamEngineTracker = new StreamEngineTracker(fieldOfUse);
                }
                else
                {
                    _streamEngineTracker = streamEngineTracker;
                }

                _hmdToWorldTransformer = new HmdToWorldTransformer(estimatedEyeTrackerLatency_s: 0.012f);
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        public void Tick()
        {
            _streamEngineTracker.Tick();
            _hmdToWorldTransformer.Tick();

            var data = _streamEngineTracker.LocalLatestData;

            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;
            _eyeTrackingDataLocal.GazeRay = data.GazeRay;
            _eyeTrackingDataLocal.IsLeftEyeBlinking = data.IsLeftEyeBlinking;
            _eyeTrackingDataLocal.IsRightEyeBlinking = data.IsRightEyeBlinking;
            _eyeTrackingDataLocal.ConvergenceDistance = data.ConvergenceDistance;
            _eyeTrackingDataLocal.ConvergenceDistanceIsValid = data.ConvergenceDistanceIsValid;

            _localToWorldMatrix = _hmdToWorldTransformer.GetLocalToWorldMatrix();
        }

        public void Destroy()
        {
            if (_streamEngineTracker != null)
            {
                _streamEngineTracker.Destroy();
                _streamEngineTracker = null;
            }
        }
    }
}