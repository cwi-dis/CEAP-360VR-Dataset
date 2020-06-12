# CEAP-360VR
CEAP-360VR DATASET

**CEAP-360VR: A Continuous Physiological and Behavioral Emotion Annotation Dataset for 360◦ Videos**

## General Information
We develop the CEAP-360VR dataset to address the lack of continuously annotated behavioral and physiological datasets for 360 video VR affective computing. Accordingly, this dataset contains a) questionnaires (SSQ, IPQ, NASA-TLX); b) continuous valence-arousal annotations; c) head and eye movements as well as left and right eye pupil diameters while watching videos; d) peripheral physiological responses (ACC, EDA, SKT, BVP, HR, IBI). Our dataset also concludes the data pre-processing and data analysis script. 

## Dataset Structure
The roof directory contains the following six subfolders
- 1_Stimuli
- 2_QuestionnaireData
- 3_AnnotationData
- 4_BehaviorData
- 5_PhysioData
- 6_Scripts

The following is a detailed description of each sub-file:

1_Stimuli

	- VideoThumbNails
		contains the eight thumbNails for each video (.jpg)
	- VideoInfo.json
		contains the detailed information for eight videos

2_QuestionnaireData

	- PXX_Questionnaire_Data.json (X = 1, 2, ..., 32)
		contains questionnaire data for each participant

3_AnnotationData

	- Raw
		contains the raw annotation data captured from the Joy-Con joystick for each participant
	- Transformed
		contains the transformed valence-arousal data generated from the raw data for each participant
	- Frame
		contains the re-sampled annotation data from the transformed data for each participant

4_BehaviorData

	- Raw
		contains the raw behavior data captured from the HTC VIVE Pro Eye Tobii Device for each participant
	- Transformed
		contains the transformed heam/eye movement data (pitch/yaw) generated from the raw data, as well as pupil diameter data for each participant
	- Frame
		contains the re-sampled behavior data generated from the transformed data for each participant
	- HM_ScanPath
		contains the head scanpath data generated from the transformed data for each participant
	- EM_Fixation
		contains the eye gaze fixation data generated from the transformed data for each participant

5_PhysioData

	- Raw
		contains the raw physiological data captured from the Empatica E4 wristband for each participant
	- Transformed
		contains the transformed physiological data generated from the raw data for each participant
	- Frame
		contains the re-sampled physiological data from the transformed data for each participant


6_Scripts

	- Unity Project
		contains the complete project of our user-controlled experiment (Unity 2018.4.1f1, HTC VIVE Pro Eye HMD)
	- Data Processed
		contains scripts that undertake the pre-processing steps for converting the raw data to the transformed/frame data in the transformed and frame folders.
		conatins scripts for continuous annotation, behavior and physiological data analysis and visualization.

## Usage

 1. We have performed the time alignment of different types of data and 
    videos for each participant, as well as the proceesing scripts that 
    can be used to generate both the transformed and frame data.   
    Researchers can run their analysis methods on them.
       
 2. For researchers who want to try other data processing methods, you can directly use the raw data.

