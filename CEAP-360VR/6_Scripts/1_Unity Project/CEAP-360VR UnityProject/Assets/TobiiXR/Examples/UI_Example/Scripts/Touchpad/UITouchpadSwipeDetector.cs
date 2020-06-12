// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;
using UnityEngine.Events;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Checks if the user swipes left or right on the touchpad when this game object is being focused.
    /// </summary>
    public class UITouchpadSwipeDetector : MonoBehaviour, IGazeFocusable
    {
        // Events.
        public UnityEvent SwipeLeft;
        public UnityEvent SwipeRight;

        // The touchpad button on the Vive controller.
        private const ControllerButton TouchpadButton = ControllerButton.Touchpad;

        // Multiplier to map the touchpad movement distance from left to right to 0 to 1.
        private const float MultiplierToMatchTouchpadMovement = 0.5f;

        // Amount needed to swipe on the touchpad to invoke a trigger a swipe event (0 to 1).
        private const float TouchpadSwipeAmountToInvokeEvent = 0.2f;

        // Private fields.
        private float _padXLastFrame;
        private float _incrementedDelta;
        private bool _hasFocus;

        void Start()
        {
            // Initialize the swipe events.
            if (SwipeLeft == null)
            {
                SwipeLeft = new UnityEvent();
            }

            if (SwipeRight == null)
            {
                SwipeRight = new UnityEvent();
            }
        }

        void Update()
        {
            // If this game object is not being focused, return.
            if (!_hasFocus) return;

            // When the touchpad is first touched, save the current touchpad x value and set the delta to 0.
            if (ControllerManager.Instance.GetButtonTouchDown(TouchpadButton))
            {
                _padXLastFrame = ControllerManager.Instance.GetTouchpadAxis().x;
                _incrementedDelta = 0f;
                return;
            }

            if (ControllerManager.Instance.GetButtonTouch(TouchpadButton))
            {
                // Get the touchpad axis value for x.
                var padXCurrentFrame = ControllerManager.Instance.GetTouchpadAxis().x;

                CheckForSwipe(padXCurrentFrame);

                _padXLastFrame = padXCurrentFrame;
            }
        }

        /// <summary>
        /// Triggers a right or a left swipe event if the user have slid enough on the touchpad.
        /// </summary>
        /// <param name="padXCurrentFrame"></param>
        private void CheckForSwipe(float padXCurrentFrame)
        {
            // Update the amount slid on the touchpad.
            var padXDelta = padXCurrentFrame - _padXLastFrame;
            _incrementedDelta += padXDelta * MultiplierToMatchTouchpadMovement;

            // If the incremented amount slid on the touchpad is enough to count as a swipe.
            if (Mathf.Abs(_incrementedDelta) > TouchpadSwipeAmountToInvokeEvent)
            {
                // If the user have slid to the left, invoke the left swipe event.
                if (_incrementedDelta < 0)
                {
                    if (SwipeLeft != null)
                    {
                        SwipeLeft.Invoke();
                    }
                }
                // If the user have slid to the right, invoke the right swipe event.
                else if (_incrementedDelta > 0)
                {
                    if (SwipeRight != null)
                    {
                        SwipeRight.Invoke();
                    }
                }

                _incrementedDelta = 0f;
            }
        }


        /// <summary>
        /// Method called by Tobii XR when the gaze focus changes by implementing <see cref="IGazeFocusable"/>.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
            // If the component is disabled, do nothing.
            if (!enabled) return;

            _hasFocus = hasFocus;

            // If the user starts focusing at the slider, set the first touch pad value.
            if (hasFocus)
            {
                _padXLastFrame = ControllerManager.Instance.GetTouchpadAxis().x;
            }
        }
    }
}
