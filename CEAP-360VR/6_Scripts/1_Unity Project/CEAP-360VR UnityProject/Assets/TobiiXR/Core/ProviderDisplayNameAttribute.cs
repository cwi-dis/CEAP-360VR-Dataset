// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;

namespace Tobii.XR
{

    [AttributeUsage(AttributeTargets.Class)]
    public class ProviderDisplayNameAttribute : Attribute
    {
        public readonly string Name;

        public ProviderDisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}