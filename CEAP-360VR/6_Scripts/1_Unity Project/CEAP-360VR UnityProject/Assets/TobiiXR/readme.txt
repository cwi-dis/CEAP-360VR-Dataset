
Requirements:

 * Minimum Unity version 2018.2.5f1

To get started developing, visit  https://vr.tobii.com/sdk

Quick Start:

See sample scene Example_GettingStarted

Changes for v1.7.0

* Added Pico provider to support Pico Neo 2 Eye

* Added namespaces to example scripts

Changes for v1.6.0

* Tobii Settings menu replaced with a PropertyDrawer for TobiiXR_Settings

* Added a TobiiXR Initializer prefab that holds a TobiiXR_Settings property and uses it to initialize TobiiXR

Changes for v1.5.3

* Fixed avatar facial animations in newer versions of Unity

Changes for v1.5.2

* Updated G2OM to version 6.2.0

Changes for v1.5.1

* Updated HTC Provider to use SR Anipal SDK 1.1.0.1

Changes for v1.5.0

* Updated Stream Engine to version 4.0.0

* Updated G2OM to version 6.1.0

* Added better local to world transform for gaze when using OpenVR

* Gaze Modifier was changed from being a provider to being a filter that gets applied to the TobiiXR facade

* Improved handling of multiple cameras and disabled cameras


Changes for v1.4.1

* Fixed native library includes for macOS


Changes for v1.4.0

* Updated Stream Engine to version 3.3.0

* Replaced eye openness with blink

* Removed left and right gaze vectors

* Added convergence distance for avatar animations

* Fixed timestamps for Tobii provider and HTC provider


Changes for v1.3.0:

* Added HTC provider to support HTC Vive Pro Eye

* Added Tobii HTC provider that chooses Tobii or HTC provider at runtime depending on connected HMD

* Compensates for predicted head movements when transforming gaze data to world space

* Fixed leaking handles when multiple eye trackers are connected to the machine


Changes for v1.2.7:

* Fixed collider size issue with ui sliders


Changes for v1.2.6:

 * Added popup for license


Changes for v1.2.4:

 * Removed dependencies on Examples

 * Removed dependecies on DevTools

 * Exposed StreamEngineContext for calibration

 * Added Assemblydefinitions


Changes for v1.2.3:

 * Bugfix DevTools

 * Rename DevKit -> DevTools

 * Rename Gaze Emulator -> Gaze Provider

Changes for v1.1.0:

 * Added Field of use

Changes for v1.0.0:

 * Removed OpenVRProvider

 * Removed SVR Provider

 * Added Mouse Provider
 
 * Added Dev Tools GUI
 
 * Added Dev Tool: GazeModifier 
 
 * Added Dev Tool: GazeVisualizer

 * Added Examples

 * Updated G2OM
