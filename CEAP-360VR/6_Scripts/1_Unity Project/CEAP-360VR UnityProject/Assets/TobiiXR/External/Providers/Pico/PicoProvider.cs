// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using Tobii.XR.Internal;

namespace Tobii.XR
{
    [CompilerFlag("TOBIIXR_PICOPROVIDER"), ProviderDisplayName("PicoVR"), SupportedPlatform(XRBuildTargetGroup.Android)]
    public class PicoProvider : IEyeTrackingProvider
    {
        public TobiiXR_EyeTrackingData EyeTrackingDataLocal { get; private set; }

#if TOBIIXR_PICOPROVIDER
        private Pvr_UnitySDKAPI.EyeTrackingData _eyeTrackingData;

        public Matrix4x4 LocalToWorldMatrix { get { return Pvr_UnitySDKManager.SDK.transform.localToWorldMatrix * Pvr_UnitySDKManager.SDK.HeadPose.Matrix; } }

        public bool Initialize(FieldOfUse fieldOfUse)
        {
            EyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
            var result = Pvr_UnitySDKAPI.System.UPvr_setTrackingMode((int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_POSITION | (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE);
            if (!result) Debug.LogWarning("Failed to enable eye tracking");

            return result;
        }

        public void Tick()
        {
            bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref _eyeTrackingData);

            EyeTrackingDataLocal.Timestamp = UnityEngine.Time.unscaledTime;

            EyeTrackingDataLocal.GazeRay = new TobiiXR_GazeRay
            {
                Direction = _eyeTrackingData.combinedEyeGazeVector,
                Origin = _eyeTrackingData.combinedEyeGazePoint,
                IsValid = (_eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazePointValid) != 0 && (_eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazeVectorValid) != 0,
            };

            var leftEyeOpennessIsValid = (_eyeTrackingData.leftEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kEyeOpennessValid) != 0;
            var rightEyeOpennessIsValid = (_eyeTrackingData.rightEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kEyeOpennessValid) != 0;

            EyeTrackingDataLocal.IsLeftEyeBlinking = !leftEyeOpennessIsValid || UnityEngine.Mathf.Approximately(_eyeTrackingData.leftEyeOpenness, 0.0f);
            EyeTrackingDataLocal.IsRightEyeBlinking = !rightEyeOpennessIsValid || UnityEngine.Mathf.Approximately(_eyeTrackingData.rightEyeOpenness, 0.0f);
        }
#else
        public Matrix4x4 LocalToWorldMatrix { get { return Matrix4x4.identity; } }
        public bool Initialize(FieldOfUse fieldOfUse) 
        {
            Debug.LogError(string.Format("Scripting define symbol \"{0}\" not set for {1}.", AssemblyUtils.GetProviderCompilerFlag(this), this.GetType().Name));
            return false; 
        }
        public void Tick() { }
#endif

        public void Destroy()
        {
        }
    }
}