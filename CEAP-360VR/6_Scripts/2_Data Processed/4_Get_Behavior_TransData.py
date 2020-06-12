# this script is used to
# 1) Transform the raw behavior data
# (20200310 created by Tong)

import json
import numpy as np
import math

# Convert eye gaze direction vector to Euler angles (yaw, pitch)
def Get_EyeRotData(_eyeRot):
    # yaw = longitude;  pitch = latitude
    _yaw = math.degrees(math.atan(_eyeRot[0] / _eyeRot[2]))
    _pitch = math.degrees(math.acos(math.sqrt((np.square(_eyeRot[0]) + np.square(_eyeRot[2])) /
                                              (np.square(_eyeRot[0]) + np.square(_eyeRot[1]) + np.square(_eyeRot[2])))
                                    )
                          )
    if _eyeRot[1] < 0:
        _pitch = -_pitch

    if _eyeRot[0] > 0 and _eyeRot[2] < 0:
        _yaw = 180 + _yaw
    elif _eyeRot[0] < 0 and _eyeRot[2] < 0:
        _yaw = -180 + _yaw

    _eyeRotData = [_yaw, _pitch]
    return _eyeRotData


# P1 - P32
for _pid in range(1, 33):

    with open('../../4_BehaviorData/Raw/P%s_Behavior_RawData.json' % str(_pid), mode='r') as _behaviorDataFile:
        m_behaviorRawData = json.load(_behaviorDataFile)

    m_transBehaviorDataStructure = {"Behavior_TransData": []}
    m_transBehaviorDataParticipantJsonStruct = {
        "ParticipantID": 'P%s' % str(_pid),
        "Video_Behavior_TransData": []
    }

    # V1 - V8
    for _vid in range(0, 8):
        _rawData = m_behaviorRawData['Behavior_RawData'][0]['Video_Behavior_RawData'][_vid]

        m_transBehaviorDataVideoJsonStruct = {
            "VideoID": 'V%s' % str(_vid + 1),
            "HM": [],
            "EM": [],
            "LEM": [],
            "REM": [],
            "LPD": _rawData['LPD'],
            "RPD": _rawData['RPD']
        }

        # Convert raw head rotation to (yaw, pitch) in range of [-180, 180], [-90, 90]
        for i in range(0, len(_rawData['HM'])):
            if _rawData['HM'][i]['Y'] > 180:
                _headYaw = _rawData['HM'][i]['Y'] - 360
            else:
                _headYaw = _rawData['HM'][i]['Y']

            if _rawData['HM'][i]['X'] > 180:
                _headPitch = _rawData['HM'][i]['X'] - 360
            else:
                _headPitch = _rawData['HM'][i]['X']

            if _rawData['HM'][i]['Z'] > 180:
                _headRoll = _rawData['HM'][i]['Z'] - 360
            else:
                _headRoll = _rawData['HM'][i]['Z']

            m_headRotData = {
                "TimeStamp": round(_rawData['HM'][i]['TimeStamp'], 3),
                "Pitch": round(_headPitch, 3),
                "Yaw": round(_headYaw, 3),
            }
            m_transBehaviorDataVideoJsonStruct['HM'].append(m_headRotData)

        # Convert raw EM to (yaw, pitch) in range of [-180, 180], [-90, 90]
        for i in range(0, len(_rawData['EM'])):
            _eyeRotData = Get_EyeRotData([_rawData['EM'][i]['X'], _rawData['EM'][i]['Y'], _rawData['EM'][i]['Z']])

            m_eyeRotData = {
                "TimeStamp": round(_rawData['EM'][i]['TimeStamp'], 3),
                "Pitch": round(_eyeRotData[1], 3),
                "Yaw": round(_eyeRotData[0], 3),
            }
            m_transBehaviorDataVideoJsonStruct['EM'].append(m_eyeRotData)

        # Convert raw LEM to (yaw, pitch) in range of [-180, 180], [-90, 90]
        for i in range(0, len(_rawData['LEM'])):
            _eyeRotData = Get_EyeRotData([_rawData['LEM'][i]['X'], _rawData['LEM'][i]['Y'], _rawData['LEM'][i]['Z']])

            m_eyeRotData = {
                "TimeStamp": round(_rawData['LEM'][i]['TimeStamp'], 3),
                "Pitch": round(_eyeRotData[1], 3),
                "Yaw": round(_eyeRotData[0], 3),
            }
            m_transBehaviorDataVideoJsonStruct['LEM'].append(m_eyeRotData)

        # Convert raw REM to (yaw, pitch) in range of [-180, 180], [-90, 90]
        for i in range(0, len(_rawData['REM'])):
            _eyeRotData = Get_EyeRotData([_rawData['REM'][i]['X'], _rawData['REM'][i]['Y'], _rawData['REM'][i]['Z']])

            m_eyeRotData = {
                "TimeStamp": round(_rawData['REM'][i]['TimeStamp'], 3),
                "Pitch": round(_eyeRotData[1], 3),
                "Yaw": round(_eyeRotData[0], 3),
            }
            m_transBehaviorDataVideoJsonStruct['REM'].append(m_eyeRotData)

        m_transBehaviorDataParticipantJsonStruct['Video_Behavior_TransData'].append(m_transBehaviorDataVideoJsonStruct)
    m_transBehaviorDataStructure['Behavior_TransData'].append(m_transBehaviorDataParticipantJsonStruct)

    # save as json file
    m_jsonStr = json.dumps(m_transBehaviorDataStructure, indent=4)
    with open('../../4_BehaviorData/Transformed/P%s_Behavior_TransData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)








