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
user_dist_dir = './Results/Clustering/User_distribution_per_cluster/'

user_lb = 1
user_ub = 32

max_no_of_videos = 8
video_duration = 60             #Each video is of duration 60 seconds
annotation_interval = 1         #Interval of 1 second, continuous annotation on every 1 second

sampling_rate_1 = 25
sampling_rate_oth = 30

no_clusters = 4

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

    all_users = ''
    if len(usr_list)>0:
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

def load_csv_data():

    hm_csv_file_name = 'VA_HM_FrameData.csv'
    df = pd.read_csv(hm_csv_file_name)

    return df

def clustering_users_based_on_3D_hm(all_user_data):

    all_video_clustering_op = []

    ind_seg_clustering_op = {}
    ind_seg_clustering_op['no_clusters'] = no_clusters
    ind_seg_clustering_op['centroids'] = []
    ind_seg_clustering_op['users'] = []         # contains a list of lists, where every list correspond to pne cluster
    ind_seg_clustering_op['frames'] = []
    ind_seg_clustering_op['pitches'] = []
    ind_seg_clustering_op['yaws'] = []
    ind_seg_clustering_op['bbox'] = []          # contains a list of tuples for bounding pitch and yaw

    # clustering size file
    custering_size_file = result_dir + 'clustering_size_K=' + str(no_clusters) + '.csv'

    # clustering op file
    custering_op_file = result_dir + 'clustering_op_K=' + str(no_clusters) + '.csv'

    fp = open(custering_op_file, 'w')
    fp1 = open(custering_size_file, 'w')

    header = 'VideoId,Segment,Number of cluster(K)'
    header1 = 'VideoId,Segment,Number of cluster(K)'

    for p in range(0, no_clusters):
        header = header + ',' + 'Centroid(C' + str(p+1) +'),Number of Users(C' + str(p+1) + '),Users(C' + str(p+1) + '),Frames(C' + str(p+1) + ')' + ',Pitches(C' + str(p+1) + ')' + ',Yaws(C' + str(p+1) + ')' + ',BBox_PitchYaw(C' + str(p+1) + ')' + ',BBox_HM_XY(C' + str(p+1) + ')'
        header1 = header1 + ',' + 'Number of Users(C' + str(p+1) + ')'

    header = header + '\n'
    header1 = header1 + '\n'

    fp.write(header)
    fp1.write(header1)

    p_ids = list(set(all_user_data['PID']))
    video_ids = list(set(all_user_data['VID']))

    # p_ids = [1]

    print("p_ids:{}".format(p_ids))
    print("video_ids:{}".format(video_ids))

    no_segments = int(video_duration / annotation_interval)
    print("no_segments:{}".format(no_segments))

    for video_id in video_ids:
        if video_id == 1:
            no_samples = sampling_rate_1 * annotation_interval
        else:
            no_samples = sampling_rate_oth * annotation_interval

        for i in range(0, no_segments):

            # initialize clustering output variables

            ind_seg_clustering_op['no_clusters'] = no_clusters
            ind_seg_clustering_op['centroids'] = []
            ind_seg_clustering_op['users'] = []  # contains a list of lists, where every list correspond to pne cluster
            ind_seg_clustering_op['frames'] = []
            ind_seg_clustering_op['pitches'] = []
            ind_seg_clustering_op['yaws'] = []
            ind_seg_clustering_op['bbox'] = []  # contains a list of tuples for bounding pitch and yaw

            user_nos = []
            frame_ids = []
            hm_x = []
            hm_y = []
            hm_z = []
            pitch = []
            yaw = []

            begin_sample_no = i * no_samples
            end_sample_no = begin_sample_no + no_samples

            for p_id in p_ids:
                hm_data = all_user_data.loc[(all_user_data['VID']==video_id) & (all_user_data['PID']==p_id)]

                user_nos.extend(hm_data['PID'][begin_sample_no:end_sample_no])
                frame_ids.extend(hm_data['FrameID'][begin_sample_no:end_sample_no])
                hm_x.extend(hm_data['HM_X'][begin_sample_no:end_sample_no])
                hm_y.extend(hm_data['HM_Y'][begin_sample_no:end_sample_no])
                hm_z.extend(hm_data['HM_Z'][begin_sample_no:end_sample_no])
                pitch.extend(hm_data['Pitch'][begin_sample_no:end_sample_no])
                yaw.extend(hm_data['Yaw'][begin_sample_no:end_sample_no])

            print("video_id:{}".format(video_id))
            print("segment_no:{}".format(i))

            # print("len(hm_x):{}".format(len(hm_x)))
            # print("len(hm_y):{}".format(len(hm_y)))
            # print("len(hm_z):{}".format(len(hm_z)))

            print("====")

            users = np.array(user_nos)
            frames = np.array(frame_ids)
            hm_x_arr = np.array(hm_x)
            hm_y_arr = np.array(hm_y)
            hm_z_arr = np.array(hm_z)
            pitch_arr = np.array(pitch)
            yaw_arr = np.array(yaw)

            X = np.vstack((users, frames, hm_x_arr, hm_y_arr, hm_z_arr, pitch_arr, yaw_arr)).T

            kmeans = KMeans(n_clusters=no_clusters)
            kmeans.fit(X[:, 2:5])
            y_kmeans = kmeans.predict(X[:, 2:5])
            centers = kmeans.cluster_centers_

            y_clusters = np.array(y_kmeans)

            op_matrix = np.vstack((users, frames, hm_x_arr, hm_y_arr, hm_z_arr, pitch_arr, yaw_arr, y_clusters)).T

            # print("OP:{}".format(y_clusters))
            print("====")

            # Store clustering op
            # Store clustering size

            line = str(video_id) + ',' + str(i) + ',' + str(no_clusters)
            line1 = str(video_id) + ',' + str(i) + ',' + str(no_clusters)

            for p in range(0,no_clusters):
                act_users,act_frames,act_pitches,act_yaws,bbox_2d,bbox_3d = find_actual_users_frames_for_the_cluster(p,op_matrix)

                no_users = len(act_users)
                # print("no_users:{}".format(no_users))

                usr_list = concatenate_all_users(act_users)
                frame_list = concatenate_all_users(act_frames)
                pitch_list = concatenate_all_users(act_pitches)
                yaw_list = concatenate_all_users(act_yaws)

                line = line + ',' + str(centers[p,:]) + ',' + str(no_users) + ',' + str(usr_list) + ',' + str(frame_list) + ',' + str(pitch_list) + ',' + str(yaw_list) + ',' + bbox_2d + ',' + bbox_3d
                line1 = line1 + ',' + str(no_users)

            line = line + '\n'
            line1 = line1 + '\n'

            fp.write(line)
            fp1.write(line1)

    fp.close()
    fp1.close()

