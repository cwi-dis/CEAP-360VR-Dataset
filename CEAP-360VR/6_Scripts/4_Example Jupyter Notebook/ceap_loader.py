#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# =============================================================================
# Created By  : Luis Quintero | luisqtr.com
# Created Date: 2022/08
# =============================================================================
"""
Functions to load the datasets from the public dataset CEAP-360VR.

Check the paper: https://doi.org/10.1109/TMM.2021.3124080
Download the dataset: https://github.com/cwi-dis/CEAP-360VR-Dataset

"""
# =============================================================================
# Imports
# =============================================================================

import os, json

# Import data manipulation libraries
from copy import deepcopy
from enum import Enum

# Import scientific
import numpy as np
import pandas as pd

# =============================================================================
# Enums
# =============================================================================

class DataFoldersCEAP(Enum):
    """
    Enum to access the dictionary with the data per video
    """
    Stimuli = "1_Stimuli"
    Demographics = "2_QuestionnaireData"
    Annotations = "3_AnnotationData"
    Behavior = "4_BehaviorData"
    Physio = "5_PhysioData"
    
    def __str__(self):
        return super().value.__str__()

class ProcessingLevelCEAP(Enum):
    """
    Enum to access the dictionary with the data per video
    """
    Raw = "Raw"
    Frame = "Frame"
    Transformed = "Transformed"
    
    def __str__(self):
        return super().value.__str__()

# =============================================================================
# Utils
# =============================================================================    

def check_or_create_folder(filename):
    """
    Creates a folder to the indicated path to be able to write files in it
    """
    import os
    if not os.path.isdir(os.path.dirname(filename)):
        os.makedirs(os.path.dirname(filename))
    return

def create_json(dictionary, json_path = "folder_tree.json", pretty=False):
    """
    Create a structured dictionary with the filenames of the
    files where the main data is located within the compressed dataset.

    :param json_path: Destination path of JSON file with the dictionary
    :type json_path: str
    :return: Loaded JSON file 
    :rtype: JSON
    """
    check_or_create_folder(json_path)
    with open(json_path, "w") as json_file:
        if (pretty):
            json.dump(dictionary, json_file, indent=4)
        else:
            json.dump(dictionary, json_file)
        print("JSON file was created in", json_path)
        return json_file


def load_json(json_path = "folder_tree.json"):
    """
    Loads in memory a structured dictionary saved with `create_json`

    :param json_path: Path of JSON file with the dictionary
    :type json_path: str
    :return: Loaded JSON file 
    :rtype: dict
    """
    with open(json_path, "r") as json_file:
        json_data = json.load(json_file)
        return json_data

"""
Generate paths for intermediate files
"""

def generate_complete_path(filename:str, main_folder="./temp/", subfolders='', file_extension = ".png", save_files=True):
    """
    Function to create the full path of a plot based on `name`. It creates all the subfolders required to save the final file.
    If `save_files=False` returns `None`, useful to control from a global variable whether files should be updated or not.

    :param filename: Name of the file (without extension)
    :type filename: str
    :param main_folder: Root folder for the files
    :type main_folder: str
    :param subfolders: Subfolders attached after the root.
    :type subfolders: str
    :param file_extension: Extension of the image
    :type file_extension: str
    :param subfolders: Create path or return None. 
    :type save_files: boolean
    :return: Complete path to create a file
    :rtype: str

    Example: generate_complete_path("histogram", main_folder="./plots/", subfolders='dataset1/') will return
                "./plots/dataset1/histogram.png"
    """
    if (save_files):
        path = main_folder + subfolders + filename + file_extension
        check_or_create_folder(path)
        return path
    else:
        return None

# =============================================================================
# Main
# =============================================================================    

