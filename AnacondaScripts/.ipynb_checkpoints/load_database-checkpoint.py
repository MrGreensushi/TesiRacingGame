import tensorflow as tf
import numpy as np

cars=4
features=5
sequence_length=20
onlyOneCar=False

def retrieve_db(base_path,number,sample_len,oneCar):
    global sequence_length
    global onlyOneCar
    
    sequence_length=sample_len
    onlyOneCar=oneCar
    return read_from_multiple_files(base_path,number)

def read_from_multiple_files(base_path,number):
    names=evaluate_names(base_path,number)
    con_train,con_val,con_test, players = multiple_race_oneFile(names.pop(0))
    for name in names:
        print(name)
        train,val,test,player=multiple_race_oneFile(name)
        con_train=con_train.concatenate(train)
        con_val=con_val.concatenate(val)
        con_test=con_test.concatenate(test)
        #print("To Add ",train.cardinality(), " Added: ",con_train.cardinality())
    return con_train,con_val,con_test, players 

def multiple_race_oneFile(f):
    first=True
    final_train=0
    final_val=0
    final_test=0
    final_players=0
    with open(f) as f:
        lines=f.readlines()
        races=split_into_race(lines)
        for race in races:
            #print("intera gara\n",race)
            players,infos=leggi_file(race)
            train,val,test,player=create_timeseries_dataset(players,infos)
            if(first):
                fist=False
                final_train=train
                final_val=val
                final_test=test
                final_players=player
            else:
                final_train=final_train.concatenate(train)
                final_test=final_test.concatenate(test)
                final_val=final_val.concatenate(val)
    
    return  final_train,final_val,final_test,final_players

def create_timeseries_dataset(players, infos): 
    if(onlyOneCar==False):
        return timeseries(players,infos)
    
    #Only one car per race:
    reshaped=np.reshape(infos,(len(infos),cars,features))
    first=True
    final_d1=0
    final_d2=0
    final_d3=0
    for index in range(cars):
        single_car=reshaped[:,index,:]
        #print(np.shape(single_car))
        d1,d2,d3,players=timeseries(players,single_car)
        if(first):
            final_d1=d1
            final_d2=d2
            final_d3=d3
            first=False
        else:
            final_d1=final_d1.concatenate(d1)
            final_d2=final_d2.concatenate(d2)
            final_d3=final_d3.concatenate(d3)
    
    return final_d1,final_d2,final_d3,players

def timeseries(players,infos):
    size_datasets=[int(0.6*len(infos)),
                   int(0.8*len(infos)),
                   len(infos)]
    
    dataset=[0,0,0]
    for index in range(len(size_datasets)):
        if(index >0):
            splitted_infos=infos[size_datasets[index-1]:size_datasets[index]]
        else:
            splitted_infos=infos[0:size_datasets[index]]
        #The model uses the last sample_lenght datas to predict the next one
        input_data = splitted_infos[:-1]
        targets = splitted_infos[sequence_length:]
        #print(len(input_data))
        dataset[index]=tf.keras.utils.timeseries_dataset_from_array(input_data,targets,
                                                                    sequence_length=sequence_length,
                                                                    batch_size=256)
    
        for batch in dataset[index]:
            inputs, targets = batch
            assert np.array_equal(inputs[0], tf.constant(splitted_infos[:sequence_length]))  # First sequence: steps [0-4]
            # Corresponding target: step 5
            assert np.array_equal(targets[0],  tf.constant(splitted_infos[sequence_length]))
            #print(tf.shape(inputs),tf.shape(targets))
            break

        
    return dataset[0],dataset[1],dataset[2],players



def create_dictionary(car,players):
    splitted=car.split(';')
    splitted[0]=str(players.index(splitted[0]))
    splitted=list(map(to_float,splitted))
    return splitted

def to_float(word):
    replaced=word.replace(',','.')
    return float(replaced)

def split_into_race(races):  
    i =0
    final=[]
    totRet=[]
    for line in races:
        if(line[0]=='&'):
            if((len(totRet)*0.2)>sequence_length):
                final.append(totRet)
            totRet=[]
            line=line[2:]
            #print(line)
        totRet.append(line)
    if ((len(totRet)*0.2)>sequence_length) :
        final.append(totRet)
    
    return final
        

def leggi_file(lines):
    infos=[]
    players=[]
    for line in lines:
        #remove \n
        line = line[:len(line)-3]
        #split along || wich indicate a new car
        cars=line.split('||')
        
        #create players array only once!
        if(len(players)==0):
            for car in cars:
                players.append(car.split(';')[0])
        line_info=[]    
        for car in cars:
            line_info.append(create_dictionary(car,players))
        
        line_info.sort(key=lambda x: x[0])
        line_info=list(map(lambda x: x[1:],line_info))
        merged_line=np.reshape(line_info,(len(line_info)*len(line_info[0])))
        infos.append(merged_line)
  
    return players,infos





def evaluate_names(path,number):
    array=[]
    for i in range(number):
        val=path
        if i>0:
            val +=" - Copia"
        if i>1:
            val +=f" ({i})"
        val +=".txt"
        array.append(val)
    #print(array)
    return array
            
    
def dataset_len(dataset):
    n=list(dataset.unbatch().as_numpy_iterator())
    return len(n);

def append_on_file(start,end,numberOfFiles):
    toAppend=[]
    names=evaluate_names(start,numberOfFiles)
    for name in names:
        
        with open(name) as f:
            lines=f.readlines()
            toAppend.extend(lines)
            toAppend.append("_")
        
    with open(end,'w') as f:
        f.writelines(toAppend)
        
def DivideIntoRaces(start,end):
    allrace_infos=[]
    with open(start) as f:
        lines=f.readlines()
        races=split_into_race(lines)
        for race in races:
            players, infos= leggi_file(race)
            allrace_infos.extend(infos)
    
    with open(end,'w') as f:
         f.writelines(allrace_infos)
            
            
def rewrite_file_in_order(path,newFile):
    with open(path) as f:
        lines=f.readlines()
        for line in lines:
            #remove \n
            line = line[:len(line)-3]
            #split along || wich indicate a new car
            cars=line.split(';')
            
            line_info=[cars[:6],cars[6:12],cars[12,18],cars[18:]]    
                   
            line_info.sort(key=lambda x: x[0] if x[0][0]!='_' else x[0][2])
            string_line=np.reshape(line_info,(len(line_info)*len(line_info[0])))

  
