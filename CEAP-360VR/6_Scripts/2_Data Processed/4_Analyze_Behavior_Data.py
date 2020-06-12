# this script is used to
# # 1) Calculate CC for HM/EM heatmaps
# # 2) Plot eye gaze data by video
# # 3) Plot HM/EM bins of pitch and yaw
# # 4) Calculate the mean of PD for each participant
# # (20200310 created by Tong Xue)

import json
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
# import matplotlib
# matplotlib.style.use('classic')

# Normalize to [0,1]
def Normalisation(img):
    img_min = np.min(img)
    img_max = np.max(img)
    return (img - img_min) / (img_max - img_min)


# Evaluate the saliency using correlation coefficient method (CC)
def CC(img_a, img_b):
    img_a = Normalisation(img_a.astype('float32')).flatten()
    img_b = Normalisation(img_b.astype('float32')).flatten()
    cor_array = np.corrcoef(img_a, img_b)
    return cor_array[0, 1]


# z_score standardisation
def z_score_normalization(x):
    x = (x - np.mean(x)) / np.std(x)
    return x


# Calculate CC for HM/EM heatmaps
def Calculate_CC_SaliencyMap():
    m_dataCountList = []

    m_participantEyeGazeDataArr = np.zeros((8, 2, 2000, 32), dtype=np.float64)
    m_participantEyeGazeDataArr[:] = np.nan

    for _vid in range(0, 8):
        for _pid in range(0, 32):

            with open('../../4_BehaviorData/Frame/P%s_Behavior_FrameData.json' % str(_pid + 1),
                      mode='r') as _participantDataFile:
                m_participantData = json.load(_participantDataFile)

                # HM / EM
                _data = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['HM']
                # _data = m_participantData['Behavior_FrameData'][0]['Video_Behavior_FrameData'][_vid]['EM']

                for _sample in range(len(_data)):
                    m_participantEyeGazeDataArr[_vid, 0, _sample, _pid] = _data[_sample]["Pitch"]
                    m_participantEyeGazeDataArr[_vid, 1, _sample, _pid] = _data[_sample]["Yaw"]
        m_dataCountList.append(len(_data))

    for _vid in range(0, 8):

        m_corrList1 = []

        m_yawList = []
        m_pitchList = []
        m_yawList1 = []
        m_pitchList1 = []

        for _sample in range(m_dataCountList[_vid]):

            for _pid in range(0, 32):
                _yaw = m_participantEyeGazeDataArr[_vid, 1, _sample, _pid]
                _pitch = m_participantEyeGazeDataArr[_vid, 0, _sample, _pid]
                if _pid < 8 or _pid > 23:
                    m_yawList.append(_yaw)
                    m_pitchList.append(_pitch)
                    if np.isnan(_yaw):
                        print(_pid, _vid)
                        print(_yaw)

                else:
                    if np.isnan(_yaw):
                        print(_pid, _vid)
                        print(_yaw)

                    m_yawList1.append(_yaw)
                    m_pitchList1.append(_pitch)

            img, xedges, yedges = np.histogram2d(m_yawList, m_pitchList, bins=20, range=[[-180, 180], [-90, 90]])
            img1, xedges1, yedges1 = np.histogram2d(m_yawList1, m_pitchList1, bins=20, range=[[-180, 180], [-90, 90]])
            m_corrList1.append(CC(np.array(img.T), np.array(img1.T)))

        # plt.figure(figsize=(16.0, 12.0))

        # extent = [xedges[0], xedges[-1], yedges[-1], yedges[0]]
        # plt.subplot(211)
        # plt.imshow(img.T, extent=extent)
        # plt.grid(True)
        # plt.colorbar()
        # plt.subplot(212)
        # plt.imshow(img1.T, extent=extent)
        # plt.grid(True)
        # plt.colorbar()
        # plt.show()

        # m_corrList.append(np.corrcoef(np.array(img), np.array(img1)))
        print(_vid + 1, np.mean(m_corrList1), np.std(m_corrList1))


