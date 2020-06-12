using Tobii.G2OM;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.XR.Examples
{
    public class SAMGazeController : MonoBehaviour, IGazeFocusable
    {
        int m_value = 0;
        int m_type = 0;

        void Start()
        {
            this.GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 0);
            m_value = int.Parse(this.name) % 10;
            //0_Valence, 1_Arousal
            m_type = int.Parse(this.name) / 10;                                
        }

        public void GazeFocusChanged(bool hasFocus)
        {
            int _proState = CEAP360VRController.CEAP360VRControllerIns.GetProState();
            Vector2 _samRating = CEAP360VRController.CEAP360VRControllerIns.GetSamRating();
            Vector2 _samValue = CEAP360VRController.CEAP360VRControllerIns.GetSamValue();

            if (_proState == 2)
            {
                switch (m_type)
                {
                    //Valence
                    case 0:                                                                                                                     
                        if (m_value != _samRating.x)
                        {
                            if (hasFocus)
                            {
                                this.GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 1);
                                CEAP360VRController.CEAP360VRControllerIns.SetSamValue(new Vector2(m_type, m_value));
                            }
                            else
                            {
                                this.GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 0);
                                if(_samValue.y == m_value)
                                    CEAP360VRController.CEAP360VRControllerIns.SetSamValue(new Vector2(m_type, 0));
                            }
                        }
                        break;
                    //Arousal
                    case 1:                                                                                                                    
                        if (m_value != _samRating.y)
                        {
                            if (hasFocus)
                            {
                                this.GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 1);
                                CEAP360VRController.CEAP360VRControllerIns.SetSamValue(new Vector2(m_type, m_value));
                            }
                            else
                            {
                                this.GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 0);
                                if (_samValue.y == m_value)
                                    CEAP360VRController.CEAP360VRControllerIns.SetSamValue(new Vector2(m_type, 0));
                            }
                        }
                        break;
                }
            } 
        }
    }
}
