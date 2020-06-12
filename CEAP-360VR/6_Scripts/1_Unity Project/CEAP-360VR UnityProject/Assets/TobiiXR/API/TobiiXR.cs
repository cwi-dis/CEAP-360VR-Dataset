// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using System.Collections.Generic;
    using Tobii.G2OM;
    using Tobii.XR.Internal;
    using UnityEngine;
    
    /// <summary>
    /// Static access point for Tobii XR eye tracker data.
    /// </summary>
    public static class TobiiXR
    {
        private static readonly TobiiXRInternal _internal = new TobiiXRInternal();
        private static GameObject _updaterGameObject;
        private static readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
        private static readonly TobiiXR_EyeTrackingData _eyeTrackingDataWorld = new TobiiXR_EyeTrackingData();

        /// <summary>
        /// Gets eye tracking data in the selected tracking space. Unless the underlying eye tracking
        /// provider does prediction, this data is not predicted.
        /// Subsequent calls within the same frame will return the same value.
        /// </summary>
        /// <param name="trackingSpace">The tracking space to report eye tracking data in.</param>
        /// <returns>The last (newest) <see cref="TobiiXR_EyeTrackingData"/>.</returns>
        public static TobiiXR_EyeTrackingData GetEyeTrackingData(TobiiXR_TrackingSpace trackingSpace)
        {
            VerifyInstanceIntegrity();

            switch (trackingSpace)
            {
                case TobiiXR_TrackingSpace.Local:
                    return _eyeTrackingDataLocal;
                case TobiiXR_TrackingSpace.World:
                    return _eyeTrackingDataWorld;
            }

            throw new System.Exception("Unknown tracking space: " + trackingSpace);
        }

        /// <summary>
        /// Gets all possible <see cref="FocusedCandidate"/> with gaze focus. Only game 
        /// objects with a <see cref="IGazeFocusable"/> component can be focused 
        /// using gaze.
        /// </summary>
        /// <returns>A list of <see cref="FocusedCandidate"/> in descending order of probability.</returns>
        public static List<FocusedCandidate> FocusedObjects
        {
            get
            {
                VerifyInstanceIntegrity();

                return Internal.G2OM.GazeFocusedObjects;
            }
        }

        public static bool Start(TobiiXR_Settings settings = null)
        {
            if (!TobiiEula.IsEulaAccepted())
            {
                Debug.LogWarning("You need to accept Tobii Software Development License Agreement to use Tobii XR Unity SDK.");
            }

            if (Internal.Provider != null)
            {
                Debug.LogWarning(string.Format("TobiiXR already started with provider ({0})", Internal.Provider));
                VerifyInstanceIntegrity();
                return false;
            }

            if (settings == null)
            {
                settings = new TobiiXR_Settings();
            }

            if (settings.FieldOfUse == FieldOfUse.NotSelected)
            {
                //For more info, see https://developer.tobii.com/vr/develop/unity/documentation/configure-tobii-xr/
                Debug.LogError("Field of use has not been selected. Please specify intended field of use in TobiiXR_Settings");
            }

            Internal.Provider = settings.EyeTrackingProvider;

            if (Internal.Provider == null)
            {
                Internal.Provider = new NoseDirectionProvider();
                Debug.LogWarning(string.Format("All configured providers failed. Using ({0}) as fallback", Internal.Provider.GetType().Name));
            }

            Debug.Log(string.Format("Starting TobiiXR with ({0}) as provider for eye tracking", Internal.Provider));

            Internal.Settings = settings;
            
            if(settings.G2OM != null)
            {
                Internal.G2OM = settings.G2OM;
            }
            else
            {
                Internal.G2OM = Tobii.G2OM.G2OM.Create(new G2OM_Description
                {
                    LayerMask = settings.LayerMask,
                    HowLongToKeepCandidatesInSeconds = settings.HowLongToKeepCandidatesInSeconds
                });
            }
            
            VerifyInstanceIntegrity();

            return true;
        }

        public static void Stop()
        {
            if (Internal.Provider == null) return;

            Internal.G2OM.Destroy();
            Internal.Provider.Destroy();

            if (_updaterGameObject != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Object.Destroy(_updaterGameObject.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(_updaterGameObject.gameObject);
                }
#else
            Object.Destroy(_updaterGameObject.gameObject);
#endif
            }


            _updaterGameObject = null;
            Internal.G2OM = null;
            Internal.Provider = null;
        }

        private static void VerifyInstanceIntegrity()
        {
            if (_updaterGameObject != null) return;

            _updaterGameObject = new GameObject("TobiiXR")
            {
                hideFlags = HideFlags.HideInHierarchy
            };

            if (Internal.Provider == null)
            {
                Start();
            }

            var updater = _updaterGameObject.AddComponent<TobiiXR_Lifecycle>();
            updater.OnUpdateAction += Tick;
            updater.OnDisableAction += Internal.G2OM.Clear;
            updater.OnApplicationQuitAction += Stop;
        }

        private static void Tick()
        {
            Internal.Provider.Tick();
            EyeTrackingDataHelper.Copy(Internal.Provider.EyeTrackingDataLocal, _eyeTrackingDataLocal);
            EyeTrackingDataHelper.TransformGazeData(Internal.Provider.EyeTrackingDataLocal, _eyeTrackingDataWorld, Internal.Provider.LocalToWorldMatrix);

            if (Internal.Filter != null && Internal.Filter.enabled)
            {
                var worldForward = Internal.Provider.LocalToWorldMatrix.MultiplyVector(Vector3.forward);
                Internal.Filter.Filter(_eyeTrackingDataLocal, Vector3.forward);
                Internal.Filter.Filter(_eyeTrackingDataWorld, worldForward);
            }
            var g2omData = CreateG2OMData(_eyeTrackingDataWorld);
            Internal.G2OM.Tick(g2omData);
        }

        private static G2OM_DeviceData CreateG2OMData(TobiiXR_EyeTrackingData data)
        {
            var t = Internal.Provider.LocalToWorldMatrix;
            return new G2OM_DeviceData {
                timestamp = data.Timestamp,
                gaze_ray_world_space = new G2OM_GazeRay{
                    is_valid = data.GazeRay.IsValid.ToByte(),
                    ray = G2OM_UnityExtensionMethods.CreateRay(data.GazeRay.Origin, data.GazeRay.Direction),
                },
                camera_up_direction_world_space = t.MultiplyVector(Vector3.up).AsG2OMVector3(),
                camera_right_direction_world_space = t.MultiplyVector(Vector3.right).AsG2OMVector3()
            };
        }

        /// <summary>
        /// For advanced and internal use only. Do not access this field before TobiiXR.Start has been called.
        /// Do not save a reference to the fields exposed by this class since TobiiXR will recreate them when restarted
        /// </summary>
        public static TobiiXRInternal Internal { get { return _internal; } }

        public class TobiiXRInternal
        {
            public TobiiXR_Settings Settings { get; internal set; }

            public IEyeTrackingProvider Provider { get; set; }

            public G2OM G2OM { get; internal set; }

            /// <summary>
            /// Defaults to no filter. If set, both EyeTrackingData and FocusedObjects will apply this filter to gaze data before using it
            /// </summary>
            public EyeTrackingFilterBase Filter { get { return Settings == null ? null : Settings.EyeTrackingFilter as EyeTrackingFilterBase; } }
        }
    }
}