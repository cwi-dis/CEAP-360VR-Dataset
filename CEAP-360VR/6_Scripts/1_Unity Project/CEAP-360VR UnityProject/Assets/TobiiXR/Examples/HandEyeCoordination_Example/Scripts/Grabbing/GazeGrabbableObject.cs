// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Monobehaviour which can be put on objects to allow the user to look at it and grab it with <see cref="GazeGrab"/>.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(GazeOutline))]
    public class GazeGrabbableObject : MonoBehaviour, IGazeFocusable
    {
        [SerializeField, Tooltip("Time in seconds for the object to fly to controller.")]
        private float _flyToControllerTimeSeconds = 0.2f;

        public float FlyToControllerTimeSeconds
        {
            get { return _flyToControllerTimeSeconds; }
        }

        [SerializeField, Tooltip("The animation curve of how the object flies to the controller.")]
        private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public AnimationCurve AnimationCurve
        {
            get { return _animationCurve; }
        }

        private float _gazeStickinessSeconds = 0.1f;

        public float GazeStickinessSeconds
        {
            get { return _gazeStickinessSeconds; }
        }

        private GazeOutline _gazeOutline;

        private void Start()
        {
            _gazeOutline = GetComponent<GazeOutline>();
            _gazeOutline.GazeStickinessSeconds = _gazeStickinessSeconds;
        }

        /// <summary>
        /// Called by TobiiXR when the object receives focus.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
        }

        /// <summary>
        /// Called by <see cref="GazeGrab"/> when the object is flying towards the hand.
        /// </summary>
        public void ObjectGrabbing()
        {
            // Disable the highlight when the object is being grabbed to show
            // that it can no longer be interacted with using gaze.
            _gazeOutline.DisableHighlight();
        }

        /// <summary>
        /// Called by <see cref="GazeGrab"/> when the object has been grabbed to the hand.
        /// </summary>
        public void ObjectGrabbed()
        {
        }


        /// <summary>
        /// Called by <see cref="GazeGrab"/> when the object has been ungrabbed.
        /// </summary>
        public void ObjectUngrabbed()
        {
            // Enable the highlight to show that it can be interacted with using gaze.
            _gazeOutline.EnableOutline();
        }
    }
}
