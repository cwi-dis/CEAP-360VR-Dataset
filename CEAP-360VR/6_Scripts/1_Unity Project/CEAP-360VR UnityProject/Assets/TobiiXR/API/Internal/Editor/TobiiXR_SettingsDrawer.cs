
using System;

namespace Tobii.XR.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(TobiiXR_Settings))]
    public class TobiiXR_SettingsDrawer : PropertyDrawer
    {
        private const string ViveProviderCompilerFlagString = "TOBIIXR_HTCPROVIDER";
        private const string PicoProviderCompilerFlagString = "TOBIIXR_PICOPROVIDER";
        private static bool _scriptsReloaded;

        private int _lineEndings = 23; // Arbitrary number, could be correct. Will count the lines in OnGUI, setting non-zero here to avoid rendering artifacts after compilation.
        private List<TobiiXR_Settings.ProviderElement> _allStandaloneProviders;
        private List<TobiiXR_Settings.ProviderElement> _allAndroidProviders;
        private ReorderableList _standaloneProviderList;
        private ReorderableList _androidProviderList;
        private float _providerListHeight;
        private bool _initialized;
        private bool _viveProviderEnabled = false;
        private bool _picoProviderEnabled = false;

        private void Init(SerializedProperty property)
        {
            if (_initialized == true) return;

            _allStandaloneProviders = EditorUtils.GetAvailableProviders(BuildTargetGroup.Standalone);
            _allAndroidProviders = EditorUtils.GetAvailableProviders(BuildTargetGroup.Android);
            _standaloneProviderList = CreateReorderableList("Standalone Eye Tracking Providers", property, property.FindPropertyRelative("StandaloneEyeTrackingProviders"), _allStandaloneProviders);
            _androidProviderList = CreateReorderableList("Android Eye Tracking Providers", property, property.FindPropertyRelative("AndroidEyeTrackingProviders"), _allAndroidProviders);

            var flags = EditorUtils.GetCompilerFlagsForBuildTarget(BuildTargetGroup.Standalone);
            _viveProviderEnabled = flags.Contains(ViveProviderCompilerFlagString);
            flags = EditorUtils.GetCompilerFlagsForBuildTarget(BuildTargetGroup.Android);
            _picoProviderEnabled = flags.Contains(PicoProviderCompilerFlagString);
            _initialized = true;
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _scriptsReloaded = true;
        }
        
        private static ReorderableList CreateReorderableList(string title, SerializedProperty property, SerializedProperty listProperty, List<TobiiXR_Settings.ProviderElement> providers)
        {
            var reorderableList = new ReorderableList(listProperty.serializedObject, listProperty);

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var providerTypeName = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("TypeName").stringValue;
                var displayName = AssemblyUtils.GetCachedDisplayNameFor(providerTypeName);
                EditorGUI.LabelField(rect, new GUIContent(displayName));
            };

            reorderableList.drawHeaderCallback = (Rect r) => EditorGUI.LabelField(r, title);

            reorderableList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                var menu = new GenericMenu();

                foreach (var provider in providers)
                {
                    if (list.serializedProperty.ArrayContains(element => element.FindPropertyRelative("TypeName").stringValue == provider.TypeName)) continue;

                    menu.AddItem(new GUIContent(provider.DisplayName), false, (object target) =>
                    {
                        var providerElement = (TobiiXR_Settings.ProviderElement)target;

                        var element = list.AddElement();
                        element.FindPropertyRelative("TypeName").stringValue = providerElement.TypeName;
                        property.serializedObject.ApplyModifiedProperties();

                    }, provider);
                }

                menu.ShowAsContext();
            };

            return reorderableList;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (2 + EditorGUIUtility.singleLineHeight) * _lineEndings + 6 + _providerListHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_scriptsReloaded)
            {
                _scriptsReloaded = false;
                _initialized = false;
                Init(property);
            }

            if (EditorApplication.isCompiling)
            {
                _initialized = false;
            }
            else if (!_initialized)
            {
                Init(property);
            }
            _lineEndings = 0;

            var fieldOfUse = property.FindPropertyRelative("FieldOfUse");
            var layerMask = property.FindPropertyRelative("LayerMask");
            var howLongToKeepCandidatesInSeconds = property.FindPropertyRelative("HowLongToKeepCandidatesInSeconds");
            var eyeTrackingFilter = property.FindPropertyRelative("EyeTrackingFilter");

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(position, "Information", EditorStyles.boldLabel);
            CarriageReturn(ref position);

            EditorGUI.LabelField(position, "Change settings used to initialize Tobii XR. More info at the site below:");
            CarriageReturn(ref position);

            var buttonPosition = position;
            var buttonContent = new GUIContent("Open Tobii Settings documentation website");
            buttonPosition.width = GUI.skin.button.CalcSize(buttonContent).x;

            if (GUI.Button(buttonPosition, buttonContent))
            {
                Application.OpenURL("https://vr.tobii.com/sdk/develop/unity/documentation/tobii-settings/");
            }

            CarriageReturn(ref position, 2);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUI.PropertyField(position, fieldOfUse, new GUIContent("Field Of Use", "Set if the application is used for interactive or analytical purposes."));
            CarriageReturn(ref position);

            CarriageReturn(ref position);
            EditorGUI.LabelField(position, new GUIContent("G2OM Settings", "Gaze-2-object mapping is a machine learning algorithm determining what object the user is looking at."), EditorStyles.boldLabel);
            CarriageReturn(ref position);

            EditorGUI.PropertyField(position, layerMask, new GUIContent("Layer Mask", "Set which layers G2OM looks for potential candidates."));
            CarriageReturn(ref position);

            EditorGUI.PropertyField(position, howLongToKeepCandidatesInSeconds);
            CarriageReturn(ref position, 2);

            EditorGUI.LabelField(position, new GUIContent("Eye Tracking Providers", "If no provider is initialized successfully TobiiXR will use Nose Direction Provider even if it is removed from this list."), EditorStyles.boldLabel);
            CarriageReturn(ref position);

            var providerList = EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android ? _androidProviderList : _standaloneProviderList;

            providerList.DoList(position);
            CarriageReturn(ref position);
            _providerListHeight = providerList.GetHeight();
            position.y += _providerListHeight;

            EditorGUI.EndDisabledGroup();

            if (eyeTrackingFilter.objectReferenceValue != null && eyeTrackingFilter.objectReferenceValue is EyeTrackingFilterBase == false)
            {
                Debug.LogError("Filter must implement interface EyeTrackingFilterBase.");
                eyeTrackingFilter.objectReferenceValue = null;
            }

            EditorGUI.PropertyField(position, eyeTrackingFilter, new GUIContent("Eye Tracking Filter", "If you want the eye tracking data to be filtered. One example is the Gaze Modifier Filter."));
            CarriageReturn(ref position);

            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone || EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
            {
                CarriageReturn(ref position);
                EditorGUI.LabelField(position, new GUIContent(EditorUserBuildSettings.selectedBuildTargetGroup.ToString() + " Player Settings*", "Settings specific to a player."), EditorStyles.boldLabel);
                CarriageReturn(ref position);

                if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone)
                {
                    var viveProviderEnabled = EditorGUI.ToggleLeft(position, new GUIContent("Support VIVE Pro Eye (requires SRanipal SDK)", "Please add VIVE SRanipal SDK to your project before enabling VIVE Pro Eye support"), _viveProviderEnabled);
                    if (viveProviderEnabled != _viveProviderEnabled)
                    {
                        var flags = EditorUtils.GetCompilerFlagsForBuildTarget(BuildTargetGroup.Standalone);
                        if (viveProviderEnabled)
                        {
                            flags.Add(ViveProviderCompilerFlagString);
                        }
                        else
                        {
                            flags.RemoveAll(x => x == ViveProviderCompilerFlagString);
                        }

                        EditorUtils.SetCompilerFlagsForBuildTarget(BuildTargetGroup.Standalone, flags);
                        _viveProviderEnabled = viveProviderEnabled;
                    }    
                } else if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
                {
                    var picoProviderEnabled = EditorGUI.ToggleLeft(position, new GUIContent("Support Pico (requires Pico VR SDK)", "Please add Pico VR SDK to your project before enabling Pico support"), _picoProviderEnabled);
                    if (picoProviderEnabled != _picoProviderEnabled)
                    {
                        var flags = EditorUtils.GetCompilerFlagsForBuildTarget(BuildTargetGroup.Android);
                        if (picoProviderEnabled)
                        {
                            flags.Add(PicoProviderCompilerFlagString);
                        }
                        else
                        {
                            flags.RemoveAll(x => x == PicoProviderCompilerFlagString);
                        }

                        EditorUtils.SetCompilerFlagsForBuildTarget(BuildTargetGroup.Android, flags);
                        _picoProviderEnabled = picoProviderEnabled;
                    }
                }
                
            }

            CarriageReturn(ref position);
            CarriageReturn(ref position);
            EditorGUI.LabelField(position, "*Changing player settings will trigger a recompile.");
        }

        private void CarriageReturn(ref Rect position, int count = 1)
        {
            for (; count > 0; count--)
            {
                position.y += position.height;
                _lineEndings++;
            }
        }
    }
}
