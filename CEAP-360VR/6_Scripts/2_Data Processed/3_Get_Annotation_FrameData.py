# this script is used to
# 1) Re-sample and align the frame annotation data
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

    with open('../../3_AnnotationData/Transformed/P%s_Annotation_TransData.json' % str(_pid), mode='r') as \
            _annotationDataFile:
        m_participantData = json.load(_annotationDataFile)

    m_continuousAnnotationStructure = {"ContinuousAnnotation_FrameData": []}

    m_jsonParticipantSelfAnnotationDataStruct = {
        "ParticipantID": str(_pid),
        "Video_Annotation_FrameData": []
    }

    # V1 - V8
    for _vid in range(0, 8):

        _frameSecList = []
        _frameValenceList = []
        _frameArousalList = []

        _secList = []
        _valenceList = []
        _arousalList = []

        _data = m_participantData['ContinuousAnnotation_TransData'][0]['Video_Annotation_TransData'][_vid][
            'TimeStamp_Valence_Arousal']

        m_selfAnnotationJsonData = {"VideoID": "V%s" % (_vid + 1),
                                    "TimeStamp_Valence_Arousal":
                                        [

                                        ],
                                    }

        for i in range(0, len(_data)):
            _secList.append(_data[i]['TimeStamp'])
            _valenceList.append(_data[i]['Valence'])
            _arousalList.append(_data[i]['Arousal'])

        _frameSecList = np.linspace(0, 60, 60 * m_videoFpsList[_vid])
        _frameValenceList = np.interp(_frameSecList, _secList, _valenceList)
        _frameArousalList = np.interp(_frameSecList, _secList, _arousalList)

        for i in range(0, len(_frameSecList)):
            m_selfAVJsonData = {
                "TimeStamp": round(_frameSecList[i], 3),
                "Valence": round(_frameValenceList[i], 3),
                "Arousal": round(_frameArousalList[i], 3),
            }
            m_selfAnnotationJsonData['TimeStamp_Valence_Arousal'].append(m_selfAVJsonData)

        m_jsonParticipantSelfAnnotationDataStruct['Video_Annotation_FrameData'].append(m_selfAnnotationJsonData)
    m_continuousAnnotationStructure["ContinuousAnnotation_FrameData"].append(m_jsonParticipantSelfAnnotationDataStruct)

    # save as json file
    m_jsonStr = json.dumps(m_continuousAnnotationStructure, indent=4)
    with open('../../3_AnnotationData/Frame/P%s_Annotation_FrameData.json' % str(_pid), 'w') as f:
        f.write(m_jsonStr)

