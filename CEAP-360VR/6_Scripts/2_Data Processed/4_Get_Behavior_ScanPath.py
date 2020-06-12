# this script is used to
# 1) Get head scan path points
# (20200310 created by Tong Xue)

import json
import math
import numpy as np

# Get video fps
m_videoFpsList = []
with open('../../1_Stimuli/VideoInfo.json', mode='r') as _videoInfoFile:
    _videoInfoData = json.load(_videoInfoFile)
for i in range(1, len(_videoInfoData["VideoInfo"])):
    m_videoFpsList.append(_videoInfoData["VideoInfo"][i]["FrameRate"])


# Calculate Angle Distance
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


# Get the centroid coordinate
def Get_CentroidPoint(_yawList, _pitchList):
    if max(_yawList) - min(_yawList) > 200:

        for _sample in range(len(_yawList)):
            if _yawList[_sample] < 0:
                _yawList[_sample] = 360 + _yawList[_sample]

    x = np.mean(_yawList)
    y = np.mean(_pitchList)

    if x > 180:
        x = x - 360

    return [x, y]


# Get the scan path points
def Get_HM_ScanPathData():
    # P1 - P32
    for _pid in range(1, 33):

        with open('../../4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)

        m_processedBehaviorFrameDataStructure = {"Behavior_HeadScanPath_Data": []}

        m_processedBehaviorDataParticipantJsonStruct = {
            "ParticipantID": 'P%s' % str(_pid),
            "Video_Behavior_ScanPath_Data": []
        }

        for _vid in range(0, 8):

            m_processedBehaviorVideoStructure = {
                "VideoID": "V%s" % str(_vid + 1),
                "ID_Pitch_Yaw": []
            }

            _frameData = m_participantData['Behavior_FrameData'][0]["Video_Behavior_FrameData"][_vid]["HM"]

            _velocityPre = 0

            m_pitchList = []
            m_yawList = []
            _windowCount = 1

            for i in range(1, len(_frameData)):

                _time = _frameData[i]["TimeStamp"]

                # each 200ms window
                if _time > 0.2 * _windowCount:
                    _centroid = Get_CentroidPoint(m_yawList, m_pitchList)

                    m_scanPath_Data = {
                        "ID": _windowCount,
                        "Pitch": round(_centroid[1], 3),
                        "Yaw": round(_centroid[0], 3),
                    }
                    m_processedBehaviorVideoStructure['ID_Pitch_Yaw'].append(m_scanPath_Data)

                    _windowCount += 1

                    m_pitchList.clear()
                    m_yawList.clear()

                    m_pitchList.append(_frameData[i]["Pitch"])
                    m_yawList.append(_frameData[i]["Yaw"])

                    continue

                m_pitchList.append(_frameData[i]["Pitch"])
                m_yawList.append(_frameData[i]["Yaw"])

            m_processedBehaviorDataParticipantJsonStruct['Video_Behavior_ScanPath_Data'].append(
                m_processedBehaviorVideoStructure)
        m_processedBehaviorFrameDataStructure['Behavior_HeadScanPath_Data'].append(
            m_processedBehaviorDataParticipantJsonStruct)

        # save as json file
        m_jsonStr = json.dumps(m_processedBehaviorFrameDataStructure, indent=4)
        with open('../../4_BehaviorData/HM_ScanPath/P%s_Behavior_HeadScanPathData.json' % str(_pid), 'w') as f:
            f.write(m_jsonStr)


Get_HM_ScanPathData()
