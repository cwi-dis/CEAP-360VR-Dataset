//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Research.Unity
{
    public class Calibration : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="Calibration"/> for easy access.
        /// Assigned in Awake() so use earliest in Start().
        /// </summary>
        public static Calibration Instance { get; private set; }

        /// <summary>
        /// Flag indicating if the latest calibration was successful
        /// or not, true/false.
        /// </summary>
        public bool LatestCalibrationSuccessful { get; private set; }

        /// <summary>
        /// Is calibration in progress?
        /// </summary>
        public bool CalibrationInProgress { get { return _calibrationInProgress; } }

        [SerializeField]
        [Tooltip("This key will start a calibration.")]
        private KeyCode _startKey = KeyCode.None;

        /// <summary>
        /// Calibration points.
        /// Example:
        /// (0.2f, 0.2f)
        /// (0.8f, 0.2f)
        /// (0.2f, 0.8f)
        /// (0.8f, 0.8f)
        /// (0.5f, 0.5f)
        /// </summary>
        [SerializeField]
        [Tooltip("Calibration points.")]
        private Vector2[] _points;

        [SerializeField]
        private Image _calibrationPoint;

        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private Image _panel;

        private CalibrationPoint _pointScript;

        // Handle blocking calls to calibration in a separate thread.
        private CalibrationThread _calibrationThread;
        private bool _calibrationInProgress;

        private bool ShowCalibrationPanel
        {
            get
            {
                return _showCalibrationPanel;
            }

            set
            {
                _showCalibrationPanel = value;
                _pointScript.gameObject.SetActive(_showCalibrationPanel);
                _canvas.gameObject.SetActive(_showCalibrationPanel);
                _panel.color = _showCalibrationPanel ? Color.black : new Color(0, 0, 0, 0);
            }
        }

        private bool _showCalibrationPanel;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _pointScript = _calibrationPoint.GetComponent<CalibrationPoint>();
            ShowCalibrationPanel = false;
        }

        /// <summary>
        /// Start a calibration. Either provide a set of calibration points,
        /// or null for default. The result callback provides a true or false
        /// answer to the success of the calibration.
        /// </summary>
        /// <param name="points">An array of calibration points, or null for default.</param>
        /// <param name="resultCallback">A result callback or null for none.</param>
        /// <returns>True if calibration was not already started, false otherwise.</returns>
        public bool StartCalibration(Vector2[] points = null, System.Action<bool> resultCallback = null)
        {
            if (_calibrationInProgress)
            {
                Debug.Log("Already performing calibration");
                return false;
            }

            _calibrationInProgress = true;
            StartCoroutine(PerformCalibration(points, resultCallback));
            return true;
        }

        /// <summary>
        /// Wait for the <see cref="CalibrationThread.MethodResult"/> to be ready.
        /// </summary>
        /// <param name="result">The method result</param>
        /// <returns>An enumerator</returns>
        private IEnumerator WaitForResult(CalibrationThread.MethodResult result)
        {
            // Wait for the thread to finish the blocking call.
            while (!result.Ready)
            {
                yield return new WaitForSeconds(0.02f);
            }

            Debug.Log(result);
        }

        /// <summary>
        /// Calibration coroutine. Drives the calibration thread states.
        /// </summary>
        /// <param name="points">Optional point list. Null means default set.</param>
        /// <param name="resultCallback">A result callback or null for none.</param>
        /// <returns>An enumerator</returns>
        private IEnumerator PerformCalibration(Vector2[] points, System.Action<bool> resultCallback)
        {
            if (points != null)
            {
                _points = points;
            }

            if (_calibrationThread != null)
            {
                _calibrationThread.StopThread();
                _calibrationThread = null;
            }

            // Create and start the calibration thread.
            _calibrationThread = new CalibrationThread(EyeTracker.Instance.EyeTrackerInterface, screenBased: true);

            // Only continue if the calibration thread is running.
            for (int i = 0; i < 10; i++)
            {
                if (_calibrationThread.Running)
                {
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            if (!_calibrationThread.Running)
            {
                Debug.LogError("Failed to start calibration thread");
                _calibrationThread.StopThread();
                _calibrationThread = null;
                _calibrationInProgress = false;
                yield break;
            }

            ShowCalibrationPanel = true;

            var enterResult = _calibrationThread.EnterCalibrationMode();

            // Wait for the call to finish
            yield return StartCoroutine(WaitForResult(enterResult));

            // Iterate through the calibration points.
            foreach (var pointPosition in _points)
            {
                // Set the local position and start the point animation
                _calibrationPoint.rectTransform.anchoredPosition = new Vector2(Screen.width * pointPosition.x, Screen.height * (1 - pointPosition.y));
                _pointScript.StartAnim();

                // Wait for animation.
                yield return new WaitForSeconds(1f);

                // As of this writing, adding a point takes about 175 ms. A failing add can take up to 3000 ms.
                var collectResult = _calibrationThread.CollectData(new CalibrationThread.Point(pointPosition));

                // Wait for the call to finish
                yield return StartCoroutine(WaitForResult(collectResult));

                // React to the result of adding a point.
                if (collectResult.Status == CalibrationStatus.Failure)
                {
                    Debug.Log("There was an error gathering data for this calibration point: " + pointPosition);
                }
            }

            // Compute and apply the result of the calibration. A succesful compute currently takes about 300 ms. A failure may bail out in a few ms.
            var computeResult = _calibrationThread.ComputeAndApply();

            // Wait for the call to finish
            yield return StartCoroutine(WaitForResult(computeResult));

            // Leave calibration mode.
            var leaveResult = _calibrationThread.LeaveCalibrationMode();

            // Wait for the call to finish
            yield return StartCoroutine(WaitForResult(leaveResult));

            // Stop the thread.
            _calibrationThread.StopThread();
            _calibrationThread = null;

            // Finish up or restart if failure.
            LatestCalibrationSuccessful = computeResult.Status == CalibrationStatus.Success;

            ShowCalibrationPanel = false;

            if (resultCallback != null)
            {
                resultCallback(LatestCalibrationSuccessful);
            }

            _calibrationInProgress = false;
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled() or inactive.
        /// </summary>
        private void OnDisable()
        {
            // Stop the calibration thread if it is not null.
            if (_calibrationThread != null)
            {
                var result = _calibrationThread.StopThread();
                _calibrationThread = null;
                Debug.Log("Calibration thread stopped: " + (result ? "YES" : "NO"));
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(_startKey))
            {
                var calibrationStartResult = StartCalibration(
                    resultCallback: (calibrationResult) =>
                        Debug.Log("Calibration was " + (calibrationResult ? "successful" : "unsuccessful"))
                    );

                Debug.Log("Calibration " + (calibrationStartResult ? "" : "not ") + "started");
            }
        }
    }
}
