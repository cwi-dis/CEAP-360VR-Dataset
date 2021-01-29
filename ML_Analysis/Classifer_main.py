# -*- coding: utf-8 -*-
"""
Created on Tue Jan 26 09:34:10 2021

@author: S4
"""

import Classifer_validation
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
# ML Classifer
for _seg in range(1, 6):
    for i in range(57, 62):
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "SVM")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "RF")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "NB")
        acc_train, acc_test, f1_test = Classifer_validation.SD(i, 1, _seg, "KNN")

        acc_train, acc_test, f1_test = Classifer_validation.SI(i, 1, _seg, "SVM")
        acc_train, acc_test, f1_test = Classifer_validation.SI(i, 1, _seg, "RF")
        acc_train, acc_test, f1_test = Classifer_validation.SI(i, 1, _seg, "NB")
        acc_train, acc_test, f1_test = Classifer_validation.SI(i, 1, _seg, "KNN")

# DL Classifer
for i in range(12, 17):
    acc_train, acc_test, f1_test = Classifer_validation.SD(i, 0, 100, "1DCNN")
    acc_train, acc_test, f1_test = Classifer_validation.SD(i, 0, 100, "LSTM")
    acc_train, acc_test, f1_test = Classifer_validation.SI(i, 0, 100, "1DCNN")
    acc_train, acc_test, f1_test = Classifer_validation.SI(i, 0, 100, "LSTM")

