//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    /// <summary>
    /// A gaze data object contains the gaze data in Unity coordinates.
    /// </summary>
    public interface IGazeData
    {
        /// <summary>
        /// Data for the left eye in Unity coordinates.
        /// </summary>
        IGazeDataEye Left { get; }

        /// <summary>
        /// Data for the right eye in Unity coordinates.
        /// </summary>
        IGazeDataEye Right { get; }

        /// <summary>
        /// The combined <see cref="Ray"/> from the screen point into the scene.
        /// Based on the combined gaze points of the eyes. If one or both of the
        /// gaze points are invalid, default(Ray) will be returned.
        /// </summary>
        Ray CombinedGazeRayScreen { get; }

        /// <summary>
        /// The validity of the combined gaze ray. True is valid.
        /// </summary>
        bool CombinedGazeRayScreenValid { get; }

        /// <summary>
        /// A reference to the unprocessed gaze data received from the eye tracker.
        /// </summary>
        GazeDataEventArgs OriginalGaze { get; }

        /// <summary>
        /// Tobii system time stamp for the data.
        /// </summary>
        long TimeStamp { get; }
    }
}
