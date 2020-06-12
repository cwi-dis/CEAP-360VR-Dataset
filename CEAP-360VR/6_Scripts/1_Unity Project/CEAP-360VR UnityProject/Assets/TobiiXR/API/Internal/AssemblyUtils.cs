// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public static class AssemblyUtils 
    {
        public static Type EyetrackingProviderType(string typeName)
        {
            return EyetrackingProviderTypes().Where(t => t.FullName == typeName).FirstOrDefault();
        }

        internal static IEnumerable<Type> EyetrackingProviderTypes()
        {
            var type = typeof(IEyeTrackingProvider);
            var types = (AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass));
            return types;
        }

        public static string GetProviderCompilerFlag(IEyeTrackingProvider provider)
        {
            var attribute = Attribute.GetCustomAttribute(provider.GetType(), typeof(CompilerFlagAttribute)) as CompilerFlagAttribute;
            if(attribute == null) return null;
            
            return attribute.Flag;            
        }

        private static Dictionary<string, string> _cachedProviderDisplayNames = new Dictionary<string, string>();
        public static string GetCachedDisplayNameFor(string providerTypeName)
        {
            if (!_cachedProviderDisplayNames.ContainsKey(providerTypeName))
            {
                var providerType = EyetrackingProviderType(providerTypeName);
                if (providerType == null) return "Unknown";
                var attribute = providerType.GetCustomAttributes(typeof(ProviderDisplayNameAttribute), false).FirstOrDefault() as ProviderDisplayNameAttribute;
                var displayName = attribute != null ? attribute.Name : providerType.FullName;
                _cachedProviderDisplayNames[providerTypeName] = displayName;
            }

            return _cachedProviderDisplayNames[providerTypeName];
        }
    }
}