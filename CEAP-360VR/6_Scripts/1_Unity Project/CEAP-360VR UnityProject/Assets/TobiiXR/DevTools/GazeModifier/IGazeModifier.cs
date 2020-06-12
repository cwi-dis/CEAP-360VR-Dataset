// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// Responsible for modifying TobiiXR_EyetrackingData
    /// </summary>
    public interface IGazeModifier
    {
        void Modify(TobiiXR_EyeTrackingData data, Vector3 forward);
    }
}
