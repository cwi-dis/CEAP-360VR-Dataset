// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR
{
    public struct TobiiXR_GazeRay
    {
        /// <summary>
        /// Vector in world space with the gaze direction.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// IsValid is true when there is available gaze data.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// The middle of the eyes in world space.
        /// </summary>
        public Vector3 Origin;
    }

    /// <summary>
    /// Stores eye data.
    /// </summary>
    public class TobiiXR_EyeTrackingData
    {
        /// <summary>
        /// Timestamp when the package was received measured in seconds from the application started.
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// Convergence distance is the distance in meters from the middle of your eyes to the point where the gaze of your eyes meet.
        /// </summary>
        public float ConvergenceDistance;

        /// <summary>
        /// This flag is true when the Convergence Distance value can be used.
        /// </summary>
        public bool ConvergenceDistanceIsValid;

        /// <summary>
        /// Stores gaze data.
        /// </summary>
        public TobiiXR_GazeRay GazeRay;

        /// <summary>
        /// Flag for closed left eye.
        /// </summary>
        public bool IsLeftEyeBlinking;

        /// <summary>
        /// Flag for closed right eye.
        /// </summary>
        public bool IsRightEyeBlinking;
    }
}