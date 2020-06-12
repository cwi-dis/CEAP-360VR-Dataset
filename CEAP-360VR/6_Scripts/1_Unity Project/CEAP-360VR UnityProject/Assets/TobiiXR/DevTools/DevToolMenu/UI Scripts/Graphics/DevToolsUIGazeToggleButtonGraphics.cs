// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.XR.DevTools
{  
    /// <summary>
    /// The graphics for the gaze toggle button, including animations.
    /// </summary>
    public class DevToolsUIGazeToggleButtonGraphics : MonoBehaviour
    {
#pragma warning disable 649
        [Header("Components")] [SerializeField] private RectTransform _buttonTransform;
        [SerializeField] private Text _label;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _outlineImage;
        [SerializeField] private Image _knobImage;
        [SerializeField] private RectTransform _buttonOffPosition;
        [SerializeField] private RectTransform _buttonOnPosition;

        // Struct describing which colors the button should use for each of its visual states: On, Off, Pressed.
        [Serializable]
        private struct VisualStateColors
        {
            [SerializeField] public Color DefaultBackgroundColor;
            [SerializeField] public Color FocusBackgroundColor;
            [SerializeField] public Color DefaultOutlineColor;
            [SerializeField] public Color FocusOutlineColor;
        }

        [Header("Colors")]
        [SerializeField, Tooltip("The color of the knob when the user is not focusing on the button.")]
        private Color _knobDefaultColor;

        [SerializeField, Tooltip("The color of the knob when the user is focusing on the button.")]
        private Color _knobFocusColor;

        [SerializeField, Tooltip("The color of the label when the user is not focusing on the button.")]
        private Color _labelDefaultColor;

        [SerializeField, Tooltip("The color of the label when the user is focusing on the button.")]
        private Color _labelFocusColor;

        [SerializeField, Tooltip("The colors used for when the button is toggled off.")]
        private VisualStateColors _toggledOffColors;

        [SerializeField, Tooltip("The colors used for when the button is toggled on.")]
        private VisualStateColors _toggledOnColors;

        [SerializeField, Tooltip("The colors used for when the button is pressed down.")]
        private VisualStateColors _pressedDownColors;

        [Header("Focused")]
        [SerializeField, Tooltip("The scale of the button when the user is focusing on it.")]
        private float _buttonFocusScale = 1.05f;

        [SerializeField, Tooltip("The scale of the knob when the user is focusing on it.")]
        private float _knobFocusScale = 1.1f;

        [SerializeField,
         Tooltip("The duration of the visual feedback when the user focuses or stops to focus on the button.")]
        private float _focusAnimationDuration = 0.2f;

        [SerializeField, Tooltip("How the visual feedback is animated.")] private AnimationCurve _focusAnimationCurve;

        [Header("Clicked")]
        [SerializeField, Tooltip("The duration for the toggle click animation.")]
        private float _toggleAnimationDuration = 0.1f;

        [SerializeField, Tooltip("How the toggle click is animated.")] private AnimationCurve _toggleAnimationCurve;
#pragma warning restore 649

        // Private fields.
        private RectTransform _knobTransform;

        private Vector3 _knobDefaultScale;
        private Vector3 _buttonDefaultScale;
        private Coroutine _visualFeedbackCoroutine;
        private Coroutine _knobMovementCoroutine;


        // Use this for initialization
        void Start()
        {
            // Store the rect transform for the toggle knob.
            _knobTransform = _knobImage.GetComponent<RectTransform>();

            // Get the default scales for the button.
            _knobDefaultScale = _knobTransform.localScale;
            _buttonDefaultScale = _buttonTransform.localScale;
        }

        /// <summary>
        /// Start the knob animation, either to go from left to right, or from right to left.
        /// </summary>
        public void StartKnobAnimation(bool isToggledOn)
        {
            // Stop the animation if it is animating.
            if (_knobMovementCoroutine != null)
            {
                StopCoroutine(_knobMovementCoroutine);
            }

            // Animate the knob to the new position.
            if (this.gameObject.activeInHierarchy)
                _knobMovementCoroutine = StartCoroutine(MoveKnob(isToggledOn));
        }

        /// <summary>
        /// Start the visual feedback animation.
        /// </summary>
        public void StartVisualFeedbackAnimation(bool hasFocus, bool isToggledOn, bool isButtonPressed)
        {
            // Stop the animation if it is animating.
            if (_visualFeedbackCoroutine != null)
            {
                StopCoroutine(_visualFeedbackCoroutine);
            }

            // Animate the visual feedback.
            if (this.gameObject.activeInHierarchy)
                _visualFeedbackCoroutine =
                    StartCoroutine(AnimateVisualFeedback(hasFocus, isToggledOn, isButtonPressed));
        }

        /// <summary>
        /// Animate the visual feedback of the button which consists of lerping a set of colors and scales.
        /// </summary>
        /// <returns></returns>
        private IEnumerator AnimateVisualFeedback(bool hasFocus, bool isToggledOn, bool isButtonPressed)
        {
            // Set up the scale values of the knob and the button.
            var startKnobScale = _knobTransform.localScale;
            var endKnobScale = hasFocus ? _knobDefaultScale * _knobFocusScale : _knobDefaultScale;
            var startButtonScale = _buttonTransform.localScale;
            var endButtonScale = hasFocus ? _buttonDefaultScale * _buttonFocusScale : _buttonDefaultScale;

            // Set up the knob and label colors.
            var startKnobColor = _knobImage.color;
            var endKnobColor = hasFocus ? _knobFocusColor : _knobDefaultColor;
            var startLabelColor = _label.color;
            var endLabelColor = hasFocus ? _labelFocusColor : _labelDefaultColor;

            // Set up the start values for the background and outline colors.
            var startBackgroundColor = _backgroundImage.color;
            var startOutlineColor = _outlineImage.color;

            // Get the active colors of the current state of the toggle.
            var currentStateColors = GetStateColors(isToggledOn, isButtonPressed);

            // Set up the background and outline colors that the coroutine should lerp to.
            var endBackgroundColor =
                hasFocus ? currentStateColors.FocusBackgroundColor : currentStateColors.DefaultBackgroundColor;
            var endOutlineColor =
                hasFocus ? currentStateColors.FocusOutlineColor : currentStateColors.DefaultOutlineColor;

            // Lerp the scales and color values.
            var progress = 0f;
            while (progress < 1f)
            {
                // Increment the progress of the coroutine, going from 0 to 1.
                progress += Time.deltaTime * (1f / _focusAnimationDuration);
                var animatedProgress = _focusAnimationCurve.Evaluate(progress);

                // Lerp the values.
                _buttonTransform.localScale = Vector3.Lerp(startButtonScale, endButtonScale, animatedProgress);
                _knobTransform.localScale = Vector3.Lerp(startKnobScale, endKnobScale, animatedProgress);
                _backgroundImage.color = Color.Lerp(startBackgroundColor, endBackgroundColor, animatedProgress);
                _outlineImage.color = Color.Lerp(startOutlineColor, endOutlineColor, animatedProgress);
                _knobImage.color = Color.Lerp(startKnobColor, endKnobColor, animatedProgress);
                _label.color = Color.Lerp(startLabelColor, endLabelColor, animatedProgress);

                yield return null;
            }

            // Null the coroutine when it has finished running.
            _visualFeedbackCoroutine = null;
        }

        /// <summary>
        /// Method that checks the state of the toggle and returns the active colors.
        /// </summary>
        /// <returns>/>The active colors of the current visual state.</returns>
        private VisualStateColors GetStateColors(bool isToggledOn, bool isButtonPressed)
        {
            if (isButtonPressed)
            {
                return _pressedDownColors;
            }
            else if (isToggledOn)
            {
                return _toggledOnColors;
            }
            else
            {
                return _toggledOffColors;
            }
        }

        /// <summary>
        /// Move the toggle button knob to the position of the new button state.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveKnob(bool isToggledOn)
        {
            // Set up the knob positions.
            var startPosition = _knobTransform.localPosition;
            var endPosition = isToggledOn ? _buttonOnPosition.localPosition : _buttonOffPosition.localPosition;

            // Lerp the knob position.
            var progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime * (1f / _toggleAnimationDuration);
                var animatedProgress = _toggleAnimationCurve.Evaluate(progress);
                _knobTransform.localPosition = Vector3.Lerp(startPosition, endPosition, animatedProgress);
                yield return null;
            }

            // Null the coroutine when it has finished running.
            _knobMovementCoroutine = null;
        }
    }
}
