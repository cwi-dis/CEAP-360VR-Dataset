import numpy as np
from dtw import *
import warnings
warnings.filterwarnings('ignore')

def dtw_distance(a,b):
    alignment = dtw(a, b, keep_internals=True)
    return alignment.distance

def feature_selection(samples,samples_v):
    Data = samples.groupby("VID")
    Data_v = samples_v.groupby("VID")
    corr = np.zeros([8,32,3,2])
    #[num_v,num_s, num_f, V-A]
    for i in range(8):#for 8 videos
        data_video = Data.get_group(i+1)
        Data_p = data_video.groupby("PID")
        data_v = Data_v.get_group(i+1)
        for j in range(32):#for 32 users
            try:
                data_p = Data_p.get_group(j+1)
                for k in range(3):#for 3 features
                    corr[i,j,k,0] = abs(np.corrcoef([data_p["Valence"],data_v[data_v.keys()[k+2]]])[0,1])
                    corr[i,j,k,1] = abs(np.corrcoef([data_p["Arousal"],data_v[data_v.keys()[k+2]]])[0,1])
                    #if np.isnan(corr[i,j,k,0]):print("v",i,j,k)
                    #if np.isnan(corr[i,j,k,1]):print("a",i,j,k)
            except:
                print("No data for video %s, participant %s"%(i+1, j+1))
    corr_v = np.nanmean(corr[:,:,:,0],axis = 1)
    corr_a = np.nanmean(corr[:,:,:,1],axis = 1)
    return corr_v, corr_a, np.argmax(corr_v, axis=1), np.argmax(corr_a, axis=1)
    #return the index of the selected features

def a_shift(annotation,p):
    # p is the bit to shift
    annotation = np.array(annotation).reshape([-1,1])
    a = annotation[p:len(annotation),:]
    a_s = np.concatenate((np.ones([p,1])*5,a),axis=0)
    return a_s

def annotation_alignment(samples,samples_v):
    corr_v, corr_a, f_v_index, f_a_index = feature_selection(samples,samples_v)
    Data = samples.groupby("VID")
    Data_v = samples_v.groupby("VID")
    a_min = 6;a_max= 33;#6=0.2s, 30=1s
    Dwt= np.ones([8,32,9,2])*np.nan
    #[num_v,num_s,shift_steps,v-a]
    for i in range(8):
        data_video = Data.get_group(i+1)
        Data_p = data_video.groupby("PID")
        data_v = Data_v.get_group(i+1)        
        for j in range (32):
            data_p = Data_p.get_group(j+1)
            for k in range(a_min,a_max,3):
                dwt_v = dtw_distance(a_shift(data_p["Valence"],k),
                                     np.array(data_v[data_v.keys()[f_v_index[i]+2]]))
                dwt_a = dtw_distance(a_shift(data_p["Arousal"],k),
                                     np.array(data_v[data_v.keys()[f_v_index[i]+2]]))
                Dwt[i,j,int(k/3)-2,0] = dwt_v; Dwt[i,j,int(k/3)-2,1] = dwt_a
            print(i,j)
    d = np.nanmean(Dwt,axis = 0)
    shift_v = np.reshape(np.argmin(d[:,:,0],axis=1)+6,[-1,1])
    shift_a = np.reshape(np.argmin(d[:,:,1],axis=1)+6,[-1,1])
    return  shift_v,shift_a,corr_v,corr_a,f_v_index,f_a_index
    #the shift value (in frames) for valence and arousal for 32 subjects

def alignment_users(samples,shift_v, shift_a):
    Data = samples.groupby("VID")
    V = np.zeros([0,1]);A=np.zeros([0,1])
    for i in range(8):
        data_video = Data.get_group(i+1)
        Data_p = data_video.groupby("PID")
        for j in range(32):
            data_p = Data_p.get_group(j+1)
            v = a_shift(data_p["Valence"],int(shift_v[j]))
            a = a_shift(data_p["Arousal"],int(shift_a[j]))
            V = np.concatenate((V, v),axis = 0)
            A = np.concatenate((A, a),axis = 0)
    samples["Valence"] = V;samples["Arousal"] = A
    return samples
