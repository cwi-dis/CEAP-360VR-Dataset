//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public class VRGazeTrail : GazeTrailBase
    {
        /// <summary>
        /// Get <see cref="VRGazeTrail"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static VRGazeTrail Instance { get; private set; }

        private VREyeTracker _eyeTracker;
        private VRCalibration _calibrationObject;

        protected override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _eyeTracker = VREyeTracker.Instance;
            _calibrationObject = VRCalibration.Instance;
        }

        protected override bool GetRay(out Ray ray)
        {
            if (_eyeTracker == null)
            {
                return base.GetRay(out ray);
            }

            var data = _eyeTracker.LatestGazeData;
            ray = data.CombinedGazeRayWorld;
            return data.CombinedGazeRayWorldValid;
        }

        protected override bool HasEyeTracker
        {
            get
            {
                return _eyeTracker != null;
            }
        }

        protected override bool CalibrationInProgress
        {
            get
            {
                return _calibrationObject != null ? _calibrationObject.CalibrationInProgress : false;
            }
        }
    }
}