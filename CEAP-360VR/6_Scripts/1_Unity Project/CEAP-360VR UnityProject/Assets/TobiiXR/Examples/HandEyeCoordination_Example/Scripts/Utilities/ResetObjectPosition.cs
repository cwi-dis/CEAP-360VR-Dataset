// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Monobehaviour which can be put on objects to allow their position and rotation to be reset by pressing a button on the Vive controller
    /// </summary>
    public class ResetObjectPosition : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Rigidbody _rigidbody;

        private const KeyCode ResetButton = KeyCode.JoystickButton0;

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;

            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKeyUp(ResetButton))
            {
                transform.position = _startPosition;
                transform.rotation = _startRotation;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}
