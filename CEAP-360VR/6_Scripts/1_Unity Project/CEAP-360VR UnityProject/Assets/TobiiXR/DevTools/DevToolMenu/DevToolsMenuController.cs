// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR.GazeModifier;
using System.Collections;
using UnityEngine;

namespace Tobii.XR.DevTools
{
    public class DevToolsMenuController : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [SerializeField] private bool startWithVisualizers = true;
        [SerializeField] private GameObject gazeVisualizerObject;
        [SerializeField] private GameObject openMenuButton;
        [SerializeField] private GameObject toolkitMenu;
        [SerializeField] private DevToolsUITriggerGazeToggleButton enableButton;
        [SerializeField] private DevToolsUITriggerGazeToggleButton gazeVisualizerToggle;
        [SerializeField] private DevToolsUITriggerGazeSlider percentileSlider;
#pragma warning restore 649

        private G2OM_DebugVisualization _debugVisualization;
        private GazeModifierFilter _gazeModifierFilter;
        private bool _gazeVisualizerEnabled = true;
        private bool _runOnce = false;
        private bool _started = false;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame(); // Wait a sec so the default G2OM instance can be instantiated

            // Ensure we use the dev tools filter
            if (TobiiXR.Internal.Settings.EyeTrackingFilter != null)
            {
                Debug.LogWarning("TobiiXR was initialized with a gaze filter on '"
                                 + TobiiXR.Internal.Settings.EyeTrackingFilter.gameObject.name
                                 + "'. Dev Tools overwrote this with its own gaze modifier filter.");
            }

            _gazeModifierFilter = gameObject.GetComponent<GazeModifierFilter>();
            TobiiXR.Internal.Settings.EyeTrackingFilter = _gazeModifierFilter;

            var cameraTransform = CameraHelper.GetCameraTransform();
            _debugVisualization = cameraTransform.gameObject.AddComponent<G2OM_DebugVisualization>();

            _gazeVisualizerEnabled = startWithVisualizers;
            SetGazeVisualizerEnabled(_gazeVisualizerEnabled);
            _started = true;
            EnsureCorrectVisualizer();
        }

        private void LateUpdate()
        {
            if (_started)
            {
                EnsureCorrectVisualizer();

                if (toolkitMenu.activeInHierarchy)
                {
                    CheckMenuSettings();
                }
            }
        }

        public void ShowMenu(bool set)
        {
            toolkitMenu.SetActive(set);
            openMenuButton.SetActive(!set);
        }

        public void SetPercentile(int percentile)
        {
            _gazeModifierFilter.Settings.SelectedPercentileIndex = percentile;
            if (toolkitMenu.activeInHierarchy &&
                percentileSlider.Value != _gazeModifierFilter.Settings.SelectedPercentileIndex)
            {
                percentileSlider.SetSliderTo(percentile);
            }
        }

        public void SetGazeModifierEnabled(bool set)
        {
            _gazeModifierFilter.enabled = set;
        }

        public void SetGazeVisualizerEnabled(bool set)
        {
            _gazeVisualizerEnabled = set;
        }

        private void EnsureCorrectVisualizer()
        {
            gazeVisualizerObject.SetActive(_gazeVisualizerEnabled);
        }

        public void SetG2OMDebugView(bool set)
        {
            _debugVisualization.SetVisualization(set);
        }

        // makes sure that the settings from the Tobii Settings Unity window matches the debug window's settings
        private void CheckMenuSettings()
        {
            if (!_runOnce)
            {
                // make menu match our tobii xr settings when first opened
                _runOnce = true;
                percentileSlider._sliderGraphics.UpdateValueText(
                    _gazeModifierFilter.Settings.SelectedPercentileIndex); //force color change
            }

            if (_gazeVisualizerEnabled)
            {
                gazeVisualizerToggle.ToggleOn();
            }
            else
            {
                gazeVisualizerToggle.ToggleOff();
            }

            if (percentileSlider.Value != _gazeModifierFilter.Settings.SelectedPercentileIndex)
            {
                percentileSlider.SetSliderTo(_gazeModifierFilter.Settings.SelectedPercentileIndex);
            }
        }
    }
}