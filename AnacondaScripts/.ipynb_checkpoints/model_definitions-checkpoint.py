import tensorflow as tf
import  tensorflow.keras.layers as layers
import matplotlib.pyplot as plt

features=5
cars=4
sequence_length=20

##Model definitions

def MLP_Model(units):
    return tf.keras.models.Sequential([layers.InputLayer(input_shape=(sequence_length,cars*features)),
                                       layers.Flatten(),
                                       layers.Dense(units,activation="linear"),
                                       layers.Dense(features*cars,activation="linear")])

def LSTM_Model(droupout_rate, cells=3,units=32,normalization=False):
    inputs=tf.keras.Input(shape=(sequence_length,cars*features))
    x=(inputs)
    if(normalization):
        x=layers.BatchNormalization() (x)
    for index in range(cells):
        x=layers.LSTM(units,dropout=droupout_rate,return_sequences=True)(x)
        x= layers.BatchNormalization()(x)
    x=layers.LSTM(units,dropout=droupout_rate)(x)
    outputs=layers.Dense(features*cars,activation="linear")(x)
            
    return tf.keras.models.Model(inputs=inputs,outputs=outputs,name=f"LSTM_NN{cells}")

#def CNN_Model(filters=32,kernel_size=1):
#    inputs=tf.keras.Input(shape=(sequence_length,cars*features,1))
#    
#    #CNN
#    cnn_=layers.Conv1D(filters,kernel_size,padding="same",activation="linear")(inputs)
#    cnn_=layers.MaxPool1D(1)(cnn_)
#    cnn_out=layers.Flatten()(cnn_)
#    return tf.keras.models.Model(inputs=inputs,outputs=cnn_out,name=f"CNN{filters}_{kernel_size}")
    
    
def CNN_LSTM_Model(droupout_rate, units=32,filters=32,kernel_size=3,pool_size=1,normalization=False):
    
    inputs=tf.keras.Input(shape=(sequence_length,cars*features))
    x=(inputs)
    if(normalization):
        x=layers.BatchNormalization() (x)
    #CNN
    x=layers.Conv1D(filters,kernel_size,padding="same",activation="linear")(x)
    x=layers.MaxPool1D(pool_size=pool_size)(x)
    #LSTM
    x=layers.LSTM(units,dropout=droupout_rate)(x)

    outputs=layers.Dense(features*cars,activation="linear")(x)
            
    return tf.keras.models.Model(inputs=inputs,outputs=outputs,name=f"CNN{filters}_{kernel_size}LSTM{units}")
    
def LSTM_Single(dropout,cells=3, normalization=False):
    inputs=tf.keras.Input(shape=(sequence_length,features))
    x=(inputs)
    if(normalization):
        x=layers.BatchNormalization() (x)
        
    for index in range(cells-1):
        x=layers.LSTM(64,dropout=dropout,return_sequences=True)(x)
        
    x=layers.LSTM(128)(x)
    outputs=layers.Dense(features)(x)
        
    return tf.keras.models.Model(inputs=inputs,outputs=outputs,name=f"Single_LSTM_NN{cells}")   
   
class SplitLayer(layers.Layer):
    def __init__(self, carNumbers,embedding_model):
        super().__init__()
        self.carNumbers = carNumbers
        self.embedding_model = embedding_model

    def call(self, inputs):
        results=[]
        splitted= tf.split(inputs,self.carNumbers,axis=-1)
        for i in range(self.carNumbers):
            results.append( self.embedding_model(splitted[i]))
        return tf.concat(results,-1)


def get_siamese_network (size,sample_length,dropout,carNumber,cells=3, normalization=False):
    embedding_model = LSTM_Single(size,sample_length,dropout,cells, normalization)
    inputs= tf.keras.Input(shape=(sample_length,size*carNumber))
    splitLayer=SplitLayer(carNumber,embedding_model) (inputs)
    siamese_network =  tf.keras.models.Model(inputs=inputs, outputs=splitLayer)
    return siamese_network 

##Training functions 

