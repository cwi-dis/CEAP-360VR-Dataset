# this script is used to
# 1) generate processed behavioral and physiological data with V-A labels for deep learning (DL) experiments.
# (20210301 1created by Tong Xue)

import json
import pandas as pd
import math
from sklearn.decomposition import PCA
import numpy as np


def Normalization(_signalList):
    _range = np.max(_signalList) - np.min(_signalList)
    filterY1 = (_signalList - np.min(_signalList)) / _range
    return filterY1


m_valList = []
m_aroList = []
m_neutral = 0
m_video_v = [1, 1, 0, 0, 1, 1, 0, 0]
m_video_a = [1, 0, 1, 0, 1, 0, 1, 0]
# Get video fps
m_videoFpsList = []
with open('../CEAP-360VR/1_Stimuli/VideoInfo.json', mode='r') as _videoInfoFile:
    _videoInfoData = json.load(_videoInfoFile)

    for i in range(1, len(_videoInfoData["VideoInfo"])):
        m_videoFpsList.append(_videoInfoData["VideoInfo"][i]["FrameRate"])


def PYtoXYZ(pitch, yaw):
    x = math.cos(pitch) * math.sin(yaw)
    y = math.cos(pitch) * math.cos(yaw)
    z = math.sin(pitch)
    return round(x, 5), round(y, 5), round(z, 5)