# plot eye gaze data by video
def Plot_EM_HeatmapByVideo():
    m_saveFormatList = ["pdf", "png"]
    sample_number = 7500
    m_participantEyeGazeDataArr = np.zeros((8, 3, sample_number, 32), dtype=np.float64)
    m_participantEyeGazeDataArr[:] = np.nan

    # get eye gaze data list
    for _vid in range(0, 8):
        for _pid in range(0, 32):

            with open('../../4_BehaviorData/Transformed/P%s_Behavior_TransData.json' % str(_pid + 1),
                      mode='r') as _participantDataFile:
                m_participantData = json.load(_participantDataFile)
                _data = m_participantData['Behavior_TransData'][0]['Video_Behavior_TransData'][_vid]['EM']

                for _sample in range(len(_data)):
                    m_participantEyeGazeDataArr[_vid, 0, _sample, _pid] = _data[_sample]["Pitch"]
                    m_participantEyeGazeDataArr[_vid, 1, _sample, _pid] = _data[_sample]["Yaw"]
                    m_participantEyeGazeDataArr[_vid, 2, _sample, _pid] = _data[_sample]["TimeStamp"]

    plt.ion()
    plt.close("all")
    plt.tight_layout()
    m_gazeDataFig = plt.figure(figsize=(32.0, 24.0))

    for _vid in range(0, 8):
        m_yawValueList = []
        m_pitchValueList = []

        for _pid in range(0, 32):
            try:
                for _sample in range(0, sample_number):

                    _pitchValue = m_participantEyeGazeDataArr[_vid, 0, _sample, _pid]
                    _yawValue = m_participantEyeGazeDataArr[_vid, 1, _sample, _pid]
                    if np.isnan(_pitchValue) or np.isnan(_yawValue):
                        None
                    else:
                        m_pitchValueList.append(_pitchValue)
                        m_yawValueList.append(_yawValue)
            except IndexError:
                None

        _thumbNailImg = plt.imread("../../1_Stimuli/VideoThumbNails/%s.jpg" % str(_vid + 1))
        _subPlt = plt.subplot(5, 4, _vid + 1)
        _subPlt.set_title("V%s" % str(_vid + 1), fontdict={'size': 20, 'weight': 'bold'})
        _subPlt.set_ylabel("Pitch [Deg]", fontdict={'size': 16, 'weight': 'bold'})
        _subPlt.set_xlabel("Yaw [Deg]", fontdict={'size': 16, 'weight': 'bold'})
        _heatmap, xedges, yedges = np.histogram2d(m_yawValueList, m_pitchValueList, bins=20,
                                                  range=[[-180, 180], [-90, 90]])
        # Modify axis ticks depending on definition of pitch and yaw

        extent = [xedges[0], xedges[-1], yedges[-1], yedges[0]]

        _subPlt.imshow(_thumbNailImg, extent=extent)
        _subPlt.imshow(_heatmap.T, extent=extent, origin='lower', alpha=.6, interpolation='bilinear')

    for _format in m_saveFormatList:
        m_gazeDataFig.savefig("../../4_BehaviorData/Figure/HeadEyeGaze_HeatMap.%s" % (_format), bbox_inches='tight',
                              dpi=300)


