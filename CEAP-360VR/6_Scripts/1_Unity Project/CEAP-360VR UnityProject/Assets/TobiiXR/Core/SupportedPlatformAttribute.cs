// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;

namespace Tobii.XR
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SupportedPlatformAttribute : Attribute
    {
        public readonly XRBuildTargetGroup [] Targets;

        public SupportedPlatformAttribute(params XRBuildTargetGroup [] targets)
        {
            Targets = targets;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UnSelectableProviderAttribute : Attribute {}

    public enum XRBuildTargetGroup
    {
        Standalone,
        Android
    }

}