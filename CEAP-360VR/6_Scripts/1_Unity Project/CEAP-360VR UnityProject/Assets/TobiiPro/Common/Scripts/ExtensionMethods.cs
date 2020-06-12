//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Xml;
using UnityEngine;

namespace Tobii.Research.Unity
{
    internal static class TobiiExtensionMethods
    {
        private static float _lastLcsMm;
        internal const string FORMAT_FLOAT = "0.00000000";

        /// <summary>
        /// Get <see cref="Point3D"/> in unity coordinates.
        /// The sign of x is reversed and millimeters are converted to meters.
        /// </summary>
        /// <param name="value">The <see cref="Point3D"/> value</param>
        /// <returns>The <see cref="Vector3"/> in unity coordinates.</returns>
        public static Vector3 InUnityCoord(this Point3D value)
        {
            return new Vector3(-value.X / 1000f, value.Y / 1000f, value.Z / 1000f);
        }

        /// <summary>
        /// Get <see cref="NormalizedPoint3D"/> in unity coordinates.
        /// The sign of x is reversed.
        /// </summary>
        /// <param name="value">The <see cref="NormalizedPoint3D"/> value</param>
        /// <returns>The <see cref="Vector3"/> in unity coordinates.</returns>
        public static Vector3 InUnityCoord(this NormalizedPoint3D value)
        {
            return new Vector3(-value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Convert <see cref="Point3D"/> to <see cref="Vector3"/> without modification.
        /// </summary>
        /// <param name="value">The <see cref="Point3D"/> value</param>
        /// <returns>The <see cref="Vector3"/> converted value.</returns>
        public static Vector3 ToVector3(this Point3D value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Convert <see cref="NormalizedPoint3D"/> to <see cref="Vector3"/> without modification.
        /// </summary>
        /// <param name="value">The <see cref="NormalizedPoint3D"/> value</param>
        /// <returns>The <see cref="Vector3"/> converted value.</returns>
        public static Vector3 ToVector3(this NormalizedPoint3D value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Convert <see cref="NormalizedPoint2D"/> to <see cref="Vector2"/> without modification.
        /// </summary>
        /// <param name="value">The <see cref="NormalizedPoint2D"/> value</param>
        /// <returns>The <see cref="Vector3"/> converted value.</returns>
        public static Vector2 ToVector2(this NormalizedPoint2D value)
        {
            return new Vector2(value.X, value.Y);
        }

        /// <summary>
        /// Convert <see cref="Vector2"/> to the <see cref="NormalizedPoint2D"/> type without modificatin.
        /// No extra normalization will be done, only a type change.
        /// </summary>
        /// <param name="value">The <see cref="Vector2"/> value.</param>
        /// <returns>The <see cref="NormalizedPoint2D"/> converted value.</returns>
        public static NormalizedPoint2D ToNormalizedPoint2D(this Vector2 value)
        {
            return new NormalizedPoint2D(value.x, value.y);
        }

        /// <summary>
        /// Update lens configuration. Call periodically, not too often, or on changing the Vive IPD.
        /// Avoid calling this at the same time as doing a calibration.
        /// </summary>
        /// <param name="eyeTracker"></param>
        /// <returns>True if lens config was updated, false otherwise.</returns>
        internal static bool UpdateLensConfiguration(this IEyeTracker eyeTracker)
        {
            if (eyeTracker == null || (eyeTracker.DeviceCapabilities & Capabilities.HasHMDLensConfig) == 0)
            {
                return false;
            }

            var lcsMm = VRUtility.LensCupSeparation * 1000f;

            if (lcsMm > 0 && Mathf.Abs(lcsMm - _lastLcsMm) > 0.1f)
            {
                var lensConfig = new HMDLensConfiguration(new Point3D(lcsMm / 2f, 0, 0), new Point3D(lcsMm / -2f, 0, 0));
                eyeTracker.SetHMDLensConfiguration(lensConfig);
                _lastLcsMm = lcsMm;

                return true;
            }

            return false;
        }

        internal static EyeTrackerOriginPose GetPose(this Transform transform, long timeStamp)
        {
            return new EyeTrackerOriginPose(timeStamp, transform);
        }

        internal static Transform ApplyPose(this Transform transform, EyeTrackerOriginPose pose)
        {
            transform.position = pose.Position;
            transform.rotation = pose.Rotation;
            return transform;
        }

        internal static bool Valid(this Validity validity)
        {
            return validity == Validity.Valid;
        }

        #region XML writing

        internal static void WriteWithValid(this XmlWriter file, string name, string valString, bool valid)
        {
            file.WriteStartElement(name);
            file.WriteAttributeString("Value", valString);
            file.WriteAttributeString("Valid", valid.ToString());
            file.WriteEndElement();
        }

        internal static void WriteRay(this XmlWriter file, Ray ray, bool valid, string name)
        {
            file.WriteStartElement(name);
            file.WriteAttributeString("Origin", ray.origin.ToString(FORMAT_FLOAT));
            file.WriteAttributeString("Direction", ray.direction.ToString(FORMAT_FLOAT));
            file.WriteAttributeString("Valid", valid.ToString());
            file.WriteEndElement();
        }

        internal static void Write3D<T>(this XmlWriter file, T point, string elementName, string attributeName, Validity valid) where T : Point3D
        {
            file.WriteStartElement(elementName);
            file.WriteAttributeString(attributeName, string.Format("({0}, {1}, {2})",
                point.X.ToString(FORMAT_FLOAT), point.Y.ToString(FORMAT_FLOAT), point.Z.ToString(FORMAT_FLOAT)));
            file.WriteAttributeString("Validity", valid.ToString());
            file.WriteEndElement();
        }

        internal static void Write2D<T>(this XmlWriter file, T point, string elementName, string attributeName, Validity valid) where T : NormalizedPoint2D
        {
            file.WriteStartElement(elementName);
            file.WriteAttributeString(attributeName, string.Format("({0}, {1})",
                point.X.ToString(FORMAT_FLOAT), point.Y.ToString(FORMAT_FLOAT)));
            file.WriteAttributeString("Validity", valid.ToString());
            file.WriteEndElement();
        }

        internal static void WriteFloat(this XmlWriter file, float val, string elementName, string attributeName, Validity valid)
        {
            file.WriteStartElement(elementName);
            file.WriteAttributeString(attributeName, val.ToString(FORMAT_FLOAT));
            file.WriteAttributeString("Validity", valid.ToString());
            file.WriteEndElement();
        }

        internal static void HMDWritePose(this XmlWriter file, EyeTrackerOriginPose pose)
        {
            file.WriteStartElement("Pose");
            file.WriteAttributeString("Position", pose.Position.ToString(FORMAT_FLOAT));
            file.WriteAttributeString("Rotation", pose.Rotation.eulerAngles.ToString(FORMAT_FLOAT));
            file.WriteAttributeString("Valid", pose.Valid.ToString());
            file.WriteEndElement();
        }

        internal static void HMDWriteEye(this XmlWriter file, IVRGazeDataEye eye, string name)
        {
            file.WriteStartElement(name);
            file.WriteWithValid("GazeDirection", eye.GazeDirection.ToString(FORMAT_FLOAT), eye.GazeDirectionValid);
            file.WriteWithValid("GazeOrigin", eye.GazeOrigin.ToString(FORMAT_FLOAT), eye.GazeOriginValid);
            file.WriteWithValid("PupilDiameter", eye.PupilDiameter.ToString(FORMAT_FLOAT), eye.PupilDiameterValid);
            file.WriteRay(eye.GazeRayWorld, eye.GazeRayWorldValid, "GazeRayWorld");
            file.WriteEndElement();
        }

        internal static void HMDWriteEyeData(this XmlWriter file, HMDEyeData eye, string name)
        {
            file.WriteStartElement(name);
            file.Write3D(eye.GazeDirection.UnitVector, "GazeDirection", "UnitVector", eye.GazeDirection.Validity);
            file.Write3D(eye.GazeOrigin.PositionInHMDCoordinates, "GazeOrigin", "PositionInHMDCoordinates", eye.GazeOrigin.Validity);
            file.WriteFloat(eye.Pupil.PupilDiameter, "Pupil", "PupilDiameter", eye.Pupil.Validity);
            file.Write2D(eye.PupilPosition.PositionInTrackingArea, "PupilPosition", "PositionInTrackingArea", eye.PupilPosition.Validity);
            file.WriteEndElement();
        }

        internal static void HMDWriteRawGaze(this XmlWriter file, HMDGazeDataEventArgs originalGaze)
        {
            file.WriteStartElement("OriginalGaze");
            file.WriteAttributeString("DeviceTimeStamp", originalGaze.DeviceTimeStamp.ToString());
            file.WriteAttributeString("SystemTimeStamp", originalGaze.SystemTimeStamp.ToString());
            file.HMDWriteEyeData(originalGaze.LeftEye, "LeftEye");
            file.HMDWriteEyeData(originalGaze.RightEye, "RightEye");
            file.WriteEndElement();
        }

        internal static void WriteEye(this XmlWriter file, IGazeDataEye eye, string name)
        {
            file.WriteStartElement(name);
            file.WriteWithValid("GazeOriginInTrackBoxCoordinates", eye.GazeOriginInTrackBoxCoordinates.ToString(FORMAT_FLOAT), eye.GazeOriginValid);
            file.WriteWithValid("GazeOriginInUserCoordinates", eye.GazeOriginInUserCoordinates.ToString(FORMAT_FLOAT), eye.GazeOriginValid);
            file.WriteWithValid("GazePointInUserCoordinates", eye.GazePointInUserCoordinates.ToString(FORMAT_FLOAT), eye.GazePointValid);
            file.WriteWithValid("GazePointOnDisplayArea", eye.GazePointOnDisplayArea.ToString(FORMAT_FLOAT), eye.GazePointValid);
            file.WriteRay(eye.GazeRayScreen, eye.GazePointValid, "GazeRayScreen");
            file.WriteWithValid("PupilDiameter", eye.PupilDiameter.ToString(FORMAT_FLOAT), eye.PupilDiameterValid);
            file.WriteEndElement();
        }

        internal static void WriteEyeData(this XmlWriter file, EyeData eye, string name)
        {
            file.WriteStartElement(name);
            file.Write3D(eye.GazeOrigin.PositionInTrackBoxCoordinates, "GazeOrigin", "PositionInTrackBoxCoordinates", eye.GazeOrigin.Validity);
            file.Write3D(eye.GazeOrigin.PositionInUserCoordinates, "GazeOrigin", "PositionInUserCoordinates", eye.GazeOrigin.Validity);
            file.Write3D(eye.GazePoint.PositionInUserCoordinates, "GazePoint", "PositionInUserCoordinates", eye.GazePoint.Validity);
            file.Write2D(eye.GazePoint.PositionOnDisplayArea, "GazePoint", "PositionOnDisplayArea", eye.GazePoint.Validity);
            file.WriteFloat(eye.Pupil.PupilDiameter, "Pupil", "PupilDiameter", eye.Pupil.Validity);
            file.WriteEndElement();
        }

        internal static void WriteRawGaze(this XmlWriter file, GazeDataEventArgs originalGaze)
        {
            file.WriteStartElement("OriginalGaze");
            file.WriteAttributeString("DeviceTimeStamp", originalGaze.DeviceTimeStamp.ToString());
            file.WriteAttributeString("SystemTimeStamp", originalGaze.SystemTimeStamp.ToString());
            file.WriteEyeData(originalGaze.LeftEye, "LeftEye");
            file.WriteEyeData(originalGaze.RightEye, "RightEye");
            file.WriteEndElement();
        }

        #endregion XML writing
    }
}