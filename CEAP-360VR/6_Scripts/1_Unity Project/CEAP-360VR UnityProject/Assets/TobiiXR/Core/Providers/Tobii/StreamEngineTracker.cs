// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tobii.StreamEngine;
using UnityEngine;

namespace Tobii.XR
{
    internal static class StreamEngineUtils
    {
        internal static Vector3 ToVector3(this TobiiVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
    }

    public class StreamEngineTracker
    {
        private static readonly tobii_wearable_consumer_data_callback_t _wearableDataCallback = OnWearableData; // Needed to prevent GC from removing callback
        private StreamEngineConnection _connection;
        private Stopwatch _stopwatch = new Stopwatch();

        private bool _isReconnecting;
        private float _reconnectionTimestamp;
        private static bool _convergenceDistanceSupported;
        private GCHandle _nativePointerToSelf;

        public TobiiXR_EyeTrackingData LocalLatestData { get; private set; }

        public bool ReceivedDataThisFrame { get; private set; }

        public StreamEngineContext Context
        {
            get
            {
                if (_connection == null) return null;
                return _connection.Context;
            }
        }

        public StreamEngineTracker(FieldOfUse fieldOfUse, StreamEngineTracker_Description description = null, StreamEngineConnection connection = null)
        {
            if (description == null)
            {
                description = new StreamEngineTracker_Description();
            }

            if (connection == null)
            {
                connection = new StreamEngineConnection(new InteropWrapper());
            }

            LocalLatestData = new TobiiXR_EyeTrackingData();

            _connection = connection;

            if (TryConnectToTracker(fieldOfUse, _connection, _stopwatch, description) == false)
            {
                throw new Exception("Failed to connect to tracker");
            }

            _nativePointerToSelf = GCHandle.Alloc(this);
            if (SubscribeToWearableData(_connection.Context.Device, GCHandle.ToIntPtr(_nativePointerToSelf)) == false)
            {
                throw new Exception("Failed to subscribe to tracker");
            }

            CheckForCapabilities(_connection.Context.Device);
        }

        public void Tick()
        {
            ReceivedDataThisFrame = false;

            if (_isReconnecting)
            {
                // do not try to reconnect more than once every 500 ms
                if (Time.unscaledTime - _reconnectionTimestamp < 0.5f) return;

                var connected = _connection.TryReconnect();
                _isReconnecting = !connected;
                return;
            }
            var result = ProcessCallback(_connection.Context.Device, _stopwatch);
            if (result == tobii_error_t.TOBII_ERROR_CONNECTION_FAILED)
            {
                UnityEngine.Debug.Log("Reconnecting...");
                _reconnectionTimestamp = Time.unscaledTime;
                _isReconnecting = true;
            }
        }

        public void Destroy()
        {
            if (_nativePointerToSelf.IsAllocated) _nativePointerToSelf.Free();
            if (_connection == null) return;
            if (_connection.Context == null) return;
            var url = _connection.Context.Url;
            _connection.Close();

            UnityEngine.Debug.Log(string.Format("Disconnected from {0}", url));

            _stopwatch = null;
        }

        [AOT.MonoPInvokeCallback(typeof(tobii_wearable_consumer_data_callback_t))]
        private static void OnWearableData(ref tobii_wearable_consumer_data_t data, IntPtr user_data)
        {
            var gch = GCHandle.FromIntPtr(user_data);
            var t = (StreamEngineTracker)gch.Target;
            CopyEyeTrackingData(t.LocalLatestData, ref data);
            t.ReceivedDataThisFrame = true;
        }

        private static tobii_error_t ProcessCallback(IntPtr deviceContext, Stopwatch stopwatch)
        {
            StartStopwatch(stopwatch);
            var result = Interop.tobii_device_process_callbacks(deviceContext);
            var milliseconds = StopStopwatch(stopwatch);

            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to process callback. Error {0}", result));
            }

            if (milliseconds > 1)
            {
                UnityEngine.Debug.LogWarning(string.Format("Process callbacks took {0}ms", milliseconds));
            }

            return result;
        }

        private static void CopyEyeTrackingData(TobiiXR_EyeTrackingData latestDataLocalSpace, ref tobii_wearable_consumer_data_t data)
        {
            latestDataLocalSpace.GazeRay.IsValid = data.gaze_direction_combined_validity == tobii_validity_t.TOBII_VALIDITY_VALID && data.gaze_origin_combined_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
            latestDataLocalSpace.GazeRay.Origin.x = data.gaze_origin_combined_mm_xyz.x * -1 / 1000f;
            latestDataLocalSpace.GazeRay.Origin.y = data.gaze_origin_combined_mm_xyz.y / 1000f;
            latestDataLocalSpace.GazeRay.Origin.z = data.gaze_origin_combined_mm_xyz.z / 1000f;
            latestDataLocalSpace.GazeRay.Direction.x = data.gaze_direction_combined_normalized_xyz.x * -1;
            latestDataLocalSpace.GazeRay.Direction.y = data.gaze_direction_combined_normalized_xyz.y;
            latestDataLocalSpace.GazeRay.Direction.z = data.gaze_direction_combined_normalized_xyz.z;

            if (_convergenceDistanceSupported)
            {
                latestDataLocalSpace.ConvergenceDistance = data.convergence_distance_mm / 1000f;
                latestDataLocalSpace.ConvergenceDistanceIsValid = data.convergence_distance_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
            }

            latestDataLocalSpace.IsLeftEyeBlinking = data.left.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE || data.left.blink_validity == tobii_validity_t.TOBII_VALIDITY_INVALID;
            latestDataLocalSpace.IsRightEyeBlinking = data.right.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE || data.right.blink_validity == tobii_validity_t.TOBII_VALIDITY_INVALID;
        }

        private static long StopStopwatch(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static void StartStopwatch(Stopwatch stopwatch)
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        private static bool TryConnectToTracker(FieldOfUse fieldOfUse, StreamEngineConnection connection, Stopwatch stopwatch, StreamEngineTracker_Description description)
        {
            StartStopwatch(stopwatch);

            try
            {
                if (connection.Open(fieldOfUse, description) == false)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error connecting to eye tracker: " + e.ToString());
                return false;
            }

            var elapsedTime = StopStopwatch(stopwatch);

            UnityEngine.Debug.Log(string.Format("Connected to SE tracker: {0} and it took {1}ms", connection.Context.Url, elapsedTime));
            return true;
        }

        private static bool SubscribeToWearableData(IntPtr context, IntPtr userData)
        {
            tobii_error_t result;
            try
            {
                result = Interop.tobii_wearable_consumer_data_subscribe(context, _wearableDataCallback, userData);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Failed to subscribe to wearable stream: " + e.ToString());
                return false;
            }

            if (result == tobii_error_t.TOBII_ERROR_NO_ERROR) return true;
            UnityEngine.Debug.LogError("Failed to subscribe to wearable stream." + result);
            return false;
        }

        private static void CheckForCapabilities(IntPtr context)
        {
            bool supported;
            Interop.tobii_capability_supported(context, tobii_capability_t.TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_CONVERGENCE_DISTANCE, out supported);
            _convergenceDistanceSupported = supported;
        }
    }
}
