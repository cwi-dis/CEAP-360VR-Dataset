//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Research.Unity.Examples
{
    public class ScreenBasedPrefabDemo : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Attach text object here.")]
        private Text _text;

        private EyeTracker _eyeTracker;
        private GazeTrail _gazeTrail;
        private Calibration _calibration;
        private ScreenBasedSaveData _saveData;
        private TrackBoxGuide _trackBoxGuide;

        private void Start()
        {
            // Cache our prefab scripts.
            _eyeTracker = EyeTracker.Instance;
            _gazeTrail = GazeTrail.Instance;
            _calibration = Calibration.Instance;
            _saveData = ScreenBasedSaveData.Instance;
            _trackBoxGuide = TrackBoxGuide.Instance;
        }

        private void Update()
        {
            // We really should run this in full screen.
            if (!Screen.fullScreen)
            {
                _text.text = "<color=red>Please run in full screen!</color>";
                return;
            }

            // Quit if escape is pressed.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Application.isEditor)
                {
                    Application.Quit();
                }
            }

            // We are expecting to have all objects.
            if (!_eyeTracker || !_gazeTrail || !_calibration || !_saveData || !_trackBoxGuide)
            {
                return;
            }

            // Thin out updates a bit.
            if (Time.frameCount % 6 != 0)
            {
                return;
            }

            // Create an informational string.
            var info = string.Format("<color=yellow>{0}\nLatest hit object: {1}\nCalibration in progress: {2}\nSaving data: {3}\nPositioning guide visible: {4}</color>",
                string.Format("L: {0}\nR: {1}",
                    _eyeTracker.LatestProcessedGazeData.Left.GazeOriginValid ? _eyeTracker.LatestProcessedGazeData.Left.GazeRayScreen.ToString() : "No gaze",
                    _eyeTracker.LatestProcessedGazeData.Right.GazeOriginValid ? _eyeTracker.LatestProcessedGazeData.Right.GazeRayScreen.ToString() : "No gaze"),
                _gazeTrail.LatestHitObject != null ? _gazeTrail.LatestHitObject.name : "Nothing",
                _calibration.CalibrationInProgress ? "Yes" : "No",
                _saveData.SaveData ? "Yes" : "No",
                _trackBoxGuide.TrackBoxGuideActive ? "Yes" : "No");

            _text.text = info;
        }
    }
}