import matplotlib
matplotlib.use('TkAgg')
import matplotlib.pyplot as plt
import json
import scipy
from itertools import repeat
import seaborn as sns; sns.set()  # for plot styling
import numpy as np
import pandas as pd
import os
from scipy import stats
from sklearn.cluster import KMeans
from statsmodels.distributions.empirical_distribution import ECDF


behavior_data_path = './CEAP-360VR/4_BehaviorData'
frame_path = behavior_data_path + '/Frame'
result_dir = './Results/Clustering/'

user_lb = 1
user_ub = 32

max_no_of_videos = 8
video_duration = 60             #Each video is of duration 60 seconds
annotation_interval = 1         #Interval of 1 second, continuous annotation on every 1 second

no_clusters = 2

def load_data():

    all_user_data = []

    for usr_no in range(user_lb, user_ub + 1):
        file_path = frame_path + '/P' + str(usr_no) + '_Behavior_FrameData.json'
        print(file_path)

        try:
            with open(file_path) as json_file:
                usr_data = json.load(json_file)
                behavior_data = usr_data["Behavior_FrameData"]
                p_id = behavior_data[0]["ParticipantID"]
                video_behavior_data = behavior_data[0]["Video_Behavior_FrameData"]

                ind_user_data = {}
                ind_user_data["TimeStamp"] = []
                ind_user_data["Pitch"] = []
                ind_user_data["Yaw"] = []

                for video_idx in range(max_no_of_videos):
                    hm_data = video_behavior_data[video_idx]["HM"]

                    hm_pitch = []
                    hm_yaw = []
                    hm_timestamps = []

                    for hm_idx in range(0, len(hm_data)):
                        hm_timestamps.append(hm_data[hm_idx]["TimeStamp"])
                        hm_pitch.append(hm_data[hm_idx]["Pitch"])
                        hm_yaw.append(hm_data[hm_idx]["Yaw"])

                    ind_user_data["TimeStamp"].append(hm_timestamps)
                    ind_user_data["Pitch"].append(hm_pitch)
                    ind_user_data["Yaw"].append(hm_yaw)

                all_user_data.append(ind_user_data)

        except Exception as e:
            print('File {} not found...'.format(file_path))

    print("all_user_data_size:{}".format(len(all_user_data)))

    for i in range(0,len(all_user_data)):
        print("ind_user_data:{}".format(len(all_user_data[i]['TimeStamp'][0])))
        print("ind_user_data:{}".format(len(all_user_data[i]['Pitch'][0])))
        print("ind_user_data:{}".format(len(all_user_data[i]['Yaw'][0])))

    return all_user_data