class DatasetCEAP():
    """
    This class loads the data from the dataset CEAP-360VR: 
    URL: https://github.com/cwi-dis/CEAP-360VR-Dataset
    
    The output of this class is a configured object that loads the list of available
    participants and their data.

        - object.stimuli      > 
        - object.demographics   >
        - object.data        >

    The output of the data loading process is:
        - data["Annotations"]["Raw"] > Returns the path to load the JSON file
                                corresponding to data type "Annotaions" and
                                "Raw" level of processing.
    
        The options for `data type` are: ["Annotations", "Behavior", "Physio"]
        The options for `level of processing` are: ["Raw", "Transformed", "Frame"]
    """

    # Structure of the dataset containing the data.
    # The values of the dict correspond to filename where data is stored
    
    LIST_DATA_TYPES = ["Annotations","Behavior","Physio"]
    LIST_PROCESSING_LEVELS = ["Raw", "Frame", "Transformed"]
    
    DATA_PROC_LEVEL_DICT = { level:{} for level in LIST_PROCESSING_LEVELS }
    DATA_TYPES_DICT = { 
        LIST_DATA_TYPES[0]:deepcopy(DATA_PROC_LEVEL_DICT),
        LIST_DATA_TYPES[1]:deepcopy(DATA_PROC_LEVEL_DICT),
        LIST_DATA_TYPES[2]:deepcopy(DATA_PROC_LEVEL_DICT),
        }

    ### CONSTANTS
    INPUT_FILE_EXTENSION = ".json"
    DATA_FILE_EXTENSION = ".csv"

    # Shortcut for keys
    K_PARTICIPANT = "ParticipantID"
    K_VIDEO = "VideoID"
    K_TIMESTAMP = "TimeStamp"

    # OUTPUT VALUES
    VIDEO_INFO_FILENAME = "video_info_summary.csv"
    DEMOGRAPHICS_FILENAME = "demographics_info_summary.csv"
    INDEX_TREE_FILENAME = "data_tree_index.json"

    # MAIN VARIABLES TO ACCESS DATA
    # Filenames
    folder_data_path = ""    # Root folder of the original dataset
    index_file_path = ""     # Filepath for the json file containing the index

    # Data Variables
    stimuli = None            # DataFrame to store information about each videofile
    demographics = None         # DataFrame to store participants' demographics
    data = None             # Dictionary of Pandas DataFrame with different types of data

    def __init__(self, folder_path):
        """
        Initializes object that analyzes dataset

        :param folder_path: Input folder with data
        :type folder_path: str
        :param dictionary_path: Output path with directory from 
        :type dictionary_path: str
        """
        self.folder_data_path = folder_path
        self.index_file_path = os.path.join(self.folder_data_path, self.INDEX_TREE_FILENAME)

        self.load_or_create_index()

        if(self.index != None):
            self.stimuli = pd.read_csv(self.index["stimuli_path"])
            self.demographics = pd.read_csv(self.index["demographics_path"])
        return

    def __load_index_file(self):
        """
        Loads the dictionary with the index file into memory.
        If error, returns None
        """
        try:  
            self.index = load_json(self.index_file_path)
            return 0
        except:
            # File hasn't been created yet
            return None

    def load_or_create_index(self):
        """
        Analyzes the folder to see which files are available.
        Enables access to the variable `self.index`, which contains a 
        dictionary with path to key event and data files.
        It also creates the json file at the root of the dataset.

        :return: Nothing
        :rtype: None
        """

        # Entry condition
        if(self.__load_index_file() is not None):
            print("Index already exists: Loading from ", self.index_file_path)
            return
        
        ##### Create index from the dataset folder
        print("There is no index yet! Creating it in ", self.index_file_path)
    
        # Dictionary to store files
        files_index = {}

        ########################################
        ### Load Video info
        video_info_path = os.path.join(self.folder_data_path, str(DataFoldersCEAP.Stimuli), "VideoInfo"+self.INPUT_FILE_EXTENSION)
        video_info_dict = load_json(video_info_path)
        video_info_dict = video_info_dict["VideoInfo"] # Unpack dict

        video_info_df = { k:[] for k in video_info_dict[0].keys() }
        for entry in video_info_dict:
            for k,v in entry.items():
                video_info_df[k].append(v)
        
        video_info_df = pd.DataFrame(video_info_df)

        # Save to CSV file
        filepath_temp = os.path.join(self.folder_data_path, self.VIDEO_INFO_FILENAME)
        video_info_df.to_csv(filepath_temp, index=False)

        files_index["stimuli_path"] = filepath_temp

        ########################################
        ### Load Demographics info
        demographics_path = os.path.join(self.folder_data_path, str(DataFoldersCEAP.Demographics))
        demographics_dict = None  # Where the compilation of info will be stored
        with os.scandir(demographics_path) as iterator:
            for entry in iterator:
                # A file is equivalent to a participant
                if( not entry.name.startswith(".") and 
                        entry.is_file() and 
                        entry.name.endswith(self.INPUT_FILE_EXTENSION) and
                        entry.name.startswith("P")):

                    participant_data_filename = os.path.join(demographics_path, entry.name)
                    data_participant_dict = load_json(participant_data_filename)["QuestionnaireData"][0] # Unpack dict
                    # Replace P0 with only the number
                    data_participant_dict[self.K_PARTICIPANT] = data_participant_dict[self.K_PARTICIPANT][1:]

                    # Extract specific SAM ratings per video from embedded dictionary
                    data_participant_dict_SAM = data_participant_dict.pop("Video_SAMRating_VideoTime_Data")
                    for video_rat_SAM in data_participant_dict_SAM:
                        # Each video rating is moved as a prefix of the column name
                        # to generate one row per participant
                        video_id = video_rat_SAM.pop(self.K_VIDEO)
                        video_start_time = video_rat_SAM.pop("Start_End_TimeStamp")[0]["UnixTimeStamp"]
                        
                        data_participant_dict[video_id+"_start_UnixTimestamp"] = video_start_time
                        for k,v in video_rat_SAM.items():
                            data_participant_dict[video_id+"_"+k] = v

            
                    # Populate final keys (columns) from the first file
                    if(demographics_dict is None):
                        demographics_dict = { k:[] for k in data_participant_dict.keys() }
                    
                    # Add values to array of columns
                    for k,v in data_participant_dict.items():
                        demographics_dict[k].append(v)

        demographics_df = pd.DataFrame(demographics_dict)
        # Save to CSV file
        filepath_temp = os.path.join(self.folder_data_path, self.DEMOGRAPHICS_FILENAME)
        demographics_df.to_csv(filepath_temp, index=False)

        files_index["demographics_path"] = filepath_temp

        ########################################
        # Store the paths that contain the long datasets
        files_index["data"] = deepcopy(self.DATA_TYPES_DICT)  # Data is stored per experiment session (>50MB/each file)

        iterate_data_types_dict = {
            self.LIST_DATA_TYPES[0]: DataFoldersCEAP.Annotations,
            self.LIST_DATA_TYPES[1]: DataFoldersCEAP.Behavior,
            self.LIST_DATA_TYPES[2]: DataFoldersCEAP.Physio,
        }

        # Iterate per data types
        for datatype_name, datatype_foldername in iterate_data_types_dict.items():
            # datatype_name = "Annotations" ### DEBUG
            # datatype_foldername = DataFoldersCEAP.Annotations  ### DEBUG
            # Iterate per level of processing according to original paper
            for processing_level_name in self.LIST_PROCESSING_LEVELS:
                # processing_level_name = self.LIST_PROCESSING_LEVELS[0]   ### DEBUG: Raw
                ### Load Annotations
                root_path = os.path.join(self.folder_data_path, str(datatype_foldername), str(processing_level_name))
                root_data_dict = None  # Where the compilation of info will be stored
                with os.scandir(root_path) as iterator:
                    for entry in iterator:
                        # A file is equivalent to a participant
                        if( not entry.name.startswith(".") and 
                                entry.is_file() and 
                                entry.name.endswith(self.INPUT_FILE_EXTENSION) and
                                entry.name.startswith("P")):
                            
                            participant_id = entry.name.split("_")[0][1:]
                            # print(f"\t {datatype_name} \t{str(processing_level_name)} \t{participant_id}")
                            files_index["data"][datatype_name][str(processing_level_name)][participant_id] = os.path.join(root_path, entry.name)
                            # print(files_index)

        # Store the files in a JSON
        create_json(files_index, self.index_file_path, pretty=True)

        print(f"Json file with index of the dataset was saved in {self.index_file_path}")

        # Global variable for the index
        self.index = files_index.copy()
        return

    def _load_json_data_from_filepath(self, path_to_requested_file):
        # Variable to store the dataframe
        df_data = None

        # Load the file from disk
        data_participant_dict = load_json(path_to_requested_file)
        # Extract the first value regardless the key. It's always an array
        data_participant_dict = next(iter(data_participant_dict.values()))[0]
        # All Json files have the `participantID` and then the corresponding data
        participant_id = data_participant_dict.pop(self.K_PARTICIPANT)
        participant_id = int(participant_id[1:]) if participant_id.startswith("P") else int(participant_id)
        # Access the array with feature names
        data_all_videos = next(iter(data_participant_dict.values()))
        # Iterate all videos per feature
        for i in range(len(data_all_videos)):
            # Accessing data per video. It has the videoId, then another 
            # (key,value) pair  with the actual measurements
            video_id = int(data_all_videos[i].pop(self.K_VIDEO)[1:])

            # Temporary dataframe to store results of a video.
            df_this_video_group = None

            # After removing the video Id, it's needed to get the feature names
            for ft_group_name, data_feature_group in data_all_videos[i].items():

                # print(f"\t\tAnalyzing video {video_id} - Feature Group {ft_group_name}")

                # A single video contains multiple feature types (e.g., "Physio" contains "HR", "EDA", etc.)
                # ITERATE FEATURE GROUP
                df_this_feature = {}
                for single_timestamp in range(len(data_feature_group)):
                    data_single_sample = data_feature_group[single_timestamp]

                    # Populate keys if empty. Otherwise it will just add rows to the existing data
                    for ft_colname, single_datapoint in data_single_sample.items():
                        # Create column names combining the feature_group+key in sample
                        key_combination = ft_group_name.split("_")[0]+"_"+ft_colname if (ft_colname != self.K_TIMESTAMP and len(data_all_videos[i].keys())>1) else ft_colname
                        if(key_combination not in df_this_feature.keys()):
                            df_this_feature[key_combination] = []
                        # Append data
                        df_this_feature[key_combination].append(single_datapoint)
                    ## End of a single sample
                ## End of all samples for a feature
                df_this_feature = pd.DataFrame(df_this_feature)
                # Process if the resulting feature has the column "TimeStamp". E.g, IBI data is not present in some participants and should not be included
                if(self.K_TIMESTAMP in df_this_feature.columns):
                    # df_this_feature.set_index(self.K_TIMESTAMP, inplace=True) # Not needed anymore, merging without being the index
                    # Add to main video data
                    df_this_video_group = df_this_feature if (df_this_video_group is None) else df_this_video_group.merge(df_this_feature, how="outer", on=self.K_TIMESTAMP, sort=True)
                    """ EXAMPLE of merge function
                    # The desired behavior is that it will add columns to existing timestamps,
                    # while adding TimeStamps if they do not exist. This is mostly useful for 
                    # `Raw` data because they contain variables with different sampling frequency
                    df1 = pd.DataFrame({'TimeStamp': [1.5, 1.8], 'b': [1, 2]})
                    df2 = pd.DataFrame({'TimeStamp': [1.5, 1.65], 'c': [3, 4]})
                    df1.merge(df2, how='outer', on='TimeStamp', sort=True)
                    """
                # print(f"{ft_group_name} - Size: {df_this_feature.shape}")
                # print(df_this_feature)
                # if ft_group_name=="BVP_RawData":
                #     break
                
            ## End of all features in a video
            df_this_video_group.insert(0, column=self.K_VIDEO, value=video_id)
            # df_this_video_group.reset_index(inplace=True)

            df_data = df_this_video_group if (df_data is None) else pd.concat([df_data, df_this_video_group.copy(deep=True)], axis=0, ignore_index=True)

        ## End of reading the file and going through all videos, feature groups, and individual samples.
        df_data.insert(0, column=self.K_PARTICIPANT, value=participant_id)
        
        return df_data


    def _process_clean_physio(self,
                            df:pd.DataFrame):
        """
        Removes the missing values caused by the IBI feature in physiological data
        """
         # Alias name
        data_postprocessed = df

        # Name of the column containing IBI data (this column will be removed and replaced by R-peaks)
        ibi_colname = "IBI_IBI"

        # Array with main column names in the dataset. Used to filter main columns in the dataset
        participant_colname = self.K_PARTICIPANT
        ts_colname = self.K_TIMESTAMP
        video_colname = self.K_VIDEO
        basic_cols = [participant_colname, video_colname, ts_colname]
        
        # Remove IBI colnames. 
        data_postprocessed = data_postprocessed.drop([ibi_colname], axis=1)

        # Identify the non-numeric `basic` colnames from the `data` colnames containing the relevant time-series.
        # The rows whose `data_colnames` are all NaN will be removed. Because it was a row created by the IBI feature.

        # Difference between sets of colnames
        data_colnames = set(data_postprocessed.columns).difference(set(basic_cols))
        # Remove all the rows that are empty, after removing IBI data
        remaining_data_index = data_postprocessed[data_colnames].dropna(axis=0, how="all").index
        # New post_processed data should have `1800` samples per time-series (except Video1, which has 1500)
        data_postprocessed = data_postprocessed.loc[remaining_data_index]

        return data_postprocessed
    

    def __estimate_pupil_response_caused_by_luminance(self, pupil_response:np.array, luminance:np.array):
        """
        Returns an array with the estimated pupil diameter response
        dependent on the luminance changes in a video.

        Based on discussion in section 4 from DOI: 10.1155/2020/2909267
        Eye-Tracking Analysis for Emotion Recognition
        """

        y = pupil_response
        x = luminance

        min_samples = np.min([y.size, x.size])
        y = y[:min_samples]
        x = x[:min_samples]

        # Arrange dimensions
        if x.ndim == 1:
            x = np.ones( (luminance.size, 2) )
            x[:,0] = luminance

        if y.ndim == 1:
            y = y.reshape(-1,1)

        # Solution of linear model
        y_solution = np.linalg.lstsq(x, y, rcond=None)

        # Calculate PD given the luminance
        coeffs = y_solution[0]  # Coefficients (b0, b1,...)

        # Estimated pupil diameter given the luminance
        y_est = np.matmul(x, coeffs)

        return y_est


    def _process_clean_pupil_diameter(self, df_data):
        """
        Normalizes the pupil diameter data based on the
        average luminance from the stimuli.
        This preprocessing step requires the file `VideoLuminance.csv`
        that can be generated from the notebook `ceap_calculate_video_luminance.ipynb`.
        """
        video_luminance_path = os.path.join(self.folder_data_path, str(DataFoldersCEAP.Stimuli), "VideoLuminance"+self.DATA_FILE_EXTENSION)

        if not os.path.isfile(video_luminance_path):
            print("WARNING!! The video luminance data cannot be loaded, please check the documentation of this function")
            return
        
        ## Process luminance
        PD_AVG_COLNAME = "PD_avg"
        PD_FROM_LUMINANCE_COLNAME = "PD_est"
        PD_RESIDUAL_COLNAME = "PD_corrected"

        results_luminance = pd.read_csv(video_luminance_path,index_col=0)

        PD_cols = ["LPD_PD", "RPD_PD"]
        df_data["PD_avg"] = df_data[PD_cols].mean(axis=1)
        
        # Detect how many videoIds are in the dataframe
        videos_ids = df_data["VideoID"].unique()
        videos_ids_str = [ f"V{i}" for i in videos_ids ]    # The luminance is in a column format "V1","V2","V3"...

        for i,v in enumerate(videos_ids):
            VIDEO_LUMINANCE_COLNAME = videos_ids_str[i]

            df_pd_participant_in_video = df_data.loc[ (df_data["VideoID"] == videos_ids[i]), PD_AVG_COLNAME ]
            df_pd_participant_in_video = df_pd_participant_in_video.reset_index().drop(["index"],axis=1)
            # print(f"{videos_ids_str[i]} {df_pd_participant_in_video.shape} {df_pd_participant_in_video.isna().sum()}")

            df_luminance_in_video = results_luminance[ VIDEO_LUMINANCE_COLNAME ].dropna()
            # print(f"{videos_ids_str[i]} {df_luminance_in_video.shape} {df_luminance_in_video.isna().sum()}")

            ###### Match both to the minimum samples
            df_pd_combined = pd.merge(df_pd_participant_in_video, df_luminance_in_video, left_index=True, right_index=True, how="left")
            df_pd_combined = df_pd_combined.fillna(method="ffill") # In case there are few missing values vs. what was writtenin VideoInfo.json

            # Estimate the linear relationship due to luminance
            est_pupil_response = self.__estimate_pupil_response_caused_by_luminance(df_pd_combined[ PD_AVG_COLNAME ].values,
                                                                                df_pd_combined[ VIDEO_LUMINANCE_COLNAME ].values)
            df_pd_adjusted = pd.DataFrame(data=est_pupil_response, columns=[ PD_FROM_LUMINANCE_COLNAME ])
            df_pd_combined = df_pd_combined.join(df_pd_adjusted)

            # Residual response not caused by videos' luminance
            df_pd_combined[ PD_RESIDUAL_COLNAME ] = df_pd_combined[ PD_AVG_COLNAME ] - df_pd_combined[ PD_FROM_LUMINANCE_COLNAME ]

            ### Add to final data
            df_data.loc[ df_data["VideoID"]==v, PD_RESIDUAL_COLNAME ] = df_pd_combined[PD_RESIDUAL_COLNAME].values
        
        return df_data


    def load_data_from_participant(self, 
                                participant_idx:int, 
                                data_type:str = "Annotations", 
                                processing_level:str = "Raw",
                                clean_physio = False,
                                clean_pd_with_luminance = False,
                                ):
        """
        Loads the recorded data from a specific participant and a given 
        experiment session segment.
        
        :param participant_idx: Index of the participant (generally from 1 to 32)
        :param data_type: String denoting the type of data to load:  ["Annotations", "Behavior", "Physio"]
        :param processing_level: String denoting the level of processing to be retrieved: ["Raw", "Transformed", "Frame"]
        :param clean_physio: Removes IBI feature from Physio to avoid inducing missing values
        :rtype: A single pandas DataFrame with the loaded data
        """
        path_to_requested_file = self.index["data"][data_type][processing_level][str(participant_idx)]
        print("Loading from: ", path_to_requested_file)

        df_data = self._load_json_data_from_filepath(path_to_requested_file)

        # Remove IBI and missing rows in case the flag is True
        if (clean_physio and (processing_level=="Frame") and (data_type=="Physio")):
            df_data = self._process_clean_physio(df_data)

        if (clean_pd_with_luminance and (processing_level=="Frame") and (data_type=="Behavior")):
            df_data = self._process_clean_pupil_diameter(df_data)

        # Add metadata
        df_data.insert(0, column="processing_level", value=processing_level)
        df_data.insert(0, column="data_type", value=data_type)

        if(df_data[self.K_PARTICIPANT].iloc[0] != participant_idx):
            raise ValueError(f"The participant ID is different between the name of the file and the content for file {path_to_requested_file}")

        return df_data.copy(deep=True)



