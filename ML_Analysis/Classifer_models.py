# -*- coding: utf-8 -*-
"""
Created on Tue Jan 26 09:33:08 2021

@author: S4
"""

import numpy as np
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Model
from tensorflow.keras.layers import Dense, Input, LSTM, Conv1D, GlobalMaxPooling1D
# from tensorflow.keras.activations import sigmoid
from tensorflow.keras import optimizers
from tensorflow.keras.callbacks import EarlyStopping, ModelCheckpoint
from keras.utils.np_utils import to_categorical
from sklearn.metrics import accuracy_score, f1_score
from sklearn import svm
from sklearn.naive_bayes import GaussianNB
from sklearn.ensemble import RandomForestClassifier
from sklearn.neighbors import KNeighborsClassifier
import os


def init_tf_gpus():
    # print(tf.__version__)
    gpus = tf.config.experimental.list_physical_devices('GPU')
    # print("Num GPUs Available: ", len(tf.config.experimental.list_physical_devices('GPU')))
    if gpus:
        try:
            for gpu in gpus:
                tf.config.experimental.set_memory_growth(gpu, True)
            # logical_gpus = tf.config.experimental.list_logical_devices('GPU')
            # print("Physical GPUs available: %d  -- Logical GPUs available: %d" % (len(gpus), len(logical_gpus)))
        except RuntimeError as e:
            print(e)


def create_mode_DL(model_type, num_s):
    window_size = 50 * 2  # 50Hz*2s
    num_c = 12  # channel num
    input_signals = Input(shape=(window_size, num_c))
    if model_type == "LSTM":
        x = LSTM(window_size, recurrent_dropout=0.2)(input_signals)
        x = Dense(num_s, activation='softmax')(x)  # if binary, activation = 'sigmoid'
    elif model_type == "1DCNN":
        x = Conv1D(4, 256, activation='relu', input_shape=(window_size, num_c), padding="same")(input_signals)
        x = Conv1D(8, 128, activation='relu', padding="same")(x)
        x = Conv1D(32, 64, activation='relu', padding="same")(x)
        x = GlobalMaxPooling1D()(x)
        x = Dense(128, activation='relu')(x)
        x = Dense(num_s, activation='softmax')(x)  # if binary, activation = 'sigmoid'
    else:
        print("Not a supported model")
        exit(0)
    model = Model(input_signals, x)
    return model


def create_model_ML(model_type):
    if model_type == "SVM":
        model = svm.SVC(decision_function_shape="ovr")
    elif model_type == "RF":
        model = RandomForestClassifier(max_depth=2, random_state=0)
    elif model_type == "NB":
        model = GaussianNB()
    elif model_type == "KNN":
        model = KNeighborsClassifier(n_neighbors=3)
    else:
        print("Not a supported model")
        exit(0)
    return model


def train_model_DL(train_x, train_y, model_type):
    init_tf_gpus()
    num_s = max(train_y) + 1
    model = create_mode_DL(model_type, num_s)
    train_y = to_categorical(train_y, int(max(train_y) + 1))
    if not os.path.exists("./model/%s/%s/" % (model_type, num_s)):
        os.makedirs("./model/%s/%s/" % (model_type, num_s))
    callbacks = [
        EarlyStopping(monitor='loss', patience=5, verbose=0),
        ModelCheckpoint('./model/%s/model_%s.h5' % (model_type, num_s), monitor='loss', save_best_only=True, verbose=0),
        keras.callbacks.ReduceLROnPlateau(monitor='loss', factor=0.1, patience=3, verbose=0, mode='auto', min_lr=0)
    ]
    if num_s == 2:
        loss = "binary_crossentropy"
    else:
        loss = "categorical_crossentropy"
    RMSprop = optimizers.RMSprop(lr=0.001)
    model.compile(loss=loss,
                  optimizer=RMSprop,
                  metrics=["acc"])
    model.fit(train_x, train_y,
              batch_size=256,
              callbacks=callbacks,
              epochs=50,
              verbose=0)
    return model


def train_model_ML(train_x, train_y, model_type):
    train_y = train_y[:, 0]
    model = create_model_ML(model_type)
    model.fit(train_x, train_y)
    return model


def to_result(model_type, train_x, train_y, test_x, test_y):
    if model_type in ["SVM", "RF", "NB", "KNN"]:
        model = train_model_ML(train_x, train_y, model_type)
        result_train = model.predict(train_x)
        result_test = model.predict(test_x)
    elif model_type in ["1DCNN", "LSTM"]:
        model = train_model_DL(train_x, train_y, model_type)
        result_train = np.argmax(model.predict(train_x), axis=1)
        result_test = np.argmax(model.predict(test_x), axis=1)
    acc_train = accuracy_score(train_y, result_train)
    acc_test = accuracy_score(test_y, result_test)
    f1_test = f1_score(test_y, result_test, average='weighted')
    return acc_train, acc_test, f1_test
