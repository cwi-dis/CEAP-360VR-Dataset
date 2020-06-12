using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tobii.StreamEngine;

namespace Tobii.XR
{
    public class StreamEngineConnection
    {
        private static readonly tobii_device_url_receiver_t _deviceUrlReceiver = DeviceUrlReceiver; // Needed to prevent GC from removing callback
        private readonly tobii_custom_log_t _customLog;
        private readonly IStreamEngineInterop _interop;

        public StreamEngineConnection(IStreamEngineInterop interop)
        {
            _interop = interop;
            _customLog = new tobii_custom_log_t { log_func = LogCallback };
        }

        public StreamEngineContext Context { get; private set; }

        public bool Open(FieldOfUse fieldOfUse, StreamEngineTracker_Description description)
        {
            IntPtr apiContext;
            if (Context != null) throw new InvalidOperationException("There is already an instantiated connection");

            if (CreateApiContext(_interop, out apiContext, _customLog) == false) return false;

            List<string> connectedDevices;
            if (GetAvailableTrackers(_interop, apiContext, out connectedDevices) == false)
            {
                DestroyApiContext(_interop, apiContext);
                return false;
            }

            IntPtr deviceContext;
            string hmdEyeTrackerUrl;
            var interopFieldOfUse = fieldOfUse == FieldOfUse.Interactive ? Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_INTERACTIVE : Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_ANALYTICAL;
            if (GetFirstSupportedTracker(_interop, apiContext, interopFieldOfUse, connectedDevices, description, out deviceContext, out hmdEyeTrackerUrl) == false)
            {
                DestroyApiContext(_interop, apiContext);
                return false;
            }

            Context = new StreamEngineContext(apiContext, deviceContext, hmdEyeTrackerUrl);
            return true;
        }

        public void Close()
        {
            if (Context == null) return;

            DestroyDeviceContext(_interop, Context.Device);
            DestroyApiContext(_interop, Context.Api);

            Context = null;
        }

        public bool TryReconnect()
        {
            if (Context == null) throw new InvalidOperationException("No valid connection to retry");
            return ReconnectToDevice(_interop, Context.Device);
        }

        private static bool CreateDeviceContext(IStreamEngineInterop interop, string url, Interop.tobii_field_of_use_t fieldOfUse, IntPtr apiContext, string[] licenseKeys, out IntPtr deviceContext)
        {
            if (licenseKeys == null || licenseKeys.Length == 0)
            {
                return interop.tobii_device_create(apiContext, url, fieldOfUse, out deviceContext) == tobii_error_t.TOBII_ERROR_NO_ERROR;
            }

            var licenseResults = new List<tobii_license_validation_result_t>();
            var result = interop.tobii_device_create_ex(apiContext, url, fieldOfUse, licenseKeys, licenseResults, out deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to create device context for {0}. {1}", url, result));
                return false;
            }

            for (int i = 0; i < licenseKeys.Length; i++)
            {
                var licenseResult = licenseResults[i];
                if (licenseResult == tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_OK) continue;

                UnityEngine.Debug.LogError("License " + licenseKeys[i] + " failed. Return code " + licenseResult);
            }

            return true;
        }

        private static void DestroyDeviceContext(IStreamEngineInterop interop, IntPtr deviceContext)
        {
            if (deviceContext == IntPtr.Zero) return;

            var result = interop.tobii_device_destroy(deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to destroy device context. Error {0}", result));
            }
        }

        private static bool CreateApiContext(IStreamEngineInterop interop, out IntPtr apiContext, tobii_custom_log_t customLog = null)
        {
            var result = interop.tobii_api_create(out apiContext, customLog);
            if (result == tobii_error_t.TOBII_ERROR_NO_ERROR) return true;

            UnityEngine.Debug.LogError("Failed to create api context. " + result);
            apiContext = IntPtr.Zero;
            return false;
        }

