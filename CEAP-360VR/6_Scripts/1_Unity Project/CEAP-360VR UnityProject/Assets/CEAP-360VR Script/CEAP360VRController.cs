using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.Video;

namespace Tobii.XR.Examples
{
    
}
public class CEAP360VRController : MonoBehaviour
{
    public static CEAP360VRController CEAP360VRControllerIns;
    
    //joy-con value
    private List<Joycon> m_joycons;
    public int m_joyconIndex = 0;
    Joycon _joycon;

    public Image m_lightFrameImg;
    public Image m_dotSizeImg;
    //1_lightFrame, 2_dotSize
    public int m_visType;                                                                                   
    public Transform m_headCam;

    public Image m_helpTipImg;
    public RenderTexture m_videoTex;
    public GameObject m_cubeTest;

    //vis feedback color
    Color _visColor1 = new Color(238 / 255f, 205 / 255f, 172 / 255f);                                    
    Color _visColor2 = new Color(127 / 255f, 192 / 255f, 135 / 255f);
    Color _visColor3 = new Color(135 / 255f, 154 / 255f, 240 / 255f);
    Color _visColor4 = new Color(244 / 255f, 151 / 255f, 142 / 255f);
    Color _visColor5 = new Color(255 / 255f, 255 / 255f, 255 / 255f, 0f);

    Color _visColor12 = new Color((238+127) / 510f, (205+192) / 510f, (172+135) / 510f);
    Color _visColor23 = new Color((127+135) / 510f, (192+154) / 510f, (135+240) / 510f);
    Color _visColor34 = new Color((135+244) / 510f, (154+151) / 510f, (240+142) / 510f);
    Color _visColor41 = new Color((244+238) / 510f, (151+205) / 510f, (142+172) / 510f);

    string _headMoveFileName = "HeadRotationTable_";
    private string _avFileName = "SelfAnnotationTable_";
    private string _helperFileName = "HelperTimeTable_";
    private string __samFileName = "SamRatingTable_";

    public string m_participantID;
    public string m_videoID;

    public AudioSource m_videoAudioSource;
    public VideoPlayer m_videoPlayer;

    public VideoClip[] m_testVideoClips;
    public AudioClip[] m_testAudioClips;

    int m_proState;
    //0_annotationUI, 1_samUI, 2_ValenceSelecObj, 3_ArousalSelectObj
    public GameObject[] m_uiObj;
    //1_canSubmit, 2_focus, 3_down, 4_waitingForUp, 5_up, 6_done
    int m_submitBtnState;                                                                                  
    public Text m_samTipWords;

    Vector2 m_samRatingValue;
    Vector2 m_samSelectValue;

    string m_savePath;


    float m_headOutputTimer;
    float m_avOutputTimer;

    float _horizontalValue;
    float _verticalValue;

    int m_helperState = 0;
    string m_helperOpenTime;
    string m_helperCloseTime;

    string m_timeStamp;

    List<string> m_headTimeStamp = new List<string>();
    List<string> m_annotateTimeStamp = new List<string>();
    List<Vector3> m_headData = new List<Vector3>();
    List<Vector2> m_annotateData = new List<Vector2>();
    List<string> m_helperOpenTimeData = new List<string>();
    List<string> m_helperCloseTimeData = new List<string>();
    List<Vector2> m_samRatingData = new List<Vector2>();

    List<string> m_samRatingData1 = new List<string>();
    List<string> m_samRatingData2 = new List<string>();

    private void Awake()
    {
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        CEAP360VRControllerIns = this;
        m_savePath = Application.dataPath + "/CEAP-360VR_Data/";
        print(m_savePath);
        m_helpTipImg.gameObject.SetActive(false);

        string _date = System.DateTime.Now.ToString("yyyyMMddTHHmmss");

        _headMoveFileName = string.Format("HeadRotation_{0}_{1}_{2}", m_videoID, m_participantID, _date);
        _avFileName = string.Format("SelfAnnotation_{0}_{1}_{2}", m_videoID, m_participantID, _date);
        _helperFileName = string.Format("HelperTime_{0}_{1}_{2}", m_videoID, m_participantID, _date);
        __samFileName = string.Format("SamRating_{0}_{1}_{2}", m_videoID, m_participantID, _date);

        m_headTimeStamp.Clear();
        m_annotateTimeStamp.Clear();
        m_headData.Clear();
        m_annotateData.Clear();
        m_helperOpenTimeData.Clear();
        m_helperCloseTimeData.Clear();
        m_samRatingData.Clear();

        m_samRatingData1.Clear();
        m_samRatingData2.Clear();
    }

