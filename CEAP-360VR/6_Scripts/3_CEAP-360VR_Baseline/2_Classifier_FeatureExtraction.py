# this script is used to
# 1) extract behavioral and physiological features with V-A labels for hand-crafted machine learning (ML) experiments
# (20210301 1created by Tong Xue)


import pandas as pd
import numpy as np
import math


# Get the Saccades
def Get_SaccadeData(_pitchList, _yawList):
    _startFrame = 0
    _endFrame = 0
    _velocity = 0
    _velocityPre = 0
    _count = 0
    _fixationDurList = []

    _saccadeDurList = []
    _saccStartTime = 0
    _saccVelList = []
    _saccAmplitudeList = []

    for _sample in range(1, len(_pitchList)):
        _longitude1 = _yawList[_sample - 1]
        _longitude2 = _yawList[_sample]

        _latitude1 = _pitchList[_sample - 1]
        _latitude2 = _pitchList[_sample]

        assert abs(_latitude1) <= 90 and abs(_latitude2) <= 90, "invalid latitude"
        assert abs(_longitude1) <= 180 and abs(_longitude2) <= 180, "invalid longtigude"

        _angle = Get_AngleDistance(_longitude1, _latitude1, _longitude2, _latitude2)

        _velocity = _angle * 50
        _saccVelList.append(_velocity)
        _acceleration = (_velocity - _velocityPre) * 50
        _velocityPre = _velocity

        if abs(_velocity) > 75 and abs(_acceleration) > 200:
            _timeDur = _endFrame - _startFrame

            if _timeDur / 50 >= 0.15:
                _fixationDurList.append(_timeDur)

                if len(_fixationDurList) == 1:
                    if _startFrame != 0:
                        _saccadeDurList.append(_startFrame)
                        _saccAmplitudeList.append(np.max(_saccVelList[0: _startFrame]))
                else:
                    _saccadeDurList.append(_startFrame - _saccStartTime)
                    _saccAmplitudeList.append(np.max(_saccVelList[_saccStartTime: _startFrame]))
                _saccStartTime = _endFrame

            _startFrame = _sample + 1
            _endFrame = _sample + 1

            continue

        _endFrame += 1

        if _sample == len(_pitchList) - 1:
            _timeDur = _endFrame - _startFrame

            if _timeDur / 50 >= 0.15:
                _fixationDurList.append(_timeDur)
            else:
                _saccadeDurList.append(_endFrame - _saccStartTime)
                _saccAmplitudeList.append(np.max(_saccVelList[_saccStartTime: _endFrame]))

    return _saccadeDurList, _saccAmplitudeList


# Get the fixations
def Get_FixationData(_pitchList, _yawList):
    _startFrame = 0
    _endFrame = 0
    _velocity = 0
    _velocityPre = 0
    _count = 0
    _fixationDurList = []

    for _sample in range(1, len(_pitchList)):
        _longitude1 = _yawList[_sample - 1]
        _longitude2 = _yawList[_sample]

        _latitude1 = _pitchList[_sample - 1]
        _latitude2 = _pitchList[_sample]

        assert abs(_latitude1) <= 90 and abs(_latitude2) <= 90, "invalid latitude"
        assert abs(_longitude1) <= 180 and abs(_longitude2) <= 180, "invalid longtigude"

        _angle = Get_AngleDistance(_longitude1, _latitude1, _longitude2, _latitude2)

        _velocity = _angle * 50
        _acceleration = (_velocity - _velocityPre) * 50
        _velocityPre = _velocity

        if abs(_velocity) > 75 and abs(_acceleration) > 200:
            _timeDur = _endFrame - _startFrame

            if _timeDur / 50 >= 0.15:
                _fixationDurList.append(_timeDur)

            _startFrame = _sample + 1
            _endFrame = _sample + 1

            continue

        _endFrame += 1

        if _sample == len(_pitchList) - 1:
            _timeDur = _endFrame - _startFrame

            if _timeDur / 50 >= 0.15:
                _fixationDurList.append(_timeDur)

    return _fixationDurList


# Calculate Angle Dis
def Get_AngleDistance(_longitude1, _latitude1, _longitude2, _latitude2):
    _radLong1 = math.radians(_longitude1)
    _radLat1 = math.radians(_latitude1)
    _radLong2 = math.radians(_longitude2)
    _radLat2 = math.radians(_latitude2)

    _longDis = _radLong2 - _radLong1
    _latDis = _radLat2 - _radLat1
    _radDis = 2 * math.asin(math.sqrt(math.pow(math.sin(_latDis / 2), 2) +
                                      math.cos(_radLat1) * math.cos(_radLat2) * math.pow(math.sin(_longDis / 2), 2)))
    _angleDis = math.degrees(_radDis)
    return _angleDis


