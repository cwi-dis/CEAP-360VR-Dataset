// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// All the graphics for a slider, including animations.
/// </summary>
public class UIGazeSliderGraphics : MonoBehaviour {
#pragma warning disable 649
    [Header("Components")]
    [SerializeField]
    private Image _backgroundImage;
    [SerializeField]
    private Image _fillImage;
    [SerializeField]
    private Image _handleImage;
    [SerializeField]
    private Text _valueText;
    [SerializeField]
    private Text _label;

    [Header("Focused")]
    [SerializeField, Tooltip("The color of the background when the user is focusing on the slider.")]
    private Color _backgroundFocusColor;
    [SerializeField, Tooltip("The color of the filled slider when the user is focusing on the slider.")]
    private Color _fillFocusColor;
    [SerializeField, Tooltip("The color of the text label when the user is focusing on the slider.")]
    private Color _labelFocusColor;
    [SerializeField, Tooltip("The scale of the slider when the user is focusing on the slider.")]
    private float _sliderFocusScale = 1.03f;
    [SerializeField, Tooltip("The duration it takes for the visual feedback to either fully highlight the button or go back to the default.")]
    private float _visualFeedbackDuration = 0.2f;
    [SerializeField, Tooltip("How the visual highlighted is animated.")]
    private AnimationCurve _visualFeedbackAnimationCurve;

    [Header("Handle")]
    [SerializeField, Tooltip("The color of the handle when the user is focusing on the slider and touching the touchpad.")]
    private Color _handleOnTouchColor;
    [SerializeField, Tooltip("The duration for the handle to become visible when the user looks at the slider and touches the touchpad or go back to the default color when the user looks away.")]
    private float _handleAnimationDuration = 0.1f;
    [SerializeField, Tooltip("How the handle is animated.")]
    private AnimationCurve _handleAnimationCurve;
#pragma warning restore 649

    // The number of decimals used when displaying the value text.
    private const int NumberOfDecimals = 0;

    // Coroutines.
    private Coroutine _visualFeedbackCoroutine;
    private Coroutine _handleAnimationCoroutine;

    // Private fields.
    private RectTransform _handleRect;
    private float _sliderWidth;
    private RectTransform _sliderRect;
    private Color _backgroundDefaultColor;
    private Color _fillDefaultColor;
    private Color _handleDefaultColor;
    private Color _labelDefaultColor;
    private Vector3 _defaultHandleScale;
    private Vector3 _defaultSliderScale;

    private void Awake () {

        // Get transforms and width of the slider.
        _handleRect = _handleImage.GetComponent<RectTransform>();
        _sliderWidth = _backgroundImage.GetComponent<RectTransform>().rect.width;
        _sliderRect = _backgroundImage.rectTransform;

        // Get the default values of the slider.
        _backgroundDefaultColor = _backgroundImage.color;
        _fillDefaultColor = _fillImage.color;
        _handleDefaultColor = _handleImage.color;
        _labelDefaultColor = _label.color;
        _defaultHandleScale = _handleRect.localScale;
        _defaultSliderScale = _sliderRect.localScale;
    }
	
    /// <summary>
    /// Fills the slider to the current amount.
    /// </summary>
    /// <param name="amount">The amount of the slider that should be filled.</param>
    public void SetFillAmount(float amount)
    {
        _fillImage.fillAmount = amount;

        UpdateHandlePosition();
    }

    /// <summary>
    /// Updates the value text according to the current value.
    /// </summary>
    /// <param name="value">The current value of the slider.</param>
    public void UpdateValueText(int value)
    {
        _valueText.text = value.ToString("F" + NumberOfDecimals);
    }

    /// <summary>
    /// Updates the handle position within the slider.
    /// </summary>
    private void UpdateHandlePosition()
    {
        var handlePosition = _handleImage.transform.localPosition;
        handlePosition.x = -_sliderWidth / 2f + _fillImage.fillAmount * _sliderWidth;
        _handleImage.transform.localPosition = handlePosition;
    }

    /// <summary>
    /// Starts the animation of the slider handle.
    /// </summary>
    /// <param name="isTouching">Whether the touchpad is being touched or not.</param>
    public void StartHandleAnimation(bool isTouching)
    {
        // If the handle is being animated, stop the animation and then start a new animation.
        if (_handleAnimationCoroutine != null)
        {
            StopCoroutine(_handleAnimationCoroutine);
        }
        _handleAnimationCoroutine = StartCoroutine(AnimateHandle(isTouching));
    }

    /// <summary>
    /// Starts the animation of the visual feedback.
    /// </summary>
    public void StartVisualFeedbackAnimation(bool hasFocus)
    {
        // If the visual feedback is being animated, stop it and then start a new animation.
        if (_visualFeedbackCoroutine != null)
        {
            StopCoroutine(_visualFeedbackCoroutine);
        }
        _visualFeedbackCoroutine = StartCoroutine(AnimateVisualFeedback(hasFocus));
    }

    /// <summary>
    /// Animates the visual feedback, lerping the color of different components.
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimateVisualFeedback(bool hasFocus)
    {
        // Set start and end values for the lerp.
        var startSliderScale = _sliderRect.localScale;
        var endSliderScale = hasFocus ? _defaultSliderScale * _sliderFocusScale : _defaultSliderScale;

        var startFilledColor = _fillImage.color;
        var startLabelColor = _label.color;
        var startBackgroundColor = _backgroundImage.color;
        var endFilledColor = hasFocus ? _fillFocusColor : _fillDefaultColor;
        var endLabelColor = hasFocus ? _labelFocusColor : _labelDefaultColor;
        var endBackgroundColor = hasFocus ? _backgroundFocusColor : _backgroundDefaultColor;

        // Lerp the colors over time.
        var progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * (1f / _visualFeedbackDuration);

            _sliderRect.localScale = Vector3.Lerp(startSliderScale, endSliderScale, _visualFeedbackAnimationCurve.Evaluate(progress));
            _fillImage.color = Color.Lerp(startFilledColor, endFilledColor, _visualFeedbackAnimationCurve.Evaluate(progress));
            _backgroundImage.color = Color.Lerp(startBackgroundColor, endBackgroundColor, _visualFeedbackAnimationCurve.Evaluate(progress));
            _label.color = Color.Lerp(startLabelColor, endLabelColor, _visualFeedbackAnimationCurve.Evaluate(progress));
            yield return null;
        }

        // Null the coroutine when it has finished running.
        _visualFeedbackCoroutine = null;
    }

    /// <summary>
    /// Animates the slider knob.
    /// </summary>
    /// <param name="hasFocus"></param>
    /// <returns></returns>
    private IEnumerator AnimateHandle(bool hasFocus)
    {
        // Set the start and end colors.
        var startHandleColor = _handleImage.color;
        var startHandleScale = _handleRect.localScale;
        var endHandleColor = hasFocus ? _handleOnTouchColor : _handleDefaultColor;
        var endHandleScale = hasFocus ? _defaultHandleScale : Vector3.zero;

        // Lerp the colors over time.
        var progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * (1f / _handleAnimationDuration);
            _handleImage.color = Color.Lerp(startHandleColor, endHandleColor, _handleAnimationCurve.Evaluate(progress));
            _handleRect.localScale = Vector3.Lerp(startHandleScale, endHandleScale, _handleAnimationCurve.Evaluate(progress));
            yield return null;
        }

        // Null the coroutine when it has finished running.
        _handleAnimationCoroutine = null;
    }
}
