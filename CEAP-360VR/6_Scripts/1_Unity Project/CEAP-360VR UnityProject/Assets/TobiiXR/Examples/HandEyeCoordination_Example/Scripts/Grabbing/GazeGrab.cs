using Tobii.XR;
using UnityEngine;
using UnityEngine.Events;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Monobehaviour which can be put on the player's controller to allow grabbing a <see cref="GazeGrabbableObject"/> by looking at it and pressing a button on the controller.
    /// </summary>
    [RequireComponent(typeof(ControllerManager))]
    public class GazeGrab : MonoBehaviour
    {
        // Action which will be triggered when the object is released from the controller.
        public UnityAction<GameObject> OnObjectReleased;

        public bool IsObjectGrabbing { get; private set; }

        private enum GrabState
        {
            Idle,
            Grabbing,
            Grabbed
        }

        private GrabState _currentGrabState = GrabState.Idle;

        // Fields to keep track of objects which currently have focus or are grabbed.
        private GazeGrabbableObject _grabbedObject;
        private Rigidbody _grabbedObjectRigidBody;
        private GazeGrabbableObject _focusedGameObject;
        private float _focusedGameObjectTime;
        private float _objectGazeStickinessSeconds;

        // Fields related to animating the object flying to the hand.
        private float _grabAnimationProgress;
        private Vector3 _startPosition;
        private Quaternion _startControllerRotation;
        private Quaternion _startObjectRotation;
        private float _flyToControllerTimeSeconds;
        private AnimationCurve _animationCurve;


        // Multiplier to the velocity when releasing the object from the hand.
        private const float ObjectVelocityMultiplier = 1.2f;

        // Distance at which the object snaps to the controller.
        private const float ObjectSnapDistance = 0.1f;
        private const ControllerButton TriggerButton = ControllerButton.Trigger;

        private void Update()
        {
            UpdateFocusedObject();
            UpdateObjectState();
        }

        private void UpdateObjectState()
        {
            // If the object is the "idle" state, and the user looks at the object and presses the grab button, start moving the object to the controller.
            if (_currentGrabState == GrabState.Idle)
            {
                if (_focusedGameObject != null && ControllerManager.Instance.GetButtonPressDown(TriggerButton))
                {
                    ChangeObjectState(GrabState.Grabbing);
                }
            }

            // The object is in its "grabbing" state, meaning it's moving towards the controller.
            if (_currentGrabState == GrabState.Grabbing)
            {
                // If the grab* button is held, move the object towards the controller using a lerp function.
                if (ControllerManager.Instance.GetButtonPress(TriggerButton))
                {
                    _grabAnimationProgress += Time.deltaTime / _flyToControllerTimeSeconds;
                    _grabbedObjectRigidBody.position = Vector3.Lerp(_startPosition, ControllerManager.Instance.Position,
                        _animationCurve.Evaluate(_grabAnimationProgress));

                    // If the distance between the controller and the object is close enough, grab the object.
                    if (Vector3.Distance(_grabbedObjectRigidBody.position, ControllerManager.Instance.Position) <
                        ObjectSnapDistance)
                    {
                        ChangeObjectState(GrabState.Grabbed);
                    }
                }
                // If the grab button is released, drop the object.
                else if (!ControllerManager.Instance.GetButtonPress(TriggerButton))
                {
                    ChangeObjectState(GrabState.Idle);
                }
            }

            // If the object is currently being grabbed and the grab button is released, apply the controller's velocity to the object and invoke OnObjectReleased.
            if (_currentGrabState == GrabState.Grabbed)
            {
                if (ControllerManager.Instance.GetButtonPress(TriggerButton))
                {
                    // Keeps the object's original rotation as a starting point, and is otherwise locked to the controller
                    _grabbedObjectRigidBody.rotation =
                        (ControllerManager.Instance.Rotation * Quaternion.Inverse(_startControllerRotation)) *
                        _startObjectRotation;
                    _grabbedObjectRigidBody.position = ControllerManager.Instance.Position;
                }
                else
                {
                    _grabbedObjectRigidBody.angularVelocity =
                        ControllerManager.Instance.AngularVelocity * ObjectVelocityMultiplier;
                    _grabbedObjectRigidBody.velocity = ControllerManager.Instance.Velocity * ObjectVelocityMultiplier;

                    if (OnObjectReleased != null)
                    {
                        OnObjectReleased.Invoke(_grabbedObject.gameObject);
                    }

                    ChangeObjectState(GrabState.Idle);
                }
            }
        }

        /// <summary>
        /// Called when the object transitions from one <see cref="GrabState"/> to another.
        /// </summary>
        /// <param name="state">The state the object should transition to.</param>
        private void ChangeObjectState(GrabState state)
        {
            _currentGrabState = state;

            switch (state)
            {
                // Inform the object that it has been ungrabbed and set it to not be kinematic.
                case GrabState.Idle:
                    IsObjectGrabbing = false;
                    _grabbedObject.ObjectUngrabbed();
                    _grabbedObjectRigidBody.isKinematic = false;
                    break;
                // When the user starts grabbing the object, save the object and store its animation values.
                case GrabState.Grabbing:
                    IsObjectGrabbing = true;
                    _grabbedObject = _focusedGameObject;
                    _grabbedObject.ObjectGrabbing();
                    _grabbedObjectRigidBody = _focusedGameObject.GetComponent<Rigidbody>();
                    _grabbedObjectRigidBody.isKinematic = true;
                    _startObjectRotation = _grabbedObject.transform.rotation;
                    _startControllerRotation = ControllerManager.Instance.Rotation;
                    _startPosition = _grabbedObject.transform.position;
                    _grabAnimationProgress = 0f;
                    break;
                // When the object becomes grabbed to the controller, call the grabbed method and set the object's position to the hand.
                case GrabState.Grabbed:
                    ControllerManager.Instance.TriggerHapticPulse(2000);
                    _grabbedObject.ObjectGrabbed();
                    _grabbedObject.transform.position = ControllerManager.Instance.Position;
                    break;
            }
        }

        /// <summary>
        /// Updates the currently focused <see cref="GazeGrabbableObject"/> and keeps it focused for the time set by the object, <see cref="GazeGrabbableObject.GazeStickinessSeconds"/>.
        /// </summary>
        private void UpdateFocusedObject()
        {
            // Check whether Tobii XR has any focused objects.
            foreach (var focusedCandidate in TobiiXR.FocusedObjects)
            {
                var focusedObject = focusedCandidate.GameObject;
                // The candidate list is ordered so that the most likely object is first in the list.
                // So let's try to find the first focused object that also has the GazeGrabbableObject component and save it.
                if (focusedObject != null && focusedObject.GetComponent<GazeGrabbableObject>())
                {
                    SetNewFocusedObject(focusedObject);
                    break;
                }
            }

            // If enough time has passed since the object was last focused, mark it as not focused.
            if (Time.time > _focusedGameObjectTime + _objectGazeStickinessSeconds)
            {
                _focusedGameObjectTime = float.NaN;
                _focusedGameObject = null;
                _objectGazeStickinessSeconds = 0f;
            }
        }

        /// <summary>
        /// Store the values attached to focused object.
        /// </summary>
        /// <param name="focusedObject">The new focused object to store.</param>
        private void SetNewFocusedObject(GameObject focusedObject)
        {
            _focusedGameObject = focusedObject.GetComponent<GazeGrabbableObject>();
            _objectGazeStickinessSeconds = _focusedGameObject.GazeStickinessSeconds;
            _animationCurve = _focusedGameObject.AnimationCurve;
            _flyToControllerTimeSeconds = _focusedGameObject.FlyToControllerTimeSeconds;
            _focusedGameObjectTime = Time.time;
        }
    }
}
