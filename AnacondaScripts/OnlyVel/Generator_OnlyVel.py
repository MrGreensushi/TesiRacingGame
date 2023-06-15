import pandas as pd
import numpy as np
import tensorflow as tf


SEQUENCE_LENGTH=20
ONLY_ONE_CAR=True
CARS=4
FEATURES=5
DISCARD=2

def normalize_df(df,minimum,maximum):
    normalized=(df-minimum)/(maximum-minimum)
    normalized["RACE"]=df["RACE"]
    return normalized

      
    
def single_care_dataframe(path):
    df_gara=pd.read_csv(path, sep=";", header=None , decimal=',',names=["Player", "X", "Z", "VEL_X","VEL_Z","ROT","TIME"])
    #create new column called race
    df_gara["RACE"]=0
    # fill race column based on the cumulative sum of rows starting with '_'
    #idx_gara stores each row wich starts with '_'
    idx_gara=(df_gara[df_gara["Player"].str.startswith("_")].index)
    df_gara.loc[idx_gara,"Player"]=df_gara.loc[idx_gara,"Player"].str.replace("_","")#replace name without the _

    df_gara.loc[idx_gara,"RACE"]=1
    df_gara.RACE=df_gara.RACE.cumsum()
    #Create new column Length wich specifiens the total length of a race
    df_gara["LENGTH"]=df_gara.groupby("RACE")["Player"].transform("count")
    #if race is lewer then a minimum then it is discarded
    df_races=df_gara.query(f"LENGTH > {SEQUENCE_LENGTH/0.25}").reset_index(drop=True)
    df_races.drop(["Player","LENGTH"],axis=1,inplace=True)

    #Since
    dfs=[]
    for i in range(DISCARD+1):
        df_races["GROUP"]=i
        temp=df_races.iloc[i::DISCARD+1]
        temp.reset_index(drop=True,inplace=True)
        dfs.append(temp)
    
    return dfs

def subtraction_columns(df):
    df_copy=df.shift(1,fill_value=0)
    cols = df.columns.difference(['RACE','GROUP',"X","Z"])
    df[cols] = df[cols].sub(df_copy[cols])
    df["ROT"]=(df["ROT"]+180)%360-180
    df.iloc[0,:-1]=0
    return df

def get_split(x,first,second):
    first =int(x.shape[0]*first)
    second = int(x.shape[0]*second) 
    return x[first:second]
    
def split_train_validation_test(df,group_col,train_split=0.5,val_split=0.25,test_split=0.25):
    val_split +=train_split
    
    if val_split >1:
        raise ValueError(
            f"Train + Validation split cannot be higher tan 1 given {val_split}"
        )
     
    #group by race (and player name for single car) and create a new array containing foreach race a dataset
    df_train= df.groupby(group_col,group_keys=False).apply(get_split, first = 0, second= train_split)
    df_val= df.groupby(group_col,group_keys=False).apply(get_split, first = train_split, second= val_split)
    df_test= df.groupby(group_col,group_keys=False).apply(get_split, first = val_split, second= 1)
    
    
    #since each race was plittend into train,val and test the result of previous operation is an array containing the data 
    #foreach race, therefore to have the end dataframe we must concatenate each element
    df_train=recreate_dataframe(df_train)
    df_val=recreate_dataframe(df_val)
    df_test=recreate_dataframe(df_test)
    
    return df_train, df_val,df_test

def recreate_dataframe(series):
    columns=["X","Z","DIFF_VEL_X","DIFF_VEL_Z","DIFF_ROT","TIME","RACE","GROUP"] 
    series.columns=columns
    df=series.reset_index(drop=True)
    #v#alues=series.values
    #df=pd.DataFrame(values[0],columns=columns)
    #
#
    #for serie in values[1:]:
    #    df= pd.concat([df, pd.DataFrame(serie,columns=columns)],ignore_index=True)
    return df

def batch_generator(df):
    
    #crea un nuovo dataframe con sequence_length elementi per un numero di volte pari al batch
    dropped_df=df.drop(["TIME","RACE","GROUP"],axis=1).reset_index(drop=True)
    target_df=dropped_df.drop(["X","Z"],axis=1).reset_index(drop=True)
    for i in range(len(dropped_df)-SEQUENCE_LENGTH):
        inputs=np.array(dropped_df.loc[i:SEQUENCE_LENGTH-1+i,:].values)
        targets=target_df.iloc[SEQUENCE_LENGTH+i,:].values
        yield inputs,targets
  
def Generator(df):
    grouped=df.groupby(["RACE","GROUP"],group_keys=False).apply(batch_generator)
    for group in grouped:
        for single in group:
            yield single
            
class DataGenerator(tf.keras.utils.Sequence):
    def __init__(self,batch_size,df,max_batch):
        self.batch_size=batch_size
        self.df=df
        self.df_length=len(df.index)-(df["RACE"].nunique()*SEQUENCE_LENGTH*CARS)
        self.max_batch=max_batch            
        print(f'Length: {len(df.index)} races: {df["RACE"].nunique()} n batches: {self.df_length} / {batch_size}')
        self.on_epoch_end()
        #self.generator=generator_function(sequence_length,path,totFiles)
        
    def __getitem__(self,index):
        X=[]
        Y=[]
        for i in range(self.batch_size):
            #while True:
            #    x,y=next(self.generator)
            #    x_shape=np.shape(x)
            #    if x_shape[0]==x_shape[1]:
            #        break
            x,y=next(self.generator)
            X.append(x)
            Y.append(y)
            
        #print(np.shape(X))    
        tensor_x=tf.constant(X)
        tensor_y=tf.constant(Y)
        return tensor_x,tensor_y
    
    def __len__(self):
        value=int(self.df_length/self.batch_size)-1
        if value>self.max_batch:
            value=self.max_batch
        return value
    
    def on_epoch_end(self):
        self.generator=Generator(self.df)