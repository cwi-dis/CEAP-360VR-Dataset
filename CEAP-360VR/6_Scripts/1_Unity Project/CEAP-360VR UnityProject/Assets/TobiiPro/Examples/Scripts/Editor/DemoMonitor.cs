//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Collections.Generic;
using UnityEditor;

// OpenVR moved to package in 2018.3. See https://unity3d.com/unity/whats-new/unity-2018.3.0
#if UNITY_2018_3_OR_NEWER

using UnityEditor.PackageManager.Requests;

#endif

namespace Tobii.Research.Unity
{
    [InitializeOnLoad]
    internal class DemoMonitor
    {
        private const string _dialogTitle = "Virtual Reality Support";
        private static List<string> _vrScenes;
        private static List<string> _screenBasedScenes;
#if UNITY_2018_3_OR_NEWER
        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
#endif

        static DemoMonitor()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpened;
            _vrScenes = new List<string>() { "VRPrefabDemo", "CalibrationExample", "InteractionExample" };
            _screenBasedScenes = new List<string>() { "ScreenBasedPrefabDemo" };
        }

        private static void ShowMessage(string message)
        {
            EditorUtility.DisplayDialog(
                title: _dialogTitle,
                message: message,
                ok: "OK");
        }

        private static bool VRSupported
        {
            get
            {
                return PlayerSettings.virtualRealitySupported;
            }

            set
            {
                if (PlayerSettings.virtualRealitySupported == value)
                {
                    return;
                }

                PlayerSettings.virtualRealitySupported = value;
                ShowMessage(string.Format("Virtual Reality support has been {0}", PlayerSettings.virtualRealitySupported ? "enabled" : "disabled"));
            }
        }

#if UNITY_2018_3_OR_NEWER

        private static void CheckPackageInstallation()
        {
            if (_addRequest.Status == UnityEditor.PackageManager.StatusCode.InProgress)
                return;

            EditorApplication.update -= CheckPackageInstallation;

            if (_addRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
                ShowMessage("Could not install the OpenVR package into the project. It can be installed using \"Window -> Package Manager\".");
            else
                ShowMessage("Installed the OpenVR package into the project");
        }

        private static void CheckListCompletion()
        {
            if (_listRequest.Status == UnityEditor.PackageManager.StatusCode.InProgress)
                return;

            EditorApplication.update -= CheckListCompletion;
            if (_listRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
            {
                ShowMessage("Could not determine if the OpenVR package has been included in the project. It can be installed using \"Window -> Package Manager\".");
                return;
            }

            var packageName = "com.unity.xr.openvr.standalone";
            foreach (var package in _listRequest.Result)
            {
                if (package.name == packageName)
                {
                    // Package exists
                    return;
                }
            }

            if (EditorUtility.DisplayDialog(
                            title: _dialogTitle,
                            message: "This demo scene requires the OpenVR package to be added to the project.",
                            ok: "Add the OpenVR package",
                            cancel: "Cancel"))
            {
                _addRequest = UnityEditor.PackageManager.Client.Add(packageName);
                EditorApplication.update += CheckPackageInstallation;
            }
        }

        private static void CheckIfWeHaveOpenVRPackage()
        {
            UnityEngine.Debug.Log("Looking for OpenVR package");
            _listRequest = UnityEditor.PackageManager.Client.List(true);
            EditorApplication.update -= CheckIfWeHaveOpenVRPackage;
            EditorApplication.update += CheckListCompletion;
        }

#endif

        private static void SceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            if (!(scene.path.Contains("TobiiPro") && scene.path.Contains("Examples")))
            {
                return;
            }

            if (_vrScenes.Contains(scene.name))
            {
                if (!VRSupported)
                {
                    if (EditorUtility.DisplayDialog(
                            title: _dialogTitle,
                            message: "This demo scene requires enabling Unity virtual reality support.",
                            ok: "Enable VR support",
                            cancel: "Cancel"))
                    {
                        VRSupported = true;
                    }
                }

#if UNITY_2018_3_OR_NEWER
                // Redundant callback removal to avoid lingering attached methods.
                EditorApplication.update -= CheckPackageInstallation;
                EditorApplication.update -= CheckListCompletion;
                EditorApplication.update -= CheckIfWeHaveOpenVRPackage;
                if (VRSupported)
                {
                    EditorApplication.update += CheckIfWeHaveOpenVRPackage;
                }
#endif
            }
            else if (_screenBasedScenes.Contains(scene.name) && VRSupported)
            {
                if (EditorUtility.DisplayDialog(
                        title: _dialogTitle,
                        message: "This demo scene should not have Unity virtual reality support enabled.",
                        ok: "Disable VR support",
                        cancel: "Cancel"))
                {
                    VRSupported = false;
                }
            }
        }
    }
}