# =============================================================================
# Processing
# =============================================================================    

def _upsample_df_with_interpolation(df, new_timestamps, index_name="TimeStamps"):
    """
    Function used to upsample the data corresponding to VideoID=1 
    from 25Hz to 30Hz, so that it matches the sample frequency of the
    data in the rest of the videos.

    Given a dataframe `df`. It upsamples the numeric columns
    doing linear interpolation based on a function trained on each column.
    The non-numeric features are replaced by the same value of the 
    first row in the original df.ParticipantID
    
    Returns another pandas DataFrame, with the same columns than the input
    `df` but the index corresponds to the `new_timestamps`.
    """
    import scipy.interpolate

    # Create new dataframe with the resampled version
    df_resampled = pd.DataFrame(index=pd.Index(new_timestamps, name=index_name), columns=df.columns)

    # Find the numeric columns that can be interpolated
    cols_numeric = list(df.select_dtypes([np.number]).columns)
    cols_non_numeric = list(set(df.columns.values).difference(set(cols_numeric)))

    # Non-numeric columns are replaced with the first value in the original dataframe
    df_resampled[cols_non_numeric] = df[cols_non_numeric].iloc[0,:]

    # Apply interpolation
    x = df.index.values#.total_seconds().values
    y = df[cols_numeric].values
    f_interpolation = scipy.interpolate.interp1d(x,y,axis=0)
    df_resampled[cols_numeric] = f_interpolation(new_timestamps)
    return df_resampled

