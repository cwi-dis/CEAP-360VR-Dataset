// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Updates models for hands which follow the controllers.
/// </summary>
public class HandleHands : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField, Tooltip("Left hand prefab")]
    private GameObject _leftHandPrefab;

    [SerializeField, Tooltip("Right hand prefab")]
    private GameObject _rightHandPrefab;

    [SerializeField, Tooltip("Position hands locally relative to player or world")]
    private bool _positionHandsLocally;
#pragma warning restore 649

    private GameObject _leftHandGameObject;
    private GameObject _rightHandGameObject;
    private Transform _cameraTransform;
    private readonly List<XRNodeState> _nodeStates = new List<XRNodeState>();

    void Start ()
	{
        // Catch controller off or out of range.
	    InputTracking.trackingLost += InputTrackingOnTrackingLost;

	    // Catch controller on or back in range.
	    InputTracking.trackingAcquired += InputTrackingOnTrackingAcquired;

        // Instantiate hands.
        _leftHandGameObject = Instantiate(_leftHandPrefab, transform);
        _rightHandGameObject = Instantiate(_rightHandPrefab, transform);

	    HideInactiveHands();

	    _cameraTransform = Tobii.XR.CameraHelper.GetCameraTransform();
    }

    void Update ()
    {
        InputTracking.GetNodeStates(_nodeStates);
        foreach (var xrNodeState in _nodeStates.Where(xrNodeState => xrNodeState.tracked))
        {
            if (xrNodeState.nodeType != XRNode.LeftHand && xrNodeState.nodeType != XRNode.RightHand) continue;

            Vector3 position;
            Quaternion rotation;
            var go = xrNodeState.nodeType == XRNode.LeftHand ? _leftHandGameObject : _rightHandGameObject;
            if (xrNodeState.TryGetPosition(out position))
            {
                if (_positionHandsLocally) position -= _cameraTransform.position;
                go.transform.localPosition = position;
            }
            if (xrNodeState.TryGetRotation(out rotation)) go.transform.localRotation = rotation;
        }
    }

    /// <summary>
    /// Handles when input tracking is lost on an XRNode.
    /// </summary>
    /// <param name="obj">The node which lost tracking.</param>
    private void InputTrackingOnTrackingLost(XRNodeState obj)
    {
        // If its the right controller that's lost.
        if (_rightHandGameObject && obj.nodeType == XRNode.RightHand)
            _rightHandGameObject.SetActive(false);

        // If its the left controller that's lost.
        if (_leftHandGameObject && obj.nodeType == XRNode.LeftHand)
            _leftHandGameObject.SetActive(false);
    }
    /// <summary>
    /// Handles when input tracking is acquired for an XRNode.
    /// </summary>
    /// <param name="obj">The node which acquired tracking.</param>
    private void InputTrackingOnTrackingAcquired(XRNodeState obj)
    {
        // If its the right controller that's been acquired.
        if (_rightHandGameObject && obj.nodeType == XRNode.RightHand)
            _rightHandGameObject.SetActive(true);

        // If its the left controller that's been acquired.
        if (_leftHandGameObject && obj.nodeType == XRNode.LeftHand)
            _leftHandGameObject.SetActive(true);
    }

    /// <summary>
    /// Used to enable or disable the hand game objects depending on the state of the tracking for the right and left hand.
    /// </summary>
    private void HideInactiveHands()
    {
        var nodeStates = new List<XRNodeState>();
        InputTracking.GetNodeStates(nodeStates);

        var leftFound = false;
        var rightFound = false;
        foreach (var xrNodeState in nodeStates)
        {
            if (xrNodeState.nodeType == XRNode.RightHand && xrNodeState.tracked)
            {
                rightFound = true;
            }

            if (xrNodeState.nodeType == XRNode.LeftHand && !xrNodeState.tracked)
            {
                leftFound = true;
            }
        }

        _rightHandGameObject.SetActive(rightFound);
        _leftHandGameObject.SetActive(leftFound);
    }
}
