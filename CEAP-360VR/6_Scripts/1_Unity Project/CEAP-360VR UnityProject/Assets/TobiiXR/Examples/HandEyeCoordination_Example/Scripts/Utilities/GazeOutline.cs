// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections;
using Tobii.G2OM;
using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Draws an outline around an object when it is being focused. Requires the object to have the UltimateOutline shader.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GazeOutline : MonoBehaviour, IGazeFocusable
    {
        // How long the object should retain focus after losing it, to avoid visual flickering
        public float GazeStickinessSeconds
        {
            get { return _gazeStickinessSeconds; }
            set { _gazeStickinessSeconds = value; }
        }

        private float _gazeStickinessSeconds = 0.1f;

        private Material _material;
        private bool _outlineDisabled;
        private bool _hasFocus;

        private const float GazeOutlineAnimationTimeSeconds = 0.2f;

        private void Awake()
        {
            _material = GetComponent<Renderer>().material;
        }

        /// <summary>
        /// Called by TobiiXR when object receives or loses focus.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
            _hasFocus = hasFocus;
            StartOutlineAnimation(hasFocus);
        }

        /// <summary>
        /// Hides and disables the outline.
        /// </summary>
        public void DisableHighlight()
        {
            StartOutlineAnimation(false);
            _outlineDisabled = true;
        }

        /// <summary>
        /// Enables the outline.
        /// </summary>
        public void EnableOutline()
        {
            _outlineDisabled = false;
            StartOutlineAnimation(_hasFocus);
        }

        /// <summary>
        /// Start the outline coroutine.
        /// </summary>
        /// <param name="shouldOutline">Should the object be outlined or not.</param>
        private void StartOutlineAnimation(bool shouldOutline)
        {
            if (_outlineDisabled) return;

            StopAllCoroutines();
            StartCoroutine(SetOutline(shouldOutline));
        }

        /// <summary>
        /// Coroutine which will modify the alpha of the shader on the material to show or hide the outline.
        /// </summary>
        /// <param name="shouldOutline">Should the object be outlined or not.</param>
        /// <returns></returns>
        private IEnumerator SetOutline(bool shouldOutline)
        {
            var color = _material.GetColor("_FirstOutlineColor");
            var startValue = color.a;
            var endValue = shouldOutline ? 1f : 0f;
            var progress = 1.0f - Mathf.Abs(endValue - startValue);

            if (!shouldOutline)
            {
                yield return new WaitForSecondsRealtime(_gazeStickinessSeconds);
            }

            while (progress < 1.0f)
            {
                progress += Time.deltaTime / GazeOutlineAnimationTimeSeconds;
                color.a = Mathf.Lerp(startValue, endValue, progress);
                _material.SetColor("_FirstOutlineColor", color);
                yield return null;
            }
        }
    }
}
