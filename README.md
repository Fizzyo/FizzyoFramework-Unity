# Fizzyo Framework
A Unity package for Fizzyo game development .
You can view more detailed documentation in the official documentation [website](http://dev.fizzyo-ucl.co.uk/).

## Installation
Download the latest Fizzyo Framework unity package from [here](https://github.com/Fizzyo/FizzyoFramework-Unity/releases)

- From Unity **select Assets** > **Import Package** > **Custom Package**
- Select the **FizzyoFramework.unitypackage** that you have downloaded. 
- In the window that pops up select **ok** to import the package.
- Add all scenes from **Assets/Fizzyo/Scenes** to your project build settings by opening them and going to **File** > **Build Settings** > **Add Open Scenes**.

*the Unity package is also able to be cloned directly from GitHub and the fizzyo folder placed in your projects Assets folder.*

## Usage
After importing the Fizzyo Framework you will have a new Folder called Fizzyo located in your Assets directory. 
This Folder contatins everything you will need to use the Fizzyo device and services, you should not need to modify anything in the folder it's self as the framework is accessed through singletons.


## Framework 
The Fizzyo Framework is accessed though singletons. This means that one FizzyoFramework class is used throughout the lifetime of your application and can be accessed from any script using FizzyoFrameework.Instance 

## Example 
This is a example of a minimal game that scores number of good breaths during playing
```csharp
//use Fizzyo namespace
using Fizzyo;
int score = 0;
int breaths = 5;
int breathCount = 0;

private string PerfectBreathUID = "";

void Start(){
    FizzyoFramework.Instance.Recogniser.BreathStarted += OnBreathStarted;
    FizzyoFramework.Instance.Recogniser.BreathComplete += OnBreathEnded;
}

void Update(){
	//get exhale pressure between -1 and 1
	float pressure = FizzyoFramework.Instance.Device.Pressure();
     Debug.Log("Exhale pressure: " + pressure);
}

void OnBreathStarted(object sender)
{
     Debug.Log("Breath started");
}


void OnBreathEnded(object sender, ExhalationCompleteEventArgs e)
{
	breathCount++;
    
    if(e.BreathQuality() >= 2){
		score++;
	}

    if(e.BreathQuality() >= 4){
    	FizzyoFramework.Instance.Achievements.UnlockAchievement(PerfectBreathUID);
    }
    
    if(breathCount > breaths){
     	FizzyoFramework.Instance.Achievements.PostScore(score);
        Application.Quit();
     }

}
```

### Pressure data 

Returns the pressure returned by the Fizzyo device as a user exhales through it. 
The value returned are calibrated. A value of 1.0 corresponds to the maximum pressure someone produced when calibrating and 0.0 maps to the pressure when not breathing through the device. a smaller negative value is produced if the user inhales through the device and a number above 1.0 can be produced if the user exhales harder than their calibration breath.
```csharp
FizzyoFramework.Instance.Device.Pressure()
```

### Fizzyo Breath Recognizer
Events can be used to listen for the start or end of a breath.

```csharp
    FizzyoFramework.Instance.Recogniser.BreathStarted += OnBreathStarted;
    FizzyoFramework.Instance.Recogniser.BreathComplete += OnBreathEnded;

    void OnBreathStarted(object sender)
    {
        
    }

    void OnBreathEnded(object sender, ExhalationCompleteEventArgs e)
    {
        
    }
```
#### ExhalationCompleteEventArgs
The event will fire at the end of an exhaled breath and provide information for:
	a) BreathLength
	b) BreathCount
	c) ExhaledVolume
	d) IsBreathFull

### Achievements 
Achievements will need to be first registered in the (Fizzyo Portal)[] using your developers account and then can be unlocked for each user by calling. 

```csharp
FizzyoFramework.Instance.Achievements.UnlockAchievement(achievementUID);
```
### High score
Scores achieved in your game can be posted to the score board using the following.
```csharp
FizzyoFramework.Instance.Achievements.PostScore((int)score);
```
### Login
Users will need to login to the fizzyo system in order to submit analytics, achievements and leader boards. A login screen is automatically displayed in the FizzyoFramework unless disabled through the FizzyoFramework editor properties. 