def find_actual_users_frames_for_the_cluster(cluster_id,op_matrix):

    users = op_matrix[op_matrix[:, 7] == cluster_id, 0]
    fpy = op_matrix[op_matrix[:, 7] == cluster_id, :]
    print("len(fpy):{}".format(len(fpy)))

    pv = np.array(op_matrix[op_matrix[:, 7] == cluster_id, 5])
    yv = np.array(op_matrix[op_matrix[:, 7] == cluster_id, 6])

    hmxv = np.array(op_matrix[op_matrix[:, 7] == cluster_id, 2])
    hmyv = np.array(op_matrix[op_matrix[:, 7] == cluster_id, 3])

    act_frames =[]
    act_pitches = []
    act_yaws = []
    bbox_2d='(' + str(np.min(pv)) + ';' + str(np.min(yv)) + ')' + '(' + str(np.max(pv)) + ';' + str(np.max(yv)) + ')'
    bbox_3d='(' + str(np.min(hmxv)) + ';' + str(np.min(hmyv)) + ')' + '(' + str(np.max(hmxv)) + ';' + str(np.max(hmyv)) + ')'

    for y in range(len(fpy)):
        usr = fpy[y,0]
        fr = fpy[y,1]
        pitch = fpy[y,5]
        yaw = fpy[y,6]
        act_frames.append('(' + str(usr)+';'+str(fr)+')')
        act_pitches.append('('+str(usr) + ';' + str(pitch)+')')
        act_yaws.append('('+ str(usr) + ';' + str(yaw)+')')

    t_users = list(set(list(users)))
    act_users = list(set(list(users)))

    for u_id in t_users:
        max_usr_cnt = 0
        tgt_cluster = cluster_id
        for p in range(0,no_clusters):
            usr_cnt = len(op_matrix[(op_matrix[:, 7] == p) & (op_matrix[:, 0] == u_id)])
            if usr_cnt > max_usr_cnt:
                max_usr_cnt = usr_cnt
                tgt_cluster = p
        if tgt_cluster != cluster_id:
            act_users.remove(u_id)

    # Following approach is based on selecting all frames of the user that belonged to a cluster

    # act_frames = []
    # for u_id in act_users:
    #     frames = list(op_matrix[((op_matrix[:, 7] == cluster_id) & (op_matrix[:, 0] == u_id)),1])
    #     for x in range(0,len(frames)):
    #         t = str(u_id) + '-' + str(frames[x])
    #         act_frames.append(t)
    #
    # act_pitches = []
    # act_yaws = []
    # for u_id in act_users:
    #     pitch = list(op_matrix[((op_matrix[:, 7] == cluster_id) & (op_matrix[:, 0] == u_id)),5])
    #     yaw = list(op_matrix[((op_matrix[:, 7] == cluster_id) & (op_matrix[:, 0] == u_id)),6])
    #
    #     act_pitches.extend(pitch)
    #     act_yaws.extend(yaw)

    return act_users,act_frames,act_pitches,act_yaws,bbox_2d,bbox_3d

