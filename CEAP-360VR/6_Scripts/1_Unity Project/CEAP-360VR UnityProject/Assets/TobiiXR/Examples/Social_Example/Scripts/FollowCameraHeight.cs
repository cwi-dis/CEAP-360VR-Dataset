// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

/// <summary>
/// Updates the y position of the transform to match the camera plus an offset.
/// </summary>
public class FollowCameraHeight : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField, Tooltip("Keep character's face at camera height")]
    private float _heightOffset;
#pragma warning restore 649

    private float _avatarHeadPosY;
    private Transform _cameraTransform;

    private const float SmoothTime = 2.2f;

    private void Start()
    {
        _cameraTransform = Tobii.XR.CameraHelper.GetCameraTransform();
    }

	private void Update ()
    {
        // Lerp the y position of the avatar's transform to match the camera plus a height offset.
        _avatarHeadPosY = Mathf.Lerp(_avatarHeadPosY, _cameraTransform.position.y + _heightOffset, Time.deltaTime * SmoothTime);
        transform.position = new Vector3(transform.position.x, _avatarHeadPosY, transform.position.z);
	}
}
