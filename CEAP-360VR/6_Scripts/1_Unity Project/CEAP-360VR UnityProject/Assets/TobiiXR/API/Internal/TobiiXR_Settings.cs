// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using UnityEngine;
    using Tobii.G2OM;
    using System;
    using Tobii.XR.Internal;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class TobiiXR_Settings
    {
        [Serializable]
        public struct ProviderElement
        {
            private string _displayName;

            public string TypeName;
            public string DisplayName
            {
                get
                {
                    if (!string.IsNullOrEmpty(_displayName)) return _displayName;
                    if (string.IsNullOrEmpty(TypeName)) return "Unknown";

                    _displayName = AssemblyUtils.GetCachedDisplayNameFor(TypeName);
                    return _displayName;
                }
            }

            public static ProviderElement FromProviderType(Type type)
            {
                return new ProviderElement
                {
                    TypeName = type.FullName,
                };
            }
        }

        private static readonly RuntimePlatform _platform = Application.platform;
        private IEyeTrackingProvider _eyeTrackingProvider;

        public IEyeTrackingProvider EyeTrackingProvider
        {
            get
            {
                if (_eyeTrackingProvider != null) return _eyeTrackingProvider;
                _eyeTrackingProvider = GetProvider();
                return _eyeTrackingProvider;
            }
            set
            {
                _eyeTrackingProvider = value;
            }
        }

        public Tobii.G2OM.G2OM G2OM { get; set; }

        public EyeTrackingFilterBase EyeTrackingFilter;

        public FieldOfUse FieldOfUse = FieldOfUse.NotSelected;

        [HideInInspector]
        public List<ProviderElement> StandaloneEyeTrackingProviders = new List<ProviderElement>
        {
            ProviderElement.FromProviderType(typeof(TobiiProvider)),
            ProviderElement.FromProviderType(typeof(NoseDirectionProvider)),
            ProviderElement.FromProviderType(typeof(MouseProvider)),
        };

        [HideInInspector]
        public List<ProviderElement> AndroidEyeTrackingProviders = new List<ProviderElement>
        {
            ProviderElement.FromProviderType(typeof(TobiiProvider)),
            ProviderElement.FromProviderType(typeof(NoseDirectionProvider)),
            ProviderElement.FromProviderType(typeof(MouseProvider)),
        };

        public LayerMask LayerMask = G2OM_Description.DefaultLayerMask;

        public float HowLongToKeepCandidatesInSeconds = G2OM_Description.DefaultCandidateMemoryInSeconds;

        public IEyeTrackingProvider GetProvider()
        {
            var eyeTrackingProviders = _platform == RuntimePlatform.Android ? AndroidEyeTrackingProviders : StandaloneEyeTrackingProviders;

            // Get first viable provider from list
            var providerResult = eyeTrackingProviders
                .Select(element => AssemblyUtils.EyetrackingProviderType(element.TypeName))
                .Where(type => type != null)
                .Select(GetProviderFrom)
                .FirstOrDefault(provider => provider.Initialize(FieldOfUse));

            return providerResult;
        }

        private static IEyeTrackingProvider GetProviderFrom(Type type)
        {
            if (type == null) return null;
            try
            {
                return Activator.CreateInstance(type) as IEyeTrackingProvider;
            }
            catch (Exception) { }
            return null;
        }
    }
}