//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Collections;
using System.Threading;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public class EyeTrackerBase : MonoBehaviour
    {
        #region Public Properties

        /// <summary>
        /// Get the IEyeTracker instance.
        /// </summary>
        public IEyeTracker EyeTrackerInterface { get { return _eyeTracker; } }

        /// <summary>
        /// Get the number of gaze data items left in the queue.
        /// </summary>
        public virtual int GazeDataCount { get { return -1; } }

        /// <summary>
        /// Get how many unprocessed gaze data objects that are queued.
        /// </summary>
        public virtual int UnprocessedGazeDataCount { get { return -1; } }

        /// <summary>
        /// Is the eye tracker connected?
        /// </summary>
        public bool Connected { get { return _eyeTracker != null; } }

        /// <summary>
        /// Get the latest user position guide data.
        /// </summary>
        public IUserPositionGuideData LatestUserPositionGuideData
        {
            get
            {
                lock (_userPositionGuideLock)
                {
                    return _latestUserPositionGuideData;
                }
            }
        }

        /// <summary>
        /// Connect or disconnect the gaze stream.
        /// </summary>
        public virtual bool SubscribeToGazeData
        {
            get
            {
                return false;
            }

            set
            {
                UpdateSubscriptions();
            }
        }

        /// <summary>
        /// Connect or disconnect the user position guide stream. Requires that the eyetracker is connected.
        /// </summary>
        public bool SubscribeToUserPositionGuide
        {
            get
            {
                return _subscribingToUserPositionGuide;
            }

            set
            {
                if (Connected)
                {
                    if (value)
                    {
                        if (!_subscribingToUserPositionGuide)
                        {
                            _eyeTracker.UserPositionGuideReceived += OnUserPositionGuideReceived;
                            _subscribingToUserPositionGuide = true;
                        }
                    }
                    else
                    {
                        _eyeTracker.UserPositionGuideReceived -= OnUserPositionGuideReceived;
                        _subscribingToUserPositionGuide = false;
                    }
                }
            }
        }

        #endregion Public Properties

        #region Protected Fields

        /// <summary>
        /// The IEyeTracker instance.
        /// </summary>
        protected IEyeTracker _eyeTracker = null;

        /// <summary>
        /// Flag to remember if we are subscribing to gaze data.
        /// </summary>
        protected bool _subscribingToGazeData;

        /// <summary>
        /// Max queue size for gaze data. Example: For a 120 Hz tracker, this is
        /// a little more than a second. For 1200 Hz, a little more than 100 ms.
        /// </summary>
        protected const int _maxGazeDataQueueSize = 130;

        /// <summary>
        /// Thread for connection monitoring.
        /// </summary>
        protected Thread _autoConnectThread;

        /// <summary>
        /// Lock for communication with the thread.
        /// </summary>
        protected object _autoConnectLock = new object();

        /// <summary>
        /// Lock for the user position data.
        /// </summary>
        protected object _userPositionGuideLock = new object();

        /// <summary>
        /// The thread-running flag.
        /// </summary>
        protected bool _autoConnectThreadRunning;

        /// <summary>
        /// Locked access to the thread-runnign flag.
        /// </summary>
        protected bool AutoConnectThreadRunning
        {
            get
            {
                lock (_autoConnectLock)
                {
                    return _autoConnectThreadRunning;
                }
            }

            set
            {
                lock (_autoConnectLock)
                {
                    _autoConnectThreadRunning = value;
                }
            }
        }

        protected IEyeTracker _foundEyeTracker;

        protected IEyeTracker FoundEyeTracker
        {
            get
            {
                lock (_autoConnectLock)
                {
                    return _foundEyeTracker;
                }
            }

            set
            {
                lock (_autoConnectLock)
                {
                    _foundEyeTracker = value;
                }
            }
        }

        private bool _tooManyEyeTrackerInstances;

        private bool _subscribingToUserPositionGuide;

        /// <summary>
        /// Hold the latest user position guide data. Initialized to an invalid object.
        /// </summary>
        private IUserPositionGuideData _latestUserPositionGuideData = new UserPositionGuideData();

        #endregion Protected Fields

        #region Inspector Properties

        /// <summary>
        /// Flag to indicate if we want to subscribe to gaze data.
        /// </summary>
        [Tooltip("Checking this will subscribe to gaze at application startup.")]
        [SerializeField]
        protected bool _subscribeToGaze = true;

        #endregion Inspector Properties

        #region Unity Methods

        private void Awake()
        {
            if (FindObjectsOfType<EyeTrackerBase>().Length > 1)
            {
                _tooManyEyeTrackerInstances = true;
                Debug.LogError("Too many eye EyeTrackerBase instances. Please use only one [EyeTracker] or [VREyeTracker] in a scene.");
                return;
            }

            OnAwake();
        }

        private void Start()
        {
            if (_tooManyEyeTrackerInstances)
            {
                return;
            }

            OnStart();
        }

        private void Update()
        {
            OnUpdate();
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
            // Init autoconnect
            StartCoroutine(AutoConnectMonitoring());
        }

        protected virtual void OnUpdate()
        {
            // Check for state transitions to or from subscribing.
            UpdateSubscriptions();

            if (SubscribeToGazeData)
            {
                ProcessGazeEvents();
            }
        }

        private void OnDestroy()
        {
            StopAutoConnectThread();
        }

        private void OnApplicationQuit()
        {
            if (_tooManyEyeTrackerInstances)
            {
                return;
            }

            SubscribeToUserPositionGuide = false;

            EyeTrackingOperations.Terminate();
        }

        #endregion Unity Methods

        #region Protected and private Eye Tracking Methods

        protected virtual void ProcessGazeEvents()
        {
        }

        protected IEnumerator AutoConnectMonitoring()
        {
            StartAutoConnectThread();

            while (true)
            {
                if (_eyeTracker == null && FoundEyeTracker != null)
                {
                    _eyeTracker = FoundEyeTracker;
                    FoundEyeTracker = null;
                    UpdateSubscriptions();
                    StopAutoConnectThread();
                    Debug.Log("Connected to Eye Tracker: " + _eyeTracker.SerialNumber);
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        protected virtual void StartAutoConnectThread()
        {
        }

        protected void StopAutoConnectThread()
        {
            if (_autoConnectThread != null)
            {
                AutoConnectThreadRunning = false;
                _autoConnectThread.Join(1000);
                _autoConnectThread = null;
            }
        }

        protected virtual void UpdateSubscriptions()
        {
        }

        private void OnUserPositionGuideReceived(object sender, UserPositionGuideEventArgs e)
        {
            lock (_userPositionGuideLock)
            {
                _latestUserPositionGuideData = new UserPositionGuideData(e);
            }
        }

        #endregion Protected and private Eye Tracking Methods
    }
}