def clustering_users(all_user_data):

    all_segment_clustering_op = []
    ind_segment_clustering_op = {}
    ind_segment_clustering_op['VideoID'] = ''
    ind_segment_clustering_op['similar_users'] = []
    ind_segment_clustering_op['no_of_similar_users'] = []

    no_segments = int(video_duration / annotation_interval)
    print("no_segments:{}".format(no_segments))

    # need to find similar users for every segment of every video

    for video_idx in range(max_no_of_videos):
    # for every video

        ind_segment_clustering_op = {}
        ind_segment_clustering_op['VideoID'] = 'V'+str(video_idx+1)
        ind_segment_clustering_op['similar_users'] = []
        ind_segment_clustering_op['no_of_similar_users'] = []

        for i in range(0,no_segments):
        # for every segment

            pitch_values = []
            yaw_values = []
            user_nos = []

            for j in range(0,len(all_user_data)):
            # gather data from all users

                no_samples = int(len(all_user_data[j]['TimeStamp'][video_idx]) / no_segments)

                begin_sample_no = i * no_samples
                end_sample_no = begin_sample_no + no_samples

                # print("begin_sample_no:{}".format(begin_sample_no))
                # print("end_sample_no:{}".format(end_sample_no))

                pitch_values.extend(all_user_data[j]['Pitch'][video_idx][begin_sample_no:end_sample_no])
                yaw_values.extend((all_user_data[j]['Yaw'][video_idx][begin_sample_no:end_sample_no]))
                user_nos.extend(repeat(j+1,no_samples))

            print("video_idx:{}".format(video_idx))
            print("segment_no:{}".format(i))

            print("pitch_values:{}".format(pitch_values))
            print("yaw_values:{}".format(yaw_values))

            print("len_pitch_values:{}".format(len(pitch_values)))
            print("len_yaw_values:{}".format(len(yaw_values)))

            print("====")

            pitch = np.array(pitch_values)
            yaw = np.array(yaw_values)
            users = np.array(user_nos)

            X = np.vstack((users, pitch, yaw)).T

            # print("X:{}".format(X))
            # print("X:{}".format(X[:,1:3]))

            kmeans = KMeans(n_clusters=no_clusters)
            kmeans.fit(X[:,1:3])
            y_kmeans = kmeans.predict(X[:,1:3])

            y_clusters = np.array(y_kmeans)

            op_matrix = np.vstack((users, pitch, yaw, y_clusters)).T
            # print("op_matrix:{}".format(op_matrix))

            all_cluster = []

            for n in range(0, no_clusters):

                ind_cluster = {}
                ind_cluster['cluster_no'] = n
                ind_cluster['unique_users'] = []
                ind_cluster['users'] = op_matrix[(op_matrix[:, 3] == n), 0]
                # ind_cluster['unique_users'] = list(set(ind_cluster['users']))
                # print("size:{}".format(len(ind_cluster['unique_users'])))

                all_cluster.append(ind_cluster)

            for u in range(user_lb, user_ub+1):
                target_cluster = -1
                max_occurrence = -1
                for n in range(0,len(all_cluster)):
                    occurrence = list(all_cluster[n]['users']).count(u)
                    if occurrence >= max_occurrence:
                        max_occurrence = occurrence
                        target_cluster = n
                all_cluster[target_cluster]['unique_users'].append(u)

            print("all_cluster:{}".format(all_cluster))

            # find the dominant cluster (i.e. cluster having highest number of users)
            dominant_cluster = 0
            for l in range(0,len(all_cluster)):
                if len(all_cluster[l]['unique_users']) >= len(all_cluster[dominant_cluster]['unique_users']):
                    dominant_cluster = l


            # store the similar users
            ind_segment_clustering_op['similar_users'].append(all_cluster[dominant_cluster]['unique_users'])

            # store the number of similar users from the dominant cluster for the segment
            ind_segment_clustering_op["no_of_similar_users"].append(len(all_cluster[dominant_cluster]['unique_users']))

            # print("y_kmeans:{}".format(y_kmeans))
            # print("y_kmeans_cluster_1_size:{}".format((y_kmeans==0).sum()))
            # print("y_kmeans_cluster_2_size:{}".format((y_kmeans == 1).sum()))

            #
            fig = plt.figure(1, figsize=(8, 5))
            ax = fig.gca()
            ax.scatter(X[:, 1], X[:, 2], c=y_kmeans, s=50, cmap='viridis')
            centers = kmeans.cluster_centers_
            ax.scatter(centers[:, 0], centers[:, 1], c='black', s=200, alpha=0.5)

            # plt.scatter(X[:, 1], X[:, 2], c=y_kmeans, s=50, cmap='viridis')
            #
            # centers = kmeans.cluster_centers_
            # plt.scatter(centers[:, 0], centers[:, 1], c='black', s=200, alpha=0.5)

            plt.xlabel('Pitch', size=20)
            plt.ylabel('Yaw', size=20)
            plt.xticks(size=18)
            plt.yticks(size=18)

            target_dir = result_dir + 'K=' + str(no_clusters) + '/'
            # Save the figure
            plt.savefig(target_dir + 'V' + str(video_idx+1) + '_segment_'+ str(i+1)+ '_clustering_K='+str(no_clusters)+'.PNG',
                        bbox_inches='tight')

            # plt.show()
            plt.close(fig)
        all_segment_clustering_op.append(ind_segment_clustering_op)

        print("all_segment_clustering_op:{}".format(all_segment_clustering_op))

    return all_segment_clustering_op

