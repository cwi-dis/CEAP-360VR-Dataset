// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using Tobii.XR.DevTools;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// A gaze aware toggle button that is interacted with the trigger button on the Vive controller.
    /// </summary>
    [RequireComponent(typeof(DevToolsUIGazeToggleButtonGraphics))]
    public class DevToolsUITriggerGazeToggleButton : MonoBehaviour, IGazeFocusable
    {
        // Property updated when the toggle button has been changed. 
        // Invokes the OnButtonToggled event.
        public bool IsToggledOn
        {
            get { return _isToggledOn; }
            private set
            {
                // Don't update the value if it is the same as the previous.
                if (value == IsToggledOn) return;

                _isToggledOn = value;
                if (OnButtonToggled != null)
                {
                    OnButtonToggled.Invoke(value);
                }

            }
        }

        // Event variables.
        public DevToolsUIToggleEvent OnButtonToggled;

        private bool _isToggledOn;

        // The trigger button on the Vive controller.
        private const DevToolsControllerManager.ControllerButton TriggerButton = DevToolsControllerManager.ControllerButton.Trigger;

        // Haptic strength for the button click.
        private const ushort HapticStrength = 1000;

        // Private fields.
        private bool _hasFocus;

        private bool _buttonPressed;
        private DevToolsUIGazeToggleButtonGraphics _toolkitUiGazeToggleButtonGraphics;

        private void Start()
        {
            // Store the graphics class.
            _toolkitUiGazeToggleButtonGraphics = GetComponent<DevToolsUIGazeToggleButtonGraphics>();

            // Initialize the toggle event.
            if (OnButtonToggled == null)
            {
                OnButtonToggled = new DevToolsUIToggleEvent();
            }
        }

        private void Update()
        {
            // If the interaction button is pressed when the toggle has focus, press the button down.
            if (DevToolsControllerManager.Instance.GetButtonPressDown(TriggerButton) && _hasFocus)
            {
                OnPressedDown();
            }
            // If the interaction button is released.
            if (DevToolsControllerManager.Instance.GetButtonPressUp(TriggerButton))
            {
                // If the interaction button is released from being pressed down, toggle the button.
                if (_buttonPressed)
                {
                    Toggle();
                }

                // Animate the toggle button.
                _toolkitUiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, _isToggledOn,
                    _buttonPressed);
            }
        }

        /// <summary>
        /// Method that updates the visual state of the button to be pressed down.
        /// </summary>
        private void OnPressedDown()
        {
            _buttonPressed = true;

            // Animate the visual feedback. This method will also first stop an animation if it is already running.
            _toolkitUiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, _isToggledOn, _buttonPressed);
        }

        /// <summary>
        /// Method that updates the <see cref="IsToggledOn"/> of the button and updates the visuals to the new state.
        /// </summary>
        private void Toggle()
        {
            _buttonPressed = false;
            IsToggledOn = !IsToggledOn;

            DevToolsControllerManager.Instance.TriggerHapticPulse(HapticStrength);

            // Animate the visual feedback, if an animation is running, stop it first.
            _toolkitUiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, _isToggledOn, _buttonPressed);

            // Move the knob to its new position, stop any running knob movements.
            _toolkitUiGazeToggleButtonGraphics.StartKnobAnimation(IsToggledOn);
        }

        /// <summary>
        /// Toggles the button off, if it isn't already off.
        /// </summary>
        public void ToggleOff()
        {
            // If the toggle button is already off, return.
            if (!IsToggledOn) return;

            Toggle();
        }

        /// <summary>
        /// Toggles the button on, if it isn't already on.
        /// </summary>
        public void ToggleOn()
        {
            // If the toggle button is already on, return.
            if (IsToggledOn) return;

            Toggle();
        }

        /// <summary>
        /// Method called by Tobii XR when the gaze focus changes by implementing <see cref="IGazeFocusable"/>.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
            _hasFocus = hasFocus;

            // Return if the trigger button is pressed down, meaning, when the user has locked on any element, this element shouldn't be highlighted when gazed on.
            if (DevToolsControllerManager.Instance.GetButtonPress(TriggerButton)) return;

            // Update the visual feedback to match gaze focus
            _toolkitUiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, _isToggledOn, _buttonPressed);
        }
    }
}