for _pid in range(1, 33):
    m_vidList = []
    m_hmNewList = []
    m_emNewList = []

    m_hmPitchList = []
    m_hmYawList = []
    m_emPitchList = []
    m_emYawList = []

    m_hmPitch_n_List = []
    m_hmYaw_n_List = []
    m_emPitch_n_List = []
    m_emYaw_n_List = []

    m_valNewList = []
    m_aroNewList = []
    m_valBinList = []
    m_aroBinList = []
    m_val3classList = []
    m_aro3classList = []
    m_vaClassList = []
    m_lpdNewList = []
    m_rpdNewList = []

    m_edaNewList = []
    m_bvpNewList = []
    m_hrNewList = []
    m_sktNewList = []

    m_samValenceList = []
    m_samArousalList = []

    m_samV_label = []
    m_samA_Label = []

    for _vid in range(0, 8):
        _oldSecList = np.linspace(0, 60, 60 * m_videoFpsList[_vid])
        _newSecList = np.linspace(0, 60, 60 * 50)
        for i in range(60 * 50):
            m_samV_label.append(m_video_v[_vid])
            m_samA_Label.append(m_video_a[_vid])

        with open('Processed_PD_Data/P%s_PD_ProcessedData.json' % str(_pid),
                  mode='r') as _behaviorDataFile:
            m_behaviorRawData = json.load(_behaviorDataFile)

        _pdEmotion = m_behaviorRawData['Video_PD_Data'][_vid]['PD_emotion']
        _pd = m_behaviorRawData['Video_PD_Data'][_vid]['PD_Mean']

        with open('../CEAP-360VR/2_QuestionnaireData/P%s_Questionnaire_Data.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)
        _SAMv = m_participantData['QuestionnaireData'][0]['Video_SAMRating_VideoTime_Data'][_vid]['ValenceValue']
        _SAMa = m_participantData['QuestionnaireData'][0]['Video_SAMRating_VideoTime_Data'][_vid]['ArousalValue']

        for i in range(50 * 60):
            m_vidList.append(_vid + 1)
            m_samValenceList.append(_SAMv)
            m_samArousalList.append(_SAMa)

        with open('../CEAP-360VR/3_AnnotationData/Frame/P%s_Annotation_FrameData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)
        _dataVA = m_participantData['ContinuousAnnotation_FrameData'][0]['Video_Annotation_FrameData'][_vid][
            'TimeStamp_Valence_Arousal']

        with open('../CEAP-360VR/5_PhysioData/Frame/P%s_Physio_FrameData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)
        _dataEDA = m_participantData['Physio_FrameData'][0]['Video_Physio_FrameData'][_vid]['EDA_FrameData']
        _dataBVP = m_participantData['Physio_FrameData'][0]['Video_Physio_FrameData'][_vid]['BVP_FrameData']
        _dataSKT = m_participantData['Physio_FrameData'][0]['Video_Physio_FrameData'][_vid]['SKT_FrameData']
        _dataHR = m_participantData['Physio_FrameData'][0]['Video_Physio_FrameData'][_vid]['HR_FrameData']

        with open('../CEAP-360VR/4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)
        _dataHM = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['HM']
        _dataEM = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['EM']
        _dataLPD = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['LPD']
        _dataRPD = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['RPD']

        _hmRXList = []
        _hmRYList = []
        _hmRZList = []
        _emRXList = []
        _emRYList = []
        _emRZList = []
        _pdList = []
        _pdEmotionList = []

        _hmPitchList = []
        _hmYawList = []
        _emPitchList = []
        _emYawList = []

        _edaList = []
        _bvpList = []
        _hrList = []
        _sktList = []

        _valenceList = []
        _arousalList = []

        for _sample in range(len(_dataVA)):
            _valenceList.append(_dataVA[_sample]['Valence'])
            _arousalList.append(_dataVA[_sample]['Arousal'])

            _hmPitch = math.radians(_dataHM[_sample]['Pitch'])
            _hmYaw = math.radians(_dataHM[_sample]['Yaw'])
            _hmX, _hmY, _hmZ = PYtoXYZ(_hmPitch, _hmYaw)

            _empitch = math.radians(_dataEM[_sample]['Pitch'])
            _emyaw = math.radians(_dataEM[_sample]['Yaw'])
            _emX, _emY, _emZ = PYtoXYZ(_empitch, _emyaw)

            _edaList.append(_dataEDA[_sample]['EDA'])
            _bvpList.append(_dataBVP[_sample]['BVP'])
            _hrList.append(_dataHR[_sample]['HR'])
            _sktList.append(_dataSKT[_sample]['SKT'])

            _hmRXList.append(_hmX)
            _hmRYList.append(_hmY)
            _hmRZList.append(_hmZ)

            _emRXList.append(_emX)
            _emRYList.append(_emY)
            _emRZList.append(_emZ)

            _hmPitchList.append(_dataHM[_sample]['Pitch'])
            _hmYawList.append(_dataHM[_sample]['Yaw'])
            _emPitchList.append(_dataEM[_sample]['Pitch'])
            _emYawList.append(_dataEM[_sample]['Yaw'])


        _pdList = _pd
        _pdEmotionList = _pdEmotion
        _hmRList = list(zip(_hmRXList, _hmRYList, _hmRZList))
        pca = PCA(1)
        pca.fit(_hmRList)
        _hmListTemp = np.reshape(pca.transform(_hmRList), -1)
        _hmList = _hmListTemp.tolist()
        _hmNewList = np.interp(_newSecList, _oldSecList, _hmList)

        _emRList = list(zip(_emRXList, _emRYList, _emRZList))
        pca = PCA(1)
        pca.fit(_emRList)
        _emListTemp = np.reshape(pca.transform(_emRList), -1)
        _emList = _emListTemp.tolist()
        _emNewList = np.interp(_newSecList, _oldSecList, _emList)

        m_hmNewList.extend(Normalization(_hmNewList))
        m_emNewList.extend(Normalization(_emNewList))

        _hmYawlp = np.interp(_newSecList, _oldSecList, _hmYawList)
        m_hmYawList.extend(_hmYawlp)
        _hmPitchlp = np.interp(_newSecList, _oldSecList, _hmPitchList)
        m_hmPitchList.extend(_hmPitchlp)

        _emYawlp = np.interp(_newSecList, _oldSecList, _emYawList)
        m_emYawList.extend(_emYawlp)
        _emPitchlp = np.interp(_newSecList, _oldSecList, _emPitchList)
        m_emPitchList.extend(_emPitchlp)

        _hmYaw_n_lp = np.interp(_newSecList, _oldSecList, _hmYawList)
        m_hmYaw_n_List.extend(Normalization(_hmYaw_n_lp))
        _hmPitch_n_lp = np.interp(_newSecList, _oldSecList, _hmPitchList)
        m_hmPitch_n_List.extend(Normalization(_hmPitch_n_lp))

        _emYaw_n_lp = np.interp(_newSecList, _oldSecList, _emYawList)
        m_emYaw_n_List.extend(Normalization(_emYaw_n_lp))
        _emPitch_n_lp = np.interp(_newSecList, _oldSecList, _emPitchList)
        m_emPitch_n_List.extend(Normalization(_emPitch_n_lp))

        _valNewList = np.interp(_newSecList, _oldSecList, _valenceList)
        _aroNewList = np.interp(_newSecList, _oldSecList, _arousalList)
        m_valNewList.extend(_valNewList)
        m_aroNewList.extend(_aroNewList)

        _lpdNewList = np.interp(_newSecList, _oldSecList, _pdList)
        _rpdNewList = np.interp(_newSecList, _oldSecList, _pdEmotionList)
        m_lpdNewList.extend(Normalization(_lpdNewList))
        m_rpdNewList.extend(Normalization(_rpdNewList))

        _edaNewList = np.interp(_newSecList, _oldSecList, _edaList)
        _bvpNewList = np.interp(_newSecList, _oldSecList, _bvpList)
        _hrNewList = np.interp(_newSecList, _oldSecList, _hrList)
        _sktNewList = np.interp(_newSecList, _oldSecList, _sktList)
        m_edaNewList.extend(_edaNewList)
        m_bvpNewList.extend(_bvpNewList)
        m_hrNewList.extend(Normalization(_hrNewList))
        m_sktNewList.extend(Normalization(_sktNewList))

    for i in range(len(m_valNewList)):
        if m_valNewList[i] < 5:
            m_valBinList.append(0)
        else:
            m_valBinList.append(1)
        if m_aroNewList[i] < 5:
            m_aroBinList.append(0)
        else:
            m_aroBinList.append(1)

        if m_valNewList[i] < 3:
            m_val3classList.append(0)
        elif m_valNewList[i] >= 6:
            m_val3classList.append(2)
        else:
            m_val3classList.append(1)
        if m_aroNewList[i] < 3:
            m_aro3classList.append(0)
        elif m_aroNewList[i] >= 6:
            m_aro3classList.append(2)
        else:
            m_aro3classList.append(1)

        if m_valNewList[i] > 5 and m_aroNewList[i] > 5:
            m_vaClassList.append(0)
        elif m_valNewList[i] > 5 and m_aroNewList[i] < 5:
            m_vaClassList.append(1)
        elif m_valNewList[i] < 5 and m_aroNewList[i] < 5:
            m_vaClassList.append(2)
        elif m_valNewList[i] < 5 and m_aroNewList[i] > 5:
            m_vaClassList.append(3)
        else:
            m_vaClassList.append(4)

    m_dataList = {
        "HM": m_hmNewList,
        "EM": m_emNewList,
        "HM_Pitch_N": m_hmPitch_n_List,
        "HM_Yaw_N": m_hmYaw_n_List,
        "EM_Pitch_N": m_emPitch_n_List,
        "EM_Yaw_N": m_emYaw_n_List,
        "PD_Mean": m_lpdNewList,
        "PD_Emotion": m_rpdNewList,
        "EDA": m_edaNewList,
        "BVP": m_bvpNewList,
        "HR": m_hrNewList,
        "SKT": m_sktNewList,
        "V_binary": m_valBinList,
        "A_binary": m_aroBinList,
        "V_3class": m_val3classList,
        "A_3class": m_aro3classList,
        "V_A_5class": m_vaClassList,
        "Valence": m_valNewList,
        "Arousal": m_aroNewList,
        "SAM_Valence": m_samValenceList,
        "SAM_Arousal": m_samArousalList,
        "HM_Pitch": m_hmPitchList,
        "HM_Yaw": m_hmYawList,
        "EM_Pitch": m_emPitchList,
        "EM_Yaw": m_emYawList,
        "SAM_V_Label": m_samV_label,
        "SAM_A_Label": m_samA_Label,
        "VID": m_vidList,
    }
    df = pd.DataFrame(m_dataList)
    df.to_csv("sub_split/%s.csv" % _pid, index=False)

#     m_valList.extend(m_valNewList)
#     m_aroList.extend(m_aroNewList)
#
# for i in range(len(m_valList)):
#     if m_valList[i] == 5 and m_aroList[i] != 5:
#         m_neutral += 1
#     if m_aroList[i] != 5 and m_valList[i] == 5:
#         m_neutral += 1
#
# print(m_neutral)
