// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Provides emulated gaze data to TobiiXR using camera position and rotation
    /// </summary>
    [ProviderDisplayName("Nose Direction")]
    public class NoseDirectionProvider : IEyeTrackingProvider
    {
        private Transform _hmdOrigin;
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();

        public Matrix4x4 LocalToWorldMatrix { get { return CameraHelper.GetCameraTransform().localToWorldMatrix; } }

        public TobiiXR_EyeTrackingData EyeTrackingDataLocal { get { return _eyeTrackingDataLocal; } }

        public bool Initialize(FieldOfUse fieldOfUse)
        {
            _eyeTrackingDataLocal.GazeRay.Origin = Vector3.zero;
            _eyeTrackingDataLocal.GazeRay.Direction = Vector3.forward;
            _eyeTrackingDataLocal.GazeRay.IsValid = true;

            return true;
        }

        public void Tick()
        {
            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;
        }

        public void Destroy() { }
    }
}