# plot EM fixation bins of Yaw
def Plot_EM_YawCount():
    _yawCount = [[0 for i in range(8)] for i in range(12)]
    m_labelsList = []

    for _vid in range(0, 8):

        for _pid in range(1, 33):
            with open('../../4_BehaviorData/EM_Fixation/P%s_Behavior_FixationData.json' % str(_pid),
                      mode='r') as _behaviorDataFile:
                m_behaviorRawData = json.load(_behaviorDataFile)

            _data = m_behaviorRawData["Behavior_FixationData"][0]["Video_Behavior_FixationData"][_vid]["Fixation"]

            for i in range(0, len(_data)):
                if -180 < _data[i]["Yaw"] <= -150:
                    _yawCount[0][_vid] += 1
                elif -150 < _data[i]["Yaw"] <= -120:
                    _yawCount[1][_vid] += 1
                elif -120 < _data[i]["Yaw"] <= -90:
                    _yawCount[2][_vid] += 1
                elif -90 < _data[i]["Yaw"] <= -60:
                    _yawCount[3][_vid] += 1
                elif -60 < _data[i]["Yaw"] <= -30:
                    _yawCount[4][_vid] += 1
                elif -30 < _data[i]["Yaw"] <= 0:
                    _yawCount[5][_vid] += 1
                elif 0 < _data[i]["Yaw"] <= 30:
                    _yawCount[6][_vid] += 1
                elif 30 < _data[i]["Yaw"] <= 60:
                    _yawCount[7][_vid] += 1
                elif 60 < _data[i]["Yaw"] <= 90:
                    _yawCount[8][_vid] += 1
                elif 90 < _data[i]["Yaw"] <= 120:
                    _yawCount[9][_vid] += 1
                elif 120 < _data[i]["Yaw"] <= 150:
                    _yawCount[10][_vid] += 1
                elif 150 < _data[i]["Yaw"] <= 180:
                    _yawCount[11][_vid] += 1
                else:
                    print(_data[i]["Yaw"])

        m_labelsList.append("V%s" % str(_vid + 1))

    print(_yawCount)
    m_labelsList = []

    m_yawPercent = [[0 for i in range(8)] for i in range(12)]

    for i in range(0, 8):
        m_labelsList.append("V%s" % str(i + 1))
        _temp = 0
        for j in range(0, 12):
            _temp += _yawCount[j][i]
        for j in range(0, 12):
            m_yawPercent[j][i] = round(_yawCount[j][i] / _temp * 100, 2)

    print(m_yawPercent)

    plt.figure(figsize=(32, 20), dpi=80)
    plt.figure(1)

    plt.barh(m_labelsList, m_yawPercent[0], color='black', label='(-180, -150]')
    plt.barh(m_labelsList, m_yawPercent[1], left=np.sum([m_yawPercent[i] for i in range(0, 1)], axis=0), color='grey',
             label='(-150, -120]')
    plt.barh(m_labelsList, m_yawPercent[2], left=np.sum([m_yawPercent[i] for i in range(0, 2)], axis=0), color='silver',
             label='(-120, -90]')
    plt.barh(m_labelsList, m_yawPercent[3], left=np.sum([m_yawPercent[i] for i in range(0, 3)], axis=0),
             color='rosybrown', label='(-90, -60]')
    plt.barh(m_labelsList, m_yawPercent[4], left=np.sum([m_yawPercent[i] for i in range(0, 4)], axis=0),
             color='lightcoral', label='(-60, -30]')
    plt.barh(m_labelsList, m_yawPercent[5], left=np.sum([m_yawPercent[i] for i in range(0, 5)], axis=0), color='brown',
             label='(-30, 0]')
    plt.barh(m_labelsList, m_yawPercent[6], left=np.sum([m_yawPercent[i] for i in range(0, 6)], axis=0), color='red',
             label='(0, 30]')
    plt.barh(m_labelsList, m_yawPercent[7], left=np.sum([m_yawPercent[i] for i in range(0, 7)], axis=0),
             color='lightsalmon', label='(30, 60]')
    plt.barh(m_labelsList, m_yawPercent[8], left=np.sum([m_yawPercent[i] for i in range(0, 8)], axis=0),
             color='darkorange', label='(60, 90]')
    plt.barh(m_labelsList, m_yawPercent[9], left=np.sum([m_yawPercent[i] for i in range(0, 9)], axis=0),
             color='forestgreen', label='(90, 120]')
    plt.barh(m_labelsList, m_yawPercent[10], left=np.sum([m_yawPercent[i] for i in range(0, 10)], axis=0),
             color='darkslategray', label='(120, 150]')
    plt.barh(m_labelsList, m_yawPercent[11], left=np.sum([m_yawPercent[i] for i in range(0, 11)], axis=0), color='navy',
             label='(150, 180]')

    plt.xlabel('EM Yaw Percent (%)', fontdict={'size': 80})
    plt.ylabel('VideoID', fontdict={'size': 80})
    plt.xticks(size=80)
    plt.yticks(size=80)

    plt.legend(loc='upper center', bbox_to_anchor=(0.45, 1.45), ncol=3, prop={'size': 75})

    plt.savefig("../../4_BehaviorData/Figure/EM Yaw Analysis.png", bbox_inches='tight')
    plt.show()


