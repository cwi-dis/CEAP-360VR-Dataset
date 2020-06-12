// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR.DevTools;

namespace Tobii.XR.GazeModifier
{
    public interface IGazeModifierSettings
    {
        void AddDisabler(IDisableGazeModifier disabler);

        bool Active { get; }

        int SelectedPercentileIndex { get; set; }

        int NumberOfPercentiles { get; }

        IPercentileRepository Repository { get; }
    }
}