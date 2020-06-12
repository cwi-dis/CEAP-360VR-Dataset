// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using Tobii.XR;

/// <summary>
/// Monobehaviour which handles facial expressions.
/// </summary>
[RequireComponent(typeof(Handle3DEyes))]
public class Handle3DExpressions : MonoBehaviour
{
    // Provides access to eye direction
    private Handle3DEyes _handle3DEyes;

#pragma warning disable 649
    [Header("Blend Shapes")]

    [SerializeField, Tooltip("Face Skinned Mesh Renderer for blend shapes.")]
    private SkinnedMeshRenderer _faceBlendShapes;

    [SerializeField, Tooltip("Teeth Skinned Mesh Renderer for blend shapes.")]
    private SkinnedMeshRenderer _teethBlendShapes;

    [SerializeField, Tooltip("Facial blend shapes animation curve.")]
    private AnimationCurve _blendShapeAnimationCurve;

    [SerializeField, Tooltip("Facial blend shape animation time.")]
    private float _blendShapeAnimationTimeSeconds = 0.5f;
#pragma warning restore 649

    // Running blend shapes animation values.
    private float _winkL;
    private float _winkR;
    private float _currentSnarlExtent;
    private float _animationProgress;

    // Blink or wink logic values.
    private float _winkTimer;
    private const float BlinkWinkTime = 0.03f;

    // Catch change in state flags.
    private bool _leftEyeClosed;
    private bool _rightEyeClosed;
    private bool _crossEyed;

    private const float EyeBrowBlendShapeHorizontalFactor = 100;
    private const float EyeBrowBlendShapeVerticalFactor = 300;
    private const float BlendShapeFactor = 100;
    private const float CrossEyednessSnarlTriggerValue = 0.3f;

    // Blend Shape constants.
    private const int BlendShapeCrossEyedSnarl = 0;
    private const int BlendShapeLeftEyeBrowUp = 1;
    private const int BlendShapeRightEyeBrowUp = 2;
    private const int BlendShapeLeftWink = 3;
    private const int BlendShapeRightWink = 4;
    private const int BlendShapeTeethLeftWink = 0;
    private const int BlendShapeTeethRightWink = 1;

    private void Start()
    {
        _handle3DEyes = GetComponent<Handle3DEyes>();
    }

    private void Update()
    {
        AnimateFacialExpressions(_handle3DEyes.LeftEyeDirection, _handle3DEyes.RightEyeDirection, TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local));
    }

    /// <summary>
    /// Animate facial expression blend shapes.
    /// </summary>
    /// <param name="newDirectionL">The gaze direction for the left eye.</param>
    /// <param name="newDirectionR">The gaze direction for the right eye.</param>
    /// <param name="eyeDataLeft">Eye data for the left eye.</param>
    /// <param name="eyeDataRight">Eye data for the right eye.</param>
    private void AnimateFacialExpressions(Vector3 newDirectionL, Vector3 newDirectionR, TobiiXR_EyeTrackingData eyeData)
    {
        // Eyebrows.
        var leftEyeBrowBlendShapeWeight = -newDirectionL.x * EyeBrowBlendShapeHorizontalFactor + newDirectionL.y * EyeBrowBlendShapeVerticalFactor;
        var rightEyeBrowBlendShapeWeight = newDirectionR.x * EyeBrowBlendShapeHorizontalFactor + newDirectionR.y * EyeBrowBlendShapeVerticalFactor;
        _faceBlendShapes.SetBlendShapeWeight(BlendShapeLeftEyeBrowUp, Mathf.Clamp(leftEyeBrowBlendShapeWeight, 0f, 100f));
        _faceBlendShapes.SetBlendShapeWeight(BlendShapeRightEyeBrowUp, Mathf.Clamp(rightEyeBrowBlendShapeWeight, 0f, 100f));

        // If eye openness data is invalid use last value.
        var leftEyeClosed = eyeData.IsLeftEyeBlinking;
        var rightEyeClosed = eyeData.IsRightEyeBlinking;

        // Use a timer to filter unintentional winks.
        if (leftEyeClosed ^ rightEyeClosed)
        {
            _winkTimer += Time.deltaTime;
        }
        else
        {
            _winkTimer = 0;
        }

        // If eye openness has changed reset animation curve.
        if (leftEyeClosed != _leftEyeClosed || rightEyeClosed != _rightEyeClosed)
        {
            _animationProgress = 0;
        }

        var leftEyeBlendShapeWinkValue = (leftEyeClosed && _winkTimer > BlinkWinkTime ? BlendShapeFactor : 0);
        var rightEyeBlendShapeWinkValue = (rightEyeClosed && _winkTimer > BlinkWinkTime ? BlendShapeFactor : 0);
        _winkL = Mathf.Lerp(_winkL, leftEyeBlendShapeWinkValue, _blendShapeAnimationCurve.Evaluate(_animationProgress));
        _winkR = Mathf.Lerp(_winkR, rightEyeBlendShapeWinkValue, _blendShapeAnimationCurve.Evaluate(_animationProgress));

        _faceBlendShapes.SetBlendShapeWeight(BlendShapeLeftWink, Mathf.Clamp(_winkL, 0f, 100f));
        _faceBlendShapes.SetBlendShapeWeight(BlendShapeRightWink, Mathf.Clamp(_winkR, 0f, 100f));
        _teethBlendShapes.SetBlendShapeWeight(BlendShapeTeethLeftWink, Mathf.Clamp(_winkL, 0f, 100f));
        _teethBlendShapes.SetBlendShapeWeight(BlendShapeTeethRightWink, Mathf.Clamp(_winkR, 0f, 100f));

        // Cross eyed snarl.
        if (!leftEyeClosed && !rightEyeClosed)
        {
            var crossEyedness = Mathf.Abs(newDirectionL.x - newDirectionR.x);
            var crossEyedThisFrame = crossEyedness > CrossEyednessSnarlTriggerValue;
            if (crossEyedThisFrame != _crossEyed)
            {
                _animationProgress = 0;
            }

            _currentSnarlExtent = Mathf.Lerp(_currentSnarlExtent, crossEyedThisFrame ? crossEyedness * BlendShapeFactor : 0, _blendShapeAnimationCurve.Evaluate(_animationProgress));
            _crossEyed = crossEyedThisFrame;

            _faceBlendShapes.SetBlendShapeWeight(BlendShapeCrossEyedSnarl, Mathf.Clamp(_currentSnarlExtent, 0f, 100f));
        }

        _leftEyeClosed = leftEyeClosed;
        _rightEyeClosed = rightEyeClosed;

        _animationProgress += Time.deltaTime / _blendShapeAnimationTimeSeconds;
    }
}