# plot EM fixation bins of Pitch
def Plot_EM_PitchCount():
    _pitchCount = [[0 for i in range(8)] for i in range(6)]
    m_labelsList = []

    for _vid in range(0, 8):

        for _pid in range(1, 33):
            with open('../../4_BehaviorData/EM_Fixation/P%s_Behavior_FixationData.json' % str(_pid),
                      mode='r') as _behaviorDataFile:
                m_behaviorRawData = json.load(_behaviorDataFile)

            _data = m_behaviorRawData["Behavior_FixationData"][0]["Video_Behavior_FixationData"][_vid]["Fixation"]

            for i in range(0, len(_data)):
                if -90 < _data[i]["Pitch"] <= -60:
                    _pitchCount[0][_vid] += 1
                elif -60 < _data[i]["Pitch"] <= -30:
                    _pitchCount[1][_vid] += 1
                elif -30 < _data[i]["Pitch"] <= 0:
                    _pitchCount[2][_vid] += 1
                elif 0 < _data[i]["Pitch"] <= 30:
                    _pitchCount[3][_vid] += 1
                elif 30 < _data[i]["Pitch"] <= 60:
                    _pitchCount[4][_vid] += 1
                elif 60 < _data[i]["Pitch"] <= 90:
                    _pitchCount[5][_vid] += 1
                else:
                    print(_data[i]["Pitch"])

        m_labelsList.append("V%s" % str(_vid + 1))

    print(_pitchCount)
    m_labelsList = []

    m_pitchPercent = [[0 for i in range(8)] for i in range(6)]

    for i in range(0, 8):
        m_labelsList.append("V%s" % str(i + 1))
        _temp = 0
        for j in range(0, 6):
            _temp += _pitchCount[j][i]
        for j in range(0, 6):
            m_pitchPercent[j][i] = round(_pitchCount[j][i] / _temp * 100, 2)

    print(m_pitchPercent)

    plt.figure(figsize=(32, 20), dpi=80)
    plt.figure(1)

    plt.barh(m_labelsList, m_pitchPercent[0], color='rosybrown', label='(-90, -60]')
    plt.barh(m_labelsList, m_pitchPercent[1], left=np.sum([m_pitchPercent[i] for i in range(0, 1)], axis=0),
             color='lightcoral', label='(-60, -30]')
    plt.barh(m_labelsList, m_pitchPercent[2], left=np.sum([m_pitchPercent[i] for i in range(0, 2)], axis=0),
             color='brown', label='(-30, 0]')
    plt.barh(m_labelsList, m_pitchPercent[3], left=np.sum([m_pitchPercent[i] for i in range(0, 3)], axis=0),
             color='red', label='(0, 30]')
    plt.barh(m_labelsList, m_pitchPercent[4], left=np.sum([m_pitchPercent[i] for i in range(0, 4)], axis=0),
             color='lightsalmon', label='(30, 60]')
    plt.barh(m_labelsList, m_pitchPercent[5], left=np.sum([m_pitchPercent[i] for i in range(0, 5)], axis=0),
             color='darkorange', label='(60, 90]')

    plt.xlabel('Pitch Percent (%)', fontdict={'size': 80})
    plt.ylabel('VideoID', fontdict={'size': 80})
    plt.xticks(size=80)
    plt.yticks(size=80)

    plt.legend(loc='upper center', bbox_to_anchor=(0.5, 1.30), ncol=3, prop={'size': 75})

    plt.savefig("../../4_BehaviorData/Figure/Pitch Analysis.png", bbox_inches='tight')
    plt.show()


