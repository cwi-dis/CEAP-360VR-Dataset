import pandas as pd
import numpy as np
import VA_alignment
import VA_clusters
import VA_fusion
import VA_util

if __name__=="__main__":
    samples =  pd.read_csv("../data/VA_HM_FrameData.csv")
    samples_v = pd.read_csv("../data/VisualFeatureDataList.csv")
    '''
    time aligment
    '''
    shift_v,shift_a,corr_v, corr_a, f_v_index,f_a_index = VA_alignment.annotation_alignment(samples,samples_v)
    #shift_v = np.load("../results/shift.npz")["shift_v"];shift_a = np.load("shift.npz")["shift_a"]
    samples_shift = VA_alignment.alignment_users(samples,shift_v, shift_a)    
    '''
    clustering
    '''
    cluster_videos = VA_clusters.clustering_users(samples_shift)
    '''
    annotation fusion
    '''
    valence, arousal = VA_fusion.annotation_fusion(cluster_videos)
    pitch,yaw,box =  VA_fusion.bounding_box_fusion(cluster_videos)
    '''
    save data
    '''
    np.savez("../results/fusion_result.npz",valence = valence, arousal = arousal, 
             pitch= pitch, yaw = yaw, box = box )
    np.savez("../results/shift.npz", shift_v= shift_v, shift_a = shift_a )
    '''
    draw result
    '''
    VA_util.draw_fusion_result(valence,arousal)
