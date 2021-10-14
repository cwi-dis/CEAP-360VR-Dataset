# this script is used to
# 1) Run baseline experiments
# (20210301 created by Tong Xue)


import Classifer_validation
from pyserverchan import pyserver

SCKEY = "SCU156287T1f895ec833e95a84892afe1a2e816d66601403c50c6bb"
user_URL = 'https://sc.ftqq.com/' + SCKEY + '.send'
svc = pyserver.ServerChan(user_URL)



# supported methods are: SVM, RF, NB, KNN, 1DCNN and LSTM
'''
=======================Subject-dependent model============================
The test method use 10-fold cross validation on all the users individually
and get the average results for all of them
==========================================================================
'''
# For exmample:
# acc_train, acc_test, f1_test = validation.SD(30, 1, "RF")
'''
label_id = 30, the used label is on the column #30 of the .csv files
feature = 1, use the manually extracted features for 
             to use a DL model (i.e., 1DCNN and LSTM), set feature = 0
model_type = "RF", use the random forests for classification
==========================================================================
'''

'''
=======================Subject-independent model===========================
The test method use leave-one-subject-out cross validation on all the users 
individually and get the average results for all of them
==========================================================================
'''
# For exmample:
# acc_train, acc_test, f1_test = validation.SI(30, 1, "RF")
'''
==========================================================================
'''

'''
=======================Get results from individual user ==================
==========================================================================
'''
# 10-fold cross validataion for subject 1:
# acc_train, acc_test, f1_test = validation.SDs(1, 30, 1, "RF")
# LOSOCV testing on subject 1:
# acc_train, acc_test, f1_test = validation.SIs(1, 30, 1, "RF")
'''
==========================================================================
'''

# DL Classifer
for _seg in range(1, 5):
    for i in range(12, 17):
        # svc.output_to_weixin("seg=" + str(_seg) + label=" + str(i))
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 0, _seg, "1DCNN")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 0, _seg, "LSTM")
        acc_train, acc_test, f1_test = Classifer_validation.SI(i, 0, _seg, "1DCNN")
        acc_train, acc_test, f1_test = Classifer_validation.SI(i, 0, _seg, "LSTM")



# ML Classifer
for _seg in range(2, 3):
    for i in range(61, 66):   # 61-66
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "SVM")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "RF")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "NB")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "KNN")