    private void Start()
    {
        m_joycons = JoyconManager.Instance.j;
        if (m_joycons.Count < m_joyconIndex + 1)
        {
            Destroy(gameObject);
        }
        _joycon = m_joycons[1];

        m_uiObj[0].SetActive(false);
        m_uiObj[1].SetActive(false);

        if (m_visType == 1)
            m_dotSizeImg.gameObject.SetActive(false);


        m_videoAudioSource.clip = m_testAudioClips[int.Parse(m_videoID)];
        m_videoPlayer.clip = m_testVideoClips[int.Parse(m_videoID)];
    }

    void FixedUpdate()
    {
        if(m_proState==1)
        {
            _horizontalValue = _joycon.GetStick()[0];
            _verticalValue = _joycon.GetStick()[1];

            m_headOutputTimer += Time.fixedDeltaTime;
            if (m_headOutputTimer >= 0.25f)
            {
                m_headOutputTimer = 0;
                Vector3 _camAngle = GetInspectorRotationValue.Instance.GetInspectorRotationValueMethod(m_headCam);

                m_headTimeStamp.Add(m_timeStamp);
                m_headData.Add(_camAngle);
            }

            m_avOutputTimer += Time.fixedDeltaTime;
            if (m_avOutputTimer >= 0.1f)
            {
                m_avOutputTimer = 0;

                m_annotateTimeStamp.Add(m_timeStamp);
                m_annotateData.Add(new Vector2(_verticalValue, _horizontalValue));
            }
        }

        
    }

