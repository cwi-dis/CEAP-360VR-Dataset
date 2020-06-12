using UnityEngine;

namespace Tobii.XR.Examples
{
    public enum ControllerButton
    {
        Menu,
        Touchpad,
        Trigger
    }

    public interface IControllerAdapter
    {
        /// <summary>
        /// The velocity of the controller.
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// The angular velocity of the controller.
        /// </summary>
        Vector3 AngularVelocity { get; }

        /// <summary>
        /// The position of the controller in world space. 
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The rotation of the controller in world space.
        /// </summary>
        Quaternion Rotation { get; }


        /// <summary>
        /// Is a given button pressed or not during this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if button is being pressed this frame, otherwise false.</returns>
        bool GetButtonPress(ControllerButton button);

        /// <summary>
        /// Did a button go from not pressed to pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being up to being pressed down this frame, otherwise false.</returns>
        bool GetButtonPressDown(ControllerButton button);

        /// <summary>
        /// Did a button go from pressed to not pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being pressed down to not being pressed this frame, otherwise false.</returns>
        bool GetButtonPressUp(ControllerButton button);

        /// <summary>
        /// Is a given button being touched or not during this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if button is being touched this frame, otherwise false.</returns>
        bool GetButtonTouch(ControllerButton button);

        /// <summary>
        /// Did a button go from not being touched to being touched this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from not being touched to being touched this frame, otherwise false.</returns>
        bool GetButtonTouchDown(ControllerButton button);

        /// <summary>
        /// Did a button go from being touched to not touched this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being touched to not being touched this frame, otherwise false.</returns>
        bool GetButtonTouchUp(ControllerButton button);

        /// <summary>
        /// Trigger a haptic pulse on the controller.
        /// </summary>
        /// <param name="pulseDurationMicroSeconds">Duration of the haptic pulse in microseconds.</param>
        void TriggerHapticPulse(ushort pulseDurationMicroSeconds);

        /// <summary>
        /// Get the touchpad touch position.
        /// </summary>
        /// <returns>Vector2 with the thumb's position on the touchpad.</returns>
        Vector2 GetTouchpadAxis();
    }

    public class ControllerManager : MonoBehaviour
    {
        private static ControllerManager _instance;

#if TOBIIXR_PICOPROVIDER
    private readonly IControllerAdapter _controllerAdapter = new PicoControllerAdapter();
#else
        private readonly IControllerAdapter _controllerAdapter = new OpenVRControllerAdapter();
#endif

        /// <summary>
        /// Instance of the controller manager which can be statically accessed.
        /// </summary>
        public static ControllerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ControllerManager>();
                }

                return _instance;
            }
        }

        public Vector3 Velocity
        {
            get { return transform.TransformVector(_controllerAdapter.Velocity); }
        }

        public Vector3 AngularVelocity
        {
            get { return _controllerAdapter.AngularVelocity; }
        }

        public Vector3 Position
        {
            get { return transform.TransformPoint(_controllerAdapter.Position); }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation * _controllerAdapter.Rotation; }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public bool GetButtonPress(ControllerButton button)
        {
            return _controllerAdapter.GetButtonPress(button);
        }

        public bool GetButtonPressDown(ControllerButton button)
        {
            return _controllerAdapter.GetButtonPressDown(button);
        }

        public bool GetButtonPressUp(ControllerButton button)
        {
            return _controllerAdapter.GetButtonPressUp(button);
        }

        public bool GetButtonTouch(ControllerButton button)
        {
            return _controllerAdapter.GetButtonTouch(button);
        }

        public bool GetButtonTouchDown(ControllerButton button)
        {
            return _controllerAdapter.GetButtonTouchDown(button);
        }

        public bool GetButtonTouchUp(ControllerButton button)
        {
            return _controllerAdapter.GetButtonTouchUp(button);
        }

        public void TriggerHapticPulse(ushort pulseDurationMicroSeconds)
        {
            _controllerAdapter.TriggerHapticPulse(pulseDurationMicroSeconds);
        }

        public Vector2 GetTouchpadAxis()
        {
            return _controllerAdapter.GetTouchpadAxis();
        }
    }
}