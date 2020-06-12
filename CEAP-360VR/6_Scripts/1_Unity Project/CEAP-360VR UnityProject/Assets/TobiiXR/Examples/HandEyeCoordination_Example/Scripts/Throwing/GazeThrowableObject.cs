// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using Tobii.G2OM;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Add this to an object to allow it to use gaze assisted throwing when used with <see cref="ThrowAtGaze"/>.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class GazeThrowableObject : MonoBehaviour, IGazeFocusable
    {
        public float LowerVelocityMultiplier
        {
            get { return _lowerVelocityMultiplier; }
        }

        public float UpperVelocityMultiplier
        {
            get { return _upperVelocityMultiplier; }
        }

        public float MaxYVelocityModifier
        {
            get { return _maxYVelocityModifier; }
        }

        public float XzAngleThresholdDegrees
        {
            get { return _xzAngleThresholdDegrees; }
        }

        [Header("Customization for throwing adjustments")]
        [SerializeField,
         Tooltip(
             "Multiplier for how much the lower end of the allowed difference from original throw to adjusted throw velocity.")]
        private float _lowerVelocityMultiplier = 0.8f;

        [SerializeField,
         Tooltip(
             "Multiplier for how much the upper end of the allowed difference from original throw to adjusted throw velocity.")]
        private float _upperVelocityMultiplier = 2f;

        [SerializeField,
         Tooltip(
             "Multiplier for how much the y component of the throw can be modified from original throw to adjusted throw velocity.")]
        private float _maxYVelocityModifier = 1.0f;

        [SerializeField,
         Tooltip(
             "How many degrees left or right of the target the user is allowed to throw until it no longer adjusts.")]
        private float _xzAngleThresholdDegrees = 45.0f;

        public void GazeFocusChanged(bool hasFocus)
        {
        }
    }
}
