// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved


namespace Tobii.XR
{
    using UnityEditor;
    public interface IEditorSettings
    {
        void SetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup, string defines);
        string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup);
    }
}