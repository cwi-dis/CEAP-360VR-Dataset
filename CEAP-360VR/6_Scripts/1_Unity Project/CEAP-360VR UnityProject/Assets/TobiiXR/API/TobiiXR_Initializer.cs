// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using UnityEngine;

/// <summary>
/// Optional convenience initializer for TobiiXR. Used by the TobiiXR Initializer prefab.
///
/// Feel free to replace this script with a manual call to TobiiXR.Start.
/// 
/// Ideally TobiiXR.Start should be called after any required VR SDK has been
/// initialized but before any game object that use TobiiXR is called.
/// </summary>
[DefaultExecutionOrder(-10)]
public class TobiiXR_Initializer : MonoBehaviour
{
    public TobiiXR_Settings Settings;

    private void Awake()
    {
        TobiiXR.Start(Settings);
    }
}