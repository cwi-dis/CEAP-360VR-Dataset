// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using Tobii.XR.DevTools;
using UnityEngine;
using UnityEngine.Events;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// A gaze aware button that is interacted with the trigger button on the Vive controller.
    /// </summary>
    [RequireComponent(typeof(DevToolsUIGazeButtonGraphics))]
    public class DevToolsUITriggerGazeButton : MonoBehaviour, IGazeFocusable
    {
        // Event called when the button is clicked.
        public UnityEvent OnButtonClicked;

        // The trigger button on the Vive controller.
        private const DevToolsControllerManager.ControllerButton TriggerButton =
            DevToolsControllerManager.ControllerButton.Trigger;

        // Haptic strength for the button click.
        private const ushort HapticStrength = 1000;

        // The state the button is currently  in.
        private ButtonState _currentButtonState = ButtonState.Idle;

        // Private fields.
        private bool _hasFocus;

        private DevToolsUIGazeButtonGraphics _toolkitUiGazeButtonGraphics;

        void Start()
        {
            // Store the graphics class.
            _toolkitUiGazeButtonGraphics = GetComponent<DevToolsUIGazeButtonGraphics>();

            // Initialize click event.
            if (OnButtonClicked == null)
            {
                OnButtonClicked = new UnityEvent();
            }
        }

        private void Update()
        {
            // When the button is being focused and the interaction button is pressed down, set the button to the PressedDown state.
            if (_currentButtonState == ButtonState.Focused &&
                DevToolsControllerManager.Instance.GetButtonPressDown(TriggerButton))
            {
                UpdateState(ButtonState.PressedDown);
            }
            // When the trigger button is released.
            else if (DevToolsControllerManager.Instance.GetButtonPressUp(TriggerButton))
            {
                // Invoke a button click event if this button has been released from a PressedDown state.
                if (_currentButtonState == ButtonState.PressedDown)
                {
                    // Invoke click event.
                    if (OnButtonClicked != null)
                    {
                        OnButtonClicked.Invoke();
                    }

                    DevToolsControllerManager.Instance.TriggerHapticPulse(HapticStrength);
                }

                // Set the state depending on if it has focus or not.
                UpdateState(_hasFocus ? ButtonState.Focused : ButtonState.Idle);
            }
        }

        /// <summary>
        /// Updates the button state and starts an animation of the button.
        /// </summary>
        /// <param name="newState">The state the button should transition to.</param>
        private void UpdateState(ButtonState newState)
        {
            var oldState = _currentButtonState;
            _currentButtonState = newState;

            // Variables for when the button is pressed or clicked.
            var buttonPressed = newState == ButtonState.PressedDown;
            var buttonClicked = (oldState == ButtonState.PressedDown && newState == ButtonState.Focused);

            // If the button is being pressed down or clicked, animate the button click.
            if (buttonPressed || buttonClicked)
            {
                _toolkitUiGazeButtonGraphics.AnimateButtonPress(_currentButtonState);
            }
            // In all other cases, animate the visual feedback.
            else
            {
                _toolkitUiGazeButtonGraphics.AnimateButtonVisualFeedback(_currentButtonState);
            }
        }

        /// <summary>
        /// Method called by Tobii XR when the gaze focus changes by implementing <see cref="IGazeFocusable"/>.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
            if (!enabled)
                return;

            _hasFocus = hasFocus;

            // Return if the trigger button is pressed down, meaning, when the user has locked on any element, this element shouldn't be highlighted when gazed on.
            if (DevToolsControllerManager.Instance.GetButtonPress(TriggerButton)) return;

            UpdateState(hasFocus ? ButtonState.Focused : ButtonState.Idle);
        }
    }
}
