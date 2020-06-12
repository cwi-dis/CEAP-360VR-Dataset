//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public sealed class VRGazeData : IVRGazeData
    {
        public long TimeStamp { get; private set; }

        public EyeTrackerOriginPose Pose { get; private set; }

        public IVRGazeDataEye Left { get; private set; }

        public IVRGazeDataEye Right { get; private set; }

        public Ray CombinedGazeRayWorld { get; private set; }

        public bool CombinedGazeRayWorldValid { get; private set; }

        public HMDGazeDataEventArgs OriginalGaze { get; private set; }

        internal VRGazeData(HMDGazeDataEventArgs originalGaze, EyeTrackerOriginPose pose)
        {
            TimeStamp = originalGaze.SystemTimeStamp;
            Pose = pose;

            var eyeTrackerOrigin = VRUtility.TemporaryTransformWithAppliedPose(pose);
            Left = new VRGazeDataEye(originalGaze.LeftEye, eyeTrackerOrigin);
            Right = new VRGazeDataEye(originalGaze.RightEye, eyeTrackerOrigin);

            var combinedDirection = ((Left.GazeDirection + Right.GazeDirection) / 2f).normalized;
            var combinedOrigin = (Left.GazeOrigin + Right.GazeOrigin) / 2f;

            CombinedGazeRayWorld = new Ray(eyeTrackerOrigin.TransformPoint(combinedOrigin), eyeTrackerOrigin.TransformDirection(combinedDirection));
            CombinedGazeRayWorldValid = Left.GazeRayWorldValid && Right.GazeRayWorldValid;

            OriginalGaze = originalGaze;
        }

        internal VRGazeData()
        {
            Left = new VRGazeDataEye();
            Right = new VRGazeDataEye();
        }
    }

    public sealed class VRGazeDataEye : IVRGazeDataEye
    {
        public Vector3 GazeDirection { get; private set; }

        public bool GazeDirectionValid { get; private set; }

        public Vector3 GazeOrigin { get; private set; }

        public bool GazeOriginValid { get; private set; }

        public float PupilDiameter { get; private set; }

        public bool PupilDiameterValid { get; private set; }

        public Ray GazeRayWorld { get; private set; }

        public bool GazeRayWorldValid { get; private set; }

        public Vector2 PupilPosiitionInTrackingArea { get; private set; }

        public bool PupilPosiitionInTrackingAreaValid { get; private set; }

        internal VRGazeDataEye(HMDEyeData eye, Transform eyeTrackerOrigin)
        {
            GazeDirection = eye.GazeDirection.UnitVector.InUnityCoord();
            GazeDirectionValid = eye.GazeDirection.Validity.Valid();

            GazeOrigin = eye.GazeOrigin.PositionInHMDCoordinates.InUnityCoord();
            GazeOriginValid = eye.GazeOrigin.Validity.Valid();

            PupilDiameter = eye.Pupil.PupilDiameter / 1000f;
            PupilDiameterValid = eye.Pupil.Validity.Valid();

            GazeRayWorld = new Ray(eyeTrackerOrigin.TransformPoint(GazeOrigin), eyeTrackerOrigin.TransformDirection(GazeDirection));
            GazeRayWorldValid = GazeDirectionValid && GazeOriginValid;

            PupilPosiitionInTrackingArea = new Vector2(eye.PupilPosition.PositionInTrackingArea.X, eye.PupilPosition.PositionInTrackingArea.Y);
            PupilPosiitionInTrackingAreaValid = eye.PupilPosition.Validity.Valid();
        }

        internal VRGazeDataEye()
        {
            GazeDirection = Vector3.zero;
            GazeDirectionValid = false;

            GazeOrigin = Vector3.zero;
            GazeOriginValid = false;

            PupilDiameter = 0;
            PupilDiameterValid = false;

            GazeRayWorld = new Ray();
            GazeRayWorldValid = false;

            PupilPosiitionInTrackingArea = Vector2.zero;
            PupilPosiitionInTrackingAreaValid = false;
        }
    }

    /// <summary>
    /// Struct to hold the eye tracker origin position and rotation.
    /// </summary>
    public struct EyeTrackerOriginPose : IComparable<EyeTrackerOriginPose>
    {
        public long TimeStamp { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public bool Valid { get; private set; }

        internal EyeTrackerOriginPose(long timeStamp, Vector3 position, Quaternion rotation)
        {
            TimeStamp = timeStamp;
            Position = position;
            Rotation = rotation;
            Valid = true;
        }

        internal EyeTrackerOriginPose(long timeStamp, Transform transform) : this(timeStamp, transform.position, transform.rotation)
        {
        }

        internal EyeTrackerOriginPose(long timeStamp) : this(timeStamp, Vector3.zero, Quaternion.identity)
        {
            Valid = false;
        }

        public int CompareTo(EyeTrackerOriginPose other)
        {
            return TimeStamp.CompareTo(other.TimeStamp);
        }

        /// <summary>
        /// Get an interpolated pose based on an in between time stamp.
        /// </summary>
        /// <param name="laterPose">The pose later in time</param>
        /// <param name="timeStamp">A time stamp in between the two poses</param>
        /// <returns>The interpolated pose</returns>
        internal EyeTrackerOriginPose Interpolate(EyeTrackerOriginPose laterPose, long timeStamp)
        {
            var ratio = (float)(timeStamp - TimeStamp) / (float)(laterPose.TimeStamp - TimeStamp);
            return new EyeTrackerOriginPose(timeStamp, Vector3.Lerp(Position, laterPose.Position, ratio), Quaternion.Lerp(Rotation, laterPose.Rotation, ratio));
        }

        public override string ToString()
        {
            return string.Format("TS: {0}, Pos: {1}, Rot: {2}, Valid: {3}", TimeStamp, Position, Rotation.eulerAngles, Valid);
        }
    }
}
