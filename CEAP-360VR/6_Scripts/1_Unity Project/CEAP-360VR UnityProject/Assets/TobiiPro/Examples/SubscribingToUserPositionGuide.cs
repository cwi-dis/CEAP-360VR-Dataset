using System.Collections.Generic;
using System.Linq;
using Tobii.Research;
using UnityEngine;

namespace Tobii.Research.Unity.CodeExamples
{
    // the events in the SDK are called on a thread internal to the SDK. That thread can not safely set values
    // that are to be read on the main thread. The simplest way to make it safe is to enqueue the date, and dequeue it
    // on the main thread, e.g. via Update() in a MonoBehaviour.
    class SubscribingToUserPositionGuide : MonoBehaviour
    {
        private IEyeTracker _eyeTracker;
        private Queue<UserPositionGuideEventArgs> _queue = new Queue<UserPositionGuideEventArgs>();

        void Awake()
        {
            var trackers = EyeTrackingOperations.FindAllEyeTrackers();
            foreach (IEyeTracker eyeTracker in trackers)
            {
                Debug.Log(string.Format("{0}, {1}, {2}, {3}, {4}", eyeTracker.Address, eyeTracker.DeviceName, eyeTracker.Model, eyeTracker.SerialNumber, eyeTracker.FirmwareVersion));
            }
            _eyeTracker = trackers.FirstOrDefault(s => (s.DeviceCapabilities & Capabilities.HasGazeData) != 0);
            if (_eyeTracker == null)
            {
                Debug.Log("No screen based eye tracker detected!");
            }
            else
            {
                Debug.Log("Selected eye tracker with serial number {0}" + _eyeTracker.SerialNumber);
            }
        }

        void Update()
        {
            PumpUserPositionGuide();
        }

        void OnEnable()
        {
            if (_eyeTracker != null)
            {
                Debug.Log("Calling OnEnable with eyetracker: " + _eyeTracker.DeviceName);
                _eyeTracker.UserPositionGuideReceived += EnqueueUserPositionGuide;
            }
        }

        void OnDisable()
        {
            if (_eyeTracker != null)
            {
                _eyeTracker.UserPositionGuideReceived -= EnqueueUserPositionGuide;
            }
        }

        void OnDestroy()
        {
            EyeTrackingOperations.Terminate();
        }

        // This method will be called on a thread belonging to the SDK, and can not safely change values
        // that will be read from the main thread.
        private void EnqueueUserPositionGuide(object sender, UserPositionGuideEventArgs e)
        {
            lock (_queue)
            {
                _queue.Enqueue(e);
            }
        }

        private UserPositionGuideEventArgs GetNextUserPositionGuide()
        {
            lock (_queue)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : null;
            }
        }

        private void PumpUserPositionGuide()
        {
            var next = GetNextUserPositionGuide();
            while (next != null)
            {
                HandleUserPositionGuide(next);
                next = GetNextUserPositionGuide();
            }
        }

        // This method will be called on the main Unity thread
        private void HandleUserPositionGuide(UserPositionGuideEventArgs e)
        {
            // Do something with user position guide
            // Debug.Log(string.Format(
            //     "Got user position guide with validity: {0} with normalized coordinates ({1}, {2}, {3}).",
            //     e.LeftEye.Validity,
            //     e.LeftEye.UserPosition.X,
            //     e.LeftEye.UserPosition.Y,
            //     e.LeftEye.UserPosition.Z));
        }
    }
}
