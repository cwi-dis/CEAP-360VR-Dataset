using UnityEngine;

#if TOBIIXR_PICOPROVIDER
namespace Tobii.XR.Examples
{
	public class PicoControllerAdapter : IControllerAdapter
	{
		private Vector3 _position;
		private Quaternion _rotation;
		private int _previousFrameCount;
		private Vector3 _angularVelocity;
		private Vector3 _velocity;
		private int _mainHand;
		private float _deltaTime;
		private float _previousFrameTime;
		private Vector3 _previousFramePosition;

		private void UpdateController()
		{
			if (Time.frameCount == _previousFrameCount) return;

			_previousFrameCount = Time.frameCount;
			_deltaTime = Time.time - _previousFrameTime;
			_mainHand = Pvr_ControllerManager.controllerlink.GetMainControllerIndex();
			_position = Pvr_UnitySDKAPI.Controller.UPvr_GetControllerPOS(_mainHand);
			_rotation = Pvr_UnitySDKAPI.Controller.UPvr_GetControllerQUA(_mainHand);
			_angularVelocity = Pvr_UnitySDKAPI.Controller.UPvr_GetAngularVelocity(_mainHand);
			_velocity = (_position - _previousFramePosition) / _deltaTime;

			_previousFramePosition = _position;
			_previousFrameTime = Time.time;
		}

		public Vector3 Velocity
		{
			get
			{
				UpdateController();
				return _velocity;
			}
		}

		public Vector3 AngularVelocity
		{
			get
			{
				UpdateController();
				return _angularVelocity;
			}
		}

		public Vector3 Position
		{
			get
			{
				UpdateController();
				return _position;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				UpdateController();
				return _rotation;
			}
		}

		private static Pvr_UnitySDKAPI.Pvr_KeyCode PvrButtonIdFrom(ControllerButton button)
		{
			switch (button)
			{
				case ControllerButton.Menu:
					return Pvr_UnitySDKAPI.Pvr_KeyCode.APP;
				case ControllerButton.Touchpad:
					return Pvr_UnitySDKAPI.Pvr_KeyCode.TOUCHPAD;
				case ControllerButton.Trigger:
					return Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER;
				default:
					throw new System.Exception("Unmapped controller button: " + button.ToString());
			}
		}

		public bool GetButtonPress(ControllerButton button)
		{
			return Pvr_UnitySDKAPI.Controller.UPvr_GetKey(_mainHand, PvrButtonIdFrom(button));
		}

		public bool GetButtonPressDown(ControllerButton button)
		{
			return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(_mainHand, PvrButtonIdFrom(button));
		}

		public bool GetButtonPressUp(ControllerButton button)
		{
			return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyUp(_mainHand, PvrButtonIdFrom(button));
		}

		public bool GetButtonTouch(ControllerButton button)
		{
			var click = Pvr_UnitySDKAPI.Controller.UPvr_GetTouchPadClick(_mainHand);
			return click != Pvr_UnitySDKAPI.TouchPadClick.No;
		}

		public bool GetButtonTouchDown(ControllerButton button)
		{
			return false; // Not supported
		}

		public bool GetButtonTouchUp(ControllerButton button)
		{
			return false; // Not supported
		}

		public void TriggerHapticPulse(ushort pulseDurationMicroSeconds)
		{
			Pvr_UnitySDKAPI.Controller.UPvr_VibrateNeo2Controller(0.1f, pulseDurationMicroSeconds, _mainHand);
		}

		public Vector2 GetTouchpadAxis()
		{
			return Pvr_UnitySDKAPI.Controller.UPvr_GetAxis2D(_mainHand);
		}
	}
}
#endif