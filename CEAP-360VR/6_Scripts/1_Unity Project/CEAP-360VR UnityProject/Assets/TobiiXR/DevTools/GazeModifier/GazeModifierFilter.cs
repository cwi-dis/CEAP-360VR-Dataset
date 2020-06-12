// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

namespace Tobii.XR.GazeModifier
{
    public class GazeModifierFilter : EyeTrackingFilterBase
    {
        public GazeModifierSettings Settings
        {
            get { return _settings; }
        }

        public IEnumerable<IGazeModifier> Modifiers
        {
            get
            {
                if (_modifiers == null)
                {
                    _modifiers = new List<IGazeModifier>()
                        {_accuracyModifier, _precisionModifier, new TrackabilityModifier(_settings)};
                }

                return _modifiers;
            }

            set { _modifiers = value; }
        }

        [SerializeField] private GazeModifierSettings _settings = new GazeModifierSettings();

        private IEnumerable<IGazeModifier> _modifiers;
        private readonly AccuracyModifier _accuracyModifier;
        private readonly PrecisionModifier _precisionModifier;

        public GazeModifierFilter()
        {
            _accuracyModifier = new AccuracyModifier(_settings);
            _precisionModifier = new PrecisionModifier(_settings);

        }
        private void Start()
        {
            // Don't remove this function. Without this function there will be no way to disable this component from the Editor.
        }

        public override void Filter(TobiiXR_EyeTrackingData data, Vector3 forward)
        {
            if (!enabled) return;
            if (!_settings.Active) return;

            foreach (var gazeModifier in Modifiers)
            {
                gazeModifier.Modify(data, forward);
            }
        }

        public void FilterAccuracyOnly(TobiiXR_EyeTrackingData data, Vector3 forward)
        {
            if (!enabled) return;
            if (!_settings.Active) return;
            _accuracyModifier.Modify(data, forward);
        }

        public float GetMaxPrecisionAngleDegrees(Vector3 gazeDirection, Vector3 forward)
        {
            if (!enabled) return 0f;
            if (!_settings.Active) return 0f;
            return _precisionModifier.GetMaxAngle(gazeDirection, forward);
        }
    }
}