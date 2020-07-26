import numpy as np
import sympy
import math 

def annotation_fusion(cluster_videos):
    V=[];A=[]
    for i in range(8):#8 videos
        V_video =[];A_video=[]
        for j in range(60):#60s pervideos
            seg_data = cluster_videos[i][j]
            if i==0:n=25 
            else:n =30
            F_v=[];F_a=[];N_v=[];N_a=[]
            for k in range(n):
                #frame level-fusion
                index_frame = np.where(seg_data[:,2]==(k+j*n+1))[0]
                if index_frame.shape[0] != 0:
                    frame_data = seg_data[index_frame]
                    f_v = frame_data[:,3].reshape([-1,1]);f_a = frame_data[:,4].reshape([-1,1])
                    F_v.append(bayes_fusion(f_v)[0]);F_a.append(bayes_fusion(f_a)[0])
                    N_v.append(f_v.shape[0]);N_a.append(f_a.shape[0])
                else:
                    F_v.append(np.nan);F_a.append(np.nan)
                    N_v.append(0);N_a.append(0)                    
            #seg level fusion
            V_video.append(sum(np.divide(N_v,sum(N_v))*np.array(F_v)))
            A_video.append(sum(np.divide(N_a,sum(N_a))*np.array(F_a)))
            #find the bounding box
        V.append(np.array(V_video));A.append(np.array(A_video))
    return V,A


def bounding_box_fusion(cluster_videos):
    B=[]
    for i in range(8):#8 videos
        B_video=[]
        for j in range(60):#60s pervideos
            seg_data = cluster_videos[i][j]
            B_video.append(bounding_box(seg_data))
        B.append(np.array(B_video))
    return B


def bounding_box(seg_data):
    pmax = np.max(seg_data[:,8])#pitch
    pmin = np.min(seg_data[:,8])
    ymax = np.max(seg_data[:,9])#yaw
    ymin = np.min(seg_data[:,9])
    pitch = seg_data[:,8].reshape([-1,1]); yaw = seg_data[:,9].reshape([-1,1])
    return pitch, yaw, np.array([pmax,pmin,ymax,ymin])
    
def bayes_fusion(data):
    s = np.mean(data,axis=1)
    data_fusion=[]
    for i in range(data.shape[1]):
        D = np.zeros([data.shape[0],data.shape[0]])
        X = np.reshape(data[:,i],[1,-1])
        for j in range(X.shape[1]):
            X0 = X[0,j]
            for k in range(X.shape[1]):
                D[j,k] = float(sympy.erf(abs(X0-X[0,k])/(math.sqrt(2)*s[j])))
        m = fuse_data(D,X,0.5)
        if(np.isnan(m)):
            data_fusion.append(np.mean(X))
        else:
            data_fusion.append(m)
    return data_fusion

def fuse_data(D,data,thres):
    le = np.shape(D)[0]-1
    gt = np.sum(D,axis=1)/le
    D = np.delete(D,np.where(gt>thres)[0],axis=0)
    D = np.delete(D,np.where(gt>thres)[0],axis=1)
    data = np.delete(data,np.where(gt>thres)[0],axis=1)
    DD = np.sum(D,axis=0)/D.shape[0]
    DD = 1-(DD/sum(DD))
    data_fused = np.sum(np.multiply(data,np.reshape(DD,[1,-1])))/D.shape[0]
    return data_fused