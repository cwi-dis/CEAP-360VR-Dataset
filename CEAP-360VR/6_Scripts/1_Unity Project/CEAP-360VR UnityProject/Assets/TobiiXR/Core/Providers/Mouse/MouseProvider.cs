// Copyright © 2019 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Provides emulated gaze data to TobiiXR using the mouse. 
    /// Note: This provider adds a child camera to the main camera that will render on top.
    /// If main camera is changed during the scene the child camera will not move.
    /// Only use this provider for debugging in Unity Editor.
    /// </summary>
    [ProviderDisplayName("Mouse")]
    public class MouseProvider : IEyeTrackingProvider
    {
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();

        private static Camera _mouseProviderCamera;

        public Matrix4x4 LocalToWorldMatrix { get { return _mouseProviderCamera.cameraToWorldMatrix; } }

        public TobiiXR_EyeTrackingData EyeTrackingDataLocal { get { return _eyeTrackingDataLocal; } }
        
        public bool Initialize(FieldOfUse fieldOfUse)
        {
            return true;
        }

        public void Tick()
        {
            if (_mouseProviderCamera == null)
            {
                _mouseProviderCamera = GetMouseProviderCamera(GetType().Name);
            }

            // Mouse ray will be in world space
            var mouseRay = _mouseProviderCamera.ScreenPointToRay(Input.mousePosition);
            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;

            // Transform to local space
            var mat = _mouseProviderCamera.worldToCameraMatrix;
            _eyeTrackingDataLocal.GazeRay.Origin = mat.MultiplyPoint(mouseRay.origin);
            _eyeTrackingDataLocal.GazeRay.Direction = mat.MultiplyVector(mouseRay.direction.normalized);
            _eyeTrackingDataLocal.GazeRay.IsValid = true;
        }

        public void Destroy()
        {
            if (_mouseProviderCamera == null) return;

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(_mouseProviderCamera.gameObject);
            }
            else
            {
                Object.DestroyImmediate(_mouseProviderCamera.gameObject);
            }
#else
            Object.Destroy(_mouseProviderCamera.gameObject);
#endif
            _mouseProviderCamera = null;
        }

        // TODO We should not create extra camera when mouse position issue is fixed in Unity
        // https://issuetracker.unity3d.com/issues/screenpointtoray-is-offset-when-used-in-vr-with-openvr-sdk
        private static Camera GetMouseProviderCamera(string name)
        {
            var parent = CameraHelper.GetCameraTransform();
            var camera = new GameObject(string.Format("{0} Camera", name)).AddComponent<Camera>();
            camera.transform.parent = parent;
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localScale = Vector3.one;
            camera.transform.localRotation = Quaternion.identity;

            camera.stereoTargetEye = StereoTargetEyeMask.None;

            return camera;
        }
    }
}
