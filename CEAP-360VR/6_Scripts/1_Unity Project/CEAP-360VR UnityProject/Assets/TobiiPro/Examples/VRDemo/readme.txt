------------------------------------------------------------------------------
 Copyright © 2019 Tobii Pro AB. All rights reserved.
------------------------------------------------------------------------------

Points 1 - 2 are needed to enable Steam VR and Tobii eye tracking in a project. 

The Vive can use either the Unity built in OpenVR support or the SteamVR
package from the Unity asset store. The <Num>.a [SteamVR] choices below are for
creating a scene using SteamVR, the <Num>.b [UnityVR] choices are for using the
Unity built in support. Use one or the other, not both.

1.a [SteamVR] Import the SteamVR package from the asset store.
1.b [UnityVR] Enable VR support under the menu option:
              "Edit -> Project Settings... -> Player -> XR Settings -> Virtual
              Reality Supported". Also make sure the OpenVR package is added to
			  the project. The package manager is found under "Window ->
			  Package Manager".
2. Import the TobiiPro.SDK.Unity.Windows package.

Points 3 - 6 show how to enable Steam VR in a scene

3.a [SteamVR] Remove any camera in the scene (the default camera is called
              "Main Camera"). When creating a scene from scratch and importing
              Steam VR, there will be a conflict with the default camera in the
              scene.
3.b [UnityVR] Set the camera position to (0, 0, 0).
4.a [SteamVR] Drag and drop the "SteamVR\Prefabs\[CameraRig]" prefab into the
              scene.

Points 5 - 10 show how to enable Tobii eye tracking using the TobiiControl
package and show the gaze point on an object.

5. Drag and drop the prefab "TobiiPro\Examples\VRDemo\Prefabs\TobiiControl"
   into the scene.
6. Place an object in the scene, such as a cube, and make sure it has a
   collider attached.
7. Drag the "TobiiPro\VR\Prefabs\[VREyeTracker]" prefab into the scene. Make
   sure the "Subscribe To Gaze" check box is enabled.
8. Drag the "TobiiPro\VR\Prefabs\[VRCalibration]" prefab into the scene.
9. Drag the "TobiiPro\VR\Prefabs\[VRGazeTrail]" prefab into the scene. Change
    the "Particle Count" to 1. Make sure "On" is checked.
10. Drag the "TobiiPro\VR\Prefabs\[VRSaveData]" prefab into the scene. Select
    which kind of data that should be saved by checking "Save Unity Data" and/or
    "Save Raw Data".

11. Play the scene. Follow the instructions on the sign in the scene. Looking
    at the object should place the gaze point on it. The tracking data for each
    session is stored in Data folder in the root folder of the application.

--

There are two example scenes that can be opened to see how to use the package:
CalibrationExample and InteractionExample.

CalibrationExample is a very basic scene. A simple room, with nothing on it.
The gaze point will collide with the walls, floor and roof.

InteractionExample is a more advanced example in which the objects in the scene
(cubes/cylinders) change colour when the user looks at them.
