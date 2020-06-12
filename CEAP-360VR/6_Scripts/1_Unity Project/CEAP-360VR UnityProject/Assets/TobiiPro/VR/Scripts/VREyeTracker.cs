//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

//-----------------------------------------------------------------------
// Un-comment USE_OPENVR_BINDINGS to use OpenVR bindings if they are
// available and the camera is not moved from its original position. That
// will give more accurate world gaze data than the prediction offset.
// See readme_hmd_pose_prediction.txt for more information.
//-----------------------------------------------------------------------
//#define USE_OPENVR_BINDINGS

using System.Threading;
using UnityEngine;

#if USE_OPENVR_BINDINGS

using Valve.VR;

#endif

namespace Tobii.Research.Unity
{
    public class VREyeTracker : EyeTrackerBase
    {
        #region Public Properties

        /// <summary>
        /// Get <see cref="VREyeTracker"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static VREyeTracker Instance { get; private set; }

        /// <summary>
        /// Get the latest gaze data. If there are new arrivals,
        /// they will be processed before returning.
        /// </summary>
        public IVRGazeData LatestGazeData
        {
            get
            {
                if (UnprocessedGazeDataCount > 0)
                {
                    // We have more data.
                    ProcessGazeEvents();
                }

                return _latestGazeData;
            }
        }

        /// <summary>
        /// Get the latest processed processed gaze data.
        /// Don't care if there a newer one has arrived.
        /// </summary>
        public IVRGazeData LatestProcessedGazeData { get { return _latestGazeData; } }

        /// <summary>
        /// Pop and get the next gaze data object from the queue.
        /// </summary>
        public IVRGazeData NextData
        {
            get
            {
                if (_gazeDataQueue.Count < 1)
                {
                    return default(IVRGazeData);
                }

                return _gazeDataQueue.Next;
            }
        }

        /// <summary>
        /// Connect or disconnect the gaze stream.
        /// </summary>
        public override bool SubscribeToGazeData
        {
            get
            {
                return _subscribeToGaze;
            }

            set
            {
                _subscribeToGaze = value;
                base.SubscribeToGazeData = value;
            }
        }

        public override int GazeDataCount { get { return _gazeDataQueue.Count; } }

        public override int UnprocessedGazeDataCount { get { return _originalGazeData.Count; } }

        #endregion Public Properties

        #region Inspector Properties

#if !USE_OPENVR_BINDINGS
        /// <summary>
        /// Estimated HMD prediction offset in milliseconds. Some testing has shown about 34 milliseconds to give an OK approximation in some cases,
        /// See readme_hmd_pose_prediction.txt for more information.
        /// </summary>
        [Tooltip("Estimated HMD prediction offset in milliseconds.")]
        [Range(0f, 70f)]
        [SerializeField]
        private float _hmdPosePredictionOffset = 34;
#endif

        #endregion Inspector Properties

        #region Private Fields

        ///// <summary>
        ///// The IEyeTracker instance.
        ///// </summary>
        //private IEyeTracker _eyeTracker = null;

        /// <summary>
        /// Flag to remember if we are subscribing to gaze data.
        /// </summary>
        private bool _subscribingToHMDGazeData;

        /// <summary>
        /// The eye tracker origin.
        /// </summary>
        private Transform _eyeTrackerOrigin;

        /// <summary>
        /// Locked access and size management.
        /// </summary>
        private LockedQueue<HMDGazeDataEventArgs> _originalGazeData = new LockedQueue<HMDGazeDataEventArgs>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// Size managed queue.
        /// </summary>
        private SizedQueue<IVRGazeData> _gazeDataQueue = new SizedQueue<IVRGazeData>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// The list of eye tracker poses kept for each frame.
        /// Just keep roughly one second of poses at 90 fps. 100 is a nice round number.
        /// </summary>
        private PoseList _eyeTrackerOriginPoses = new PoseList(100);

        /// <summary>
        /// Hold the latest processed gaze data. Initialized to an invalid object.
        /// </summary>
        private IVRGazeData _latestGazeData = new VRGazeData();

#if USE_OPENVR_BINDINGS
        private TrackedDevicePose_t[] poseArray = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
#endif

        #endregion Private Fields

        #region Override Methods

        protected override void OnAwake()
        {
            Instance = this;
            base.OnAwake();
        }

        protected override void OnStart()
        {
            // The eye tracker origin is not exactly in the camera position when using Vive in Unity.
            _eyeTrackerOrigin = VRUtility.EyeTrackerOriginVive;

            base.OnStart();
        }

        protected override void OnUpdate()
        {
#if !USE_OPENVR_BINDINGS
            // Save the current pose for the current time adding estimated pose prediction time offset.
            _eyeTrackerOriginPoses.Add(_eyeTrackerOrigin.GetPose(EyeTrackingOperations.GetSystemTimeStamp() + Mathf.RoundToInt(_hmdPosePredictionOffset * 1000f)));
#endif
            base.OnUpdate();
        }

