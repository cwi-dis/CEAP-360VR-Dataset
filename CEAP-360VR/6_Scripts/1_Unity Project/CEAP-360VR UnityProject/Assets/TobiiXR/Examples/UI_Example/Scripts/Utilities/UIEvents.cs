// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Event for when the button is clicked, containing the game object which .
    /// </summary>
    [Serializable]
    public class UIButtonEvent : UnityEvent<GameObject>
    {
    }

    /// <summary>
    /// Event for when the toggle button is updated.
    /// </summary>
    [Serializable]
    public class UIToggleEvent : UnityEvent<GameObject, bool>
    {
    }

    /// <summary>
    /// Event for when the slider is updated.
    /// </summary>
    [Serializable]
    public class UISliderEvent : UnityEvent<GameObject, int>
    {
    }
}