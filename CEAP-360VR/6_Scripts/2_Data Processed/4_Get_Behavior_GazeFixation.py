# this script is used to
# 1) Get eye gaze fixation data
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


# Get the fixations
def Get_FixationData():
    m_fixationCount = [[0 for i in range(8)] for i in range(32)]
    # P1 - P32
    for _pid in range(1, 33):

        with open('../../4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid),
                  mode='r') as _participantDataFile:
            m_participantData = json.load(_participantDataFile)

        m_processedBehaviorFrameDataStructure = {"Behavior_FixationData": []}

        m_processedBehaviorDataParticipantJsonStruct = {
            "ParticipantID": 'P%s' % str(_pid),
            "Video_Behavior_FixationData": []
        }
        # V1 - V8
        for _vid in range(0, 8):

            m_processedBehaviorVideoStructure = {
                "VideoID": "V%s" % str(_vid + 1),
                "Fixation": []
            }

            _frameData = m_participantData['Behavior_FrameData'][0]["Video_Behavior_FrameData"][_vid]["EM"]

            _startFrame = 0
            _endFrame = 0
            _velocity = 0
            _velocityPre = 0
            _count = 0

            _yawList = []
            _pitchList = []

            # Calculate Velocity and acceleration
            for _sample in range(1, len(_frameData)):
                _longitude1 = _frameData[_sample - 1]["Yaw"]
                _longitude2 = _frameData[_sample]["Yaw"]

                _latitude1 = _frameData[_sample - 1]["Pitch"]
                _latitude2 = _frameData[_sample]["Pitch"]

                assert abs(_latitude1) <= 90 and abs(_latitude2) <= 90, "invalid latitude"
                assert abs(_longitude1) <= 180 and abs(_longitude2) <= 180, "invalid longtigude"

                _angle = Get_AngleDistance(_longitude1, _latitude1, _longitude2, _latitude2)

                _velocity = _angle * m_videoFpsList[_vid]
                _acceleration = (_velocity - _velocityPre) * m_videoFpsList[_vid]
                _velocityPre = _velocity

                if abs(_velocity) > 75 or abs(_acceleration) > 200:

                    _timeDur = _endFrame - _startFrame

                    if _timeDur / m_videoFpsList[_vid] >= 0.2:
                        _fixation = Get_CentroidPoint(_yawList, _pitchList)

                        m_HE_FixationData = {
                            "StartFrame": _startFrame,
                            "EndFrame": _endFrame - 1,
                            "Pitch": round(_fixation[1], 3),
                            "Yaw": round(_fixation[0], 3)
                        }

                        m_processedBehaviorVideoStructure['Fixation'].append(m_HE_FixationData)
                        m_fixationCount[_pid - 1][_vid] += 1

                        _yawList.clear()

                    _startFrame = _sample + 1
                    _endFrame = _sample + 1

                    _yawList.clear()
                    _pitchList.clear()

                    continue

                _endFrame += 1

                _yawList.append(_longitude2)
                _pitchList.append(_latitude2)

                if _sample == len(_frameData) - 1:
                    _timeDur = _endFrame - _startFrame

                    if _timeDur / m_videoFpsList[_vid] >= 0.2:
                        _fixation = Get_CentroidPoint(_yawList, _pitchList)

                        m_HE_FixationData = {
                            "StartFrame": _startFrame,
                            "EndFrame": _endFrame - 1,
                            "Pitch": round(_fixation[1], 3),
                            "Yaw": round(_fixation[0], 3)
                        }
                        m_processedBehaviorVideoStructure['Fixation'].append(m_HE_FixationData)
                        m_fixationCount[_pid - 1][_vid] += 1

            m_processedBehaviorDataParticipantJsonStruct['Video_Behavior_FixationData'].append(
                m_processedBehaviorVideoStructure)
        m_processedBehaviorFrameDataStructure['Behavior_FixationData'].append(
            m_processedBehaviorDataParticipantJsonStruct)

        # save as json file
        m_jsonStr = json.dumps(m_processedBehaviorFrameDataStructure, indent=4)
        with open('../../4_BehaviorData/EM_Fixation/P%s_Behavior_FixationData.json' % str(_pid), 'w') as f:
            f.write(m_jsonStr)


Get_FixationData()
