from matplotlib import pyplot as plt
import numpy as np
from scipy.cluster.hierarchy import dendrogram

def draw_fusion_result(V,A):
    fig = plt.figure(figsize=(4, 4))
    ax = fig.add_subplot(111)
    ax.spines['top'].set_color('none')
    ax.spines['right'].set_color('none')
    ax.xaxis.set_ticks_position('bottom')
    ax.spines['bottom'].set_position(('data', 0))
    ax.yaxis.set_ticks_position('left')
    ax.spines['left'].set_position(('data', 0))
    for i in range(8):
        plt.plot(V[i]-5,A[i]-5)
    #plt.style.use('ggplot')
    ax.set_xticks([-4, 4])
    ax.set_yticks([-4, 4])    
    plt.show()
    
def users_clusters(cluster_videos):
    D = np.zeros([8,60,32])
    for i in range(8):
        if i ==0:n=25 
        else: n =30
        for j in range(60):
            for k in range(32):
                t = len(np.where(cluster_videos[i][j][:,0]==(k+1))[0])/n
                if t>0: D[i,j,k] =1
    D = np.sum(D,axis=2)/32
    return D

def draw_num_users(cluster_videos):
    D = users_clusters(cluster_videos)
    color = ['r','m','y','g','c','b','k',"#123456"]
    x = np.linspace(1,60,60)
    
    fig, ax = plt.subplots(4, 2, sharex='col', sharey='row',figsize=(32, 20))
    for i in range (8):
        plt.subplot(4, 2, i+1)
        plt.bar(x, D[i,:], color=color[i])
        plt.axis([0, 60, 0.4, 1])
        plt.ylabel("percentage of users in the cluster")
        plt.xlabel("seconds")
        plt.title("Video %d"%(i+1))
    plt.savefig("../figs/percentage/video_all.png",dpi=300)
    plt.show()
    
def draw_hierarchical_clustering(Z):
    #input: the cluster result
    plt.title('Hierarchical Clustering Dendrogram (truncated)')
    plt.xlabel('sample index')
    plt.ylabel('distance')
    dendrogram(Z, truncate_mode='lastp', p=100, show_leaf_counts=False, leaf_rotation=90., leaf_font_size=12., show_contracted=True)
    plt.savefig("../figs/hierarchical_clustering.png", dip =300)
    plt.show()
