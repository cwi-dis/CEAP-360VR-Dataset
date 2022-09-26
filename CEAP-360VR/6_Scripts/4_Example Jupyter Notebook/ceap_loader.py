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

    def load_data_from_participant(self, 
                                participant_idx:int, 
                                data_type:str = "Annotations", 
                                processing_level:str = "Raw",
                                ):
        """
        Loads the recorded data from a specific participant and a given 
        experiment session segment.
        
        :param participant_idx: Index of the participant (generally from 1 to 32)
        :param data_type: String denoting the type of data to load:  ["Annotations", "Behavior", "Physio"]
        :param processing_level: String denoting the level of processing to be retrieved: ["Raw", "Transformed", "Frame"]
        :rtype: A single pandas DataFrame with the loaded data
        """
        path_to_requested_file = self.index["data"][data_type][processing_level][str(participant_idx)]
        print("Loading from: ", path_to_requested_file)

        df_data = self._load_json_data_from_filepath(path_to_requested_file)
        df_data.insert(0, column="processing_level", value=processing_level)
        df_data.insert(0, column="data_type", value=data_type)

        if(df_data[self.K_PARTICIPANT].iloc[0] != participant_idx):
            raise ValueError(f"The participant ID is different between the name of the file and the content for file {path_to_requested_file}")

        return df_data.copy(deep=True)


    def load_data_from_participant(self, 
                                participant_idx:int, 
                                data_type:str = "Annotations", 
                                processing_level:str = "Raw",
                                ):
        """
        Loads the recorded data from a specific participant and a given 
        experiment session segment.
        
        :param participant_idx: Index of the participant (generally from 1 to 32)
        :param data_type: String denoting the type of data to load:  ["Annotations", "Behavior", "Physio"]
        :param processing_level: String denoting the level of processing to be retrieved: ["Raw", "Transformed", "Frame"]
        :rtype: A single pandas DataFrame with the loaded data
        """
        path_to_requested_file = self.index["data"][data_type][processing_level][str(participant_idx)]
        print("Loading from: ", path_to_requested_file)

        df_data = self._load_json_data_from_filepath(path_to_requested_file)
        df_data.insert(0, column="processing_level", value=processing_level)
        df_data.insert(0, column="data_type", value=data_type)

        if(df_data[self.K_PARTICIPANT].iloc[0] != participant_idx):
            raise ValueError(f"The participant ID is different between the name of the file and the content for file {path_to_requested_file}")

        return df_data.copy(deep=True)



############################
#### ENTRY POINT
############################

import sys, argparse

def help():
    m = f"""
        Experiment execution with dataset ''
        Parameters:
            
        """
    # print(m)
    return m

def main(args):
    input_folder_path = args.datasetroot 
    print(f"Analyzing folder {input_folder_path}")
    return

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-p","--datasetroot", type=str, required=True, help=f"Path to the dataset EMTEQ")

    # args = parser.parse_args()
    # main(args)

    print(" >>>> TESTING MANUALLY")
    data_loader_etl2 = DatasetCEAP(os.path.join(THIS_PATH,"../../datasets/CEAP-360VR/"))

