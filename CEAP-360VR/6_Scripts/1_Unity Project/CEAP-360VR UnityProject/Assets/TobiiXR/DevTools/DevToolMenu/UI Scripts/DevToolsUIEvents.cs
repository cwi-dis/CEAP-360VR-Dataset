// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using UnityEngine.Events;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// Event for when the slider is updated.
    /// </summary>
    [Serializable]
    public class DevToolsUISliderEvent : UnityEvent<int>
    {
    }

    /// <summary>
    /// Event for when the toggle button is updated.
    /// </summary>
    [Serializable]
    public class DevToolsUIToggleEvent : UnityEvent<bool>
    {
    }
}