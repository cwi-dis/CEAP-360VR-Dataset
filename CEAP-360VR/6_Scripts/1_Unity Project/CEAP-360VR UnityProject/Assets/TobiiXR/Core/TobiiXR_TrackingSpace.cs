namespace Tobii.XR
{
    /// <summary>
    /// Tracking spaces that eye tracking data can be reported in.
    /// </summary>
    public enum TobiiXR_TrackingSpace
    {
        /// <summary>
        /// The local eye tracking space share origin with the XR camera. Data
        /// reported in this space is unaffected by head movements and is
        /// good for use cases where you need eye tracking data relative
        /// to the head, like avatar eye animations and interaction with
        /// head locked content.
        /// </summary>
        Local,
        /// <summary>
        /// World space is the main tracking space used by Unity.
        /// Eye tracking data in world space uses the same tracking space
        /// as objects in your scene and is useful when computing what object
        /// is being focused by the user.
        /// </summary>
        World
    }
}