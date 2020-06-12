// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;

    public static class EditorUtils
    {
        private static readonly IEditorSettings _editorSettings = new EditorSettings();

        public static string GetProviderCompilerFlag(string eyetrackerProviderType)
        {
            var type = AssemblyUtils.EyetrackingProviderType(eyetrackerProviderType);
            var attribute = Attribute.GetCustomAttribute(type, typeof(CompilerFlagAttribute)) as CompilerFlagAttribute;
            if(attribute == null) return null;
            
            return attribute.Flag;
        }

        internal static string GetProviderCompilerFlagMessage(string eyetrackerProviderType)
        {
            var type = AssemblyUtils.EyetrackingProviderType(eyetrackerProviderType);
            var attribute = Attribute.GetCustomAttribute(type, typeof(CompilerFlagAttribute)) as CompilerFlagAttribute;
            if(attribute == null) return null;
            
            return attribute.DisplayMessage;
        }

        public static List<string> GetCompilerFlagsForBuildTarget(BuildTargetGroup target)
        {
            return _editorSettings.GetScriptingDefineSymbolsForGroup(target).Split(';').ToList();
        }

        public static void SetCompilerFlagsForBuildTarget(BuildTargetGroup target, List<string> flags)
        {
            _editorSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", flags.ToArray()));
        }

        public static List<TobiiXR_Settings.ProviderElement> GetAvailableProviders(BuildTargetGroup target)
        {
            return EditorUtils.EyetrackingProviderTypes(target)
                .Select(x => TobiiXR_Settings.ProviderElement.FromProviderType(x))
                .OrderBy(x => x.DisplayName)
                .ToList();
        }

        private static IEnumerable<Type> EyetrackingProviderTypes(BuildTargetGroup buildTarget)
        {
            var include = new List<Type>();
            var providers = AssemblyUtils.EyetrackingProviderTypes().ToList();
            foreach (var provider in providers)
            {
                var unselectable = Attribute.GetCustomAttribute(provider, typeof(UnSelectableProviderAttribute)) != null;
                if(unselectable) continue;

                var attribute = Attribute.GetCustomAttribute(provider, typeof(SupportedPlatformAttribute)) as SupportedPlatformAttribute;

                if (attribute == null || attribute.Targets.Select(ConvertFromXRTargetGroup).Contains(buildTarget))
                {
                    include.Add(provider);
                }
            }

            return include;
        }

        private static BuildTargetGroup ConvertFromXRTargetGroup(XRBuildTargetGroup xrBuildTargetGroup)
        {
            return xrBuildTargetGroup == XRBuildTargetGroup.Android ? BuildTargetGroup.Android : BuildTargetGroup.Standalone;
        }

        private class EditorSettings : IEditorSettings
        {
            public void SetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup, string defines)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            }

            public string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup)
            {
                return PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            }
        }        
    }
}