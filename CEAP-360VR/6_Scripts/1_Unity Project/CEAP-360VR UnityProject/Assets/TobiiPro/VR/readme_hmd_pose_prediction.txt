------------------------------------------------------------------------------
 Copyright © 2019 Tobii Pro AB. All rights reserved.
------------------------------------------------------------------------------

---[ Gaze data in world coordinates ]---

When transforming gaze origin and direction from eyetracker local to world
coordinates, it is necessary to use a representation of the eyetracker position
and rotation (pose) in Unity world coordinates at the point in time the eye
images for the gaze data were acquired. Since the eyetracker is mounted in the
HMD, it follows the movements of the head.

When gaze data is handled by VREyeTracker in the Unity Update() method, it
arrives with a latency of about 11 ms plus the time elapsed until the code sees
the packet. In addition to this, the HMD pose available in the Update() method
is not the current one, but a prediction of the future pose at the point in time
this frame will be displayed in the HMD. To get an accurately positioned and
rotated eyetracker pose object, both the eytracker latency and the HMD
prediction has to be accounted for.


---[ Pose prediction compensation ]---

There are two different implementations to compensate for HMD pose prediction.

 * Adding an estimated time offset
The default implementation is based on saving a history of eyetracker poses
paired with a timestamp. When gaze data arrives, a lookup is done based on the
gaze timestamp and an interpolated pose is generated based on the gaze
timestamp. To compensate for the HMD prediction when storing the eyetracker pose
in the history, a time offset is added to the paired timestamp. The offset is by
default set to 34 milliseconds, but is adjustable between 0 and 70 milliseconds.
The initial setting is based on some limited testing and may be inaccurate.

 * Using the OpenVR API
If the camera object is kept in its original position, OpenVR can be used to get
a more accurate HMD pose. The OpenVR API method
GetDeviceToAbsoluteTrackingPose() is used and it requires the openvr_api.cs
binding source file, which is not distributed together with the Tobii Pro SDK
Unity package. NOTE that this version of the solution does not handle a camera
that is moved from its original position.

Two steps are required to enable OpenVR lookup of HMD pose.


1. Add the OpenVR API bindings.

The bindings to the OpenVR API can be installed either by adding the SteamVR
plugin from the Unity Asset Store or by manually adding the openvr_api.cs
binding file to the project.

The SteamVR plugin can be found by searching for "SteamVR" in the Unity Asset
Store and installing it from there. The openvr_api.cs file is bundled with the
plugin.

To add the openvr_api.cs bindings manually, the source file should be placed in
a "Plugins" folder in Unity. The file can be found on the Valve github page:

https://github.com/ValveSoftware/openvr/tree/master/headers


2. Enable the OpenVR API calls in the VREyeTracker class

Uncomment, i.e. remove the two leading slashes ("//"), from the following line
in Assets/TobiiPro/VRVREyeTracker.cs

//#define USE_OPENVR_BINDINGS

to

#define USE_OPENVR_BINDINGS

Save the VREyeTracker.cs file. OpenVR API pose lookup should now be enabled.
