using UnityEngine;

namespace Tobii.XR
{
    public abstract class EyeTrackingFilterBase : MonoBehaviour
    {
        /// <summary>
        /// Applies filter to data parameter
        /// </summary>
        /// <param name="data">Eye tracking data that will be modified</param>
        /// <param name="forward">A unit direction vector pointing forward in the coordinate system used by the eye tracking data</param>
        public abstract void Filter(TobiiXR_EyeTrackingData data, Vector3 forward);
    }
}