# plot HM scanpath bins of Yaw
def Plot_HM_YawCount():
    _yawCount = [[0 for i in range(8)] for i in range(12)]
    m_labelsList = []

    for _vid in range(0, 8):

        for _pid in range(1, 33):

            with open('../../4_BehaviorData/HM_ScanPath/P%s_Behavior_HeadScanPathData.json' % str(_pid),
                      mode='r') as _behaviorDataFile:
                m_behaviorRawData = json.load(_behaviorDataFile)

            _data = m_behaviorRawData["Behavior_HeadScanPath_Data"][0]["Video_Behavior_ScanPath_Data"][_vid][
                "ID_Pitch_Yaw"]

            for i in range(0, len(_data)):
                if -180 < _data[i]["Yaw"] <= -150:
                    _yawCount[0][_vid] += 1
                elif -150 < _data[i]["Yaw"] <= -120:
                    _yawCount[1][_vid] += 1
                elif -120 < _data[i]["Yaw"] <= -90:
                    _yawCount[2][_vid] += 1
                elif -90 < _data[i]["Yaw"] <= -60:
                    _yawCount[3][_vid] += 1
                elif -60 < _data[i]["Yaw"] <= -30:
                    _yawCount[4][_vid] += 1
                elif -30 < _data[i]["Yaw"] <= 0:
                    _yawCount[5][_vid] += 1
                elif 0 < _data[i]["Yaw"] <= 30:
                    _yawCount[6][_vid] += 1
                elif 30 < _data[i]["Yaw"] <= 60:
                    _yawCount[7][_vid] += 1
                elif 60 < _data[i]["Yaw"] <= 90:
                    _yawCount[8][_vid] += 1
                elif 90 < _data[i]["Yaw"] <= 120:
                    _yawCount[9][_vid] += 1
                elif 120 < _data[i]["Yaw"] <= 150:
                    _yawCount[10][_vid] += 1
                elif 150 < _data[i]["Yaw"] <= 180:
                    _yawCount[11][_vid] += 1
                else:
                    print(_data[i]["Yaw"])

        m_labelsList.append("V%s" % str(_vid + 1))

    print(_yawCount)
    m_labelsList = []

    m_yawPercent = [[0 for i in range(8)] for i in range(12)]

    for i in range(0, 8):
        m_labelsList.append("V%s" % str(i + 1))
        _temp = 0
        for j in range(0, 12):
            _temp += _yawCount[j][i]
        for j in range(0, 12):
            m_yawPercent[j][i] = round(_yawCount[j][i] / _temp * 100, 2)

    print(m_yawPercent)

    plt.figure(figsize=(32, 20), dpi=80)
    plt.figure(1)

    plt.barh(m_labelsList, m_yawPercent[0], color='black', label='(-180, -150]')
    plt.barh(m_labelsList, m_yawPercent[1], left=np.sum([m_yawPercent[i] for i in range(0, 1)], axis=0), color='grey',
             label='(-150, -120]')
    plt.barh(m_labelsList, m_yawPercent[2], left=np.sum([m_yawPercent[i] for i in range(0, 2)], axis=0), color='silver',
             label='(-120, -90]')
    plt.barh(m_labelsList, m_yawPercent[3], left=np.sum([m_yawPercent[i] for i in range(0, 3)], axis=0),
             color='rosybrown', label='(-90, -60]')
    plt.barh(m_labelsList, m_yawPercent[4], left=np.sum([m_yawPercent[i] for i in range(0, 4)], axis=0),
             color='lightcoral', label='(-60, -30]')
    plt.barh(m_labelsList, m_yawPercent[5], left=np.sum([m_yawPercent[i] for i in range(0, 5)], axis=0), color='brown',
             label='(-30, 0]')
    plt.barh(m_labelsList, m_yawPercent[6], left=np.sum([m_yawPercent[i] for i in range(0, 6)], axis=0), color='red',
             label='(0, 30]')
    plt.barh(m_labelsList, m_yawPercent[7], left=np.sum([m_yawPercent[i] for i in range(0, 7)], axis=0),
             color='lightsalmon', label='(30, 60]')
    plt.barh(m_labelsList, m_yawPercent[8], left=np.sum([m_yawPercent[i] for i in range(0, 8)], axis=0),
             color='darkorange', label='(60, 90]')
    plt.barh(m_labelsList, m_yawPercent[9], left=np.sum([m_yawPercent[i] for i in range(0, 9)], axis=0),
             color='forestgreen', label='(90, 120]')
    plt.barh(m_labelsList, m_yawPercent[10], left=np.sum([m_yawPercent[i] for i in range(0, 10)], axis=0),
             color='darkslategray', label='(120, 150]')
    plt.barh(m_labelsList, m_yawPercent[11], left=np.sum([m_yawPercent[i] for i in range(0, 11)], axis=0), color='navy',
             label='(150, 180]')

    plt.xlabel('HM Yaw Percent (%)', fontdict={'size': 80})
    plt.ylabel('VideoID', fontdict={'size': 80})
    plt.xticks(size=80)
    plt.yticks(size=80)

    plt.legend(loc='upper center', bbox_to_anchor=(0.45, 1.45), ncol=3, prop={'size': 75})

    plt.savefig("../../4_BehaviorData/Figure/HM Yaw Analysis.png", bbox_inches='tight')
    plt.show()


