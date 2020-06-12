// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace Tobii.XR.DevTools
{
    public class DevToolsControllerManager : MonoBehaviour
    {
        private int _previousFrameCount;
        private Vector3 _velocity;
        private Quaternion _controllerLocalRotation;
        private Vector3 _controllerLocalPosition;
        private readonly List<XRNodeState> _nodeStates = new List<XRNodeState>();

        public enum ControllerButton
        {
            Trigger = KeyCode.JoystickButton15
        }

        private static DevToolsControllerManager _instance;

        /// <summary>
        /// Instance of the controller manager which can be statically accessed.
        /// </summary>
        public static DevToolsControllerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("ControllerManager must be added to a GameObject in the scene");
                }
                return _instance;
            }
        }

        /// <summary>
        /// The velocity of the controller.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                UpdateController();
                return _velocity;
            }
        }

        /// <summary>
        /// The position of the controller in world space. 
        /// </summary>
        public Vector3 Position
        {
            get
            {
                UpdateController();
                return transform.position + _controllerLocalPosition;
            }
        }

        /// <summary>
        /// The rotation of the controller in world space.
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                UpdateController();
                return transform.rotation * _controllerLocalRotation;
            }
        }

      

        // Use the right hand controller for tracking position and rotation.
        private const XRNode ControllerHand = XRNode.RightHand;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Update()
        {
            UpdateController();
        }

        /// <summary>
        /// Is a given button pressed or not during this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if button is being pressed this frame, otherwise false.</returns>
        public bool GetButtonPress(ControllerButton button)
        {
            UpdateController();
            return Input.GetKey((KeyCode) button);
        }

        /// <summary>
        /// Did a button go from not pressed to pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being up to being pressed down this frame, otherwise false.</returns>
        public bool GetButtonPressDown(ControllerButton button)
        {
            UpdateController();
            return Input.GetKeyDown((KeyCode) button);
        }

        /// <summary>
        /// Did a button go from pressed to not pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being pressed down to not being pressed this frame, otherwise false.</returns>
        public bool GetButtonPressUp(ControllerButton button)
        {

            UpdateController();
            return Input.GetKeyUp((KeyCode) button);
        }

        public void TriggerHapticPulse(ushort hapticStrength)
        {
        }


        /// <summary>
        /// Updates the controller state from OpenVR and Unity's InputTracking.
        /// </summary>
        private void UpdateController()
        {
            if (Time.frameCount != _previousFrameCount)
            {
                _previousFrameCount = Time.frameCount;
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

                Vector3 velocity;
                if (xrNodeState.TryGetVelocity(out velocity))
                {
                    _velocity = velocity;
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
    }
}
