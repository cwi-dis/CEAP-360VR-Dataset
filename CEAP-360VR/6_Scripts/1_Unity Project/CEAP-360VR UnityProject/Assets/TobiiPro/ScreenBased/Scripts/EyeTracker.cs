//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Threading;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public class EyeTracker : EyeTrackerBase
    {
        #region Public Properties

        /// <summary>
        /// Get <see cref="EyeTracker"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static EyeTracker Instance { get; private set; }

        /// <summary>
        /// Get the latest gaze data. If there are new arrivals,
        /// they will be processed before returning.
        /// </summary>
        public IGazeData LatestGazeData
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
        public IGazeData LatestProcessedGazeData { get { return _latestGazeData; } }

        /// <summary>
        /// Pop and get the next gaze data object from the queue.
        /// </summary>
        public IGazeData NextData
        {
            get
            {
                if (_gazeDataQueue.Count < 1)
                {
                    return default(IGazeData);
                }

                return _gazeDataQueue.Next;
            }
        }

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

        [SerializeField]
        [Tooltip("Connect to the first found eye tracker. Otherwise use provided serial number.")]
        private bool _connectToFirst;

        [SerializeField]
        [Tooltip("Check for this specific eyetracker serial number. Matches start of string so a partial start of a serial number can be used.")]
        private string _eyeTrackerSerialStart = "IS";

        #endregion Inspector Properties

        #region Private Fields

        /// <summary>
        /// Locked access and size management.
        /// </summary>
        private LockedQueue<GazeDataEventArgs> _originalGazeData = new LockedQueue<GazeDataEventArgs>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// Size managed queue.
        /// </summary>
        private SizedQueue<IGazeData> _gazeDataQueue = new SizedQueue<IGazeData>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// Hold the latest processed gaze data. Initialized to an invalid object.
        /// </summary>
        private IGazeData _latestGazeData = new GazeData();

        #endregion Private Fields

        #region Unity Methods

        protected override void OnAwake()
        {
            Instance = this;
            base.OnAwake();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        #endregion Unity Methods

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

                gazeData = new GazeData(originalGaze);
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
                        if (_connectToFirst || eyeTrackerEntry.SerialNumber.StartsWith(_eyeTrackerSerialStart))
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

            if (_subscribeToGaze && !_subscribingToGazeData)
            {
                _eyeTracker.GazeDataReceived += GazeDataReceivedCallback;
                _subscribingToGazeData = true;
            }
            else if (!_subscribeToGaze && _subscribingToGazeData)
            {
                _eyeTracker.GazeDataReceived -= GazeDataReceivedCallback;
                _subscribingToGazeData = false;
            }
        }

        private void GazeDataReceivedCallback(object sender, GazeDataEventArgs eventArgs)
        {
            _originalGazeData.Next = eventArgs;
        }

        #endregion Private Eye Tracking Methods
    }
}