def compile_and_fit(model, train,val, patience=5, epochs=10, learning_rate=0.01, summary=False, verbose=0,name="logs",path="logs"):
    early_stopping = tf.keras.callbacks.EarlyStopping(monitor='val_loss',
                                                    patience=patience,
                                                    mode='min')
    reduce_lr=tf.keras.callbacks.ReduceLROnPlateau(monitor='val_loss',
                                                   factor=0.5,
                                                   patience=2,
                                                   verbose=verbose,
                                                   mode='auto',
                                                   min_delta=0.01,
                                                   cooldown=5,
                                                   min_lr=1e-6
                                                  )
    tensorboard_callback = tf.keras.callbacks.TensorBoard(log_dir=f"./{path}/{name}")


    model.compile(loss=tf.losses.MeanSquaredError(),
                optimizer=tf.optimizers.Adam(learning_rate=learning_rate),
                metrics=[tf.metrics.MeanAbsoluteError(),tf.keras.metrics.Accuracy()])
    
    history = model.fit(train, epochs=epochs,
                      validation_data=val,
                      verbose=verbose,
                      callbacks=[early_stopping,reduce_lr,tensorboard_callback])
    if(summary):
        model.summary()
    
    return history

def plt_metric(history, metric, title, ax, has_valid=True,):
    ax.plot(history[metric])
    x=(len(history[metric])/2)
    y=(max(history[metric])/2)
    stringa=f"Min: {min(history[metric]): .3f}"
    if has_valid:
        ax.plot(history["val_" + metric])
        ax.legend(["train", "validation"], loc="upper left")
        mini=min(history[f'val_{metric}'] )
        stringa+=f"\nMin val: {mini:.3f}"
    ax.set_title(title)
    ax.set_ylabel(metric)
    ax.set_xlabel("epoch")
    #ax.text(x,y,stringa,  horizontalalignment='right',verticalalignment='top')
    #ax.show()
    
def Print_Train(history,modelName):
    fig,ax=plt.subplots(nrows=1, ncols=3, figsize=(24, 9))
    plt_metric(history=history.history, metric="mean_absolute_error", 
               title=f"Model Mean Absolute Error-{modelName}",ax=ax[0])
    plt_metric(history=history.history, metric="loss", title=f"Loss-{modelName}",ax=ax[1])
    plt_metric(history=history.history, metric="accuracy", title=f"ModelAccuracy-{modelName}",ax=ax[2])


def MLP_Train(train,val,learning_rate=0.01,verbose=0,epoch=10,units=32,name="logs",path="logs"):
    model=MLP_Model(units)
    history = compile_and_fit(model,train,val,epochs=epoch,learning_rate=learning_rate,verbose=verbose,name=name, path=path)
    
    Print_Train(history,"MLP")
    
    return model,history

def LSTM_Train(train,val,learning_rate=0.01,verbose=0,epoch=10, patience=5,name="logs",path="logs",
               dropout=0.2, cells=3,units=32, normalization=False):
    
    model = LSTM_Model(dropout,cells=cells,units=units, normalization=normalization)
    history = compile_and_fit(model,train,val,epochs=epoch,learning_rate=learning_rate,verbose=verbose,patience=patience,name=name,path=path)
    
    Print_Train(history,"LSTM")
    
    return model,history

def CNN_LSTM_Train(train,val,learning_rate=0.01,verbose=0,epoch=10, patience=5,name="logs",path="logs",
                   dropout=0.2, kernel_size=3,units=32, filters=32,pool_size=1, normalization=False):
    
    model = CNN_LSTM_Model(dropout,kernel_size=kernel_size,units=units,filters=filters, normalization=normalization,pool_size=pool_size)
    history = compile_and_fit(model,train,val,epochs=epoch,learning_rate=learning_rate,verbose=verbose,patience=patience,name=name,path=path)
    
    Print_Train(history,"CNN_LSTM")
    
    return model,history

def LSTM_Siamese_Train(train,val,learning_rate=0.01,verbose=0,epoch=10, dropout=0.2, cells=3):
    
    model = get_siamese_network(dropout,cells=cells)
    history = compile_and_fit(model,train,val,epochs=epoch,learning_rate=learning_rate,verbose=verbose)
    
    Print_Train(history,"LSTM Siamese")
    
    return model,history