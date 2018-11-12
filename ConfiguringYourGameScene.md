![](/Images/FizzyoFrameworkLogo.png)

# Configuring the game scene

When you start building your project, there are multiple things to consider including menus, credits and achievements / high scores screens.

However, in your main game scene, you need to ensure the Fizzyo Framework is registered and active to enable you to read pressures and button presses from the device.

Adding this to your scene is easy, once you've configured your project.

## Configuring the Fizzyo project

As indicated in the [Installation instructions](/InstallingFizzyoFramework.md), you should have configured the main configuration profile for the Fizzyo Project in your scene, to do this:

- Go to the **Fizzyo** Folder
- Select **FizzyoConfigurationProfile** file, which can be found in the "**Assets/Fizzyo**" folder.

![](/Images/FizzyoConfigurationProfile.png)

- Insert your Game ID and Game Secret from the [Fizzyo UCL site](http://fizzyo-ucl.co.uk)

## Adding the Framework in to your game scene

> **Note**, you don't need to add the Framework to your menu scenes, just your initial "Game" scene.

The Fizzyo Framework is a Singleton in your game scene, meaning there can only ever be one.  This is initialised in the Calibration scene using your configuration profile, but you should ensure your main game screen has one as well.

> **Note** you should only add the Fizzyo Framework to your main game screen.  If you game has multiple screens, then simply add it to the first.  The Framework will remain active in your project once it is activated.

TO add the FizzyoFramework to your game scene, simply:

1. Create an "Empty Game Object" (by right clicking in the Scene Hierarchy and selecting Create Empty)
2. Name this new object "FizzyoFramework"
3. Add the "FizzyoFramework" script to the new GameObject, either by using the "Add Component" button and searching for the "Fizzyo Framework" or dragging the script to the object.
4. Drag your "FizzyoConfigurationProfile" asset to the "Fizzyo Configuration Profile" property on the GameObject (as shown below)

![](/Images/FizzyoFrameworkGameObject.png)

## Accessing the Fizzyo API in your game

Accessing the Fizzyo API to read the pressure values is quick and easy with the Fizzyo Framework in the scene.

### Accessing current Breath Pressure

To access the current breath pressure from the device, simply call:

``` csharp

/* (float) Return the current pressure value, either from the device or streamed from a log file.
*   range: -1.0f - 1.0f
*   comment: if useRecordedData is set pressure data is streamed from the specified data file instead of the device.
*/
Fizzyo.FizzyoDevice.Instance().Pressure();
```

### Accessing the device button

To access when the button is pressed down on the device, simply check on update:

``` csharp
/* (bool) Return if the fizzyo device button is pressed */
Fizzyo.FizzyoDevice.Instance().ButtonDown();
```

Check the sample applications for ways to integrate with the Fizzyo API and also track Sessions / Breaths per session.

> For more details on the Fizzyo API and the useful Breath Analyser class, check the [Fizzyo API](/FizzyoAPI.md) docs


## Next Steps

* [Building your Project for UWP](/BuildingYourProject.md)