    void VisualizeAnotation()
    {
        switch (m_helperState)
        {
            case 0:
                if (_joycon.GetButtonDown(Joycon.Button.SHOULDER_2))
                {
                    m_helpTipImg.gameObject.SetActive(true);
                    m_helperOpenTime = m_timeStamp;
                    m_helperState = 1;
                }
                break;
            case 1:
                if (_joycon.GetButtonUp(Joycon.Button.SHOULDER_2))
                {
                    m_helpTipImg.gameObject.SetActive(false);
                    m_helperCloseTime = m_timeStamp;
                    m_helperState = 2;
                }
                break;
            case 2:
                m_helperOpenTimeData.Add(m_helperOpenTime);
                m_helperCloseTimeData.Add(m_helperCloseTime);
                m_helperState = 0;
                break;
        }

        if (m_visType == 1)
        {
            float _alpha = (float)Math.Sqrt(_horizontalValue * _horizontalValue + _verticalValue * _verticalValue) * 0.8f;


            if (_horizontalValue > 0)
            {
                if (_verticalValue > 0)
                    m_lightFrameImg.color = new Color(_visColor1.r, _visColor1.g, _visColor1.b, _alpha);
                else if (_verticalValue < 0)
                    m_lightFrameImg.color = new Color(_visColor2.r, _visColor2.g, _visColor2.b, _alpha);
                else
                    m_lightFrameImg.color = new Color(_visColor12.r, _visColor12.g, _visColor12.b, _alpha);
            }
            else if (_horizontalValue < 0)
            {
                if (_verticalValue > 0)
                    m_lightFrameImg.color = new Color(_visColor4.r, _visColor4.g, _visColor4.b, _alpha);
                else if (_verticalValue < 0)
                    m_lightFrameImg.color = new Color(_visColor3.r, _visColor3.g, _visColor3.b, _alpha);
                else
                    m_lightFrameImg.color = new Color(_visColor34.r, _visColor34.g, _visColor34.b, _alpha);
            }
            else if (_horizontalValue == 0)
            {
                if (_verticalValue > 0)
                    m_lightFrameImg.color = new Color(_visColor41.r, _visColor41.g, _visColor41.b, _alpha);
                else if (_verticalValue < 0)
                    m_lightFrameImg.color = new Color(_visColor23.r, _visColor23.g, _visColor23.b, _alpha);
                else
                    m_lightFrameImg.color = _visColor5;
            }


        }
        else if (m_visType == 2)
        {
            float _alpha = 0.6f;
            float _size = Math.Abs(_horizontalValue) + Math.Abs(_verticalValue);

            if (_horizontalValue > 0)
            {
                if (_verticalValue > 0)
                    m_dotSizeImg.color = new Color(_visColor1.r, _visColor1.g, _visColor1.b, _alpha);
                else if (_verticalValue < 0)
                    m_dotSizeImg.color = new Color(_visColor2.r, _visColor2.g, _visColor2.b, _alpha);
                else
                    m_dotSizeImg.color = new Color(_visColor12.r, _visColor12.g, _visColor12.b, _alpha);
            }
            else if (_horizontalValue < 0)
            {
                if (_verticalValue > 0)
                    m_dotSizeImg.color = new Color(_visColor4.r, _visColor4.g, _visColor4.b, _alpha);
                else if (_verticalValue < 0)
                    m_dotSizeImg.color = new Color(_visColor3.r, _visColor3.g, _visColor3.b, _alpha);
                else
                    m_dotSizeImg.color = new Color(_visColor34.r, _visColor34.g, _visColor34.b, _alpha);
            }
            else if (_horizontalValue == 0)
            {
                if (_verticalValue > 0)
                    m_dotSizeImg.color = new Color(_visColor41.r, _visColor41.g, _visColor41.b, _alpha);
                else if (_verticalValue < 0)
                    m_dotSizeImg.color = new Color(_visColor23.r, _visColor23.g, _visColor23.b, _alpha);
                else
                    m_dotSizeImg.color = _visColor5;
            }
            m_dotSizeImg.transform.localScale = new Vector3(_size, _size, 1);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            m_videoPlayer.Stop();
            m_videoAudioSource.Stop();
            m_proState = 2;
            m_uiObj[0].SetActive(false);
            m_uiObj[1].SetActive(true);
        }

        switch (m_proState)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_videoPlayer.Play();
                    m_proState = -1;
                    m_cubeTest.SetActive(false);
                }
                break;
            case -1:
                if (m_videoPlayer.isPlaying)
                {
                    m_lightFrameImg.color = _visColor5;
                    m_uiObj[0].SetActive(true);
                    m_videoAudioSource.Play();
                    string _nowtimer = System.DateTime.Now.Minute.ToString() + ":" + System.DateTime.Now.Second.ToString() + ":" + System.DateTime.Now.Millisecond.ToString();
                    print("StartTime: " + _nowtimer);

                    m_samRatingData1.Add("StartTime: " + _nowtimer);
                    m_samRatingData2.Add(m_timeStamp);

                    m_proState = 1;
                }
                break;
            case 1:
                VisualizeAnotation();
                if (!m_videoPlayer.isPlaying)
                {
                    m_uiObj[0].SetActive(false);
                    m_uiObj[1].SetActive(true);
                    string _nowtimer = System.DateTime.Now.Minute.ToString() + ":" + System.DateTime.Now.Second.ToString() + ":" + System.DateTime.Now.Millisecond.ToString();
                    print("StopTime: " + _nowtimer);
                    m_samRatingData1.Add("StopTime: " + _nowtimer);
                    m_samRatingData2.Add(m_timeStamp);

                    m_videoAudioSource.Stop();
                    m_proState = 2;
                    m_samTipWords.text = "Gaze one pic and click the X Button to choose!";
                }
                break;
            //SamRating Choose
            case 2:                                                                                                                                 
                if (_joycon.GetButtonDown(Joycon.Button.DPAD_UP))
                {
                    Vector2 _samValue = m_samSelectValue;
                    //Choose Valence 
                    if (_samValue.x == 0 && _samValue.y != 0)                                                                                             
                    {
                        if (m_samRatingValue.x != 0)
                            m_uiObj[2].transform.GetChild((int)m_samRatingValue.x - 1).GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 0);
                        m_samRatingValue.x = _samValue.y;

                        m_uiObj[2].transform.GetChild((int)_samValue.y - 1).GetComponent<Image>().color = new Color(1, 0, 0, 1);
                    }
                    //Choose Arousal 
                    else if (_samValue.x == 1 && _samValue.y != 0)                                                                                   
                    {
                        if (m_samRatingValue.y != 0)
                            m_uiObj[3].transform.GetChild((int)m_samRatingValue.y - 1).GetComponent<Image>().color = new Color(0.373f, 0, 0.93f, 0);
                        m_samRatingValue.y = _samValue.y;

                        m_uiObj[3].transform.GetChild((int)_samValue.y - 1).GetComponent<Image>().color = new Color(1, 0, 0, 1);
                    }

