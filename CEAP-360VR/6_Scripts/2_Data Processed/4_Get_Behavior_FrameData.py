# this script is used to
# 1) Re-sample and align the transformed behavior data
# (20200310 created by Tong Xue)

import json
import numpy as np


# Get video fps
m_videoFpsList = []
with open('../../1_Stimuli/VideoInfo.json', mode='r') as _videoInfoFile:
    _videoInfoData = json.load(_videoInfoFile)

    for i in range(1, len(_videoInfoData["VideoInfo"])):
        m_videoFpsList.append(_videoInfoData["VideoInfo"][i]["FrameRate"])

# P1 - P32
for _pid in range(1, 33):

    m_rawBehaviorFrameDataStructure = {"Behavior_FrameData": []}

    m_rawBehaviorDataParticipantFrameJsonStruct = {
        "ParticipantID": 'P%s' % str(_pid),
        "Video_Behavior_FrameData": []
    }

    with open('../../4_BehaviorData/Transformed/P%s_Behavior_TransData.json' % str(_pid),
              mode='r') as _participantDataFile:
        m_participantData = json.load(_participantDataFile)

    # V1 - V8
    for _vid in range(0, 8):

        m_rawBehaviorFrameDataVideoJsonStruct = {
            "VideoID": 'V%s' % str(_vid + 1),
            "HM": [],
            "EM": [],
            "LEM": [],
            "REM": [],
            "LPD": [],
            "RPD": []
        }

        m_pitchFrameList = []
        m_yawFrameList = []
        m_timeByFrameList = np.linspace(0, 60, 60 * m_videoFpsList[_vid])

        m_pupilTimeList = []
        m_pupilList = []
        m_pupilFrameList = []

        # HM data
        _data = m_participantData['Behavior_TransData'][0]["Video_Behavior_TransData"][_vid]["HM"]
        _index = 0

        for i in range(0, len(_data)):

            if i == len(_data) - 2:
                m_pitchFrameList.append(_data[i + 1]["Pitch"])
                m_yawFrameList.append(_data[i + 1]["Yaw"])
                _index += 1
                break

            if _index == 0:
                while m_timeByFrameList[_index] < _data[i]["TimeStamp"]:
                    m_pitchFrameList.append(_data[i]["Pitch"])
                    m_yawFrameList.append(_data[i]["Yaw"])
                    _index += 1

            while _data[i]["TimeStamp"] <= m_timeByFrameList[_index] <= _data[i + 1]["TimeStamp"]:
                m_pitchFrameList.append(_data[i]["Pitch"])
                m_yawFrameList.append(_data[i]["Yaw"])
                _index += 1

        if _index != len(m_timeByFrameList):
            while _index <= len(m_timeByFrameList) - 1:
                m_pitchFrameList.append(_data[len(_data) - 1]["Pitch"])
                m_yawFrameList.append(_data[len(_data) - 1]["Yaw"])
                _index += 1

        for i in range(0, len(m_pitchFrameList)):
            m_headRotData = {
                "TimeStamp": round(m_timeByFrameList[i], 3),
                "Pitch": m_pitchFrameList[i],
                "Yaw": m_yawFrameList[i]
            }
            m_rawBehaviorFrameDataVideoJsonStruct['HM'].append(m_headRotData)

        m_pitchFrameList.clear()
        m_yawFrameList.clear()

        # EM data
        _data = m_participantData['Behavior_TransData'][0]["Video_Behavior_TransData"][_vid]["EM"]
        _index = 0

        for i in range(0, len(_data)):

            if i == len(_data) - 2:
                m_pitchFrameList.append(_data[i + 1]["Pitch"])
                m_yawFrameList.append(_data[i + 1]["Yaw"])
                _index += 1
                break

            if _index == 0:
                while m_timeByFrameList[_index] < _data[i]["TimeStamp"]:
                    m_pitchFrameList.append(_data[i]["Pitch"])
                    m_yawFrameList.append(_data[i]["Yaw"])
                    _index += 1

            while _data[i]["TimeStamp"] <= m_timeByFrameList[_index] <= _data[i + 1]["TimeStamp"]:
                m_pitchFrameList.append(_data[i]["Pitch"])
                m_yawFrameList.append(_data[i]["Yaw"])
                _index += 1

        if _index != len(m_timeByFrameList):
            while _index <= len(m_timeByFrameList) - 1:
                m_pitchFrameList.append(_data[len(_data) - 1]["Pitch"])
                m_yawFrameList.append(_data[len(_data) - 1]["Yaw"])
                _index += 1

        for i in range(0, len(m_pitchFrameList)):
            m_headRotData = {
                "TimeStamp": round(m_timeByFrameList[i], 3),
                "Pitch": m_pitchFrameList[i],
                "Yaw": m_yawFrameList[i]
            }
            m_rawBehaviorFrameDataVideoJsonStruct['EM'].append(m_headRotData)

        m_pitchFrameList.clear()
        m_yawFrameList.clear()

        # LEM data
        _data = m_participantData['Behavior_TransData'][0]["Video_Behavior_TransData"][_vid]["LEM"]
        _index = 0

        for i in range(0, len(_data)):

            if i == len(_data) - 2:
                m_pitchFrameList.append(_data[i + 1]["Pitch"])
                m_yawFrameList.append(_data[i + 1]["Yaw"])
                _index += 1
                break

            if _index == 0:
                while m_timeByFrameList[_index] < _data[i]["TimeStamp"]:
                    m_pitchFrameList.append(_data[i]["Pitch"])
                    m_yawFrameList.append(_data[i]["Yaw"])
                    _index += 1

            while _data[i]["TimeStamp"] <= m_timeByFrameList[_index] <= _data[i + 1]["TimeStamp"]:
                m_pitchFrameList.append(_data[i]["Pitch"])
                m_yawFrameList.append(_data[i]["Yaw"])
                _index += 1

        if _index != len(m_timeByFrameList):
            while _index <= len(m_timeByFrameList) - 1:
                m_pitchFrameList.append(_data[len(_data) - 1]["Pitch"])
                m_yawFrameList.append(_data[len(_data) - 1]["Yaw"])
                _index += 1

        for i in range(0, len(m_pitchFrameList)):
            m_headRotData = {
                "TimeStamp": round(m_timeByFrameList[i], 3),
                "Pitch": m_pitchFrameList[i],
                "Yaw": m_yawFrameList[i]
            }
            m_rawBehaviorFrameDataVideoJsonStruct['LEM'].append(m_headRotData)

        m_pitchFrameList.clear()
        m_yawFrameList.clear()

        # REM data
        _data = m_participantData['Behavior_TransData'][0]["Video_Behavior_TransData"][_vid]["REM"]
        _index = 0

        for i in range(0, len(_data)):

            if i == len(_data) - 2:
                m_pitchFrameList.append(_data[i + 1]["Pitch"])
                m_yawFrameList.append(_data[i + 1]["Yaw"])
                _index += 1
                break

            if _index == 0:
                while m_timeByFrameList[_index] < _data[i]["TimeStamp"]:
                    m_pitchFrameList.append(_data[i]["Pitch"])
                    m_yawFrameList.append(_data[i]["Yaw"])
                    _index += 1

            while _data[i]["TimeStamp"] <= m_timeByFrameList[_index] <= _data[i + 1]["TimeStamp"]:
                m_pitchFrameList.append(_data[i]["Pitch"])
                m_yawFrameList.append(_data[i]["Yaw"])
                _index += 1

        if _index != len(m_timeByFrameList):
            while _index <= len(m_timeByFrameList) - 1:
                m_pitchFrameList.append(_data[len(_data) - 1]["Pitch"])
                m_yawFrameList.append(_data[len(_data) - 1]["Yaw"])
                _index += 1

        for i in range(0, len(m_pitchFrameList)):
            m_headRotData = {
                "TimeStamp": round(m_timeByFrameList[i], 3),
                "Pitch": m_pitchFrameList[i],
                "Yaw": m_yawFrameList[i]
            }
            m_rawBehaviorFrameDataVideoJsonStruct['REM'].append(m_headRotData)

        m_pitchFrameList.clear()
        m_yawFrameList.clear()

        # LPD data
        _data = m_participantData['Behavior_TransData'][0]["Video_Behavior_TransData"][_vid]["LPD"]

        for i in range(0, len(_data)):
            _leftPupil = _data[i]["PD"]
            if _leftPupil != 0:
                m_pupilTimeList.append(_data[i]["TimeStamp"])
                m_pupilList.append(_data[i]["PD"])

        m_pupilFrameList = np.interp(m_timeByFrameList, m_pupilTimeList, m_pupilList)

        for i in range(0, len(m_pupilFrameList)):
            m_headRotData = {
                "TimeStamp": round(m_timeByFrameList[i], 3),
                "PD": round(m_pupilFrameList[i], 3),
            }
            m_rawBehaviorFrameDataVideoJsonStruct['LPD'].append(m_headRotData)

        # RPD data
        _data = m_participantData['Behavior_TransData'][0]["Video_Behavior_TransData"][_vid]["RPD"]

        for i in range(0, len(_data)):
            _leftPupil = _data[i]["PD"]
            if _leftPupil != 0:
                m_pupilTimeList.append(_data[i]["TimeStamp"])
                m_pupilList.append(_data[i]["PD"])

        m_pupilFrameList = np.interp(m_timeByFrameList, m_pupilTimeList, m_pupilList)
        for i in range(0, len(m_pupilFrameList)):
            m_headRotData = {
                "TimeStamp": round(m_timeByFrameList[i], 3),
                "PD": round(m_pupilFrameList[i], 3),
            }
            m_rawBehaviorFrameDataVideoJsonStruct['RPD'].append(m_headRotData)

        m_rawBehaviorDataParticipantFrameJsonStruct['Video_Behavior_FrameData'].append(
            m_rawBehaviorFrameDataVideoJsonStruct)

    m_rawBehaviorFrameDataStructure['Behavior_FrameData'].append(m_rawBehaviorDataParticipantFrameJsonStruct)

    m_jsonStr = json.dumps(m_rawBehaviorFrameDataStructure, indent=4)
    with open('../../4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)
