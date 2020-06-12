using UnityEngine;
using Tobii.Valve;

namespace Tobii.XR
{
    public class OpenVRManager
    {
        private readonly TrackedDevicePose_t[] _poseArray = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        private bool _trackingLost;

        public static bool IsAvailable()
        {
            try
            {
                if (!OpenVR.IsHmdPresent())
                    return false;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        public Matrix4x4 GetHeadPoseFor(float secondsAgo)
        {
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(OpenVR.Compositor.GetTrackingSpace(), -secondsAgo,
                _poseArray);
            if (_poseArray[OpenVR.k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
            {
                _trackingLost = false;
            }
            else
            {
                if (!_trackingLost) Debug.Log("Failed to get historical pose"); // Only log once
                _trackingLost = true;
                return Matrix4x4.identity;
            }

            return ToMatrix4x4(_poseArray[OpenVR.k_unTrackedDeviceIndex_Hmd].mDeviceToAbsoluteTracking);
        }

        private static Matrix4x4 ToMatrix4x4(HmdMatrix34_t pose)
        {
            var m = Matrix4x4.identity;

            m[0, 0] = pose.m0;
            m[0, 1] = pose.m1;
            m[0, 2] = -pose.m2;
            m[0, 3] = pose.m3;

            m[1, 0] = pose.m4;
            m[1, 1] = pose.m5;
            m[1, 2] = -pose.m6;
            m[1, 3] = pose.m7;

            m[2, 0] = -pose.m8;
            m[2, 1] = -pose.m9;
            m[2, 2] = pose.m10;
            m[2, 3] = -pose.m11;

            return m;
        }
    }
}
