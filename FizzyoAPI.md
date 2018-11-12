# Usage

After importing the Fizzyo Framework you will have a new Folder called Fizzyo located in your Assets directory.
This Folder contains everything you will need to use the Fizzyo device and services, you should not need to modify anything in the folder it's self as the framework is accessed through singletons.

## Framework

The Fizzyo Framework is accessed though singletons. This means that one FizzyoFramework class is used throughout the lifetime of your application and can be accessed from any script using FizzyoFramework.Instance

# Fizzyo Classes

This example <https://github.com/Fizzyo/Creating-Games-for-Fizzyo/tree/master/Sample%20Games/Fizzyo-Unity-Example> includes a test harness and test data that allows you to load and playback breath data saved from a fizzyo device. There is a selection of good and bad breadths available at <https://github.com/Fizzyo/Creating-Games-for-Fizzyo/tree/master/Sample%20Games/Fizzyo-Unity-Example/Assets/Data>

To use this a singleton class is provided FizzyoDevice.Instance() that can be used at any point in your code if FizzyoDevice.cs is present in your project.

By default FizzyoDevice plays back pre-recorded data but can also be used to gather data directly from the device if the bool useRecordedData is set to false.

This can be done through the editor or programmatically in your code.

This allows you to program your game completely against pre-recorded pressure values if desired and switched over to live values at a later stage.

``` C#

FizzyoDevice.cs

/* (float) Return the current pressure value, either from the device or streamed from a log file.
*   range: -1.0f - 1.0f
*   comment: if useRecordedData is set pressure data is streamed from the specified data file instead of the device.
*/
Fizzyo.FizzyoDevice.Instance().Pressure();

/* (bool) Return if the fizzyo device button is pressed */
Fizzyo.FizzyoDevice.Instance().ButtonDown();

```

## Example

This is a example of a minimal game that scores number of good breaths during playing

``` csharp

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