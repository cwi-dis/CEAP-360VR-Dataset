------------------------------------------------------------------------------
 Copyright © 2019 Tobii Pro AB. All rights reserved.
------------------------------------------------------------------------------

The following describes how to create a simple scene with calibration, gaze
trail rendering, track box guide, and data saving.

1. Create a new scene.
2. Import the TobiiPro.SDK.Unity.Windows package.

Points 3 - 8 show how to create a scene with calibration, gaze trail, data
saving, and a track box guide.

3. Drag and drop the "TobiiPro\ScreenBased\Prefabs\[EyeTracker]" prefab into the
   scene. Select the [EyeTracker] prefab and tick the check box to connect to
   the first found eye tracker, or deselect it and a provide a serial number.
4. Drag and drop the "TobiiPro\ScreenBased\Prefabs\[Calibration]" prefab into
   the scene. Select the [Calibration] prefab and in the inspector, select a key
   to be used to start a calibration.
5. Drag and drop the "TobiiPro\ScreenBased\Prefabs\[SaveData]" prefab into the
   scene. Select the [SaveData] prefab and in the inspector, select a key to be
   used to start and stop saving data.
6. Drag and drop the "TobiiPro\ScreenBased\Prefabs\[TrackBoxGuide]" prefab into
   the scene. Select the [TrackBoxGuide] prefab and in the inspector, select a
   key to be used to show and hide the track box guide.  
7. Drag and drop the "TobiiPro\ScreenBased\Prefabs\[GazeTrail]" prefab into the
   scene.
8. Right click in the hierarchy and select "3D Object -> Cube". Place the cube
    at position (0, 1, -4) in the scene.

9. Play the scene.
   * Press the track box guide key selected earlier to show and hide
     the track box guide. Adjust your position until the eyes are placed as
     centered as possible.
   * Press the calibration key selected earlier to perform a calibration.
   * Look at the cube. A gaze trail should be rendered on it.
   * Press the save data key selected earlier to start saving data. Press it
     again to stop saving. The saved XML data can be found in the "Data" folder
     in the project root.