def CalculateVA(m_valNewList, m_aroNewList):
    m_valBinList = []
    m_aroBinList = []
    m_val3List = []
    m_aro3List = []
    m_vaClassList = []
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
            m_val3List.append(0)
        elif m_valNewList[i] >= 6:
            m_val3List.append(2)
        else:
            m_val3List.append(1)
        if m_aroNewList[i] < 3:
            m_aro3List.append(0)
        elif m_aroNewList[i] >= 6:
            m_aro3List.append(2)
        else:
            m_aro3List.append(1)

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

    return m_valBinList, m_aroBinList, m_val3List, m_aro3List, m_vaClassList


def CalculateVA_SAM(m_valNewList, m_aroNewList):
    m_valBinList = []
    m_aroBinList = []
    for i in range(len(m_valNewList)):
        if m_valNewList[i] < 5:
            m_valBinList.append(0)
        else:
            m_valBinList.append(1)
        if m_aroNewList[i] < 5:
            m_aroBinList.append(0)
        else:
            m_aroBinList.append(1)
    return m_valBinList, m_aroBinList


# segment number and length (second)
m_segLength = [1, 2, 3, 4, 5]


# _segLength: the length of segment
def OutputFeatureFile(_segLength):
    for _pid in range(1, 33):
        _sampleNum = _segLength * 50
        _segNum = int(60/_segLength)
        df = pd.read_csv("sub_split/%s.csv" % _pid, index_col=False)

        m_dataList = [[0 for i in range(23)] for j in range(50 * 60 * 8)]

        m_dataList[0] = df['HM'].tolist()
        m_dataList[1] = df['EM'].tolist()
        m_dataList[2] = df['PD_Mean'].tolist()
        m_dataList[3] = df['PD_Emotion'].tolist()
        m_dataList[4] = df['EDA'].tolist()
        m_dataList[5] = df['BVP'].tolist()
        m_dataList[6] = df['HR'].tolist()
        m_dataList[7] = df['SKT'].tolist()

        m_dataList[8] = df['HM_Pitch'].tolist()
        m_dataList[9] = df['HM_Yaw'].tolist()
        m_dataList[10] = df['EM_Pitch'].tolist()
        m_dataList[11] = df['EM_Yaw'].tolist()

        m_dataList[12] = df['Valence'].tolist()
        m_dataList[13] = df['Arousal'].tolist()
        m_dataList[14] = df['V_binary'].tolist()
        m_dataList[15] = df['A_binary'].tolist()
        m_dataList[16] = df['V_A_5class'].tolist()
        m_dataList[17] = df['VID'].tolist()

        m_dataList[18] = df['SAM_Valence'].tolist()
        m_dataList[19] = df['SAM_Arousal'].tolist()

        m_featureList = [[0 for i in range(_segNum * 8)] for j in range(63)]

        for i in range(12):
            for _seg in range(_segNum * 8):
                m_featureList[i * 3][_seg] = np.mean(m_dataList[i][_seg * _sampleNum:(_seg + 1) * _sampleNum])
                m_featureList[i * 3 + 1][_seg] = np.std(m_dataList[i][_seg * _sampleNum:(_seg + 1) * _sampleNum])
                m_featureList[i * 3 + 2][_seg] = np.median(m_dataList[i][_seg * _sampleNum:(_seg + 1) * _sampleNum])

        for _seg in range(_segNum * 8):
            m_featureList[36][_seg] = np.mean(m_dataList[12][_seg * _sampleNum:(_seg + 1) * _sampleNum])
            m_featureList[37][_seg] = np.median(m_dataList[12][_seg * _sampleNum:(_seg + 1) * _sampleNum])
            m_featureList[38][_seg] = np.mean(m_dataList[13][_seg * _sampleNum:(_seg + 1) * _sampleNum])
            m_featureList[39][_seg] = np.median(m_dataList[13][_seg * _sampleNum:(_seg + 1) * _sampleNum])
            m_featureList[40][_seg] = np.mean(m_dataList[18][_seg * _sampleNum:(_seg + 1) * _sampleNum])
            m_featureList[41][_seg] = np.median(m_dataList[19][_seg * _sampleNum:(_seg + 1) * _sampleNum])

        m_featureList[42], m_featureList[43] = CalculateVA_SAM(m_featureList[40], m_featureList[41])
        m_featureList[44], m_featureList[45], m_featureList[46], m_featureList[47], m_featureList[48] = CalculateVA(
            m_featureList[36], m_featureList[38])

        for _seg in range(_segNum * 8):
            m_fixationDurList = Get_FixationData(m_dataList[10][_seg * _sampleNum:(_seg + 1) * _sampleNum],
                                                 m_dataList[11][_seg * _sampleNum:(_seg + 1) * _sampleNum])
            m_saccadeDurList, m_saccadeAmpList = Get_SaccadeData(
                m_dataList[10][_seg * _sampleNum:(_seg + 1) * _sampleNum],
                m_dataList[11][_seg * _sampleNum:(_seg + 1) * _sampleNum])

            m_featureList[50][_seg] = len(m_fixationDurList)

            if len(m_fixationDurList) != 0:
                m_featureList[51][_seg] = np.mean(m_fixationDurList)
                m_featureList[52][_seg] = np.std(m_fixationDurList)
                m_featureList[53][_seg] = np.max(m_fixationDurList)
                m_featureList[54][_seg] = np.min(m_fixationDurList)
            else:
                for i in range(51, 55):
                    m_featureList[i][_seg] = 0

            if len(m_saccadeDurList) != 0:
                m_featureList[55][_seg] = np.mean(m_saccadeDurList)
                m_featureList[56][_seg] = np.std(m_saccadeDurList)
                m_featureList[57][_seg] = np.max(m_saccadeDurList)
                m_featureList[58][_seg] = np.min(m_saccadeDurList)
            else:
                for i in range(55, 59):
                    m_featureList[i][_seg] = 0

            if len(m_saccadeAmpList) != 0:
                m_featureList[59][_seg] = np.mean(m_saccadeAmpList)
                m_featureList[60][_seg] = np.std(m_saccadeAmpList)
                m_featureList[61][_seg] = np.max(m_saccadeAmpList)
                m_featureList[62][_seg] = np.min(m_saccadeAmpList)
            else:
                for i in range(59, 63):
                    m_featureList[i][_seg] = 0

        m_dataList = {
            "HM_Mean": m_featureList[0],
            "HM_Std": m_featureList[1],
            "HM_Median": m_featureList[2],
            "EM_Mean": m_featureList[3],
            "EM_Std": m_featureList[4],
            "EM_Median": m_featureList[5],

            "HM_Pitch_Mean": m_featureList[6],
            "HM_Pitch_Std": m_featureList[7],
            "HM_Pitch_Median": m_featureList[8],
            "HM_Yaw_Mean": m_featureList[9],
            "HM_Yaw_Std": m_featureList[10],
            "HM_Yaw_Median": m_featureList[11],
            "EM_Pitch_Mean": m_featureList[12],
            "EM_Pitch_Std": m_featureList[13],
            "EM_Pitch_Median": m_featureList[14],
            "EM_Yaw_Mean": m_featureList[15],
            "EM_Yaw_Std": m_featureList[16],
            "EM_Yaw_Median": m_featureList[17],

            "Fixation_Num": m_featureList[50],
            "Fixation_Duration_Mean": m_featureList[51],
            "Fixation_Duration_Std": m_featureList[52],
            "Fixation_Duration_Max": m_featureList[53],
            "Fixation_Duration_Min": m_featureList[54],
            "Saccade_Duration_Mean": m_featureList[55],
            "Saccade_Duration_Std": m_featureList[56],
            "Saccade_Duration_Max": m_featureList[57],
            "Saccade_Duration_Min": m_featureList[58],
            "Saccade_Amplitude_Mean": m_featureList[59],
            "Saccade_Amplitude_Std": m_featureList[60],
            "Saccade_Amplitude_Max": m_featureList[61],
            "Saccade_Amplitude_Min": m_featureList[62],

            "PD_Mean": m_featureList[18],
            "PD_Std": m_featureList[19],
            "PD_Median": m_featureList[20],
            "PDemo_Mean": m_featureList[21],
            "PDemo_Std": m_featureList[22],
            "PDemo_Median": m_featureList[23],
            "EDA_Mean": m_featureList[24],
            "EDA_Std": m_featureList[25],
            "EDA_Median": m_featureList[26],
            "BVP_Mean": m_featureList[27],
            "BVP_Std": m_featureList[28],
            "BVP_Median": m_featureList[29],
            "HR_Mean": m_featureList[30],
            "HR_Std": m_featureList[31],
            "HR_Median": m_featureList[32],
            "SKT_Mean": m_featureList[33],
            "SKT_Std": m_featureList[34],
            "SKT_Median": m_featureList[35],

            "Valence_Mean": m_featureList[36],
            "Valence_Median": m_featureList[37],
            "Arousal_Mean": m_featureList[38],
            "Arousal_Median": m_featureList[39],
            "SAM_Valence": m_featureList[40],
            "SAM_Arousal": m_featureList[41],

            "V_SAM": m_featureList[42],
            "A_SAM": m_featureList[43],
            "V_binary": m_featureList[44],
            "A_binary": m_featureList[45],
            "V_3class": m_featureList[46],
            "A_3class": m_featureList[47],
            "V_A_5class": m_featureList[48],

            "VID": m_featureList[49],
        }
        df = pd.DataFrame(m_dataList)
        _fileName = "sub_split_feature_" + str(_segLength)
        df.to_csv(_fileName + "/%s.csv" % _pid, index=False)


for i in range(len(m_segLength)):
    OutputFeatureFile(m_segLength[i])