                    if (m_submitBtnState == 2)
                    {
                        m_submitBtnState = 3;
                        m_proState = 3;
                    }

                }
                if (m_submitBtnState == 0)
                {
                    if (m_samRatingValue.x != 0 && m_samRatingValue.y != 0)
                        m_submitBtnState = 1;
                }


                break;
            //Waiting for Submit
            case 3:                                                                                                                                
                if (_joycon.GetButtonUp(Joycon.Button.DPAD_UP))
                {
                    m_submitBtnState = 5;
                }
                break;
            case 4:
                m_samRatingData.Add(m_samRatingValue);

                m_samRatingData1.Add(m_samRatingValue.x.ToString());
                m_samRatingData2.Add(m_samRatingValue.y.ToString());

                m_proState = 7;
                m_samTipWords.text = "The test ends! Thank you!";
                break;
        }
    }

    public int GetProState()
    {
        return m_proState;
    }

    public void SetProState(int _state)
    {
        m_proState = _state;
    }

    public Vector2 GetSamRating()
    {
        return m_samRatingValue;
    }

    public Vector2 GetSamValue()
    {
        return m_samSelectValue;
    }

    public void SetSamValue(Vector2 _value)
    {
        m_samSelectValue = _value;
    }
    //1_pressDown, 2_press, 3_pressUp
    public int GetClickSubmitBtnState()                                                                                                             
    {
        return m_submitBtnState;
    }

    public void SetClickSubmitBtnState(int _state)
    {
        m_submitBtnState = _state;
    }

    public void SetTimeStamp(string _timeStamp)
    {
        m_timeStamp = _timeStamp;
    }

    private void OnApplicationQuit()
    {
        StringBuilder _annotateSb = new StringBuilder();
        _annotateSb.Append("ID; TimeStamp; Arousal; Valence;");
        _annotateSb.AppendLine();
        for (int i = 0; i < m_annotateTimeStamp.Count; i++)
        {
            _annotateSb.AppendFormat("{0};{1};{2};{3};", i, m_annotateTimeStamp[i], m_annotateData[i].x, m_annotateData[i].y);
            _annotateSb.AppendLine();
        }
        System.IO.File.WriteAllText(m_savePath + _avFileName + ".txt", _annotateSb.ToString());

        StringBuilder _headRotSb = new StringBuilder();
        _headRotSb.Append("ID; TimeStamp; HMDRX; HMDRY; HMDRZ;");
        _headRotSb.AppendLine();
        for (int i = 0; i < m_headTimeStamp.Count; i++)
        {
            _headRotSb.AppendFormat("{0};{1};{2};{3};{4};", i, m_headTimeStamp[i], m_headData[i].x, m_headData[i].y, m_headData[i].z);
            _headRotSb.AppendLine();
        }
        System.IO.File.WriteAllText(m_savePath + _headMoveFileName + ".txt", _headRotSb.ToString());

        StringBuilder _helperDataSb = new StringBuilder();
        _helperDataSb.Append("ID; OpenTime; CloseTime;");
        _helperDataSb.AppendLine();
        for (int i = 0; i < m_helperOpenTimeData.Count; i++)
        {
            _helperDataSb.AppendFormat("{0};{1};{2};", i, m_helperOpenTimeData[i], m_helperCloseTimeData[i]);
            _helperDataSb.AppendLine();
        }
        System.IO.File.WriteAllText(m_savePath + _helperFileName + ".txt", _helperDataSb.ToString());

        StringBuilder _SamRatingDataSb = new StringBuilder();
        _SamRatingDataSb.Append("ID; ValenceSAM; ArousalSAM;");
        _SamRatingDataSb.AppendLine();
        for (int i = 0; i < m_samRatingData1.Count; i++)
        {
            _SamRatingDataSb.AppendFormat("{0};{1};{2};", i, m_samRatingData1[i], m_samRatingData2[i]);
            _SamRatingDataSb.AppendLine();
        }
        System.IO.File.WriteAllText(m_savePath + __samFileName + ".txt", _SamRatingDataSb.ToString());

        m_videoTex.Release();
    }
}