import numpy as np
from sklearn.cluster import KMeans
from collections import Counter
from scipy.cluster.hierarchy import linkage,fcluster

def hierarchy_clustering(samples,p):
    # p is the percentage of view points for the largest cluster
    Z = linkage(samples, 'ward')
    n = samples.shape[0]
    max_d = int(max(Z[:,2]))
    min_d = int(min(Z[:,2]))
    t = min_d;c=0
    while t<=max_d and c<p:
        clusters = fcluster(Z, t, criterion='distance')
        c = Counter(clusters).most_common(1)[0][1]/n
        t = t+1
    return clusters,Counter(clusters).most_common(1)[0][0]
    #return the label of cluster and the cluster index of the largest cluster 
    
def kmeans_clustering(samples,k):
    #k is the number of clusters for K-means
    estimator = KMeans(n_clusters=3)
    estimator.fit(samples)
    clusters = estimator.labels_
    return clusters,Counter(clusters).most_common(1)[0][0]
    #return the label of cluster and the cluster index of the largest cluster

def clustering_users(samples):
    data = samples.groupby("VID")
    cluster_videos = []
    for i in range(8):#for 8 videos
        clustering_data = np.array(data.get_group(i+1))
        cluster_video_1s = []
        for j in range(60):#for 60s
            n=30
            if i == 0:n=25
            clustering_data_all = clustering_data[np.where((clustering_data[:,2]>=(j*n+1)) & 
                                                         (clustering_data[:,2]<((j+1)*n+1)))]
            clustering_data_s = clustering_data_all[:,5:8]
            # the index of the biggest cluster
            #clusters, c =kmeans_clustering(clustering_data_s,3)
            clusters, c =hierarchy_clustering(clustering_data_s,0.8)
            fuse_data = clustering_data_all[np.where(clusters==c)]
            cluster_video_1s.append(fuse_data)
            #print(c)
        cluster_videos.append(cluster_video_1s)
        print("Finish video %s"%(i+1))
    return cluster_videos