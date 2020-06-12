// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using Tobii.XR;
using UnityEngine;

namespace Tobii.XR.Examples
{
    using Tobii.G2OM;
    
    /// <summary>
    /// Monobehaviour which will initialize Tobii XR with a custom G2OM to be able to dynamically change which objects should be able to receive gaze focus. It does this by registering and deregistering objects.
    /// </summary>
    public class TobiiXRThrowingSceneManager : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GazeGrab _gazeGrabComponent;
#pragma warning restore 649

        public TobiiXR_Settings Settings;

        private enum GazeObjectType
        {
            GazeThrowTarget = 0,
            GazeGrabbableObject
        }

        private GazeObjectType _relevantGazeObjectType;

        private GazeGrabbableObject[] _gazeGrabbableObjects;
        private GazeThrowTarget[] _gazeThrowTargets;

        private G2OM _g2omInstance;

        private const KeyCode ToggleG2OMVisualizationKeyCode = KeyCode.Space;

        private const int DisabledLayer = 1;
        private const int EnabledLayer = 2;


        private void Awake()
        {
            // Get all objects of the type and save them to a list.
            _gazeGrabbableObjects = FindObjectsOfType<GazeGrabbableObject>();
            _gazeThrowTargets = FindObjectsOfType<GazeThrowTarget>();

            // Create a custom G2OM description and set the layer mask to be used for switching between which objects should be focusable
            var description = new G2OM_Description
            {
                LayerMask = EnabledLayer << 1
            };

            _g2omInstance = G2OM.Create(description);
            Settings.G2OM = _g2omInstance;

            TobiiXR.Start(Settings);

            // Start by enabling the grabbable objects to receive focus from G2OM.
            EnableGrabbableObjects();
        }

        private void Update()
        {
            // If an object is being grabbed, set the relevant gaze objects to targets, otherwise set them to grabbable objects.
            if (_gazeGrabComponent.IsObjectGrabbing)
            {
                SetRelevantGazeObjectType(GazeObjectType.GazeThrowTarget);
            }
            else
            {
                SetRelevantGazeObjectType(GazeObjectType.GazeGrabbableObject);
            }
        }

        /// <summary>
        /// Set the relevant game objects for G2OM to track.
        /// </summary>
        /// <param name="type">The type of gaze interactable object to track.</param>
        private void SetRelevantGazeObjectType(GazeObjectType type)
        {
            if (type == _relevantGazeObjectType) return;

            _g2omInstance.Clear();

            switch (type)
            {
                case GazeObjectType.GazeThrowTarget:
                    EnableThrowingTargets();
                    break;
                case GazeObjectType.GazeGrabbableObject:
                    EnableGrabbableObjects();
                    break;
            }

            _relevantGazeObjectType = type;
        }

        /// <summary>
        /// Make the grabbable objects enabled for receiving focus, and disable the gaze throw targets
        /// </summary>
        private void EnableGrabbableObjects()
        {
            foreach (var gazeGrabbableObject in _gazeGrabbableObjects)
            {
                gazeGrabbableObject.gameObject.layer = EnabledLayer;
            }

            foreach (var gazeThrowTarget in _gazeThrowTargets)
            {
                gazeThrowTarget.gameObject.layer = DisabledLayer;
            }
        }

        /// <summary>
        /// Make the throw targets enabled for receiving focus, and disable the grabbable objects
        /// </summary>
        private void EnableThrowingTargets()
        {
            foreach (var gazeGrabbableObject in _gazeGrabbableObjects)
            {
                gazeGrabbableObject.gameObject.layer = DisabledLayer;
            }

            foreach (var gazeThrowTarget in _gazeThrowTargets)
            {
                gazeThrowTarget.gameObject.layer = EnabledLayer;
            }
        }
    }
}
