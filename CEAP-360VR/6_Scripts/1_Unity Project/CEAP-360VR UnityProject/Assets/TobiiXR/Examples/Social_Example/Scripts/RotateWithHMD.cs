// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

/// <summary>
/// Rotates this game object to the same rotation as the camera (HMD) rotation.
/// </summary>
public class RotateWithHMD : MonoBehaviour
{
    private Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = Tobii.XR.CameraHelper.GetCameraTransform();
    }

	private void Update ()
	{
	    transform.localRotation = _cameraTransform.rotation;
	}
}
