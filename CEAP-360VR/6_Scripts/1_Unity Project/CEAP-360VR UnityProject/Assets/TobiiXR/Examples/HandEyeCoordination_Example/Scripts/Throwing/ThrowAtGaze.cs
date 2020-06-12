// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.HEC;
using Tobii.XR;
using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// MonoBehaviour attached which will listen to events when an object is released and use the user's gaze to see what is being aimed at, and try to adjust the trajectory to hit.
    /// </summary>
    [RequireComponent(typeof(GazeGrab))]
    public class ThrowAtGaze : MonoBehaviour
    {
        // Fields related to the gaze focused objects.
        private GameObject _focusedTargetGameObject;
        private float _focusedTargetGameObjectTime;

        // The time before clearing a focused object after it has lost focus.
        private const float GazeObjectMemoryTimeInSeconds = 0.2f;

        private readonly float _gravity = Physics.gravity.y;

        private void Start()
        {
            GetComponent<GazeGrab>().OnObjectReleased += OnObjectReleased;
        }

        private void Update()
        {
            UpdateFocusedObject();
        }

        /// <summary>
        /// Called when an object is released from the controller.
        /// </summary>
        /// <param name="thrownGameObject">The object that has been released</param>
        private void OnObjectReleased(GameObject thrownGameObject)
        {
            var focusedObject = _focusedTargetGameObject;
            var thrownObject = thrownGameObject.GetComponent<GazeThrowableObject>();

            // Check if there is a focused object, and also if the thrown object has the GazeThrowableObject component on it.
            if (focusedObject != null && thrownObject != null)
            {
                // Try to rotate the throw to hit the target. If the angle is too large the method call will return false and we will return.
                Vector3 rotatedVelocity;
                if (!TryRotateThrow(thrownObject, focusedObject, out rotatedVelocity))
                {
                    return;
                }

                // If the throw could be rotated correctly, try to adjust the velocity to hit the target.
                Vector3 adjustedVelocity;
                if (TryAdjustTrajectory(thrownObject, focusedObject, rotatedVelocity, out adjustedVelocity))
                {
                    thrownObject.GetComponent<Rigidbody>().velocity = adjustedVelocity;
                }
            }
        }

        /// <summary>
        /// Adjusts the throw by checking whether the XZ angle threshold is within the allowed range, and if so, adjust the rotation.
        /// </summary>
        /// <param name="thrownObject">The thrown <see cref="GazeThrowableObject"/>.</param>
        /// <param name="focusedGameObject">The focused GameObject.</param>
        /// <param name="rotatedVelocity">The rotated velocity of the thrown object.</param>
        /// <returns>True if the throw was within the angle threshold as set by the thrown object, otherwise false.</returns>
        private bool TryRotateThrow(GazeThrowableObject thrownObject, GameObject focusedGameObject,
            out Vector3 rotatedVelocity)
        {
            rotatedVelocity = Vector3.zero;
            // Get the velocity of the thrown object and project it on the XZ plane by zeroing the y component.
            var velocity = thrownObject.GetComponent<Rigidbody>().velocity;
            var velocityXz = velocity;
            velocityXz.y = 0;

            // Get the direction from the thrown object to the target and project on XZ plane.
            var correctDirectionXz = focusedGameObject.transform.position - thrownObject.transform.position;
            correctDirectionXz.y = 0;


            // Measure angle between the throw direction and the direction to the object.
            var angle = Vector3.Angle(velocityXz, correctDirectionXz);

            // If the angle is within the threshold set by the object, apply the rotation to the throw and return it as the rotatedVelocity variable.
            if (angle < thrownObject.XzAngleThresholdDegrees)
            {
                var side = AngleDir(velocityXz, correctDirectionXz, Vector3.up);
                rotatedVelocity = Quaternion.AngleAxis(angle * side, Vector3.up) * velocity;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to adjust the trajectory of a thrown object to hit a focused object by modifying the thrown object's trajectory with values set on the <see cref="GazeThrowableObject"/>.
        /// </summary>
        /// <param name="thrownObject">The <see cref="GazeThrowableObject"/> component of the thrown game object.</param>
        /// <param name="focusedGameObject">The focused game object.</param>
        /// <param name="initialVelocity">The initial velocity of the thrown game object.</param>
        /// <param name="throwVelocity">The resulting throw velocity.</param>
        /// <returns>True if the trajectory could be modified to hit the target.</returns>
        private bool TryAdjustTrajectory(GazeThrowableObject thrownObject, GameObject focusedGameObject,
            Vector3 initialVelocity, out Vector3 throwVelocity)
        {
            throwVelocity = Vector3.zero;

            // Get the bounds of the target object to be able to calculate a collision.
            var bounds = focusedGameObject.GetComponent<Collider>().bounds;

            // Call the interop to calculate the throw, passing in the position of the throw, bounds of target as well as the values configured in the GazeThrowableObject.
            var result = Interop.HEC_Calculate_Throw(
                thrownObject.transform.position,
                bounds.min,
                bounds.max,
                initialVelocity,
                _gravity,
                thrownObject.LowerVelocityMultiplier,
                thrownObject.UpperVelocityMultiplier,
                thrownObject.MaxYVelocityModifier,
                out throwVelocity
            );

            // If the result of the method call was that there was no solution, return false, otherwise there is a solution.
            if (result == HEC_Error.NoSolution)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate if a direction is left or right of another direction
        /// </summary>
        /// <param name="fwd"></param>
        /// <param name="targetDir"></param>
        /// <param name="up"></param>
        /// <returns>-1 when to the left, 1 to the right, and 0 for forward/backward</returns>
        public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            var perp = Vector3.Cross(fwd, targetDir);
            var dir = Vector3.Dot(perp, up);

            if (dir > 0.0f)
            {
                return 1.0f;
            }
            else if (dir < 0.0f)
            {
                return -1.0f;
            }

            return 0.0f;
        }

        /// <summary>
        /// Get the currently focused object and store it for up to <see cref="GazeObjectMemoryTimeInSeconds"/> seconds.
        /// </summary>
        private void UpdateFocusedObject()
        {
            // Check whether Tobii XR has any focused objects.
            foreach (var focusedCandidate in TobiiXR.FocusedObjects)
            {
                var focusedObject = focusedCandidate.GameObject;

                // Find the first, if any, focused object with the GazeThrowTarget component and save it.
                if (focusedObject != null && focusedObject.GetComponent<GazeThrowTarget>())
                {
                    _focusedTargetGameObject = focusedObject;
                    _focusedTargetGameObjectTime = Time.time;
                    break;
                }
            }

            // If enough time has passed since the object was last focused, mark it as not focused.
            if (Time.time > _focusedTargetGameObjectTime + GazeObjectMemoryTimeInSeconds)
            {
                _focusedTargetGameObjectTime = float.NaN;
                _focusedTargetGameObject = null;
            }
        }
    }
}
