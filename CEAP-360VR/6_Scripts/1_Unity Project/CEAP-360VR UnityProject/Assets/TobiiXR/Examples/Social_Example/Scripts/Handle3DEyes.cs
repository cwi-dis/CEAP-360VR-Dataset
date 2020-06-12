// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using UnityEngine;

/// <summary>
/// Monobehaviour which handles the eye direction for 3D eyes.
/// </summary>
public class Handle3DEyes : MonoBehaviour
{
#pragma warning disable 649

    [Header("Eye Transforms")]
    [SerializeField]
    private Transform _leftEye;

    [SerializeField]
    private Transform _rightEye;

    [Header("Eye Behaviour values")]
    [SerializeField, Tooltip("Cross eye correction for models that look cross eyed. +/-20 degrees.")]
    [Range(-20.0f, 20.0f)]
    private float _crossEyeCorrection;

    [SerializeField, Tooltip("Reduce gaze direction jitters at the cost of responsiveness.")]
    private float _gazeDirectionSmoothTime = 0.03f;

    [SerializeField, Tooltip("Maximum eye vertical angle.")]
    private float _verticalGazeAngleUpperLimitInDegrees = 30;

    [SerializeField, Tooltip("Minimum eye vertical angle.")]
    private float _verticalGazeAngleLowerLimitInDegrees = 30;

    [SerializeField, Tooltip("Maximum eye horizontal angle.")]
    private float _horizontalGazeAngleLimitInDegrees = 35;

#pragma warning restore 649

    private static ExponentialSmoothing _smoothing = new ExponentialSmoothing();

    // Expose eye direction for deriving facial expressions etc. in other components.
    public Vector3 LeftEyeDirection
    {
        get { return _lastGoodDirection; }
    }

    public Vector3 RightEyeDirection
    {
        get { return _lastGoodDirection; }
    }

    private const float CrossEyedCorrectionFactor = 100;
    private const float AngularNoiseCompensationFactor = 800;
    private Transform _middleOfTheEyes;

    private Vector3 _lastGoodDirection = Vector3.forward;
    private Vector3 _previousSmoothedDirectionL = Vector3.zero;
    private Vector3 _previousSmoothedDirectionR = Vector3.zero;
    private Vector3 _smoothDampVelocityL;
    private Vector3 _smoothDampVelocityR;

    private void Awake()
    {
        var middlePoint = new GameObject();
        middlePoint.transform.parent = _leftEye.parent;
        middlePoint.name = "middleOfTheEyes";
        _middleOfTheEyes = middlePoint.transform;
        _middleOfTheEyes.localRotation = Quaternion.Euler(Vector3.zero);
        _middleOfTheEyes.localScale = Vector3.one;
        _middleOfTheEyes.position = (_leftEye.position + _rightEye.position) / 2;
    }

    private void Update()
    {
        // Get local copies.
        var eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

        // Get local transform direction.
        var gazeDirection = eyeData.GazeRay.Direction;

        // If direction data is invalid use other eye's data or if that's invalid use last good data.
        gazeDirection = eyeData.GazeRay.IsValid ? gazeDirection : _lastGoodDirection;

        // Save last good data.
        _lastGoodDirection = gazeDirection;

        // Correct how some avatar models look cross eyed.
        gazeDirection.x += (_crossEyeCorrection / CrossEyedCorrectionFactor);

        // Clamp the gaze angles within model's thresholds.
        gazeDirection = ClampVerticalGazeAngles(gazeDirection, _verticalGazeAngleLowerLimitInDegrees, _verticalGazeAngleUpperLimitInDegrees);
        gazeDirection = ClampHorizontalGazeAngles(gazeDirection, _horizontalGazeAngleLimitInDegrees);

        _smoothing.Process(eyeData.ConvergenceDistanceIsValid ? eyeData.ConvergenceDistance : 2.0f); // Smooth towards 2 meters if data is missing

        var convergencePoint = _middleOfTheEyes.position + (_middleOfTheEyes.TransformDirection(gazeDirection) * (_smoothing.CurrentData));

        var directionR = convergencePoint - _rightEye.position;
        var directionL = convergencePoint - _leftEye.position;

        // Increase smoothing for noisier higher angles
        var angle = Vector3.Angle(gazeDirection, Vector3.forward);
        var compensatedSmoothTime = _gazeDirectionSmoothTime + angle / AngularNoiseCompensationFactor;

        var smoothedDirectionLeftEye = Vector3.SmoothDamp(_previousSmoothedDirectionL, directionL, ref _smoothDampVelocityL, compensatedSmoothTime);
        var smoothedDirectionRightEye = Vector3.SmoothDamp(_previousSmoothedDirectionR, directionR, ref _smoothDampVelocityR, compensatedSmoothTime);
        _previousSmoothedDirectionL = smoothedDirectionLeftEye;
        _previousSmoothedDirectionR = smoothedDirectionRightEye;

        // Rotate the eye transforms to match the eye direction.
        var leftRotation = Quaternion.LookRotation(smoothedDirectionLeftEye);
        var rightRotation = Quaternion.LookRotation(smoothedDirectionRightEye);

        _leftEye.rotation = leftRotation;
        _rightEye.rotation = rightRotation;
    }

