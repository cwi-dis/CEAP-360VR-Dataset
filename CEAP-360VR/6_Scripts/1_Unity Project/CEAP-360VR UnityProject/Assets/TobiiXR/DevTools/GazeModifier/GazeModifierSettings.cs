// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using Tobii.XR.DevTools;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    [Serializable]
    public class GazeModifierSettings : IGazeModifierSettings
    {
        private readonly IList<IDisableGazeModifier> _disablers = new List<IDisableGazeModifier>();

        public IPercentileRepository Repository { get; private set; }

        [SerializeField]
        private int _selectedPercetileIndex = 95;

        public int SelectedPercentileIndex
        {
            get { return _selectedPercetileIndex; }
            set
            {
                _selectedPercetileIndex = Mathf.Clamp(value, 0, NumberOfPercentiles - 1);
            }
        }

        public int NumberOfPercentiles { get; private set; }

        public GazeModifierSettings()
        {
            SetPercentileRepository(new PercentileRepository());
        }

        public bool Active
        {
            get { return !_disablers.Any(d => d.Disable); }
        }

        public void AddDisabler(IDisableGazeModifier disabler)
        {
            _disablers.Add(disabler);
        }
        
        public void SetPercentileRepository(IPercentileRepository repo)
        {
            Repository = repo;
            NumberOfPercentiles  = repo.LoadAll()
                .Select(v => v.Percentile)
                .Distinct()
                .Count();
        }
    }
}