*Whilst playing through the editor the Fizzyo Framework automatically logs in to a test user specified in the FizzyoFramework.cs properties.
If you would like to show a login window at a specific point in your game you can call.
```csharp
FizzyoFramework.Instance.User.Login();
```
## Creating a Game for Fizzyo

### Registering your game 

You first need to recieve and invitaion code from the Project Fizzyo team - please contact Tim Kuzhagaliyev at tim.kuzhagaliyev.15 (at) ucl.ac.uk

Then you need to register at http://fizzyo-ucl.co.uk 

Input your code here and register with tour Microsoft Services Account

### Logging in 

Once you have registered click login and you will be asked for Microsoft Service Account which you used at the registration

### The Games Dashboard

You will see your games dashboard (which will be initally emppty)
You can see, add, edit and delete games or log out 

### Adding Games 

Clicking NEW prompts for a new windows to pop up, with the option of adding your new games details
Fields cannot be left Empty
- Game Name
- Game Version
- Unity Version

### Editing Games

Double click a editable field this will and edit windows to pop up

### Adding Achievements or Hight Scores

Go to Game Achievements or High Scores, Click the relevant button
To go back to the game dashboard or to the high scores click on the relevant button
The Achievement Dashboard - All actions performed on games can be preformed on achievements
The High Score Dashboard - At the moment you can only view the top 20 scores in your game there 

## Fizzyo Game UID
In order to use game achievements, high scores and analytics your game will need to be registed on the (Fizzyo Portal) see https://github.com/Fizzyo/Creating-Games-for-Fizzyo for further details on how to build and share your game.


### Deleting Games

Select the games you want to delete and press delete
Be suire you have selected the right game and confirm

### Configuration. 
The FizzyoFramework script can be attached to a Unity GameObject to configure though the Unity Editor.

## Testing your game 

This example https://github.com/Fizzyo/Creating-Games-for-Fizzyo/tree/master/Sample%20Games/Fizzyo-Unity-Example includes a test harness and test data that allows you to load and playback breath data saved from a fizzyo device. There is a selection of good and bad breadths available at https://github.com/Fizzyo/Creating-Games-for-Fizzyo/tree/master/Sample%20Games/Fizzyo-Unity-Example/Assets/Data 

To use this a singleton class is provided FizzyoDevice.Instance() that can be used at any point in your code if FizzyoDevice.cs is present in your project.

By default FizzyoDevice plays back pre-recorded data but can also be used to gather data directly from the device if the bool useRecordedData is set to false.
This can be done through the editor or programmatically in your code.

This allows you to program your game completely against pre-recoreded pressure values if desired and switched over to live values at a later stage.

```
FizzyoDevice.cs

/* (float) Return the current pressure value, either from the device or streamed from a log file.
*   range: -1.0f - 1.0f
*   comment: if useRecordedData is set pressure data is streamed from the specified data file instead of the device.
*/
Fizzyo.FizzyoDevice.Instance().Pressure();


/* (bool) Return if the fizzyo device button is pressed */
Fizzyo.FizzyoDevice.Instance().ButtonDown();

```

## Building UWP application 

### Deployment: 

### Export game from Unity as a UWP app and make sure the following is added to the Package.appxmanifest file: 

At present Unity doesnt allows you to specific VID & PID's so to you need to have to manuually add the following to Package.appxmanifest after exporting to ensure the game will support the Fizzyo Device. See https://docs.microsoft.com/en-gb/windows/uwp/packaging/packaging-uwp-apps


```
<Capabilities> 
<Capability Name="internetClient" /> 
<uap:Capability Name="userAccountInformation" /> 
<DeviceCapability Name="bluetooth" /> 
<DeviceCapability Name="humaninterfacedevice"> 
<Device Id="any"> 
<Function Type="usage:0001 0004" /> 
<Function Type="usage:0001 0005" /> 
</Device> 
</DeviceCapability> 
</Capabilities> 
```