        #endregion Override Methods

        #region Private Eye Tracking Methods

        protected override void ProcessGazeEvents()
        {
            const int maxIterations = 20;

            var gazeData = _latestGazeData;

            for (int i = 0; i < maxIterations; i++)
            {
                var originalGaze = _originalGazeData.Next;

                // Queue empty
                if (originalGaze == null)
                {
                    break;
                }

#if USE_OPENVR_BINDINGS
                var now = EyeTrackingOperations.GetSystemTimeStamp();
                var backInTime = (originalGaze.SystemTimeStamp - now) / 1000000.0f;

                // Look up OpenVR pose back when the eyetracker looked at the eyes.
                OpenVR.System.GetDeviceToAbsoluteTrackingPose(OpenVR.Compositor.GetTrackingSpace(), backInTime, poseArray);
                if (!poseArray[OpenVR.k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
                {
                    Debug.Log("Failed to get historical pose");
                    continue;
                }

                var bestMatchingPose = HMDPoseToETPose(poseArray[OpenVR.k_unTrackedDeviceIndex_Hmd].mDeviceToAbsoluteTracking, now);
#else
                var bestMatchingPose = _eyeTrackerOriginPoses.GetBestMatchingPose(originalGaze.SystemTimeStamp);
                if (!bestMatchingPose.Valid)
                {
                    Debug.Log("Did not find a matching pose");
                    continue;
                }
#endif

                gazeData = new VRGazeData(originalGaze, bestMatchingPose);
                _gazeDataQueue.Next = gazeData;
            }

            var queueCount = UnprocessedGazeDataCount;
            if (queueCount > 0)
            {
                Debug.LogWarning("We didn't manage to empty the queue: " + queueCount + " items left...");
            }

            _latestGazeData = gazeData;
        }

        protected override void StartAutoConnectThread()
        {
            if (_autoConnectThread != null)
            {
                return;
            }

            _autoConnectThread = new Thread(() =>
            {
                AutoConnectThreadRunning = true;

                while (AutoConnectThreadRunning)
                {
                    var eyeTrackers = EyeTrackingOperations.FindAllEyeTrackers();

                    foreach (var eyeTrackerEntry in eyeTrackers)
                    {
                        if (eyeTrackerEntry.SerialNumber.StartsWith("VR"))
                        {
                            FoundEyeTracker = eyeTrackerEntry;
                            AutoConnectThreadRunning = false;
                            return;
                        }
                    }

                    Thread.Sleep(200);
                }
            });

            _autoConnectThread.IsBackground = true;
            _autoConnectThread.Start();
        }

        protected override void UpdateSubscriptions()
        {
            if (_eyeTracker == null)
            {
                return;
            }

            if (SubscribeToGazeData && !_subscribingToHMDGazeData)
            {
                _eyeTracker.HMDGazeDataReceived += HMDGazeDataReceivedCallback;
                _subscribingToHMDGazeData = true;
            }
            else if (!SubscribeToGazeData && _subscribingToHMDGazeData)
            {
                _eyeTracker.HMDGazeDataReceived -= HMDGazeDataReceivedCallback;
                _subscribingToHMDGazeData = false;
            }
        }

        private void HMDGazeDataReceivedCallback(object sender, HMDGazeDataEventArgs eventArgs)
        {
            _originalGazeData.Next = eventArgs;
        }

#if USE_OPENVR_BINDINGS

        public EyeTrackerOriginPose HMDPoseToETPose(HmdMatrix34_t pose, long timeStamp)
        {
            var rw = Mathf.Sqrt(Mathf.Max(0, 1 + pose.m0 + pose.m5 + pose.m10)) / 2;
            var rx = Mathf.Sqrt(Mathf.Max(0, 1 + pose.m0 - pose.m5 - pose.m10)) / 2;
            var ry = Mathf.Sqrt(Mathf.Max(0, 1 - pose.m0 + pose.m5 - pose.m10)) / 2;
            var rz = Mathf.Sqrt(Mathf.Max(0, 1 - pose.m0 - pose.m5 + pose.m10)) / 2;
            rx = Mathf.Abs(rx) * Mathf.Sign(pose.m6 - pose.m9);
            ry = Mathf.Abs(ry) * Mathf.Sign(pose.m8 - pose.m2);
            rz = Mathf.Abs(rz) * Mathf.Sign(pose.m4 - pose.m1);
            return new EyeTrackerOriginPose(timeStamp, new Vector3(pose.m3, pose.m7, -pose.m11), new Quaternion(rx, ry, rz, rw));
        }

#endif

        internal bool UsingOpenVR
        {
            get
            {
#if USE_OPENVR_BINDINGS
                return true;
#else
                return false;
#endif
            }
        }

        #endregion Private Eye Tracking Methods
    }
}