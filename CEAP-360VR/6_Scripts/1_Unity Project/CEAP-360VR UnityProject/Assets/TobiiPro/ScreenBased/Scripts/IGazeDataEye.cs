//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    /// <summary>
    /// A per-eye object that contains gaze origin and gaze point
    /// in unity types.
    /// </summary>
    public interface IGazeDataEye
    {
        /// <summary>
        /// Gets the gaze origin position in 3D in the user coordinate system.
        /// </summary>
        Vector3 GazeOriginInUserCoordinates { get; }

        /// <summary>
        /// Gets normalized gaze origin in track box coordinate system.
        /// </summary>
        Vector3 GazeOriginInTrackBoxCoordinates { get; }

        /// <summary>
        /// Gets the validity of the gaze origin data.
        /// </summary>
        bool GazeOriginValid { get; }

        /// <summary>
        /// Gets the gaze point position in 3D in the user coordinate system.
        /// </summary>
        Vector3 GazePointInUserCoordinates { get; }

        /// <summary>
        /// Gets the gaze point position in 2D on the active display area.
        /// </summary>
        Vector2 GazePointOnDisplayArea { get; }

        /// <summary>
        /// Gets the validity of the gaze origin data.
        /// </summary>
        bool GazePointValid { get; }

        /// <summary>
        /// Get the <see cref="Ray"/> from the screen point into the scene.
        /// If the gaze point is invalid, default(Ray) will be returned.
        /// </summary>
        Ray GazeRayScreen { get; }

        /// <summary>
        /// Gets the diameter of the pupil in millimeters.
        /// </summary>
        float PupilDiameter { get; }

        /// <summary>
        /// Gets the validity of the pupil data.
        /// </summary>
        bool PupilDiameterValid { get; }
    }
}
