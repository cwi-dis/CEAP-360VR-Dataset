// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// The different states for a gaze aware button.
    /// </summary>
    public enum ButtonState
    {
        Idle,
        Focused,
        PressedDown
    }

    /// <summary>
    /// All the graphics for a gaze button, including animations.
    /// </summary>
    public class DevToolsUIGazeButtonGraphics : MonoBehaviour
    {
#pragma warning disable 649
        [Header("Components")] [SerializeField] private Image _buttonImage;
        [SerializeField] private Image _label;

        [Header("Focused")]
        [SerializeField, Tooltip("The color of the button background when focused.")]
        private Color _backgroundFocusColor;

        [SerializeField, Tooltip("The color of the label when focused.")] private Color _labelFocusColor;
        [SerializeField, Tooltip("The button scale when focused.")] private float _buttonFocusScale = 1.05f;

        [SerializeField,
         Tooltip(
             "The duration it takes for the visual feedback to either fully highlight or go back to the default values.")]
        private float _visualFeedbackDuration = 0.2f;

        [SerializeField, Tooltip("How the visual feedback is animated.")]
        private AnimationCurve _visualFeedbackAnimationCurve;

        [Header("Pressed")]
        [SerializeField, Tooltip("The color of the button background when the button is pressed down.")]
        private Color _backgroundPressColor;

        [SerializeField, Tooltip("The color fo the label when the button is pressed down.")]
        private Color _labelPressColor;

        [SerializeField, Tooltip("The scale of the button when pressed down.")]
        private float _buttonScaleOnPress = 0.95f;

        [SerializeField, Tooltip("The duration it takes for the button click animation.")]
        private float _buttonPressDuration = 0.1f;

        [SerializeField, Tooltip("How the button click is animated.")]
        private AnimationCurve _buttonPressAnimationCurve;
#pragma warning restore 649

        // Private fields.
        private RectTransform _buttonRect;

        private Color _buttonDefaultColor;
        private Color _labelDefaultColor;
        private Vector3 _buttonDefaultScale;
        private Coroutine _buttonAnimationCoroutine;
        private ButtonState _currentButtonState;

        // Use this for initialization
        void Start()
        {
            // Store the button rect transform.
            _buttonRect = _buttonImage.GetComponent<RectTransform>();

            // Get the default colors and scale of the button's components.
            _buttonDefaultColor = _buttonImage.color;
            _labelDefaultColor = _label.color;
            _buttonDefaultScale = _buttonRect.localScale;

        }

        /// <summary>
        /// Animate the button press to a new state.
        /// </summary>
        /// <param name="currentButtonState">The state the button should animate to.</param>
        public void AnimateButtonPress(ButtonState currentButtonState)
        {
            // Stop the animation if it is animating.
            if (_buttonAnimationCoroutine != null)
            {
                StopCoroutine(_buttonAnimationCoroutine);
            }

            _currentButtonState = currentButtonState;

            // Animate the button to the new state.
            if (this.gameObject.activeInHierarchy)
                _buttonAnimationCoroutine = StartCoroutine(AnimateButton(_buttonPressDuration,
                    _buttonPressAnimationCurve, currentButtonState));
        }

        /// <summary>
        /// Animate the visual feedback for the button.
        /// </summary>
        /// <param name="currentButtonState">The state of the button that should be animated.</param>
        public void AnimateButtonVisualFeedback(ButtonState currentButtonState)
        {
            // Stop the animation if it is animating.
            if (_buttonAnimationCoroutine != null)
            {
                StopCoroutine(_buttonAnimationCoroutine);
            }

            _currentButtonState = currentButtonState;

            // Animate the visual feedback in it's current button state.
            if (this.gameObject.activeInHierarchy)
                _buttonAnimationCoroutine = StartCoroutine(AnimateButton(_visualFeedbackDuration,
                    _visualFeedbackAnimationCurve, currentButtonState));
        }

        /// <summary>
        /// Animates the visual feedback or the button press.
        /// </summary>
        /// <param name="duration">The duration of the animation.</param>
        /// <param name="animationCurve">How the button animates.</param>
        /// <returns></returns>
        private IEnumerator AnimateButton(float duration, AnimationCurve animationCurve, ButtonState currentButtonState)
        {
            // Sets the start values of the animation.
            var startBackgroundColor = _buttonImage.color;
            Color startLabelColor = Color.magenta;
            if (_label != null)
            {
                startLabelColor = _label.color;
            }

            if (_buttonRect == null)
                yield return null;
            var startButtonScale = _buttonRect.localScale;

            // Sets the end values of the animation to the default.
            var endBackgroundColor = _buttonDefaultColor;
            var endLabelColor = _labelDefaultColor;
            var endButtonScale = _buttonDefaultScale;

            // Updates the end values of the animation depending on the button state.
            switch (currentButtonState)
            {
                case ButtonState.Focused:
                    endBackgroundColor = _backgroundFocusColor;
                    endLabelColor = _labelFocusColor;
                    endButtonScale *= _buttonFocusScale;
                    break;
                case ButtonState.PressedDown:
                    endBackgroundColor = _backgroundPressColor;
                    endLabelColor = _labelPressColor;
                    endButtonScale *= _buttonScaleOnPress;
                    break;
            }

            // Lerp the colors and scale.
            var progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime * (1f / duration);
                var animationProgress = animationCurve.Evaluate(progress);
                _buttonRect.localScale = Vector3.Lerp(startButtonScale, endButtonScale, animationProgress);
                _buttonImage.color = Color.Lerp(startBackgroundColor, endBackgroundColor, animationProgress);
                if (_label != null)
                    _label.color = Color.Lerp(startLabelColor, endLabelColor, animationProgress);
                yield return null;
            }

            _buttonAnimationCoroutine = null;
        }

        void OnEnable()
        {
            if (_buttonAnimationCoroutine != null)
            {
                StopCoroutine(_buttonAnimationCoroutine);
            }

            if (this.gameObject.activeInHierarchy)
            {
                _buttonAnimationCoroutine =
                    StartCoroutine(AnimateButton(_buttonPressDuration, _buttonPressAnimationCurve,
                        _currentButtonState));
            }
        }

        void OnDisable()
        {
            if (_buttonAnimationCoroutine != null)
            {
                StopCoroutine(_buttonAnimationCoroutine);
            }
        }
    }
}
