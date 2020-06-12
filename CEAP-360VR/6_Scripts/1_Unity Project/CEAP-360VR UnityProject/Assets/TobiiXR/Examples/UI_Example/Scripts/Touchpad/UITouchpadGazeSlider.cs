// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// A discrete gaze slider interacted with the touchpad button on the Vive controller.
    /// </summary>
    [RequireComponent(typeof(UIGazeSliderGraphics))]
    public class UITouchpadGazeSlider : MonoBehaviour, IGazeFocusable
    {
        // Property updated when the slider has been changed. 
        // Invokes the OnSliderValueChanged event.
        public int Value
        {
            get { return _value; }
            private set
            {
                // Don't update the value if it is the same as the previous.
                if (Mathf.Approximately(value, _value)) return;

                _value = value;
                if (OnSliderValueChanged != null)
                {
                    OnSliderValueChanged.Invoke(gameObject, value);
                }
            }
        }

        // Event variables.
        public UISliderEvent OnSliderValueChanged;
        private int _value;

        [Header("Functionality")]
        [SerializeField, Tooltip("Multiplier for how much you have to swipe the touchpad to update the slider.")]
        private float _touchPadMultiplier = 1f;

        [SerializeField,
         Tooltip(
             "The minimum value of the slider. A big difference between the min and max value creates a continuous slider.")]
        private int _minValue = 0;

        [SerializeField,
         Tooltip(
             "The maximum value of the slider. A big difference between the min and max value creates a continuous slider.")]
        private int _maxValue = 1;

        [SerializeField, Tooltip("The haptic feedback strength for every step in the slider.")]
        private ushort _hapticStrength = 500;

        // The touchpad button on the Vive controller.
        private const ControllerButton TouchpadButton = ControllerButton.Touchpad;

        // Multiplier to map the touchpad movement distance from left to right to 0 to 1.
        private const float MultiplierToMatchTouchpadMovement = 0.5f;

        // Private fields.
        private bool _hasFocus;
        private bool _debugHasBeenLogged;
        private float _padXLastFrame;
        private int _currentStep;
        private int _stepsToMove;
        private float _incrementedMoveAmount;
        private float _sizePerStep;
        private UIGazeSliderGraphics _sliderGraphics;
        private float _sliderFillAmount;

        private void Start()
        {
            // Get the slider graphics component.
            _sliderGraphics = GetComponent<UIGazeSliderGraphics>();

            // Initialize the slider event.
            if (OnSliderValueChanged == null)
            {
                OnSliderValueChanged = new UISliderEvent();
            }
        }

        private void Update()
        {
            // If the min value is equal or bigger than the max value, the slider won't work and will return.
            if (SliderValueOutsideBounds()) return;

            // If the slider is not being focused on by the user, return.
            if (!_hasFocus) return;

            HandleInput();
        }

        /// <summary>
        /// Handle the user input related to the slider.
        /// </summary>
        private void HandleInput()
        {
            // When the touchpad is first touched, animate the handle and save the current touchpad x value.
            if (ControllerManager.Instance.GetButtonTouchDown(TouchpadButton))
            {
                _padXLastFrame = ControllerManager.Instance.GetTouchpadAxis().x;
                _sliderGraphics.StartHandleAnimation(true);
                return;
            }

            // When the touchpad is being touched.
            if (ControllerManager.Instance.GetButtonTouch(TouchpadButton))
            {
                UpdateSlider();
            }

            // When the touchpad is released.
            if (ControllerManager.Instance.GetButtonTouchUp(TouchpadButton))
            {
                _sliderGraphics.StartHandleAnimation(false);
            }
        }

        /// <summary>
        /// Updates the slider's fill amount, handle position and sets the output value.
        /// </summary>
        private void UpdateSlider()
        {
            var padXCurrentFrame = ControllerManager.Instance.GetTouchpadAxis().x;

            // Delta of the touchpad x-values.
            var padXDelta = padXCurrentFrame - _padXLastFrame;

            // Increment how much the touchpad has been slid.
            _incrementedMoveAmount += padXDelta * MultiplierToMatchTouchpadMovement * _touchPadMultiplier;

            // Don't update the slider if the user is trying to slide outside of the scope.
            if (TryingToSlideOutsideOfScope())
            {
                _incrementedMoveAmount = 0;
                _padXLastFrame = padXCurrentFrame;
                return;
            }

            // Calculate the size per step.
            _sizePerStep = 1f / (_maxValue - _minValue);

            // If the incremented slide amount is bigger than a step on discrete slider, update the slider value.
            if (Mathf.Abs(_incrementedMoveAmount) > _sizePerStep)
            {
                // Determine the number of steps to move.
                _stepsToMove = (int) (_incrementedMoveAmount / _sizePerStep);

                // Reset the value after it has been used to update the current step.
                _incrementedMoveAmount = 0;

                // Updates the current step.
                _currentStep = Mathf.Clamp(_currentStep + _stepsToMove, 0, _maxValue - _minValue);
                _stepsToMove = 0;

                ControllerManager.Instance.TriggerHapticPulse(_hapticStrength);
            }

            // Update the variable holding how much the slider should be filled and set the graphical fill amount.
            _sliderFillAmount = _currentStep * _sizePerStep;
            _sliderGraphics.SetFillAmount(_sliderFillAmount);

            // Calculate the new value and update the value text.
            Value = (int) Mathf.Lerp(_minValue, _maxValue, _sliderFillAmount);
            _sliderGraphics.UpdateValueText(Value);

            _padXLastFrame = padXCurrentFrame;
        }


        /// <summary>
        /// Determines if the user is sliding outside the scope of the slider (e.g., sliding left when at the minimum value).
        /// </summary>
        /// <returns>True if the user is sliding outside the scope.</returns>
        private bool TryingToSlideOutsideOfScope()
        {
            var movingRight = 0 < _incrementedMoveAmount;
            var endOfSlider = _currentStep == _maxValue - _minValue;

            var movingLeft = _incrementedMoveAmount < 0;
            var beginningOfSlider = _currentStep == 0;

            return (movingRight && endOfSlider) || (movingLeft && beginningOfSlider);
        }

        /// <summary>
        /// Method to check if the min and max values are incorrectly set.
        /// </summary>
        /// <returns>True if the max value is less than the min value, otherwise false.</returns>
        private bool SliderValueOutsideBounds()
        {
            if (_maxValue <= _minValue)
            {
                // Log the error once.
                if (!_debugHasBeenLogged)
                {
                    Debug.LogErrorFormat("{0}'s maximum value ({1}) has to be bigger that the minimum value ({2})",
                        gameObject.name, _maxValue, _minValue);
                    _debugHasBeenLogged = true;
                }

                return true;
            }

            _debugHasBeenLogged = false;
            return false;
        }

        /// <summary>
        /// Method called by Tobii XR when the gaze focus changes by implementing <see cref="IGazeFocusable"/>.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
            // Don't use this method if the component is disabled.
            if (!enabled) return;

            _hasFocus = hasFocus;

            // When the user starts focusing on this slider.
            if (hasFocus)
            {
                // Set the first touch pad value.
                _padXLastFrame = ControllerManager.Instance.GetTouchpadAxis().x;

                // If the user is touching the touchpad, animate the slider handle to become visible.
                if (ControllerManager.Instance.GetButtonTouch(TouchpadButton))
                {
                    _sliderGraphics.StartHandleAnimation(true);
                }
            }
            // When the user stops focusing on this slider.
            else
            {
                // Start animating the handle to disappear.
                _sliderGraphics.StartHandleAnimation(false);
            }

            // Animate the visual feedback.
            _sliderGraphics.StartVisualFeedbackAnimation(hasFocus);
        }
    }
}
