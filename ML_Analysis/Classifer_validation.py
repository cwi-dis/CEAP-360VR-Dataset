# -*- coding: utf-8 -*-
"""
Created on Tue Jan 26 09:34:20 2021

@author: S4
"""
import pandas as pd
import numpy as np
import Classifer_models


def load_data(seg, subject_id, feature, label_id, SD):
    if feature == 0:
        path = './sub_split/'
        range_index = [0, 12]
    else:
        path = './sub_split_feature_' + str(seg) + '/'
        range_index = [0, 49]  # feature num = 49
    path_data = path + str(subject_id) + '.csv'
    data = pd.read_csv(path_data)
    test_x = np.array(data)[:, range_index[0]:range_index[1]]
    test_y = np.array(data)[:, label_id].reshape([-1, 1])
    if SD == 1:  # subject-dependent model
        train_x = test_x
        train_y = test_y
    elif SD == 0:  # subject-independent model
        train_x = np.zeros([0, range_index[1] - range_index[0]])
        train_y = np.zeros([0, 1])
        for i in range(1, 33):
            if i != subject_id:
                d = pd.read_csv(path + str(i) + '.csv')
                d_x = np.array(d)[:, range_index[0]:range_index[1]]
                d_y = np.array(d)[:, label_id].reshape([-1, 1])
                train_x = np.concatenate((train_x, d_x), axis=0)
                train_y = np.concatenate((train_y, d_y), axis=0)
    if feature == 0:
        train_x = train_x.reshape([-1, 50 * 2, range_index[1] - range_index[0]])  # 50Hz*2s
        test_x = test_x.reshape([-1, 50 * 2, range_index[1] - range_index[0]])
        train_y = train_y.reshape([-1, 50 * 2])
        train_y = np.round(np.mean(train_y, axis=1)).reshape([-1, 1])
        test_y = test_y.reshape([-1, 50 * 2])
        test_y = np.round(np.mean(test_y, axis=1)).reshape([-1, 1])
    return train_x, np.array(train_y, dtype='int'), test_x, np.array(test_y, dtype='int')


def SDs(i, label_id, feature, seg, model_type):
    ACC_train = []
    ACC_test = []
    F1_test = []
    x, y, _, _ = load_data(seg, str(i), feature, label_id, 1)
    y = y[:, 0]
    for j in range(10):  # number of folds
        xx = []
        yy = []
        if feature == 1:
            test_x = train_x = np.zeros([0, x.shape[1]])

        else:
            test_x = train_x = np.zeros([0, x.shape[1], x.shape[2]])
        test_y = train_y = np.zeros([0, 1])
        for k in range(int(max(y) + 1)):  # balance
            xx.append(x[y == k])
            yy.append(y[y == k])
        for l in range(int(max(y) + 1)):
            if feature == 0:
                test_x = np.concatenate(
                    (test_x, xx[l][int(j * np.floor(len(xx[l]) / 10)):int((j + 1) * np.floor(len(xx[l]) / 10)), :]),
                    axis=0)
                train_x = np.concatenate((train_x, np.delete(xx[l], range(int(j * np.floor(len(xx[l]) / 10)),
                                                                          int((j + 1) * np.floor(len(xx[l]) / 10))),
                                                             axis=0)), axis=0)
            else:
                test_x = np.concatenate(
                    (test_x, xx[l][int(j * np.floor(len(xx[l]) / 10)):int((j + 1) * np.floor(len(xx[l]) / 10))]),
                    axis=0)
                train_x = np.concatenate((train_x, np.delete(xx[l], range(int(j * np.floor(len(xx[l]) / 10)),
                                                                          int((j + 1) * np.floor(len(xx[l]) / 10))),
                                                             axis=0)), axis=0)
            test_y = np.concatenate((test_y, yy[l].reshape([-1, 1])[int(j * np.floor(len(yy[l]) / 10)):int(
                (j + 1) * np.floor(len(yy[l]) / 10))]), axis=0)
            train_y = np.concatenate((train_y, np.delete(yy[l].reshape([-1, 1]),
                                                         range(int(j * np.floor(len(yy[l]) / 10)),
                                                               int((j + 1) * np.floor(len(yy[l]) / 10))), axis=0)),
                                     axis=0)
        acc_train, acc_test, f1_test = Classifer_models.to_result(model_type, train_x, train_y, test_x, test_y)
        ACC_train.append(acc_train)
        ACC_test.append(acc_test)
        F1_test.append(f1_test)
    print("%s %s, train_acc = %.2f%%, test_acc = %.2f%%" % (model_type,
                                                            str(i).rjust(2, '0'),
                                                            np.mean(ACC_train) * 100,
                                                            np.mean(ACC_test) * 100))
    return np.mean(ACC_train), np.mean(ACC_test), np.mean(F1_test)


def SD(label_id, feature, seg, model_type):
    # subject independent model,leave-one-subject-out cross validation
    # feature: = 0 signal data，= 1 feature data
    ACC_train = []
    ACC_test = []
    F1_test = []
    for i in range(1, 33):
        acc_train, acc_test, f1_test = SDs(i, label_id, feature, seg, model_type)
        ACC_train.append(acc_train)
        ACC_test.append(acc_test)
        F1_test.append(f1_test)
    print("===%s all, train_acc = %.2f%%, test_acc = %.2f%%,f1_test = %.4f===" % (model_type,
                                                                                  np.mean(ACC_train) * 100,
                                                                                  np.mean(ACC_test) * 100,
                                                                                  np.mean(F1_test)))
    return np.mean(ACC_train), np.mean(ACC_test), np.mean(F1_test)


def SIs(i, label_id, feature, seg, model_type):
    # i: pid
    train_x, train_y, test_x, test_y = load_data(seg, i, feature, label_id, 0)
    acc_train, acc_test, f1_test = Classifer_models.to_result(model_type, train_x, train_y, test_x, test_y)
    print("%s %s, train_acc = %.2f%%, test_acc = %.2f%%" % (model_type,
                                                            str(i).rjust(2, '0'),
                                                            acc_train * 100,
                                                            acc_test * 100))
    return acc_train, acc_test, f1_test


def SI(label_id, feature, seg, model_type):
    # subject independent model,leave-one-subject-out cross validation
    # feature: = 0 signal data，= 1 feature data
    ACC_train = []
    ACC_test = []
    F1_test = []
    for i in range(1, 33):
        acc_train, acc_test, f1_test = SIs(i, label_id, feature, seg, model_type)
        ACC_train.append(acc_train)
        ACC_test.append(acc_test)
        F1_test.append(f1_test)
    print("===%s all, train_acc = %.2f%%, test_acc = %.2f%%,f1_test = %.4f===" % (model_type,
                                                                                  np.mean(ACC_train) * 100,
                                                                                  np.mean(ACC_test) * 100,
                                                                                  np.mean(F1_test)))
    return np.mean(ACC_train), np.mean(ACC_test), np.mean(F1_test)