def plot_cdfs_for_max_clusters_size():
    min_k=2
    max_k=4
    video_legend = ['V1','V2','V3','V4','V5','V6','V7','V8']

    for k in range(min_k, max_k+1):
        clustering_size_file_name = result_dir + 'clustering_size_K='+str(k)+'.csv'
        df = pd.read_csv(clustering_size_file_name)

        fig = plt.figure(1, figsize=(12, 8))
        ax = fig.gca()

        for video_id in range(1,max_no_of_videos+1):
            relevant_data = df.loc[(df["VideoId"]==video_id)&(df["Number of cluster(K)"]==k)]
            print(relevant_data)
            print(len(relevant_data))
            relevant_data = np.array(relevant_data)
            start_index = 3
            end_index = start_index + k

            no_similar_users =[]
            for m in range(0,len(relevant_data)):
                print(relevant_data[m,:])
                no_similar_users.append(np.max(relevant_data[m,start_index:end_index]))

            print("Video id:{}".format(video_id))
            print("no_similar_users:{}".format(no_similar_users))

            ecdf = ECDF(np.array(no_similar_users))

            plt.plot(ecdf.x, ecdf.y, linewidth=3)
            plt.xticks(size=20)
            plt.yticks(size=20)
            ax.grid(True)
            ax.legend(video_legend, loc='upper left', fontsize=20)
            title_str = 'CDF of number of users in every segment\'s majority cluster for: K=' + str(k)
            ax.set_title(title_str, size=26)
            ax.set_xlabel('Number of users in the majority cluster of the segments', size=24)
            ax.set_ylabel('Likelihood of occurrence (CDF)', size=24)

            # Save the figure
        plt.savefig(result_dir + 'K='+ str(k) + '_segment_wise_no_of_similar_users_CDF.PNG', bbox_inches='tight')

        plt.show()
        plt.close(fig)

def plot_segment_wise_user_distribution_in_clusters():

    min_k = 2
    max_k = 4
    no_segments = int(video_duration / annotation_interval)

    for k in range(min_k, max_k + 1):
        clustering_size_file_name = result_dir + 'clustering_size_K=' + str(k) + '.csv'
        df = pd.read_csv(clustering_size_file_name)

        cluster_legend = []
        for j in range(k):
            cluster_legend.append('C' + str(j+1))

        for video_id in range(1, max_no_of_videos + 1):
            relevant_data = df.loc[(df["VideoId"] == video_id) & (df["Number of cluster(K)"] == k)]
            relevant_data = np.array(relevant_data)
            usr_dist = np.zeros((no_segments,k))
            data = []

            for i in range(len(relevant_data)):
                for j in range(k):
                    usr_dist[i,j] = float(relevant_data[i,3+j])/float(np.sum(relevant_data[i,3:3+k]))

                segment_id = 'Segment' + str(i+1)
                # data.append(dict(zip(segment_id,[usr_dist[i,:]])))
                data.append(usr_dist[i,:])

            usr_dist_df = pd.DataFrame(data)
            # print("usr_dist_df:{}".format(usr_dist_df))

            # usr_dist_df_t = usr_dist_df.T
            usr_dist_df = usr_dist_df * 100

            # Create a figure instance
            fig = plt.figure(1, figsize=(18, 8))
            # ax = fig.gca()

            # Create an axes instance
            ax = usr_dist_df.plot.bar(stacked=True)
            ax.xaxis.grid(linestyle='--', linewidth=2)

            plt.xlabel('Segment Id', size=20)
            plt.ylabel('User(%)', size=20)
            plt.xticks(size=12)
            plt.yticks(size=12)

            ax.set_title('Video(V'+str(video_id)+'_K='+str(k)+')_cluster-wise user distribution')
            ax.legend(cluster_legend, loc='upper center', bbox_to_anchor=(0.5, 1.2),
                       fancybox=True, shadow=False, ncol=5, prop={'size': 11})

            # # get the current figure
            # fig = plt.gcf()

            # Save the figure
            plt.savefig(user_dist_dir+ 'Cluster_wise_user_dist_V'+str(video_id)+'_K='+str(k)+'.PNG', bbox_inches='tight')

            # Show the plot
            # plt.show()
            plt.close(fig)

def main():

    ######################################################################
    # The following set of functions are based on the pitch/yaw clustering
    ######################################################################

    # all_user_data = load_data()
    # all_segment_clustering_op = clustering_users(all_user_data)
    # generate_plot_for_segment_wise_cluster_size(all_segment_clustering_op)
    #
    # clustering_op_file = store_video_segment_wise_clustering_output(all_segment_clustering_op,no_clusters)
    #
    # # # Uncomment the following line if just want to generate the CDFs
    # # clustering_op_file = 'clustering_op.csv'
    # # generate_plot_for_dominant_cluster_size(clustering_op_file)

    ##############################################################################
    # Following set of functions are to implement the below-mentioned instructions
    # step 1: cluster users based on HM_X, HM_Y, HM_Z. output: set of clusters(e.g., 3)
    # step 2: for each cluster, collect Pitch and Yaw data for that set, and then compute centroid
    # step 3: for each centroid within a cluster, reconstruct bounding box
    ###############################################################################

    # all_user_data_csv = load_csv_data()
    # all_segment_clustering_op_csv = clustering_users_based_on_3D_hm(all_user_data_csv)
    # plot_cdfs_for_max_clusters_size()
    plot_segment_wise_user_distribution_in_clusters()

if __name__ == '__main__':
    main()