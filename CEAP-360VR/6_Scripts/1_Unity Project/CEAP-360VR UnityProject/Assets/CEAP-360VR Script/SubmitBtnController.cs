
using Tobii.G2OM;
using UnityEngine;


namespace Tobii.XR.Examples
{
    [RequireComponent(typeof(UIGazeButtonGraphics))]
    public class SubmitBtnController : MonoBehaviour, IGazeFocusable
    {
        public GameObject m_btnCover;
        private ButtonState _currentButtonState = ButtonState.Idle;
        private UIGazeButtonGraphics _uiGazeButtonGraphics;
        bool m_focus;
        int m_submitState;

        private void Start()
        {
            _uiGazeButtonGraphics = GetComponent<UIGazeButtonGraphics>();

        }

        private void Update()
        {
            m_submitState = CEAP360VRController.CEAP360VRControllerIns.GetClickSubmitBtnState();

            switch (m_submitState)
            {
                case 0:
                    m_btnCover.SetActive(true);
                    break;
                case 1:
                    m_btnCover.SetActive(false);
                    if (m_focus)
                    {
                        CEAP360VRController.CEAP360VRControllerIns.SetClickSubmitBtnState(2);
                    }
                    break;
                case 2:
                    if (!m_focus)
                    {
                        CEAP360VRController.CEAP360VRControllerIns.SetClickSubmitBtnState(1);
                    }
                    break;
                case 3:
                    UpdateState(ButtonState.PressedDown);
                    CEAP360VRController.CEAP360VRControllerIns.SetClickSubmitBtnState(4);
                    break;
                case 5:
                    CEAP360VRController.CEAP360VRControllerIns.SetProState(4);
                    CEAP360VRController.CEAP360VRControllerIns.SetClickSubmitBtnState(6);
                    break;
                case 6:
                    m_btnCover.SetActive(true);
                    break;
            }
        }

        private void UpdateState(ButtonState newState)
        {
            var oldState = _currentButtonState;
            _currentButtonState = newState;

            var buttonPressed = newState == ButtonState.PressedDown;
            var buttonClicked = (oldState == ButtonState.PressedDown && newState == ButtonState.Focused);

            if (buttonPressed || buttonClicked)
            {
                _uiGazeButtonGraphics.AnimateButtonPress(_currentButtonState);
            }
            else
            {
                _uiGazeButtonGraphics.AnimateButtonVisualFeedback(_currentButtonState);
            }
        }

        public void GazeFocusChanged(bool hasFocus)
        {
            if (!enabled) return;
            m_focus = hasFocus;

            if(m_submitState==1 || m_submitState==2)
                UpdateState(m_focus ? ButtonState.Focused : ButtonState.Idle);

        }
    }
}
