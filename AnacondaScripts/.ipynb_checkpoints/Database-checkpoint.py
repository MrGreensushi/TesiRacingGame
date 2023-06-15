import pandas as pd
import numpy as np
import tensorflow as tf

SEQUENCE_LENGTH=20
ONLY_ONE_CAR=False
CARS=4
FEATURES=5

def load(path):
    #read file
    df_gara=pd.read_csv(r"C:\Users\dansp\OneDrive\Desktop\Tesi\Logs\physics_ordered.txt", sep=";", header=None , decimal=',')
    #create new column called race
    df_gara["RACE"]=0
    # fill race column based on the cumulative sum of rows  containing a name columns starting with '_'
    #idx_gara stores each row wich starts with '_'
    idx_gara=[]
    for i in range(0,24,6):
        idx=(df_gara[df_gara[i].str.startswith("_")].index)
        df_gara.loc[idx,i]=df_gara.loc[idx,i].str.replace("_ ","")#replace name without the _
        idx_gara.extend(idx)

    df_gara.loc[idx_gara,"RACE"]=1
    df_gara.RACE=df_gara.RACE.cumsum()
    #Create new column Length wich specifiens the total length of a race
    df_gara["LENGTH"]=df_gara.groupby("RACE")[0].transform("count")
    #if race is lewer then a minimum then it is discarded
    df_races=df_gara.query(f"LENGTH > {SEQUENCE_LENGTH/0.25}").reset_index(drop=True)

    
    if(ONLY_ONE_CAR):
        return single_car_db(df_races)
    
    #drop columns with names and rearrenges names
    df_races=df_races.drop([0,6,12,18],axis=1)
    names=np.array(range(20))
    names=np.append(names,["RACE","LENGTH"])
    df_races.columns=names

    df_train_vector, df_val_vector,df_test_vector=split_train_validation_test(df_races,"RACE")

    dt_train= concatenate_vector(df_train_vector)
    dt_val= concatenate_vector(df_val_vector)
    dt_test= concatenate_vector(df_test_vector)
    
    return dt_train,dt_val,dt_test
     
def single_car_db(df):
    lst_df=[]
    for i in range(0,24,6):
        new_arr=df.loc[:,[i,i+1,i+2,i+3,i+4,i+5,"RACE"]].values
        lst_df.extend(new_arr)
    df_gara_stacked=pd.DataFrame(lst_df, columns=["Player","X","Z","VEL_X","VEL_Z","ROT","RACE"])
    
    df_train_vector, df_val_vector,df_test_vector=split_train_validation_test(df_gara_stacked,["Player","RACE"])
    #each df_ contains an array of datasets so each elements must be concatenated to retrieve final db
    dt_train= concatenate_vector(df_train_vector.to_list())
    dt_val= concatenate_vector(df_val_vector.to_list())
    dt_test= concatenate_vector(df_test_vector.to_list())
    
    return dt_train,dt_val,dt_test

def get_split(x,first,second):
    first =int(x.shape[0]*first)
    second = int(x.shape[0]*second)
    if ONLY_ONE_CAR:
        k=np.array(x)[first:second,1:(FEATURES+1)]
    else:
        k=np.array(x)[first:second,:CARS*FEATURES]
    return timeseries_new(k)
    
def split_train_validation_test(df,group_col,train_split=0.5,val_split=0.25,test_split=0.25):
    val_split +=train_split
    
    if val_split >1:
        raise ValueError(
            f"Train + Validation split cannot be higher tan 1 given {val_split}"
        )
     
    #group by race (and player name for single car) and create a new array containing foreach race a dataset
    df_train=( df.groupby(group_col).apply(get_split, first = 0, second= train_split))
    df_val=( df.groupby(group_col).apply(get_split, first = train_split, second= val_split))
    df_test=( df.groupby(group_col).apply(get_split, first = val_split, second= 1))
    
    return df_train, df_val,df_test

def timeseries_new(infos):

    splitted_infos=infos.astype('float32')
    input_data = splitted_infos[:-1]
    targets = splitted_infos[SEQUENCE_LENGTH:]
        #print(len(input_data))
    dataset=tf.keras.utils.timeseries_dataset_from_array(input_data,targets,
                                                         sequence_length=SEQUENCE_LENGTH,
                                                         batch_size=256)
    
    for batch in dataset:
        inputs, targets = batch
        assert np.array_equal(inputs[0], tf.constant(splitted_infos[:SEQUENCE_LENGTH]))  # First sequence: steps [0-4]
        # Corresponding target: step 5
        assert np.array_equal(targets[0],  tf.constant(splitted_infos[SEQUENCE_LENGTH]))
        #print(tf.shape(inputs),tf.shape(targets))
        break

        
    return dataset

def concatenate_vector(vector):
    dt=vector.pop(0)
    for i in vector:
        dt=dt.concatenate(i)
    return dt

