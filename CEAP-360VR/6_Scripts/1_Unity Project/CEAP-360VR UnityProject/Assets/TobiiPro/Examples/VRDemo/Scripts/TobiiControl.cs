//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity.Examples
{
    public sealed class ActiveObject
    {
        // The active GameObject.
        public GameObject HighlightedObject;

        // The previous material.
        public Material OriginalObjectMaterial;

        public ActiveObject()
        {
            HighlightedObject = null;
            OriginalObjectMaterial = null;
        }
    }

    public class TobiiControl : MonoBehaviour
    {
        // The text about how to start the calibration.
        public GameObject _textCalibration;

        // The background of the text.
        public GameObject _textBackground;

        // The material to use for active objects.
        public Material _highlightMaterial;

        // The object that we hit.
        private ActiveObject _highlightInfo;

        // Whatever we need to run the calibration.
        private bool _calibratedSuccessfully;

        // Remember if we have saved data.
        private bool _hasSavedData;

        // Gaze trail script.
        private VRGazeTrail _gazeTrail;

        // Toned down color when looking at sign.
        private Color _lookAtSignColor;

        // Quit the app.
        private bool _quitTime;

        // The Unity EyeTracker helper object.
        private VREyeTracker _eyeTracker;

        private bool ShowText
        {
            get
            {
                return _textCalibration.activeSelf && _textCalibration.activeSelf;
            }

            set
            {
                _textCalibration.SetActive(value);
                _textBackground.SetActive(value);
            }
        }

        private void Start()
        {
            // Get EyeTracker unity object
            _eyeTracker = VREyeTracker.Instance;
            if (_eyeTracker == null)
            {
                Debug.Log("Failed to find eye tracker, has it been added to scene?");
            }

            _gazeTrail = VRGazeTrail.Instance;
            _lookAtSignColor = new Color(0, 1, 0, 0.2f);

            _highlightInfo = new ActiveObject();
            var textRenderer = _textCalibration.GetComponent<Renderer>();
            textRenderer.sortingOrder -= 1;

            StartCoroutine(VRUtility.LoadOpenVR());
        }

        private void HandleF1Pressed()
        {
            if (_eyeTracker.Connected)
            {
                RunCalibration();
            }
        }

        private void RunCalibration()
        {
            if (_eyeTracker.EyeTrackerInterface.UpdateLensConfiguration())
            {
                Debug.Log("Updated lens configuration");
            }

            // Hide text while calibrating.
            ShowText = false;

            var calibrationStartResult = VRCalibration.Instance.StartCalibration(
                resultCallback: (calibrationResult) =>
                {
                    // The calibration result is provided.
                    Debug.Log("Calibration was " + (calibrationResult ? "successful" : "unsuccessful"));

                    // Show text again.
                    ShowText = true;

                    _calibratedSuccessfully = calibrationResult;
                });

            Debug.Log("Calibration " + (calibrationStartResult ? "" : "not ") + "started");
        }

        private void HandleF2Pressed()
        {
            _gazeTrail.ParticleCount = _gazeTrail.ParticleCount > 0 ? 0 : 1;
        }

        private void HandleQuit()
        {
            _quitTime = true;
        }

        private void Update()
        {
            if (_quitTime)
            {
                // Stop any data saving.
                VRSaveData.Instance.SaveData = false;

                // And quit!
                if (!Application.isEditor)
                {
                    Application.Quit();
                }

                return;
            }

            if (_eyeTracker.Connected)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    HandleF1Pressed();
                }

                if (Input.GetKeyDown(KeyCode.F2))
                {
                    HandleF2Pressed();
                }

                if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.Escape))
                {
                    HandleQuit();
                }

                // Check if the calibration already finish.
                if (!_hasSavedData && _calibratedSuccessfully)
                {
                    // Start saving data.
                    VRSaveData.Instance.SaveData = true;

                    // In this demo, only save once per run.
                    _hasSavedData = true;

                    // Save data for 60 seconds.
                    Invoke("StopSaving", 60);
                }

                // Reset any priviously set active object and remove its highlight
                if (_highlightInfo.HighlightedObject != null)
                {
                    var renderer = _highlightInfo.HighlightedObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = _highlightInfo.OriginalObjectMaterial;
                    }

                    _highlightInfo.HighlightedObject = null;
                    _highlightInfo.OriginalObjectMaterial = null;
                }

                var latestHitObject = _gazeTrail.LatestHitObject;
                if (latestHitObject != null)
                {
                    if (latestHitObject.gameObject != _highlightInfo.HighlightedObject &&
                        (latestHitObject.name.StartsWith("Cube") || latestHitObject.name.StartsWith("Cylinder")))
                    {
                        MeshRenderer renderer = latestHitObject.gameObject.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            _highlightInfo.HighlightedObject = latestHitObject.gameObject;
                            _highlightInfo.OriginalObjectMaterial = renderer.material;
                            renderer.material = _highlightMaterial;
                        }
                    }

                    if (latestHitObject.gameObject == _textBackground || latestHitObject.gameObject == _textCalibration)
                    {
                        _gazeTrail.ParticleColor = _lookAtSignColor;
                    }
                    else
                    {
                        _gazeTrail.ParticleColor = Color.blue;
                    }
                }
            }
        }

        private void StopSaving()
        {
            VRSaveData.Instance.SaveData = false;
        }
    }
}