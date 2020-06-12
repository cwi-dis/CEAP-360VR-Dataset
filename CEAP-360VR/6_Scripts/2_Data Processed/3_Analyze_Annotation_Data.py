# this script is used to
# 1) Plot the annotation trajectories by video
# 2) Get the mean of continuous annotation data by participant
# 3) Get the mean of SAM rating data by participant
# (20200310 created by Tong Xue)

import json
import matplotlib.pyplot as plt
import pandas as pd


# plot the annotation trajectories by video
def Plot_AnnotationTraj():
    m_colorList = ["#eecdac", "#7fc087", "#f4978e", "#879af0", "#cca176", "#3ca94a", "#d3574a", "#415bd0"]

    plt.figure(figsize=(20, 20), dpi=80)
    plt.figure(1)

    ax1 = plt.subplot2grid((1, 1), (0, 0))
    ax1.spines['left'].set_linewidth(5)
    ax1.spines['right'].set_linewidth(5)
    ax1.spines['top'].set_linewidth(5)
    ax1.spines['bottom'].set_linewidth(5)

    plt.axis([1, 9, 1, 9])
    plt.xlabel('Valence', fontdict={'size': 70, 'weight': 'bold'})
    plt.ylabel('Arousal', fontdict={'size': 70, 'weight': 'bold'})
    plt.xticks(size=60)
    plt.yticks(size=60)

    plt.grid(linestyle='--', linewidth=6)
    plt.axhline(5, linestyle='--', color='k', linewidth=2)
    plt.axvline(5, linestyle='--', color='k', linewidth=2)
    m_plotLineList = [i for i in range(0, 8)]

    for _vid in range(0, 8):

        _valenceList = []
        _arousalList = []
        _dataList = []

        for _pid in range(0, 32):
            with open('../../3_AnnotationData/Frame/P%s_Annotation_FrameData.json' % str(_pid + 1),
                      mode='r') as _annotationDataFile:
                m_participantData = json.load(_annotationDataFile)
            _dataList.append(m_participantData['ContinuousAnnotation_FrameData'][0]['Video_Annotation_FrameData'][_vid][
                                 'TimeStamp_Valence_Arousal'])

        for i in range(0, len(_dataList[0])):
            _valence = 0
            _arousal = 0
            for _pid in range(0, 32):
                _valence += _dataList[_pid][i]["Valence"]
                _arousal += _dataList[_pid][i]["Arousal"]

            _valenceList.append(_valence / 32)
            _arousalList.append(_arousal / 32)

        m_plotLineList[_vid], = plt.plot(_valenceList, _arousalList, color=m_colorList[_vid], linewidth=5.0)
        _valenceList.clear()
        _arousalList.clear()

    plt.legend(handles=m_plotLineList, labels=['V1', 'V2', 'V3', 'V4', 'V5', 'V6', 'V7', 'V8'], loc='best', fontsize=30)
    plt.tight_layout()
    plt.savefig('../../ProcessedSelfAnnotationVideoData.png')
    plt.clf()
    plt.close()


# get the mean value of continuous annotation data by participant
def Get_MeanVA_Data():
    m_valenceList = []
    m_arousalList = []
    m_videoNameList = []

    for _vid in range(0, 8):

        for _pid in range(0, 32):

            with open('../../3_AnnotationData/Transformed/P%s_Annotation_TransData.json' % str(_pid + 1),
                      mode='r') as _participantData:
                m_participantData = json.load(_participantData)

            _data1 = m_participantData['ContinuousAnnotation_TransData'][0]['Video_Annotation_TransData'][_vid][
                'TimeStamp_Valence_Arousal']
            _arousal = 0
            _valence = 0

            for i in range(0, len(_data1)):
                _valence += _data1[i]['Valence']
                _arousal += _data1[i]['Arousal']

            _valenceMean = _valence / len(_data1)
            _arousalMean = _arousal / len(_data1)
            m_valenceList.append(round(_valenceMean, 3))
            m_arousalList.append(round(_arousalMean, 3))
            m_videoNameList.append('V' + str(_vid + 1))

    m_list = [m_videoNameList, m_valenceList, m_arousalList]
    df = pd.DataFrame(m_list)
    df = df.T
    print(df)
    df.to_excel("meanAV_AllVideo.xlsx")


# get the mean value of SAM rating data by participant
def Get_MeanSAM_Data():
    m_valenceList = []
    m_arousalList = []
    m_videoNameList = []
    m_samRating_valence = []
    m_samRating_arousal = []

    for _vid in range(0, 8):

        _valenceMean = 0
        _arousalMean = 0

        _samValence = 0
        _samArousal = 0

        for _pid in range(0, 32):

            with open('../../2_QuestionnaireData/Raw/P%s_Questionnaire_Data.json' % (_pid + 1),
                      mode='r') as _participantSAMData:
                m_participantSAMData = json.load(_participantSAMData)
            with open('../../3_AnnotationData/Transformed/P%s_Annotation_TransData.json' % str(_pid + 1),
                      mode='r') as _participantData:
                m_participantData = json.load(_participantData)

            _data1 = m_participantData['ContinuousAnnotation_TransData'][0]['Video_Annotation_TransData'][_vid][
                'TimeStamp_Valence_Arousal']
            _data2 = m_participantSAMData['QuestionnaireData'][0]['Video_SAMRating_VideoTime_Data'][_vid]
            _arousal = 0
            _valence = 0

            for i in range(0, len(_data1)):
                _valence += _data1[i]['Valence']
                _arousal += _data1[i]['Arousal']

            _valenceMean = _valence / len(_data1)
            _arousalMean = _arousal / len(_data1)

            _samValence = _data2['ValenceValue']
            _samArousal = _data2['ArousalValue']

            m_videoNameList.append('P' + str(_pid + 1))

            m_valenceList.append(round(_valenceMean, 3))
            m_arousalList.append(round(_arousalMean, 3))

            m_samRating_valence.append(_samValence)
            m_samRating_arousal.append(_samArousal)

    m_list = [m_videoNameList, m_valenceList, m_arousalList, m_samRating_valence, m_samRating_arousal]
    df = pd.DataFrame(m_list)
    df = df.T
    print(df)
    df.to_excel("meanSAM_AV_All.xlsx")


Plot_AnnotationTraj()
