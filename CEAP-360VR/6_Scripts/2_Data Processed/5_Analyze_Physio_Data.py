# this script is used to
# 1) Get the Z-scored mean of Physio data (EDA SKT HR IBI) by participant
# 3) Note that the IBI data from P2 and P12 are missing
# (20200310 created by Tong Xue)

import json
import numpy as np
import pandas as pd


# EDA changes
def EDA_Vary(_signalList, wn):
    _velocityList = [0]
    for i in range(1, len(_signalList)):
        _v = (_signalList[i] - _signalList[i - 1]) / wn
        _velocityList.append(abs(_v))
    return _velocityList


# Z-score
def z_score_normalization(x):
    x = (x - np.mean(x)) / np.std(x)
    return x


# get the Z-scored mean value of physio data by participant
def Generate_MeanPhysio_Data():
    m_HR_List = [[0 for i in range(8)] for j in range(32)]
    m_EDA_List = [[0 for i in range(8)] for j in range(32)]
    m_SKT_List = [[0 for i in range(8)] for j in range(32)]
    m_IBI_List = [[0 for i in range(8)] for j in range(32)]

    m_physio_File = [[0 for i in range(5)] for j in range(32 * 8)]

    for _pid in range(1, 33):

        with open('../../5_PhysioData/Transformed/P%s_Physio_TransData.json' % str(_pid),
                  mode='r') as _behaviorDataFile:
            m_physioRawData = json.load(_behaviorDataFile)

        for _vid in range(0, 8):

            m_hrList = []
            m_sktList = []
            m_eadList = []
            m_ibilist = []

            _sktData = m_physioRawData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]['SKT_TransData']
            _hrdata = m_physioRawData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]['HR_TransData']
            _edaData = m_physioRawData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]['EDA_TransData']
            _ibiData = m_physioRawData['Physio_TransData'][0]['Video_Physio_TransData'][_vid]['IBI_TransData']

            # EDA
            for i in range(len(_edaData)):
                m_eadList.append(_edaData[i]['EDA'])
            m_vList = EDA_Vary(m_eadList, 0.25)
            m_EDA_List[_pid - 1][_vid] = np.mean(m_vList)

            # SKT
            for i in range(len(_sktData)):
                m_sktList.append(_sktData[i]['SKT'])
            m_SKT_List[_pid - 1][_vid] = np.nanmean(m_sktList)

            # HR
            for i in range(len(_hrdata)):
                m_hrList.append(_hrdata[i]['HR'])
            m_HR_List[_pid - 1][_vid] = np.nanmean(m_hrList)

            # IBI
            for i in range(len(_ibiData)):
                m_ibilist.append(float(_ibiData[i]['IBI']))
            m_IBI_List[_pid - 1][_vid] = np.nanstd(m_ibilist)

    # Z-score
    for i in range(32):
        x = z_score_normalization(m_HR_List[i])
        y = z_score_normalization(m_SKT_List[i])
        z = z_score_normalization(m_EDA_List[i])
        b = z_score_normalization(m_IBI_List[i])

        for j in range(8):
            m_physio_File[i * 8 + j][0] = 'V' + str(j + 1)
            m_physio_File[i * 8 + j][1] = round(x[j], 3)
            m_physio_File[i * 8 + j][2] = round(y[j], 3)
            m_physio_File[i * 8 + j][3] = round(z[j], 3)
            m_physio_File[i * 8 + j][4] = round(b[j], 3)

    df = pd.DataFrame(m_physio_File)
    df.to_excel("Physio_File.xlsx")