def generate_plot_for_segment_wise_cluster_size(all_segment_clustering_op):

    # set target directory
    target_dir = result_dir + 'K=' + str(no_clusters) + '/'

    # Data
    df = pd.DataFrame({'x': range(1, len(all_segment_clustering_op[0]['no_of_similar_users'])+1),
                       'y0': all_segment_clustering_op[0]['no_of_similar_users'],
                       'y1': all_segment_clustering_op[1]['no_of_similar_users'],
                       'y2': all_segment_clustering_op[2]['no_of_similar_users'],
                       'y3': all_segment_clustering_op[3]['no_of_similar_users'],
                       'y4': all_segment_clustering_op[4]['no_of_similar_users'],
                       'y5': all_segment_clustering_op[5]['no_of_similar_users'],
                       'y6': all_segment_clustering_op[6]['no_of_similar_users'],
                       'y7': all_segment_clustering_op[7]['no_of_similar_users']
                       })

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y0', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y0', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[0]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG', bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y1', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y1', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[1]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y2', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y2', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[2]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y3', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y3', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[3]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y4', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y4', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[4]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y5', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y5', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[5]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y6', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y6', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[6]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

    fig = plt.figure(1, figsize=(8, 5))
    ax = fig.gca()
    # plt.plot('x', 'y7', data=df, color='blue', linestyle='--', linewidth=2)
    plt.bar('x', 'y7', data=df)
    plt.xlabel('Time segment (in sec.)', size=20)
    plt.ylabel('Dominant cluster size', size=20)
    plt.xticks(size=18)
    plt.yticks(size=18)

    # Save the figure
    plt.savefig(target_dir + all_segment_clustering_op[7]["VideoID"] + '_segment_wise_similar_users_K='+str(no_clusters)+'.PNG',
                bbox_inches='tight')
    # show the plot
    # plt.show()
    plt.close(fig)

def store_video_segment_wise_clustering_output(all_segment_clustering_op,no_clusters):
    custering_op_file = result_dir + 'clustering_op.csv'

    if os.path.exists(custering_op_file):
        fp = open(custering_op_file, 'a')
    else:
        fp = open(custering_op_file, 'w')
        header = 'VideoId,Segment,Number of cluster(K),Number of similar users,Similar users\n'
        fp.write(header)

    for i in range(0, len(all_segment_clustering_op)):
        for j in range(0, len(all_segment_clustering_op[i]['no_of_similar_users'])):
            all_user = concatenate_all_users(all_segment_clustering_op[i]['similar_users'][j])
            # line = all_segment_clustering_op[i]["VideoID"] + ',' + str(j + 1) + ',' + str(no_clusters) +','+ str(
            #     all_segment_clustering_op[i]['no_of_similar_users'][j]) + ',' + str(all_segment_clustering_op[i]['similar_users'][j]).strip('[]') + '\n'
            line = all_segment_clustering_op[i]["VideoID"] + ',' + str(j + 1) + ',' + str(no_clusters) + ',' + str(
                all_segment_clustering_op[i]['no_of_similar_users'][j]) + ',' + all_user + '\n'
            fp.write(line)

    fp.close()

    return custering_op_file

def concatenate_all_users(usr_list):
    all_users=''
    for i in range(0, len(usr_list)-1):
        all_users = all_users + str(usr_list[i]) + '-'
    all_users = all_users + str(usr_list[len(usr_list)-1])
    return all_users

def generate_plot_for_dominant_cluster_size(clustering_op_file):

    op_file = result_dir + clustering_op_file

    df = pd.read_csv(op_file)
    video_ids = list(set(df["VideoId"]))
    number_of_clusters = list(set(df["Number of cluster(K)"]))

    nc_legend = []
    for x in number_of_clusters:
        nc_legend.append('K='+str(x))

    for video_id in video_ids:
        print(video_id)

        # Create a figure instance
        fig = plt.figure(1, figsize=(12, 8))
        ax = fig.gca()

        for nc in number_of_clusters:
            print(nc)
            no_similar_users = df.loc[((df["VideoId"]==video_id) & (df["Number of cluster(K)"]==nc)),"Number of similar users"]
            print(no_similar_users)

            ecdf = ECDF(np.array(no_similar_users))

            plt.plot(ecdf.x, ecdf.y, linewidth = 3)
            plt.xticks(size=20)
            plt.yticks(size=20)
            ax.grid(True)
            ax.legend(nc_legend,loc='upper left',fontsize=20)
            title_str='CDF of number of users in every segment\'s majority cluster for:' + video_id
            ax.set_title(title_str, size=26)
            ax.set_xlabel('Number of users in the majority cluster of the segments',size=24)
            ax.set_ylabel('Likelihood of occurrence (CDF)',size=24)

        # Save the figure
        plt.savefig(result_dir + video_id + '_segment_wise_no_of_similar_users_CDF.PNG',bbox_inches='tight')

        plt.show()
        plt.close(fig)

def main():

    all_user_data = load_data()
    all_segment_clustering_op = clustering_users(all_user_data)
    generate_plot_for_segment_wise_cluster_size(all_segment_clustering_op)

    clustering_op_file = store_video_segment_wise_clustering_output(all_segment_clustering_op,no_clusters)

    # # Uncomment the following line if just want to generate the CDFs
    # clustering_op_file = 'clustering_op.csv'
    # generate_plot_for_dominant_cluster_size(clustering_op_file)

if __name__ == '__main__':
    main()