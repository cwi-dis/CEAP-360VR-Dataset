using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Tobii.XR.Examples
{
    using Valve = global::Valve;
    
    public class OpenVRControllerAdapter : IControllerAdapter
    {
        private readonly List<XRNodeState> _nodeStates = new List<XRNodeState>();
        private uint _controllerIndex = Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid;
        private Valve.VR.VRControllerState_t _controllerState, _previousControllerState;
        private int _previousFrameCount;
        private Vector3 _angularVelocity;
        private Vector3 _velocity;
        private Quaternion _controllerLocalRotation;
        private Vector3 _controllerLocalPosition;

        // Use the right hand controller for tracking position and rotation.
        private const XRNode ControllerHand = XRNode.RightHand;
        private const Valve.VR.ETrackedControllerRole ControllerRole = Valve.VR.ETrackedControllerRole.RightHand;
        private const uint VibrationAxisId = 0u;

        public Vector3 Velocity
        {
            get
            {
                UpdateController();
                return _velocity;
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                UpdateController();
                return _angularVelocity;
            }
        }

        public Vector3 Position
        {
            get
            {
                UpdateController();
                return _controllerLocalPosition;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                UpdateController();
                return _controllerLocalRotation;
            }
        }

        public bool GetButtonPress(ControllerButton button)
        {
            var b = EVRButtonIdFrom(button);
            UpdateController();
            var buttonMask = (1ul << (int) b);
            return (_controllerState.ulButtonPressed & buttonMask) != 0;
        }

        public bool GetButtonPressDown(ControllerButton button)
        {
            var b = EVRButtonIdFrom(button);
            UpdateController();
            var buttonMask = (1ul << (int) b);
            return (_controllerState.ulButtonPressed & buttonMask) != 0 &&
                   (_previousControllerState.ulButtonPressed & buttonMask) == 0;
        }

        public bool GetButtonPressUp(ControllerButton button)
        {
            var b = EVRButtonIdFrom(button);
            UpdateController();
            var buttonMask = (1ul << (int) b);
            return (_controllerState.ulButtonPressed & buttonMask) == 0 &&
                   (_previousControllerState.ulButtonPressed & buttonMask) != 0;
        }

        public bool GetButtonTouch(ControllerButton button)
        {
            var b = EVRButtonIdFrom(button);
            UpdateController();
            var buttonMask = (1ul << (int) b);
            return (_controllerState.ulButtonTouched & buttonMask) != 0;
        }

        public bool GetButtonTouchDown(ControllerButton button)
        {
            var b = EVRButtonIdFrom(button);
            UpdateController();
            var buttonMask = (1ul << (int) b);
            return (_controllerState.ulButtonTouched & buttonMask) != 0 &&
                   (_previousControllerState.ulButtonTouched & buttonMask) == 0;
        }

        public bool GetButtonTouchUp(ControllerButton button)
        {
            var b = EVRButtonIdFrom(button);
            UpdateController();
            var buttonMask = (1ul << (int) b);
            return (_controllerState.ulButtonTouched & buttonMask) == 0 &&
                   (_previousControllerState.ulButtonTouched & buttonMask) != 0;
        }

        public void TriggerHapticPulse(ushort pulseDurationMicroSeconds)
        {
            var system = Valve.VR.OpenVR.System;
            if (system != null)
            {
                system.TriggerHapticPulse(_controllerIndex, VibrationAxisId, (char) pulseDurationMicroSeconds);
            }
        }

        public Vector2 GetTouchpadAxis()
        {
            UpdateController();
            return new Vector2(_controllerState.rAxis0.x, _controllerState.rAxis0.y);
        }

        /// <summary>
        /// Updates the controller state from OpenVR and Unity's InputTracking.
        /// </summary>
        private void UpdateController()
        {
            if (Time.frameCount != _previousFrameCount)
            {
                _previousFrameCount = Time.frameCount;
                _previousControllerState = _controllerState;

                var system = Valve.VR.OpenVR.System;
                if (system != null)
                {
                    _controllerIndex = system.GetTrackedDeviceIndexForControllerRole(ControllerRole);
                    system.GetControllerState(_controllerIndex, ref _controllerState,
                        (uint) System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VRControllerState_t)));
                }

                UpdateControllerPositionAndRotation();
            }
        }

        /// <summary>
        /// Updates the position, rotation, and velocity of the controller.
        /// </summary>
        private void UpdateControllerPositionAndRotation()
        {
            // Use Unity's InputTracking to get the velocity and angular velocity of the controller
            InputTracking.GetNodeStates(_nodeStates);
            foreach (var xrNodeState in _nodeStates)
            {
                if (xrNodeState.nodeType != ControllerHand) continue;

                if (!xrNodeState.tracked) return;

                Vector3 velocityLocal;
                if (xrNodeState.TryGetVelocity(out velocityLocal))
                {
                    _velocity = velocityLocal;
                }

                Vector3 angularVelocity;
                if (xrNodeState.TryGetAngularVelocity(out angularVelocity))
                {
                    _angularVelocity = angularVelocity;
                }

                Vector3 position;
                if (xrNodeState.TryGetPosition(out position))
                {
                    _controllerLocalPosition = position;
                }

                Quaternion rotation;
                if (xrNodeState.TryGetRotation(out rotation))
                {
                    _controllerLocalRotation = rotation;
                }
            }
        }

        private static Valve.VR.EVRButtonId EVRButtonIdFrom(ControllerButton button)
        {
            switch (button)
            {
                case ControllerButton.Menu:
                    return Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
                case ControllerButton.Touchpad:
                    return Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
                case ControllerButton.Trigger:
                    return Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
                default:
                    throw new System.Exception("Unmapped controller button: " + button.ToString());
            }
        }
    }
}
