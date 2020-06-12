//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

namespace Tobii.Research.Unity
{
    using UnityEngine;
    using UnityEngine.UI;

    public class VRPositioningGuide : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="VRPositioningGuide"/> for easy access.
        /// Assigned in Awake() so use earliest in Start().
        /// </summary>
        public static VRPositioningGuide Instance { get; private set; }

        [SerializeField]
        [Tooltip("This key will show or hide the positioning guide.")]
        private KeyCode _toggleKey = KeyCode.None;

        [SerializeField]
        [Tooltip("Activate or deactivate the positioning guide.")]
        private bool _positioningGuideActive;

        /// <summary>
        /// Activate or deactivate the positioning guide.
        /// </summary>
        public bool PositioningGuideActive
        {
            get
            {
                return _positioningGuideActive;
            }

            set
            {
                _positioningGuideActive = value;
                HMDPlacementCanvas.gameObject.SetActive(_positioningGuideActive);
            }
        }

        private Image _leftImage;
        private Image _rightImage;
        private Vector2 _center;
        public VRPositioningPlacementCanvas HMDPlacementCanvas;

        private Vector2 _sizeOfparent;
        private VREyeTracker _eyeTracker;
        private VRCalibration _calibration;
        private Vector2 _leftPupilXY;
        private Vector2 _rightPupilXY;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _sizeOfparent = HMDPlacementCanvas.PupilLeft.parent.GetComponent<RectTransform>().sizeDelta;
            _sizeOfparent.y = -1 * _sizeOfparent.y;
            _eyeTracker = VREyeTracker.Instance;
            _calibration = VRCalibration.Instance;

            transform.parent = VRUtility.EyeTrackerOriginVive;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            _leftImage = HMDPlacementCanvas.PupilLeft.GetComponent<Image>();
            _rightImage = HMDPlacementCanvas.PupilRight.GetComponent<Image>();
            _center = new Vector2(0.5f, 0.5f);
            PositioningGuideActive = _positioningGuideActive;
        }

        private void Update()
        {
            if (HMDPlacementCanvas.gameObject.activeSelf != _positioningGuideActive)
            {
                PositioningGuideActive = _positioningGuideActive;
            }

            if (Input.GetKeyDown(_toggleKey))
            {
                PositioningGuideActive = !PositioningGuideActive;
            }

            if (_eyeTracker == null || !_eyeTracker.Connected || !_positioningGuideActive || (_calibration != null && _calibration.CalibrationInProgress))
            {
                return;
            }

            if (HMDPlacementCanvas != null)
            {
                // Circa twice a second.
                if (Time.frameCount % 45 == 0)
                {
                    if (_eyeTracker.EyeTrackerInterface.UpdateLensConfiguration())
                    {
                        Debug.Log("Updated lens configuration.");
                    }

                    var hmdLcsInMM = VRUtility.LensCupSeparation * 1000f;
                    var lHPos = new Vector3(-hmdLcsInMM, 0);
                    var rHPos = new Vector3(hmdLcsInMM, 0);

                    HMDPlacementCanvas.TargetLeft.localPosition = lHPos;
                    HMDPlacementCanvas.TargetRight.localPosition = rHPos;
                }

                var data = VREyeTracker.Instance.LatestGazeData;
                _leftPupilXY = data.Left.PupilPosiitionInTrackingAreaValid ? data.Left.PupilPosiitionInTrackingArea : _leftPupilXY;
                _rightPupilXY = data.Right.PupilPosiitionInTrackingAreaValid ? data.Right.PupilPosiitionInTrackingArea : _rightPupilXY;

                HMDPlacementCanvas.PupilLeft.anchoredPosition = Vector2.Scale(_leftPupilXY, _sizeOfparent);
                HMDPlacementCanvas.PupilRight.anchoredPosition = Vector2.Scale(_rightPupilXY, _sizeOfparent);

                var leftDistance = Vector2.Distance(_center, _leftPupilXY);
                var rightDistance = Vector2.Distance(_center, _rightPupilXY);

                _leftImage.color = data.Left.PupilPosiitionInTrackingAreaValid ? Color.Lerp(Color.green, Color.red, leftDistance / 0.35f) : Color.clear;
                _rightImage.color = data.Right.PupilPosiitionInTrackingAreaValid ? Color.Lerp(Color.green, Color.red, rightDistance / 0.35f) : Color.clear;

                // Info to the user
                HMDPlacementCanvas.Status.text = (leftDistance + rightDistance < 0.25f) ? "Awesome!" : "OK";
            }
        }
    }
}