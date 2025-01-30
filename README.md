# TesiRacingGame
The project contains two folders: AnacondaScripts and RacingPrototype.

## AnacondaScripts
This folder contains all notebooks, Python scripts, and the results obtained from various training attempts of the predictive AI.
Specifically, multiple configurations were tested before arriving at the final configuration found in the OnlyVel folder.

### OnlyVel
In this configuration, the predicted value corresponds only to the car’s rotation and velocity along the x-y axis.
The Notebook `OnlyVelocity.ipynb` shows the code used for training. To reuse it, modify the `PATH` value inside the notebook to match the database location.
Within the Notebook, hyperparameters can be freely adjusted, except for the `FEATURES` and `FEATURES_PREDICTED` values.

## RacingPrototype
This folder contains the Unity game project. All the scripts used can be found in the `Assets/Scripts` folder.

### Scripts
The MPAI-SPG scripts are stored in the `MPAI architecture/Online` folder. 

#### Manager_MPAI
`Manager_MPAI`  controls for which players the prediction system is active and coordinates the overall process.
For the coordination, Manager_MPAI uses a counter: based on its value, which increments at a constant frequency, it either updates the input and starts the neural network prediction process or sends the prediction to the game manager.
Manager_MPAI requires a reference in the Unity scene to the following elements: `Dispatcher`, `Collector`, `Physic_Engine`, the `Discard` value used during neural network training, the `Neural Network` itself, and the `Ghost_car` GameObject to be instantiated.

#### Dispatcher
The `Dispatcher` represents the **Game State Demultiplexer**. When invoked by the `Manager`, it retrieves the necessary game state information for the various AI engines and forwards it to them. In this case, only the AI-Physics Engine has been implemented.

#### Physic_Engine & Ghost_Car
`Physic_Engine` and `Ghost_Car` correspond to the **AI-Physics Engine**. 
Since the number of players in a game can vary, it was impossible to train a single neural network to simultaneously predict the game state for every player.
Therefore each player is assigned a `Ghost Car`, an invisible vehicle that updates its game state by copying the reference player car's state.
Each `Ghost Car` is equipped with an instance of the predictive neural network. 
When SPG is active for a specific player, the `Dispatcher` sends the necessary data to the `Physic_Engine`, which updates the input matrix of the `Ghost Car' neural network and starts the prediction process.

The `Ghost_Car` also manages the authority over the player's car control. If the authority is assigned to the server, the server discards the player's input and uses the predictions to control the car. If the authority is given to the client, predictions are discarded, and the player's input is used instead.

The authority switch is used to control the SPG "operation mode." In all cases, the prediction updates the ghost car’s game state, but if in **Testing mode**, the authority remains with the client, meaning the player's car will not be affected by the prediction. In **Real Case mode**, both the ghost car and the player's car state are updated based on the prediction.

#### Collector
The `Collector` script is equivalent to the **Predicted Game State Multiplexer**. When called by the `Manager_MPAI`, `Collector` retrieves the neural network output for the ghost cars with SPG enabled.
Then, the retrieved predictions update the associated ghost car's game state.


### The Game
To test the project, create a build with the three main scenes in Unity, which are located in the `"Scenes/end"` folder: `Offline`, `Room`, and `Player Testing`.

The `offline` and `Room` scenes are used to create a lobby where multiple players can join, while the `Player Testing` scene is where the actual game takes place.

Once the build is created, execute the `startServerandMultipleClients.bat` file located in the root folder of the GitHub project.  
First, modify the second line of the `.bat` file so that the `cd` command points to the build folder of the game.  
Additionally, modify the `.bat` file to control the following parameters of the game:  

- `-logOutput=<path>`: In the specified `<path>`, a text file is generated with the predictions made during the game.
- `-percentageSPGPlayers <integer>`: Specifies the percentage of players for whom the prediction system can be activated.  
- `-percentageActiveSPG <integer>`: Indicates how many players have the prediction system active simultaneously.  
- `-experimentDuration <integer>`: Number of seconds the game will last. Once the time expires, the game will close automatically.  
- `-pythonDirectory=<path>`: Here, you need to specify the directory where Python is located. (Optional)  
- `-pythonScriptPath=<path>`: This field specifies the `.py` file to run alongside the experiment. (Optional)  

Once the `.bat` file is runned, the terminal will ask to specify the following parameters:  
1. The number of players to instantiate for the game.  
2. The duration (in milliseconds) for which the SPG prediction system should remain continuously active for a player.  
3. The interval (in milliseconds) at which the SPG prediction system should be activated for a player.  

At this point, the server will start, and the players will automatically connect to the game. The players are controlled by an AI and cannot collide with each other.  
All clients run on the same device as the server but without the graphical interface to reduce performance issues.  

To close al l the clients and the server prematurely, run the `deleteAll.bat` file.  

## Database
The database used for the training of the SPG prediction system, can be found at the following link
https://drive.google.com/file/d/1qIOonluv-hw7CXuOxRj1HOsOEtd2GRIy/view?usp=sharing

