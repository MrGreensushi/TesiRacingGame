import tensorflow as tf
import load_database

sequence_length=20

        
def split_into_races(lines):
    i =0
    final=[]
    totRet=[]
    for line in lines:
        if(line[0]=='&'):
            if((len(totRet)*0.2)>sequence_length):
                yield totRet
                i+=1
            totRet=[]
            line=line[2:]
            
        totRet.append(line)
        
    if((len(totRet)*0.2)>sequence_length):
        yield totRet

def RaceGenerator(file_name_generator):  
    with open(file_name) as f:
        lines=f.readlines()
        races=split_into_races(lines)
        for race in races:
            yield race
                
def TimeSeriesFromRace(infos):
    input_data = infos[:-sequence_length]
    targets = infos[sequence_length:]
    #dataset=tf.keras.utils.timeseries_dataset_from_array(input_data,targets,
    #                                                     sequence_length=sequence_length,
    #                                                     batch_size=256)
    #return dataset
    for i in range(len(input_data)):
        yield [input_data[i:i+sequence_length],targets[i]]
    
def Generator(file_path,files_number):
    race_generator=RaceGenerator(file_path)
    #print(next(race_generator)[0])
    for race in race_generator:
        players, infos=load_database.leggi_file(race)
        time_series= TimeSeriesFromRace(infos)
        for output in time_series:
            yield output
    
    
class DataGenerator(tf.keras.utils.Sequence):
    def __init__(self,batch_size,generator_function,sequence_length,totFiles,path):
        self.batch_size=batch_size
        self.generator_function=generator_function
        self.sequence_length=sequence_length
        self.totFiles=totFiles
        self.path=path
        self.on_epoch_end()
        #self.generator=generator_function(sequence_length,path,totFiles)
        
    def __getitem__(self,index):
        X=[]
        Y=[]
        for i in range(self.batch_size):
            while True:
                x,y=next(self.generator)
                x_shape=np.shape(x)
                if x_shape[0]==x_shape[1]:
                    break
            X.append(x)
            Y.append(y)
            
        #print(np.shape(X))    
        tensor_x=tf.constant(X)
        tensor_y=tf.constant(Y)
        return tensor_x,tensor_y
    
    def __len__(self):
        return 4000
    
    def on_epoch_end(self):
        self.generator=Generator(self.sequence_length,self.path,self.totFiles)
  