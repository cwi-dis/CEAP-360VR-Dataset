# this script is used to
# 1) Transform the raw physiological data (EDA, BVP, SKT) by a lwo-pass filter and normalization
# (20200310 created by Tong Xue)

import json
import scipy.signal as signal
import numpy as np

m_secList = []
m_sigList = []


# low-pass filter and normalization
def SignalTrans(_signalList, order, cutoff):
    wn = 2 * cutoff / 4
    b, a = signal.butter(order, wn, 'lowpass')
    filterY = signal.filtfilt(b, a, _signalList)

    _range = np.max(filterY) - np.min(filterY)
    filterY1 = (filterY - np.min(filterY)) / _range
    return filterY1


for _pid in range(1, 33):

    m_rawPhysioDataStructure = {"Physio_TransData": []}
    m_jsonParticipantPhysioDataStruct = {
        "ParticipantID": 'P%s' % str(_pid),
        "Video_Physio_TransData":
            [

            ]
    }

    for _vid in range(0, 8):

        with open('../../5_PhysioData/Raw/P%s_Physio_RawData.json' % str(_pid), mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)

        _data = m_participantData['Physio_RawData'][0]['Video_Physio_RawData'][_vid]

        m_jsonPhysioDataStruct = {
            "VideoID": 'V%s' % str(_vid + 1),
            "ACC_RawData": _data['ACC_RawData'],
            "SKT_TransData": [],
            "EDA_TransData": [],
            "BVP_TransData": [],
            "HR_TransData": _data['HR_RawData'],
            "IBI_TransData": _data['IBI_RawData']
        }

        # EDA
        m_secList.clear()
        m_sigList.clear()
        _data = m_participantData['Physio_RawData'][0]['Video_Physio_RawData'][_vid]["EDA_RawData"]

        for _sample in range(len(_data)):
            m_secList.append(_data[_sample]['TimeStamp'])
            m_sigList.append(_data[_sample]['EDA'])

        m_transData = SignalTrans(m_sigList, 3, 0.5)
        for i in range(0, len(m_transData)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": m_secList[i],
                "EDA": m_transData[i],
            }
            m_jsonPhysioDataStruct['EDA_TransData'].append(m_participantPhysioEDAStruct)

        # BVP
        m_secList.clear()
        m_sigList.clear()
        _data = m_participantData['Physio_RawData'][0]['Video_Physio_RawData'][_vid]["BVP_RawData"]

        for _sample in range(len(_data)):
            m_secList.append(_data[_sample]['TimeStamp'])
            m_sigList.append(_data[_sample]['BVP'])

        m_transData = SignalTrans(m_sigList, 3, 0.5)

        for i in range(0, len(m_transData)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": m_secList[i],
                "BVP": m_transData[i],
            }
            m_jsonPhysioDataStruct['BVP_TransData'].append(m_participantPhysioEDAStruct)

        # trans SKT
        m_secList.clear()
        m_sigList.clear()
        _data = m_participantData['Physio_RawData'][0]['Video_Physio_RawData'][_vid]["SKT_RawData"]

        for _sample in range(len(_data)):
            m_secList.append(_data[_sample]['TimeStamp'])
            m_sigList.append(_data[_sample]['SKT'])

        m_transData = SignalTrans(m_sigList, 3, 0.5)
        for i in range(0, len(m_transData)):
            m_participantPhysioEDAStruct = {
                "TimeStamp": m_secList[i],
                "SKT": m_transData[i],
            }
            m_jsonPhysioDataStruct['SKT_TransData'].append(m_participantPhysioEDAStruct)

        m_jsonParticipantPhysioDataStruct['Video_Physio_TransData'].append(m_jsonPhysioDataStruct)

    m_rawPhysioDataStructure['Physio_TransData'].append(m_jsonParticipantPhysioDataStruct)

    m_jsonStr = json.dumps(m_rawPhysioDataStructure, indent=4)
    with open('../../5_PhysioData/Transformed/P%s_Physio_TransData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)
