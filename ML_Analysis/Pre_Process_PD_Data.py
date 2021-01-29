"""
Created on Tue Jan 26 09:34:10 2021
this script is used to calculate PD_Emotion and PD_Luminance
@author: S1
"""

from sklearn import linear_model
import json
import numpy as np

m_pdList = []
m_hsvList = []

for _pid in range(1, 33):

    m_PD_File = {
        "ParticipantID": 'P%s' % str(_pid),
        "Video_PD_Data": [],
    }

    with open('Pre_VideoInfo_luminance.json', 'r') as _participantDataFile:
        m_videoData = json.load(_participantDataFile)

    m_lpdList = []
    m_rpdList = []
    m_pdMeanList = []
    m_pdLuminanceList = []
    m_pdEmotionList = []
    m_videoV = []

    for _vid in range(0, 8):
        with open('../CEAP-360VR/4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)
        _dataLPD = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['LPD']
        _dataRPD = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['RPD']

        for _sample in range(len(_dataLPD)):
            m_lpdList.append(_dataLPD[_sample]['PD'])
            m_rpdList.append(_dataRPD[_sample]['PD'])
            m_pdMeanList.append((_dataLPD[_sample]['PD']+_dataRPD[_sample]['PD'])/2)
        m_videoV.extend(m_videoData['Video_InfoData'][_vid]['V'])

    m_videoV = np.array(m_videoV)
    regr = linear_model.LinearRegression()
    regr.fit(m_videoV.reshape(-1, 1), m_pdMeanList)
    k, b = regr.coef_, regr.intercept_
    m_videoV = m_videoV.tolist()
    m_pdLuminanceList = k*m_videoV +b
    m_pdEmotionList = m_pdMeanList - m_pdLuminanceList

    m_videoPDdata = {
        "VideoID": 'V%s' % str(1),
        "LPD": m_lpdList[0:1500],
        "RPD": m_rpdList[0:1500],
        "PD_Mean": m_pdMeanList[0:1500],
        "PD_luminance": [float('{:.4f}'.format(i)) for i in m_pdLuminanceList[0:1500]],
        "PD_emotion": [float('{:.4f}'.format(i)) for i in m_pdEmotionList[0:1500]],
        "Linear_k": round(k[0], 4),
        "Linear_b": round(b, 4),
    }
    m_PD_File['Video_PD_Data'].append(m_videoPDdata)

    for _vid in range(1, 8):
        m_videoPDdata = {
            "VideoID": 'V%s' % str(_vid + 1),
            "LPD": m_lpdList[1500+1800*(_vid-1):1500+1800*_vid],
            "RPD": m_rpdList[1500+1800*(_vid-1):1500+1800*_vid],
            "PD_Mean": m_pdMeanList[1500+1800*(_vid-1):1500+1800*_vid],
            "PD_luminance": [float('{:.4f}'.format(i)) for i in m_pdLuminanceList[1500+1800*(_vid-1):1500+1800*_vid]],
            "PD_emotion": [float('{:.4f}'.format(i)) for i in m_pdEmotionList[1500+1800*(_vid-1):1500+1800*_vid]],
            "Linear_k": round(k[0], 4),
            "Linear_b": round(b, 4),
        }
        m_PD_File['Video_PD_Data'].append(m_videoPDdata)

    m_jsonStr = json.dumps(m_PD_File, indent=4)
    with open('Processed_PD_Data/P%s_PD_ProcessedData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)





# from sklearn import linear_model
# import json
# import numpy as np
#
# m_pdList = []
# m_hsvList = []
#
# for _pid in range(1, 33):
#
#     m_PD_File = {
#         "ParticipantID": 'P%s' % str(_pid),
#         "Video_PD_Data": [],
#     }
#
#     with open('0_Video/VideoInfo1.json', 'r') as _participantDataFile:
#         m_videoData = json.load(_participantDataFile)
#
#     for _vid in range(0, 8):
#         with open('4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid),
#                   mode='r') as _participantDataFile:
#             m_participantData = json.load(_participantDataFile)
#         _dataLPD = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['LPD']
#         _dataRPD = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['RPD']
#
#         m_lpdList = []
#         m_rpdList = []
#         m_pdMeanList = []
#         m_pdLuminanceList = []
#         m_pdEmotionList = []
#
#         for _sample in range(len(_dataLPD)):
#             m_lpdList.append(_dataLPD[_sample]['PD'])
#             m_rpdList.append(_dataRPD[_sample]['PD'])
#             m_pdMeanList.append((_dataLPD[_sample]['PD']+_dataRPD[_sample]['PD'])/2)
#         m_videoV = m_videoData['Video_InfoData'][_vid]['V']
#
#         m_videoV = np.array(m_videoV)
#         regr = linear_model.LinearRegression()
#         regr.fit(m_videoV.reshape(-1, 1), m_pdMeanList)
#         k, b = regr.coef_, regr.intercept_
#         m_videoV = m_videoV.tolist()
#         m_pdLuminanceList = k*m_videoV +b
#         m_pdEmotionList = m_pdMeanList - m_pdLuminanceList
#         print(m_pdEmotionList)
#         m_videoPDdata = {
#             "VideoID": 'V%s' % str(_vid + 1),
#             "LPD": m_lpdList,
#             "RPD": m_rpdList,
#             "PD_Mean": m_pdMeanList,
#             "PD_luminance": [float('{:.4f}'.format(i)) for i in m_pdLuminanceList],
#             "PD_emotion": [float('{:.4f}'.format(i)) for i in m_pdEmotionList],
#             "Linear_k": round(k[0], 4),
#             "Linear_b": b,
#         }
#         m_PD_File['Video_PD_Data'].append(m_videoPDdata)
#
#     m_jsonStr = json.dumps(m_PD_File, indent=4)
#     with open('Processed_PD_Data/P%s_PD_ProcessedData.json' % str(_pid), 'w') as f:
#         f.write(m_jsonStr)
#
