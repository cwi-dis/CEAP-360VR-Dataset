//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity.Examples
{
    public class VRPrefabDemo : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Attach 3D text object here.")]
        private TextMesh _threeDText;

        private VREyeTracker _eyeTracker;
        private VRGazeTrail _gazeTrail;
        private VRCalibration _calibration;
        private VRSaveData _saveData;
        private VRPositioningGuide _positioningGuide;
        private Color _particleColor;

        private void Awake()
        {
            UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        }

        private void Start()
        {
            // Cache our prefab scripts.
            _eyeTracker = VREyeTracker.Instance;
            _gazeTrail = VRGazeTrail.Instance;
            _calibration = VRCalibration.Instance;
            _saveData = VRSaveData.Instance;
            _positioningGuide = VRPositioningGuide.Instance;

            // Move HUD to be in front of user.
            var etOrigin = VRUtility.EyeTrackerOriginVive;
            var holder = _threeDText.transform.parent;
            holder.parent = etOrigin;
            holder.localPosition = new Vector3(0, -1.35f, 3);
            holder.localRotation = Quaternion.Euler(25, 0, 0);

            StartCoroutine(VRUtility.LoadOpenVR());
        }

        private void Update()
        {
            // We are expecting to have all objects.
            if (!_eyeTracker || !_gazeTrail || !_calibration || !_saveData || !_positioningGuide)
            {
                return;
            }

            // Thin out updates a bit.
            if (Time.frameCount % 9 != 0)
            {
                return;
            }

            // Create an informational string.
            var info = string.Format("{0}\nLatest hit object: {1}\nCalibration in progress: {2}, Saving data: {3}\nPositioning guide visible: {4}",
                string.Format("L: {0}\nR: {1}",
                    _eyeTracker.LatestProcessedGazeData.Left.GazeRayWorldValid ? _eyeTracker.LatestProcessedGazeData.Left.PupilDiameter.ToString() : "No gaze",
                    _eyeTracker.LatestProcessedGazeData.Right.GazeRayWorldValid ? _eyeTracker.LatestProcessedGazeData.CombinedGazeRayWorld.ToString() : "No gaze"),
                _gazeTrail.LatestHitObject != null ? _gazeTrail.LatestHitObject.name : "Nothing",
                _calibration.CalibrationInProgress ? "Yes" : "No",
                _saveData.SaveData ? "Yes" : "No",
                _positioningGuide.PositioningGuideActive ? "Yes" : "No");

            // Update HUD.
            _threeDText.text = info;
        }
    }
}