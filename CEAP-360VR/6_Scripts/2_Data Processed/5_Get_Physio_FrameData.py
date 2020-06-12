# this script is used to
# 1) Re-sample and align the transformed physio data except IBI
# (20200310 created by Tong Xue)

import json
import numpy as np


# video fps
m_videoFpsList = []
with open('../../1_Stimuli/VideoInfo.json', mode='r') as _videoInfoFile:
    _videoInfoData = json.load(_videoInfoFile)

for i in range(1, len(_videoInfoData["VideoInfo"])):
    m_videoFpsList.append(_videoInfoData["VideoInfo"][i]["FrameRate"])

for _pid in range(1, 33):

    m_rawPhysioDataStructure = {"Physio_FrameData": []}
    # participant PhysioDataStructure
    m_jsonParticipantPhysioDataStruct = {
        "ParticipantID": 'P%s' % str(_pid),
        "Video_Physio_FrameData":
            [

            ]
    }

    for _vid in range(0, 8):

        with open('../../5_PhysioData/Transformed/P%s_Physio_TransData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)

        _data = m_participantData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]

        m_jsonPhysioDataStruct = {
            "VideoID": 'V%s' % str(_vid + 1),
            "ACC_FrameData": [],
            "SKT_FrameData": [],
            "EDA_FrameData": [],
            "BVP_FrameData": [],
            "HR_FrameData": [],
            "IBI_FrameData": _data['IBI_TransData'],
        }

        _frameSignalList = []

        _secList = []
        _signalList = []

        # SKT
        _secList.clear()
        _signalList.clear()
        _frameSecList = np.linspace(0, 60, 60 * m_videoFpsList[_vid])

        _data = m_participantData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]["SKT_TransData"]

        for _sample in range(len(_data)):
            _secList.append(_data[_sample]['TimeStamp'])
            _signalList.append(_data[_sample]['SKT'])

        _frameSignalList = np.interp(_frameSecList, _secList, _signalList)

        for i in range(0, len(_frameSecList)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": round(_frameSecList[i], 3),
                "SKT": _frameSignalList[i],
            }
            m_jsonPhysioDataStruct['SKT_FrameData'].append(m_participantPhysioEDAStruct)

        # EDA
        _secList.clear()
        _signalList.clear()
        _frameSignalList = []

        _data = m_participantData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]["EDA_TransData"]

        for _sample in range(len(_data)):
            _secList.append(_data[_sample]['TimeStamp'])
            _signalList.append(_data[_sample]['EDA'])

        _frameSignalList = np.interp(_frameSecList, _secList, _signalList)

        for i in range(0, len(_frameSecList)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": round(_frameSecList[i], 3),
                "EDA": _frameSignalList[i],
            }
            m_jsonPhysioDataStruct['EDA_FrameData'].append(m_participantPhysioEDAStruct)

        # BVP
        _secList.clear()
        _frameSignalList = []
        _signalList.clear()

        _data = m_participantData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]["BVP_TransData"]

        for _sample in range(len(_data)):
            _secList.append(_data[_sample]['TimeStamp'])
            _signalList.append(_data[_sample]['BVP'])

        _frameSignalList = np.interp(_frameSecList, _secList, _signalList)

        for i in range(0, len(_frameSecList)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": round(_frameSecList[i], 3),
                "BVP": _frameSignalList[i],
            }
            m_jsonPhysioDataStruct['BVP_FrameData'].append(m_participantPhysioEDAStruct)

        # HR
        _secList.clear()
        _frameSignalList = []
        _signalList.clear()

        _data = m_participantData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]["HR_TransData"]

        for _sample in range(len(_data)):
            _secList.append(_data[_sample]['TimeStamp'])
            _signalList.append(_data[_sample]['HR'])

        _frameSignalList = np.interp(_frameSecList, _secList, _signalList)

        for i in range(0, len(_frameSecList)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": round(_frameSecList[i], 3),
                "HR": _frameSignalList[i],
            }
            m_jsonPhysioDataStruct['HR_FrameData'].append(m_participantPhysioEDAStruct)

        # ACC
        _secList.clear()
        _signalList.clear()
        _signal2List = []
        _signal3List = []
        _frameSignalList = []
        _frameSignalList2 = []
        _frameSignalList3 = []

        _data = m_participantData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]["ACC_RawData"]

        for _sample in range(len(_data)):
            _secList.append(_data[_sample]['TimeStamp'])
            _signalList.append(_data[_sample]['ACC_X'])
            _signal2List.append(_data[_sample]['ACC_Y'])
            _signal3List.append(_data[_sample]['ACC_Z'])

        _frameSignalList = np.interp(_frameSecList, _secList, _signalList)
        _frameSignalList2 = np.interp(_frameSecList, _secList, _signal2List)
        _frameSignalList3 = np.interp(_frameSecList, _secList, _signal3List)

        for i in range(0, len(_frameSecList)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": round(_frameSecList[i], 3),
                "ACC_X": _frameSignalList[i],
                "ACC_Y": _frameSignalList2[i],
                "ACC_Z": _frameSignalList3[i],
            }
            m_jsonPhysioDataStruct['ACC_FrameData'].append(m_participantPhysioEDAStruct)

        m_jsonParticipantPhysioDataStruct['Video_Physio_FrameData'].append(m_jsonPhysioDataStruct)

    m_rawPhysioDataStructure['Physio_FrameData'].append(m_jsonParticipantPhysioDataStruct)

    m_jsonStr = json.dumps(m_rawPhysioDataStructure, indent=4)
    with open('../../5_PhysioData/Frame/P%s_Physio_FrameData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)
