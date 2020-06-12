//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEditor;

namespace Tobii.Research.Unity
{
    [CustomEditor(typeof(VREyeTracker))]
    public class SomeScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var vrEt = (VREyeTracker)target;
            if (false && !vrEt.UsingOpenVR)
            {
                EditorGUILayout.HelpBox("To get a more accurate pose prediction compensation, use OpenVR bindings. For more information, see readme_hmd_pose_prediction.txt", MessageType.Info);
            }
        }
    }
}