# plot HM scanpath bins of Pitch
def Plot_HM_PitchCount():
    _pitchCount = [[0 for i in range(8)] for i in range(6)]
    m_labelsList = []

    for _vid in range(0, 8):

        for _pid in range(1, 33):
            with open('../../4_BehaviorData/HM_ScanPath/P%s_Behavior_HeadScanPathData.json' % str(_pid),
                      mode='r') as _behaviorDataFile:
                m_behaviorRawData = json.load(_behaviorDataFile)

            _data = m_behaviorRawData["Behavior_HeadScanPath_Data"][0]["Video_Behavior_ScanPath_Data"][_vid][
                "ID_Pitch_Yaw"]

            for i in range(0, len(_data)):
                if -90 < _data[i]["Pitch"] <= -60:
                    _pitchCount[0][_vid] += 1
                elif -60 < _data[i]["Pitch"] <= -30:
                    _pitchCount[1][_vid] += 1
                elif -30 < _data[i]["Pitch"] <= 0:
                    _pitchCount[2][_vid] += 1
                elif 0 < _data[i]["Pitch"] <= 30:
                    _pitchCount[3][_vid] += 1
                elif 30 < _data[i]["Pitch"] <= 60:
                    _pitchCount[4][_vid] += 1
                elif 60 < _data[i]["Pitch"] <= 90:
                    _pitchCount[5][_vid] += 1
                else:
                    print(_data[i]["Pitch"])

        m_labelsList.append("V%s" % str(_vid + 1))

    print(_pitchCount)
    m_labelsList = []

    m_pitchPercent = [[0 for i in range(8)] for i in range(6)]

    for i in range(0, 8):
        m_labelsList.append("V%s" % str(i + 1))
        _temp = 0
        for j in range(0, 6):
            _temp += _pitchCount[j][i]
        for j in range(0, 6):
            m_pitchPercent[j][i] = round(_pitchCount[j][i] / _temp * 100, 2)

    print(m_pitchPercent)

    plt.figure(figsize=(32, 20), dpi=80)
    plt.figure(1)

    plt.barh(m_labelsList, m_pitchPercent[0], color='rosybrown', label='(-90, -60]')
    plt.barh(m_labelsList, m_pitchPercent[1], left=np.sum([m_pitchPercent[i] for i in range(0, 1)], axis=0),
             color='lightcoral', label='(-60, -30]')
    plt.barh(m_labelsList, m_pitchPercent[2], left=np.sum([m_pitchPercent[i] for i in range(0, 2)], axis=0),
             color='brown', label='(-30, 0]')
    plt.barh(m_labelsList, m_pitchPercent[3], left=np.sum([m_pitchPercent[i] for i in range(0, 3)], axis=0),
             color='red', label='(0, 30]')
    plt.barh(m_labelsList, m_pitchPercent[4], left=np.sum([m_pitchPercent[i] for i in range(0, 4)], axis=0),
             color='lightsalmon', label='(30, 60]')
    plt.barh(m_labelsList, m_pitchPercent[5], left=np.sum([m_pitchPercent[i] for i in range(0, 5)], axis=0),
             color='darkorange', label='(60, 90]')

    plt.xlabel('HM Pitch Percent (%)', fontdict={'size': 80})
    plt.ylabel('VideoID', fontdict={'size': 80})
    plt.xticks(size=80)
    plt.yticks(size=80)

    plt.legend(loc='upper center', bbox_to_anchor=(0.5, 1.30), ncol=3, prop={'size': 75})

    plt.savefig("../../4_BehaviorData/Figure/HM Pitch Analysis.png", bbox_inches='tight')
    plt.show()


# Calculate each participant LPD data after Z-score
def Calculate_PD_Mean():
    m_LPD_List = [[0 for i in range(8)] for j in range(32)]
    m_PD_File = [[0 for i in range(3)] for j in range(32 * 8)]

    m_RPD_List = [[0 for i in range(8)] for j in range(32)]

    for _pid in range(1, 33):

        with open('../../4_BehaviorData/Transformed/P%s_Behavior_TransData.json' % str(_pid),
                  mode='r') as _behaviorDataFile:
            m_behaviorRawData = json.load(_behaviorDataFile)

        for _vid in range(0, 8):

            m_pdList = []
            m_rpdList = []

            _data = m_behaviorRawData['Behavior_TransData'][0]['Video_Behavior_TransData'][_vid]['LPD']
            _data1 = m_behaviorRawData['Behavior_TransData'][0]['Video_Behavior_TransData'][_vid]['RPD']

            for i in range(len(_data)):
                m_pdList.append(_data[i]['PD'])

            for i in range(len(_data1)):
                m_rpdList.append(_data1[i]['PD'])

            m_LPD_List[_pid - 1][_vid] = np.nanmean(m_pdList)
            m_RPD_List[_pid - 1][_vid] = np.nanmean(m_rpdList)

    for i in range(32):
        x = z_score_normalization(m_LPD_List[i])
        y = z_score_normalization(m_RPD_List[i])

        for j in range(8):
            m_PD_File[i * 8 + j][0] = 'V' + str(j + 1)
            m_PD_File[i * 8 + j][1] = round(x[j], 3)
            m_PD_File[i * 8 + j][2] = round(y[j], 3)

    # print(m_LPD_File)
    df = pd.DataFrame(m_PD_File)
    print(df)
    df.to_excel("PD_File.xlsx")


# calculate the mean of PD for each participant
Calculate_PD_Mean()
