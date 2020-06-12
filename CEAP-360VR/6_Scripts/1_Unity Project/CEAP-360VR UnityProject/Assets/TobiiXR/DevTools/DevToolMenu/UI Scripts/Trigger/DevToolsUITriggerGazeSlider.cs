// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using Tobii.XR.DevTools;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// A discrete gaze slider interacted with the trigger button on the Vive controller.
    /// </summary>
    [RequireComponent(typeof(DevToolsUIGazeSliderGraphics))]
    public class DevToolsUITriggerGazeSlider : MonoBehaviour, IGazeFocusable
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
                    OnSliderValueChanged.Invoke(value);
                }
            }
        }

        // Event variables.
        public DevToolsUISliderEvent OnSliderValueChanged;

        private int _value;

        [Header("Functionality")]
        [SerializeField, Tooltip("Multiplier for how much movement of the controller is needed to update the slider.")]
        private float _controllerMovementMultiplier = 7f;

        [SerializeField, Tooltip("The minimum value of the slider.")] private int _minValue = 0;
        [SerializeField, Tooltip("The maximum value of the slider.")] private int _maxValue = 1;

        [SerializeField, Tooltip("How strong the haptic feedback is for when updating the slider")]
        private ushort _hapticStrength = 500;

        // The trigger button on the Vive controller.
        private const DevToolsControllerManager.ControllerButton TriggerButton = DevToolsControllerManager.ControllerButton.Trigger;

        // Private fields.
        private bool _debugHasBeenLogged;

        private int _currentStep;
        private int _stepsToMove;
        private float _incrementedMoveAmount;
        private float _sizePerStep;
        private bool _hasFocus;
        private bool _operatingSlider;
        private float _xScaleLossy;
        [HideInInspector] public DevToolsUIGazeSliderGraphics _sliderGraphics;
        private float _sliderFillAmount;

        private void Start()
        {
            // Store the graphics class.
            _sliderGraphics = GetComponent<DevToolsUIGazeSliderGraphics>();

            // Store the global scale in x.
            _xScaleLossy = transform.lossyScale.x;

            // Intitialize the slider event.
            if (OnSliderValueChanged == null)
            {
                OnSliderValueChanged = new DevToolsUISliderEvent();
            }
        }

        private void Update()
        {
            // If the min value is equal or bigger than the max value, the slider won't work and will return.
            if (SliderValueOutsideBounds()) return;

            HandleInput();
        }

        /// <summary>
        /// Method for handling the user input.
        /// </summary>
        private void HandleInput()
        {
            // If the trigger button is pressed down when focusing on the slider.
            if (DevToolsControllerManager.Instance.GetButtonPressDown(TriggerButton) && _hasFocus)
            {
                _operatingSlider = true;
                _sliderGraphics.StartHandleAnimation(true);
                return;
            }

            // Update the slider as long as the trigger button is being pressed down.
            if (_operatingSlider)
            {
                UpdateSlider();
            }

            // When the trigger button is released, stop operating the slider.
            if (DevToolsControllerManager.Instance.GetButtonPressUp(TriggerButton))
            {
                _operatingSlider = false;
                _sliderGraphics.StartHandleAnimation(false);
                _sliderGraphics.StartVisualFeedbackAnimation(_hasFocus);
            }
        }

        private void UpdateSlider()
        {
            // Increment how much the touchpad has been dragged.
            _incrementedMoveAmount += GetRelativeControllerMovement().x * _controllerMovementMultiplier;

            // Resets the incremented delta for the discrete slider if dragging outside of the slider's scope (above max or below min).
            if (TryingToDragOutsideOfScope())
            {
                _incrementedMoveAmount = 0;
                return;
            }

            // Calculate the size per step.
            _sizePerStep = 1f / (_maxValue - _minValue);

            // If the incremented drag amount is bigger than a step on discrete slider, update the slider value.
            if (Mathf.Abs(_incrementedMoveAmount) > _sizePerStep)
            {
                // Determine the number of steps to move.
                _stepsToMove = (int) (_incrementedMoveAmount / _sizePerStep);

                // Reset the value after it has been used to update the current step.
                _incrementedMoveAmount = 0;

                // Updates the current step.
                _currentStep = Mathf.Clamp(_currentStep + _stepsToMove, 0, _maxValue - _minValue);
                _stepsToMove = 0;

                DevToolsControllerManager.Instance.TriggerHapticPulse(_hapticStrength);
            }

            // Update the variable holding how much the slider should be filled and set the graphical fill amount.
            _sliderFillAmount = _currentStep * _sizePerStep;
            _sliderGraphics.SetFillAmount(_sliderFillAmount);

            // Calculate the new value and update the value text.
            Value = (int) Mathf.Lerp(_minValue, _maxValue, _sliderFillAmount);
            _sliderGraphics.UpdateValueText(Value);

        }

        public void SetSliderTo(int number)
        {
            _sliderFillAmount = (float) (number - _minValue) / (float) (_maxValue - _minValue);
            _sliderGraphics.SetFillAmount(_sliderFillAmount);
            Value = (int) Mathf.Lerp(_minValue, _maxValue, _sliderFillAmount);
            _sliderGraphics.UpdateValueText(Value);
            _sizePerStep = 1f / (_maxValue - _minValue);
            _currentStep = (int) (_sliderFillAmount / _sizePerStep);
        }

        /// <summary>
        /// Returns the distance the controller has traveled between the current and the last frame relative to this element.
        /// </summary>
        /// <returns>The relative controller movement.</returns>
        private Vector3 GetRelativeControllerMovement()
        {
            return transform.InverseTransformVector(DevToolsControllerManager.Instance.Velocity) * _xScaleLossy *
                   Time.deltaTime;
        }

        /// <summary>
        /// Determines if the user is dragging outside the scope of the slider (e.g., dragging left when at the minimum value).
        /// </summary>
        /// <returns>True if the user is dragging outside the scope.</returns>
        private bool TryingToDragOutsideOfScope()
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
                    Debug.LogErrorFormat(
                        "The slider's maximum value ({0}) has to be bigger that the minimum value ({1})", _maxValue,
                        _minValue);
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
            _hasFocus = hasFocus;

            // Return if the trigger button is pressed down, meaning, when the user has locked on any element, this element shouldn't be highlighted when gazed on.
            if (DevToolsControllerManager.Instance.GetButtonPress(TriggerButton)) return;

            _sliderGraphics.StartVisualFeedbackAnimation(hasFocus);
        }
    }
}
