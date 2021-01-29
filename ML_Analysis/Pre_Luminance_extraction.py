"""
Created on Tue Jan 26 09:34:10 2021
this script is used to extract the luminance of the video, which was assessed by calculating for each of its frames the V component in the HSV colour space
@author: S1
"""


import json
import cv2
import numpy as np

m_videoInfoFile = {
    "Video_InfoData": []
}

for _vid in range(0, 8):

    m_luminanceList = []

    if _vid == 0:
        m_frameNum = 1501
    else:
        m_frameNum = 1801

    for i in range(1, m_frameNum):
        img = cv2.imread('../0_Video/V' + str(_vid + 1) + '_FramePic/' + str(i) + '.jpg')
        # img = img[160:320, 320:540]
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        h, s, v = cv2.split(hsv)
        # print(v)
        v_mean = np.nanmean(v)
        _v = v.ravel()[np.flatnonzero(v)]
        if len(_v) == 0:
            average_v = 0
        else:
            average_v = sum(_v) / len(_v)

        m_luminanceList.append(average_v)
        print(i, v_mean, average_v)
        v = []

    m_vidInfoStruct = {
        "VideoID": 'V%s' % str(_vid + 1),
        "V": m_luminanceList,
    }
    print(_vid)

    m_videoInfoFile['Video_InfoData'].append(m_vidInfoStruct)

m_jsonStr = json.dumps(m_videoInfoFile, indent=4)
with open('0_Video/VideoInfo1.json', 'w') as f:
    f.write(m_jsonStr)
