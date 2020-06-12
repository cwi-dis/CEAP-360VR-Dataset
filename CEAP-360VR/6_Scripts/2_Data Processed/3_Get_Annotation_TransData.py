# this script is used to
# 1) Transform the raw annotation data
# (20200310 created by Tong Xue)

import json
import numpy as np
import math


def Get_VA_Data(_valence, _arousal):
    if abs(_valence) >= abs(_arousal):
        _newValence = np.sign(_valence) * math.sqrt(_valence * _valence + _arousal * _arousal)
        if _valence == 0:
            _newArousal = _arousal
        else:
            _newArousal = np.sign(_valence) * math.sqrt(_valence * _valence + _arousal * _arousal) * _arousal / _valence
    else:
        _newValence = np.sign(_arousal) * math.sqrt(_valence * _valence + _arousal * _arousal) * _valence / _arousal
        _newArousal = np.sign(_arousal) * math.sqrt(_valence * _valence + _arousal * _arousal)

    if abs(_newValence) > 1:
        _newValence = math.modf(_newValence)[1]
    if abs(_newArousal) > 1:
        _newArousal = math.modf(_newArousal)[1]

    _newValence = _newValence * 4 + 5
    _newArousal = _newArousal * 4 + 5
    return _newValence, _newArousal


for _pid in range(1, 33):

    with open('../../3_AnnotationData/Raw/P%s_Annotation_RawData.json' % str(_pid), mode='r') as _annotationDataFile:
        m_annotationRawData = json.load(_annotationDataFile)
    m_continuousAnnotationStructure = {"ContinuousAnnotation_TransData": []}

    m_jsonParticipantSelfAnnotationDataStruct = {
        "ParticipantID": "P" + str(_pid),
        "Video_Annotation_TransData": [

        ]

    }

    for _vid in range(0, 8):
        _annotationData = m_annotationRawData['ContinuousAnnotation_RawData'][0]['Video_Annotation_RawData'][_vid][
            'TimeStamp_Xvalue_Yvalue']
        _valenceList = []
        _arousalList = []
        _timestampList = []

        m_selfAnnotationJsonData = {"VideoID": "V%s" % (_vid + 1),
                                    "TimeStamp_Valence_Arousal":
                                        [

                                        ],
                                    }

        for i in range(0, len(_annotationData)):
            _v = _annotationData[i]['X_Value']
            _a = _annotationData[i]['Y_Value']
            _timestamp = _annotationData[i]['TimeStamp']

            _valence, _arousal = Get_VA_Data(_v, _a)

            m_selfAVJsonData = {
                "TimeStamp": _timestamp,
                "Valence": round(_valence, 3),
                "Arousal": round(_arousal, 3),
            }
            m_selfAnnotationJsonData['TimeStamp_Valence_Arousal'].append(m_selfAVJsonData)

        m_jsonParticipantSelfAnnotationDataStruct['Video_Annotation_TransData'].append(m_selfAnnotationJsonData)
    m_continuousAnnotationStructure["ContinuousAnnotation_TransData"].append(m_jsonParticipantSelfAnnotationDataStruct)

    # save as json file
    m_jsonStr = json.dumps(m_continuousAnnotationStructure, indent=4)
    with open('../../3_AnnotationData/Transformed/P%s_Annotation_TransData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)