def resample_dataframes_from_video1(df, ts_colname="TimeStamps"):

    df_ceap = df
    # After removing IBI, all arrays have 1800 samples
    df_filtered_single_ts = df_ceap[ (df_ceap.VideoID == 5) # Do not use VideoID=1 here!
                        & (df_ceap.ParticipantID == 1)
                        & (df_ceap.data_type == "Physio") ]
    
    # Extract a reference TimeStamp array to be used in the resampled version of VideoID=1
    timestamps_reference = df_filtered_single_ts.set_index(ts_colname).dropna(axis=1, how="all")
    timestamps_reference = timestamps_reference.index.values

    # Subset of data with values 1 and remove it from original dataset
    data_video_1 = df_ceap[ df_ceap.VideoID ==1 ].copy(deep=True)
    # print(f"Size data video 1:{data_video_1.shape}")

    # Delete from main dataset, we will input new values with proper resampling.
    df_ceap = df_ceap[ df_ceap.VideoID != 1 ]
    # print(f"Size after removing video 1:{df_ceap.shape}")

    # New dataframe for video 1
    data_video_1_resampled = None

    # Extract each time series (per participant, and per data group)
    for pid in data_video_1.ParticipantID.unique():
        for dgroup in data_video_1.data_type.unique():

            Q = ( (data_video_1.ParticipantID == pid) 
                    & (data_video_1.data_type == dgroup) )
            df_filter = data_video_1[ Q ]

            # Define timestamps as the index, and delete 
            # columns that are not relevant to the datagroup
            df_filter.set_index(ts_colname, inplace=True)
            df_filter.dropna(axis=1, how="all", inplace=True)

            # Resample the df_filter with the same timestamps than the reference.
            df_resampled = _upsample_df_with_interpolation(df_filter, timestamps_reference, index_name=df_filter.index.name)
            df_resampled.reset_index(inplace=True)

            data_video_1_resampled = df_resampled if (data_video_1_resampled is None) else pd.concat([data_video_1_resampled, df_resampled], axis=0, ignore_index=True)
            # print(f"Original = {df_filter.shape} - Resampled = {df_resampled.shape}")
            # break
        # break
    # print(f"Size data video 1 resampled: {data_video_1_resampled.shape}\n\tEnd")

    df_ceap = pd.concat([df_ceap, data_video_1_resampled], axis=0, ignore_index=True)

    return df_ceap



