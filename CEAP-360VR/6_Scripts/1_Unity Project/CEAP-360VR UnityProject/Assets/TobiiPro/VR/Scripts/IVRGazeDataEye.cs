//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    /// <summary>
    /// A per-eye object that contains gaze origin and direction local to
    /// the eye tracker, and pupil size in meters.
    /// </summary>
    public interface IVRGazeDataEye
    {
        /// <summary>
        /// The normalized gaze direction for this eye.
        /// </summary>
        Vector3 GazeDirection { get; }

        /// <summary>
        /// The validity of the gaze direction. True is valid.
        /// </summary>
        bool GazeDirectionValid { get; }

        /// <summary>
        /// The gaze origin of this eye local to the eye tracker origin
        /// </summary>
        Vector3 GazeOrigin { get; }

        /// <summary>
        /// The validity of the gaze origin. True is valid.
        /// </summary>
        bool GazeOriginValid { get; }

        /// <summary>
        /// The gaze ray for this eye in world coordinates.
        /// </summary>
        Ray GazeRayWorld { get; }

        /// <summary>
        /// The validity of the gaze ray. True is valid.
        /// </summary>
        bool GazeRayWorldValid { get; }

        /// <summary>
        /// The pupil diameter in meters for this eye.
        /// </summary>
        float PupilDiameter { get; }

        /// <summary>
        /// The validity of the pupil diameter. True is valid.
        /// </summary>
        bool PupilDiameterValid { get; }

        /// <summary>
        /// The normalized pupil position in the sensor area. (0, 0) is the top left,
        /// and (1, 1) is the bottom right of sensor area, from the sensor’s perspective
        /// </summary>
        Vector2 PupilPosiitionInTrackingArea { get; }

        /// <summary>
        /// The validity of the pupil position in the sensor area. True is valid.
        /// </summary>
        bool PupilPosiitionInTrackingAreaValid { get; }
    }
}