        private static void DestroyApiContext(IStreamEngineInterop interop, IntPtr apiContext)
        {
            if (apiContext == IntPtr.Zero) return;

            var result = interop.tobii_api_destroy(apiContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to destroy api context. Error {0}", result));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Interop.tobii_log_func_t))]
        private static void LogCallback(IntPtr log_context, tobii_log_level_t level, string text)
        {
            UnityEngine.Debug.Log(text);
        }

        [AOT.MonoPInvokeCallback(typeof(tobii_device_url_receiver_t))]
        private static void DeviceUrlReceiver(string url, IntPtr user_data)
        {
            GCHandle gch = GCHandle.FromIntPtr(user_data);
            var urls = (List<string>)gch.Target;
            urls.Add(url);
        }

        private static bool GetAvailableTrackers(IStreamEngineInterop interop, IntPtr apiContext, out List<string> connectedDevices)
        {
            connectedDevices = new List<string>();
            GCHandle gch = GCHandle.Alloc(connectedDevices);
            var result = interop.tobii_enumerate_local_device_urls_internal(apiContext, _deviceUrlReceiver, GCHandle.ToIntPtr(gch));
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError("Failed to enumerate connected devices. " + result);
                return false;
            }

            if (connectedDevices.Count >= 1) return true;

            UnityEngine.Debug.LogWarning("No connected eye trackers found.");
            return false;
        }

        private static bool GetFirstSupportedTracker(IStreamEngineInterop interop, IntPtr apiContext, Interop.tobii_field_of_use_t fieldOfUse, IList<string> connectedDevices, StreamEngineTracker_Description description, out IntPtr deviceContext, out string deviceUrl)
        {
            var index = -1;
            deviceContext = IntPtr.Zero;
            deviceUrl = "";

            for (var i = 0; i < connectedDevices.Count; i++)
            {
                var connectedDeviceUrl = connectedDevices[i];
                if (CreateDeviceContext(interop, connectedDeviceUrl, fieldOfUse, apiContext, description.License, out deviceContext) == false) return false;

                tobii_device_info_t info;
                var result = interop.tobii_get_device_info(deviceContext, out info);
                if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    DestroyDeviceContext(interop, deviceContext);
                    UnityEngine.Debug.LogWarning("Failed to get device info. " + result);
                    return false;
                }

                var integrationType = info.integration_type.ToLowerInvariant();
                if (integrationType != description.SupportedIntegrationType)
                {
                    DestroyDeviceContext(interop, deviceContext);
                    continue;
                }

                index = i;
                deviceUrl = connectedDeviceUrl;
                break;
            }

            if (index != -1) return true;

            UnityEngine.Debug.LogWarning(string.Format("Failed to find Tobii eye trackers of integration type {0}", description.SupportedIntegrationType));
            DestroyDeviceContext(interop, deviceContext);
            return false;
        }

        private static bool ReconnectToDevice(IStreamEngineInterop interop, IntPtr deviceContext)
        {
            var nativeContext = deviceContext;
            var result = interop.tobii_device_reconnect(nativeContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR) return false;

            UnityEngine.Debug.Log("Reconnected.");
            return true;
        }
    }

    public interface IStreamEngineInterop
    {
        tobii_error_t tobii_api_create(out IntPtr apiContext, tobii_custom_log_t logger);
        tobii_error_t tobii_api_destroy(IntPtr apiContext);
        tobii_error_t tobii_device_create(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, out IntPtr device);
        tobii_error_t tobii_device_create_ex(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, string[] license_keys, List<tobii_license_validation_result_t> license_results, out IntPtr device);
        tobii_error_t tobii_device_destroy(IntPtr deviceContext);
        tobii_error_t tobii_device_reconnect(IntPtr nativeContext);
        tobii_error_t tobii_enumerate_local_device_urls_internal(IntPtr apiContext, tobii_device_url_receiver_t receiverFunction, IntPtr userData);
        tobii_error_t tobii_get_device_info(IntPtr deviceContext, out tobii_device_info_t info);
    }

    internal class InteropWrapper : IStreamEngineInterop
    {
        public tobii_error_t tobii_api_create(out IntPtr apiContext, tobii_custom_log_t logger)
        {
            return Interop.tobii_api_create(out apiContext, logger);
        }

        public tobii_error_t tobii_api_destroy(IntPtr apiContext)
        {
            return Interop.tobii_api_destroy(apiContext);
        }

        public tobii_error_t tobii_device_create(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, out IntPtr device)
        {
            return Interop.tobii_device_create(api, url, field_of_use, out device);
        }

        public tobii_error_t tobii_device_create_ex(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, string[] license_keys, List<tobii_license_validation_result_t> license_results, out IntPtr device)
        {
            return Interop.tobii_device_create_ex(api, url, field_of_use, license_keys, license_results, out device);
        }

        public tobii_error_t tobii_device_destroy(IntPtr deviceContext)
        {
            return Interop.tobii_device_destroy(deviceContext);
        }

        public tobii_error_t tobii_device_reconnect(IntPtr nativeContext)
        {
            return Interop.tobii_device_reconnect(nativeContext);
        }

        public tobii_error_t tobii_enumerate_local_device_urls_internal(IntPtr apiContext, tobii_device_url_receiver_t receiverFunction, IntPtr userData)
        {
            return Interop.tobii_enumerate_local_device_urls_internal(apiContext, receiverFunction, userData);
        }

        public tobii_error_t tobii_get_device_info(IntPtr deviceContext, out tobii_device_info_t info)
        {
            return Interop.tobii_get_device_info(deviceContext, out info);
        }
    }
}