# =============================================================================
# Visualization
# =============================================================================    

def plot_all_data_from_participant(df, time_colname="TimeStamp", y_colname="VideoID"):

    """
    This function plots a single dataframe from the CEAP dataset.
    `df` is the result of applying the function `load_data_from_participant()`.

    A single file from the dataset contains many features, which may be sampled
    at different frequencies. This function takes a large dataset and creates a
    subplot (timeStamp, videoId) to generate a plot with the loaded data
    """

    import matplotlib.pyplot as plt

    # Get the numeric columns and delete the `index_colname` to make it index later
    cols = list(df.select_dtypes([np.number]).columns)
    if time_colname not in cols: raise ValueError(f"The dataframe does not contain numeric columns for the index x_colname={time_colname}")
    if y_colname not in df.columns: raise ValueError(f"The dataframe does not contain column with y_colname={y_colname}")
    cols.remove(time_colname)
    if y_colname in cols: cols.remove(y_colname)

    NUM_ROWS = len(cols)
    colnames_labels = df[y_colname].unique()
    NUM_COLS = len(colnames_labels)

    cmap = plt.cm.get_cmap("tab20")
    fig,axes = plt.subplots(NUM_ROWS, NUM_COLS, sharex=True, figsize=(6*NUM_COLS, 2*NUM_ROWS))
    for i in range(NUM_ROWS):
        for j in range(NUM_COLS):
            ax = axes[i,j]

            # Filter the data that has to do with this column label
            df_ax = df[ df[y_colname] == colnames_labels[j] ]
            df_ax = df_ax[[time_colname, cols[i]]].set_index(time_colname).dropna(axis=0)
            timestamps = df_ax.index.values
            data = df_ax[cols[i]].values
            ax.plot(timestamps, data, label=cols[i], color=cmap.colors[i] )
            if(i==0): ax.set_title(f"{y_colname}: {colnames_labels[j]}") # Suptitles for first row
            if(j==0): ax.set_ylabel(cols[i])    # Xlabel for first column
            if(i==NUM_ROWS-1): ax.set_xlabel(time_colname)
    plt.tight_layout()
    return

############################
#### ENTRY POINT
############################

import argparse

def help():
    m = f"""
        CEAP Loader. See the example Jupyter notebook to understand
        the functionalities of this file.
        """
    print(m)
    return

def main(args):
    try:
        if(args.datasetroot):
            print(f"Analyzing folder {args.datasetroot}")
            DatasetCEAP(args.datasetroot)
    except:
        help()
    return

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-p","--datasetroot", type=str, required=True, help=f"Path to the dataset")

    args = parser.parse_args()
    main(args)

