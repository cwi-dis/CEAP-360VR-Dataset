//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public interface IUserPositionGuideData
    {
        /// <summary>
        /// Gets the user position guide data for the left eye.
        /// </summary>
        Vector3 LeftEye { get; }

        /// <summary>
        /// Gets the user position guide data for the right eye.
        /// </summary>
        Vector3 RightEye { get; }

        /// <summary>
        /// Gets the validity of the left eye user position guide.
        /// </summary>
        bool LeftEyeValid { get; }

        /// <summary>
        /// Gets the validity of the right eye user position guide.
        /// </summary>
        bool RightEyeValid { get; }
    }
}