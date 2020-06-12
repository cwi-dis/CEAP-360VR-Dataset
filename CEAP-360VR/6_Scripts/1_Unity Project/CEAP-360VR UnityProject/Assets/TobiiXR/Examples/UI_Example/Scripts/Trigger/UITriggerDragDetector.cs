// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;
using UnityEngine.Events;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Detects if the user has looked at this element, pressed the trigger button on the Vive controller
    /// and then moved the controller left or right a certain threshold.
    /// </summary>
    public class UITriggerDragDetector : MonoBehaviour, IGazeFocusable
    {
        // Events.
        public UnityEvent DragLeft;
        public UnityEvent DragRight;

        // The trigger button on the Vive controller.
        private const ControllerButton TriggerButton = ControllerButton.Trigger;

        // The amount needed to invoke a drag event (relative to this UI element).
        private const float MoveAmountToInvokeDragEvent = 0.02f;

        // Privates fields.
        private bool _hasFocus;
        private bool _buttonPressed;
        private float _xScaleLossy;
        private float _controllerMoveAmount;

        // Use this for initialization
        private void Start()
        {
            // Initialize the events.
            if (DragLeft == null)
            {
                DragLeft = new UnityEvent();
            }

            if (DragRight == null)
            {
                DragRight = new UnityEvent();
            }

            // Store the global scale in x.
            _xScaleLossy = transform.lossyScale.x;
        }

        private void Update()
        {
            // If the trigger button is pressed when the toggle button has focus, press the button down.
            if (ControllerManager.Instance.GetButtonPressDown(TriggerButton) && _hasFocus)
            {
                _buttonPressed = true;
                _controllerMoveAmount = 0f;
                return;
            }

            // If the button is being pressed, check to see whether a drag event should be invoked.
            if (_buttonPressed)
            {
                CheckForTriggerDragEvent();
            }

            // If the trigger button is released when the toggle is pressed, perform a click.
            if (ControllerManager.Instance.GetButtonPressUp(TriggerButton))
            {
                _buttonPressed = false;
            }
        }

        /// <summary>
        /// Checks if the controller have moved more than <see cref="MoveAmountToInvokeDragEvent"/> and then trigger a drag event.
        /// </summary>
        private void CheckForTriggerDragEvent()
        {
            _controllerMoveAmount += GetRelativeControllerMovement().x;

            // If the total move amount is above the movement needed to invoke a drag event.
            if (Mathf.Abs(_controllerMoveAmount) > MoveAmountToInvokeDragEvent)
            {
                // If the controller has been moved right, invoke the drag right event.
                if (_controllerMoveAmount > 0)
                {
                    if (DragRight != null)
                    {
                        DragRight.Invoke();
                    }
                }
                // If the controller has been moved left, invoke the drag left event.

                if (_controllerMoveAmount < 0)
                {
                    if (DragLeft != null)
                    {
                        DragLeft.Invoke();
                    }
                }

                _controllerMoveAmount = 0f;
            }
        }

        /// <summary>
        /// Returns the distance the controller has traveled between the current and the last frame relative to this element.
        /// </summary>
        /// <returns>The relative controller movement.</returns>
        private Vector3 GetRelativeControllerMovement()
        {
            return transform.InverseTransformVector(ControllerManager.Instance.Velocity) * _xScaleLossy *
                   Time.deltaTime;
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
        }
    }
}
