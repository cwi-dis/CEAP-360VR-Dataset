// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Tobii.XR
{
    public class HmdToWorldTransformer
    {
        struct HeadPoseSample
        {
            public long timestamp_us;
            public Matrix4x4 matrix;
        }

        private const int _estimatedPrediction_us = 33000;
        private readonly float _estimatedEyeTrackerLatency_s;
        private readonly int _headPoseDelayInFrames;
        private readonly HeadPoseSample[] _history;
        private readonly bool _useOpenVR = false;
        private Transform _cameraTransform;
        private int _writeIndex = 0;
        private OpenVRManager _openVRManager = new OpenVRManager();
        private readonly long _ticksToMicroSeconds;
        private List<XRNodeState> _nodeStates = new List<XRNodeState>();
        private Matrix4x4 _lastLocalToWorldTransform = Matrix4x4.identity;


        public Transform CameraTransform { get { return _cameraTransform; } }

        public HmdToWorldTransformer(float estimatedEyeTrackerLatency_s)
        {
            _estimatedEyeTrackerLatency_s = estimatedEyeTrackerLatency_s;
            var latency_s = (_estimatedPrediction_us / 1000000f) + estimatedEyeTrackerLatency_s;
            var frameLength_s = 1 / (XRDevice.refreshRate > 1 ? XRDevice.refreshRate : 90);
            _headPoseDelayInFrames = Mathf.CeilToInt(latency_s / frameLength_s);
            _history = new HeadPoseSample[_headPoseDelayInFrames + 1];
            _cameraTransform = CameraHelper.GetCameraTransform();
            _useOpenVR = OpenVRManager.IsAvailable();
            _ticksToMicroSeconds = 1000000L / Stopwatch.Frequency;
        }

        public void Tick()
        {
            _history[_writeIndex].timestamp_us = Stopwatch.GetTimestamp() * _ticksToMicroSeconds + _estimatedPrediction_us;
            _history[_writeIndex].matrix = GetCameraLocalToWorldMatrix();
            _writeIndex = (_writeIndex + 1) % _history.Length;
        }

        public Matrix4x4 GetLocalToWorldMatrix()
        {
            InputTracking.GetNodeStates(_nodeStates);
            var centerEyeNodeIndex = _nodeStates.FindIndex(x=> x.nodeType == XRNode.CenterEye);
            var headNodeIndex = _nodeStates.FindIndex(x=> x.nodeType == XRNode.Head);
            if (centerEyeNodeIndex < 0 || headNodeIndex < 0) return _lastLocalToWorldTransform;

            var centerEyeNode = _nodeStates[centerEyeNodeIndex];
            var headNode = _nodeStates[headNodeIndex];

            Vector3 headPosition;
            Vector3 centerEyePosition;
            if (!centerEyeNode.TryGetPosition(out centerEyePosition) || !headNode.TryGetPosition(out headPosition))
                return _lastLocalToWorldTransform;

            var centerEyeToHeadTranslation = Matrix4x4.Translate(headPosition - centerEyePosition);

            if (_useOpenVR)
            {
                Quaternion centerEyeRotation;
                if (!centerEyeNode.TryGetRotation(out centerEyeRotation))
                    return _lastLocalToWorldTransform;

                var cameraToHmdOffsetTransform = GetCameraLocalToWorldMatrix() * Matrix4x4.TRS(centerEyePosition, centerEyeRotation, Vector3.one).inverse;
                var headPose = _openVRManager.GetHeadPoseFor(secondsAgo: _estimatedEyeTrackerLatency_s);
                _lastLocalToWorldTransform = centerEyeToHeadTranslation * (cameraToHmdOffsetTransform * headPose);
            }
            else
            {
                var sample = _history[(_history.Length + _writeIndex - 1 - _headPoseDelayInFrames) % _history.Length];
                if (sample.timestamp_us == 0) return GetCameraLocalToWorldMatrix();
                _lastLocalToWorldTransform = centerEyeToHeadTranslation * sample.matrix;
            }

            return _lastLocalToWorldTransform;
        }

        private Matrix4x4 GetCameraLocalToWorldMatrix()
        {
            if (_cameraTransform != null && _cameraTransform.gameObject.activeInHierarchy)
            {
                return _cameraTransform.localToWorldMatrix;
            }

            UnityEngine.Debug.Log("Camera transform invalid. Trying to retrieve a new.");
            _cameraTransform = CameraHelper.GetCameraTransform();
            return _cameraTransform != null ? _cameraTransform.localToWorldMatrix : Matrix4x4.identity;
        }
    }
}
