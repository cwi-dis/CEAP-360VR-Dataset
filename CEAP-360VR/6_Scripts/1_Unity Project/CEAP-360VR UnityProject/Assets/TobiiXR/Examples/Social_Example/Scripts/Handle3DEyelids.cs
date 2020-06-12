// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using UnityEngine;

/// <summary>
/// Monobehaviour which handles 3D eyelids.
/// </summary>
[RequireComponent(typeof(Handle3DEyes))]
public class Handle3DEyelids : MonoBehaviour
{
    // Provides access to eye direction
    private Handle3DEyes _handle3DEyes;

#pragma warning disable 649

    [Header("Eyelid Transforms")]
    [SerializeField]
    private Transform _upperLeftEyeLid;

    [SerializeField]
    private Transform _lowerLeftEyeLid;

    [SerializeField]
    private Transform _upperRightEyeLid;

    [SerializeField]
    private Transform _lowerRightEyeLid;

    [SerializeField, Tooltip("Blink speed.")]
    private float _blinkSpeed = 0.015f;

#pragma warning restore 649

    // Eyelid constants specific to model design.
    private const float UpperEyelidOpenAngle = -135;

    private const float UpperEyelidClosedAngle = -65;
    private const float LowerEyelidOpenAngle = 135;
    private const float LowerEyelidClosedAngle = 120;
    private const float TopLidBlendShapeFactor = 50;
    private const float TopLidBlendShapeOffset = 10;

    private float _leftLidSmoothDampVelocity;
    private float _rightLidSmoothDampVelocity;

    // Running eyelid animation values.
    private float _upperLeftEyeLidAngle;

    private float _upperRightEyeLidAngle;
    private float _lowerLeftEyeLidAngle;
    private float _lowerRightEyeLidAngle;

    private void Start()
    {
        _handle3DEyes = GetComponent<Handle3DEyes>();
    }

    private void Update()
    {
        // Get local copies.
        var eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

        // Animate eyelids
        AnimateEyeLids(_handle3DEyes.LeftEyeDirection.y, !eyeData.IsLeftEyeBlinking, !eyeData.IsRightEyeBlinking);
    }

    /// <summary>
    /// Animates the eyelids to partially follow gaze. When you look up/down the eye lids follow.
    /// </summary>
    /// <param name="newDirectionY">The new Y direction of the gaze.</param>
    /// <param name="isLeftEyeOpen">If the left eye is open.</param>
    /// <param name="isRightEyeOpen">If the right eye is open.</param>
    private void AnimateEyeLids(float newDirectionY, bool isLeftEyeOpen, bool isRightEyeOpen)
    {
        var gazeDrivenTopLidOffset = newDirectionY * TopLidBlendShapeFactor - TopLidBlendShapeOffset;
        var gazeDrivenBottomLidOffset = newDirectionY * TopLidBlendShapeFactor;

        // Left eyelid.
        var newTopEulerX = isLeftEyeOpen ? UpperEyelidOpenAngle - gazeDrivenTopLidOffset : UpperEyelidClosedAngle;
        var newBottomEulerX = isLeftEyeOpen ? LowerEyelidOpenAngle - gazeDrivenBottomLidOffset : LowerEyelidClosedAngle;
        _upperLeftEyeLidAngle = Mathf.SmoothDampAngle(_upperLeftEyeLidAngle, newTopEulerX, ref _leftLidSmoothDampVelocity, _blinkSpeed);
        _lowerLeftEyeLidAngle = Mathf.SmoothDampAngle(_lowerLeftEyeLidAngle, newBottomEulerX, ref _leftLidSmoothDampVelocity, _blinkSpeed);
        _upperLeftEyeLid.localEulerAngles = new Vector3(_upperLeftEyeLidAngle, 0, 0);
        _lowerLeftEyeLid.localEulerAngles = new Vector3(_lowerLeftEyeLidAngle, 0, 0);

        // Right eyelid.
        newTopEulerX = isRightEyeOpen ? UpperEyelidOpenAngle - gazeDrivenTopLidOffset : UpperEyelidClosedAngle;
        newBottomEulerX = isRightEyeOpen ? LowerEyelidOpenAngle - gazeDrivenBottomLidOffset : LowerEyelidClosedAngle;
        _upperRightEyeLidAngle = Mathf.SmoothDampAngle(_upperRightEyeLidAngle, newTopEulerX, ref _rightLidSmoothDampVelocity, _blinkSpeed);
        _lowerRightEyeLidAngle = Mathf.SmoothDampAngle(_lowerRightEyeLidAngle, newBottomEulerX, ref _rightLidSmoothDampVelocity, _blinkSpeed);
        _upperRightEyeLid.localEulerAngles = new Vector3(_upperRightEyeLidAngle, 0, 0);
        _lowerRightEyeLid.localEulerAngles = new Vector3(_lowerRightEyeLidAngle, 0, 0);
    }
}