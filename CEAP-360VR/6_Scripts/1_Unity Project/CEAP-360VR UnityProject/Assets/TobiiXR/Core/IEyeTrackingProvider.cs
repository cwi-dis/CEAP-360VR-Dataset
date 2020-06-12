// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using UnityEngine;

    public enum FieldOfUse
    {
        NotSelected,
        Analytical,
        Interactive
    }

    public interface IEyeTrackingProvider
    {
        TobiiXR_EyeTrackingData EyeTrackingDataLocal { get; }

        Matrix4x4 LocalToWorldMatrix { get; }

        bool Initialize(FieldOfUse fieldOfUse);

        void Tick();

        void Destroy();
    }
}
