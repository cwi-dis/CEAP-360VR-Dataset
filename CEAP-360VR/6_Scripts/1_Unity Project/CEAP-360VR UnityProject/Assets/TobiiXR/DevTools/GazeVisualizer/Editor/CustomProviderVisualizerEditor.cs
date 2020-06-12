// // Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

// using UnityEditor;

// namespace Tobii.XR.GazeVisualizer
// {

//     [CustomEditor(typeof(CustomProviderVisualizer))]
//     public class CustomProviderVisualizerEditor : Editor
//     {
//         private readonly ProviderTypeDropDownData _typeDropDown = new ProviderTypeDropDownData(BuildTargetGroup.Standalone);

//         void OnEnable()
//         {
//             var customProviderVisualizer = (CustomProviderVisualizer)target;

//             if (string.IsNullOrEmpty(customProviderVisualizer.EyeTrackingProvider)) customProviderVisualizer.EyeTrackingProvider = typeof(NoseDirectionProvider).FullName;

//             _typeDropDown.SetSelectedType(customProviderVisualizer.EyeTrackingProvider);
//         }

//         public override void OnInspectorGUI()
//         {
//             var customProviderVisualizer = (CustomProviderVisualizer)target;
//             EditorGUILayout.Separator();
//             // _typeDropDown.ShowDropDown(ref customProviderVisualizer.EyeTrackingProvider, "Eyetracking Data Provider");
//             // EditorGUILayout.Separator();
//             // _typeDropDown.ShowSettings();
//         }
//     }
// }