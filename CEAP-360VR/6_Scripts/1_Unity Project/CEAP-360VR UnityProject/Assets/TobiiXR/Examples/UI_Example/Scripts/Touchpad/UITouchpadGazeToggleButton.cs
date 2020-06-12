// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// A gaze aware toggle button that is interacted with the touchpad button on the Vive controller.
    /// </summary>
    [RequireComponent(typeof(UIGazeToggleButtonGraphics))]
    public class UITouchpadGazeToggleButton : MonoBehaviour, IGazeFocusable
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
                    OnButtonToggled.Invoke(gameObject, value);
                }
            }
        }

        // Event variables.
        public UIToggleEvent OnButtonToggled;
        private bool _isToggledOn;

        // The touchpad button on the Vive controller.
        private const ControllerButton TouchpadButton = ControllerButton.Touchpad;

        // Haptic strength for the button click.
        private const ushort HapticStrength = 1000;

        // Private fields.
        private bool _hasFocus;
        private bool _buttonPressed;
        private UIGazeToggleButtonGraphics _uiGazeToggleButtonGraphics;
        private bool _initialized;

        private void Awake()
        {
            if (!_initialized)
                Initialize();
        }

        private void Update()
        {
            // If the touchpad button is pressed when the toggle has focus, press the button down.
            if (ControllerManager.Instance.GetButtonPressDown(TouchpadButton) && _hasFocus)
            {
                OnPressedDown();
            }

            // If the touchpad button is released when the toggle is pressed, perform a click.
            if (ControllerManager.Instance.GetButtonPressUp(TouchpadButton) && _buttonPressed)
            {
                Toggle();
            }
        }

        /// <summary>
        /// Initialize the graphics component and the toggle listener.
        /// </summary>
        private void Initialize()
        {
            // Store the graphics class.
            _uiGazeToggleButtonGraphics = GetComponent<UIGazeToggleButtonGraphics>();

            // Initialize the toggle event.
            if (OnButtonToggled == null)
            {
                OnButtonToggled = new UIToggleEvent();
            }

            _initialized = true;
        }

        /// <summary>
        /// Method that updates the visual state of the button to be pressed down.
        /// </summary>
        private void OnPressedDown()
        {
            _buttonPressed = true;

            // Animate the visual feedback. This method will also first stop an animation if it is already running.
            _uiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, IsToggledOn, _buttonPressed);
        }

        /// <summary>
        /// Method that updates the <see cref="IsToggledOn"/> of the button and updates the visuals to the new state.
        /// </summary>
        private void Toggle()
        {
            if (!_initialized)
                Initialize();

            _buttonPressed = false;
            IsToggledOn = !IsToggledOn;

            ControllerManager.Instance.TriggerHapticPulse(HapticStrength);

            // Animate the visual feedback, if an animation is running, stop it first.
            _uiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, IsToggledOn, _buttonPressed);

            // Move the knob to its new position, stop any running knob movements.
            _uiGazeToggleButtonGraphics.StartKnobAnimation(IsToggledOn);
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
            // If the component is disabled, do nothing.
            if (!enabled) return;

            _hasFocus = hasFocus;

            // Cancel the button press if the user stops to focus on the button and it is pressed.
            if (_buttonPressed && !hasFocus)
            {
                _buttonPressed = false;
            }

            // Update the visual feedback to match gaze focus.
            _uiGazeToggleButtonGraphics.StartVisualFeedbackAnimation(_hasFocus, IsToggledOn, _buttonPressed);
        }
    }
}
