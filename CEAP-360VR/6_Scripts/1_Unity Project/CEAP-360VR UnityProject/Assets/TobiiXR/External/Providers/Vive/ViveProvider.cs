// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using Tobii.XR.Internal;
using UnityEngine;

#if TOBIIXR_HTCPROVIDER
using ViveSR.anipal.Eye;
#endif

/// <summary>
/// Provides eye tracking data to TobiiXR using HTC's SR Anipal SDK. 
/// SR Anipal needs to be downloaded from https://hub.vive.com/en-US/profile/material-download and added to the project separately. 
/// Tested with SR Anipal version 1.1.0.1
/// </summary>
[CompilerFlag("TOBIIXR_HTCPROVIDER", "Requires SR Anipal SDK"), ProviderDisplayName("VIVE"), SupportedPlatform(XRBuildTargetGroup.Standalone)]
public class HTCProvider : IEyeTrackingProvider
{
    private Matrix4x4 _localToWorldMatrix = Matrix4x4.identity;
    private TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();

    public TobiiXR_EyeTrackingData EyeTrackingDataLocal { get { return _eyeTrackingDataLocal; } }

    public Matrix4x4 LocalToWorldMatrix { get { return _localToWorldMatrix; } }

#if TOBIIXR_HTCPROVIDER
    private GameObject _htcGameObject;
    private HmdToWorldTransformer _hmdToWorldTransformer;

    public bool Initialize(FieldOfUse fieldOfUse)
    {
        if (!SRanipal_Eye_API.IsViveProEye()) return false;

        _hmdToWorldTransformer = new HmdToWorldTransformer(estimatedEyeTrackerLatency_s: 0.040f);
        EnsureHTCFrameworkRunning();

        return SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING;
    }

    private void EnsureHTCFrameworkRunning()
    {
        if (_htcGameObject != null) return;
        _htcGameObject = new GameObject("HTC")
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        var sr = _htcGameObject.AddComponent<SRanipal_Eye_Framework>();
        sr.StartFramework();
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) sr.StartFramework(); // Try a second time since it often times out on first attempt
    }

    public void Tick()
    {
        _hmdToWorldTransformer.Tick();
        EnsureHTCFrameworkRunning();

        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;

        _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;
        _eyeTrackingDataLocal.GazeRay.IsValid = SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out _eyeTrackingDataLocal.GazeRay.Origin, out _eyeTrackingDataLocal.GazeRay.Direction);

        // Blink
        float eyeOpenness;
        var eyeOpennessIsValid = SRanipal_Eye.GetEyeOpenness(EyeIndex.LEFT, out eyeOpenness);
        _eyeTrackingDataLocal.IsLeftEyeBlinking = !eyeOpennessIsValid || eyeOpenness < 0.1;
        eyeOpennessIsValid = SRanipal_Eye.GetEyeOpenness(EyeIndex.RIGHT, out eyeOpenness);
        _eyeTrackingDataLocal.IsRightEyeBlinking = !eyeOpennessIsValid || eyeOpenness < 0.1;

        // Convergence distance
        Vector3 leftRayOrigin, rightRayOrigin, leftRayDirection, rightRayDirection;
        var leftRayValid = SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out leftRayOrigin, out leftRayDirection);
        var rightRayValid = SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out rightRayOrigin, out rightRayDirection);

        if (leftRayValid && rightRayValid)
        {
            _eyeTrackingDataLocal.ConvergenceDistanceIsValid = true;
            var convergenceDistance_mm = Convergence.CalculateDistance(
                        leftRayOrigin * 1000f,
                        leftRayDirection,
                        rightRayOrigin * 1000f,
                        rightRayDirection
                        );
            _eyeTrackingDataLocal.ConvergenceDistance = convergenceDistance_mm / 1000f; // Convert to meters
        }
        else
        {
            _eyeTrackingDataLocal.ConvergenceDistanceIsValid = false;
        }

        // Update world transform
        _hmdToWorldTransformer.Tick();
        _localToWorldMatrix = _hmdToWorldTransformer.GetLocalToWorldMatrix();
    }

    public void Destroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            GameObject.Destroy(_htcGameObject);
        }
        else
        {
            GameObject.DestroyImmediate(_htcGameObject);
        }
#else
        GameObject.Destroy(_htcGameObject);
#endif

        _htcGameObject = null;
    }

#else
    public bool Initialize(FieldOfUse fieldOfUse) 
    {
        Debug.LogError(string.Format("Scripting define symbol \"{0}\" not set for {1}.", AssemblyUtils.GetProviderCompilerFlag(this), this.GetType().Name));
        return false;
    }
    public void Tick() { }
    public void Destroy() { }
#endif
}
