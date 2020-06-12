
namespace Tobii.XR.GazeModifier
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GazeModifierFilter))]
    public class GazeModifierFilterEditor : Editor
    {
        private GazeModifierFilter _gazeModifierFilter;

        void OnEnable()
        {
            _gazeModifierFilter = target as GazeModifierFilter;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            var selectedPercentileIndex = EditorGUILayout.IntSlider("Percentile:", _gazeModifierFilter.Settings.SelectedPercentileIndex, 0, _gazeModifierFilter.Settings.NumberOfPercentiles - 1);

            // EditorGUILayout.LabelField(_gazeModifierFilter.Settings.SelectedPercentileString);

            if (EditorGUI.EndChangeCheck())
            {
                _gazeModifierFilter.Settings.SelectedPercentileIndex = selectedPercentileIndex;
                Undo.RecordObject(_gazeModifierFilter, "Gaze Modifier settings changed");
                EditorUtility.SetDirty(_gazeModifierFilter);
            }

            if (GUILayout.Button(new GUIContent("Open Gaze Modifier documentation website"), GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL("https://developer.tobii.com/vr/develop/unity/tools/gaze-modifier/");
            }
        }
    }
}