    /// <summary>
    /// Clamp vertical gaze angles - needs to be done in degrees.
    /// </summary>
    /// <param name="gazeDirection">Direction vector of the gaze.</param>
    /// <param name="lowerLimit">The lower clamp limit in degrees.</param>
    /// <param name="upperLimit">The upper clamp limit  in degrees.</param>
    /// <returns>The gaze direction clamped between the two degree limits.</returns>
    private static Vector3 ClampVerticalGazeAngles(Vector3 gazeDirection, float lowerLimit, float upperLimit)
    {
        var angleRad = Mathf.Atan(gazeDirection.y / gazeDirection.z);
        var angleDeg = angleRad * Mathf.Rad2Deg;

        var y = Mathf.Tan(upperLimit * Mathf.Deg2Rad) * gazeDirection.z;
        if (angleDeg > upperLimit)
        {
            gazeDirection = new Vector3(gazeDirection.x, y, gazeDirection.z);
        }

        y = Mathf.Tan(-lowerLimit * Mathf.Deg2Rad) * gazeDirection.z;
        if (angleDeg < -lowerLimit)
        {
            gazeDirection = new Vector3(gazeDirection.x, y, gazeDirection.z);
        }

        return gazeDirection;
    }

    /// <summary>
    /// Clamp horizontal gaze angles - needs to be done in degrees.
    /// </summary>
    /// <param name="gazeDirection">Direction vector of the gaze.</param>
    /// <param name="limit">The limit to clamp to in degrees.</param>
    /// <returns>The clamped gaze direction.</returns>
    private static Vector3 ClampHorizontalGazeAngles(Vector3 gazeDirection, float limit)
    {
        var angleRad = Mathf.Atan(gazeDirection.x / gazeDirection.z);
        var angleDeg = angleRad * Mathf.Rad2Deg;

        var x = Mathf.Tan(limit * Mathf.Deg2Rad) * gazeDirection.z;
        if (angleDeg > limit)
        {
            gazeDirection = new Vector3(x, gazeDirection.y, gazeDirection.z);
        }

        if (angleDeg < -limit)
        {
            gazeDirection = new Vector3(-x, gazeDirection.y, gazeDirection.z);
        }

        return gazeDirection;
    }

    private class ExponentialSmoothing
    {
        public float CurrentData;
        private float _alpha = 0.25f;
        private float _previousData;

        public ExponentialSmoothing()
        {
            Init(2f, 0.2f);
        }

        public float Alpha { get { return _alpha; } set { _alpha = Mathf.Clamp(value, 0, 1); } }

        public void Init(float startValue, float alpha = 0.25f)
        {
            _previousData = startValue;
            CurrentData = startValue;
            Alpha = alpha;
        }

        public void Process(float pos)
        {
            CurrentData = Alpha * pos + (1 - Alpha) * _previousData;
            _previousData = CurrentData;
        }
    }
}