![](/Images/FizzyoFrameworkLogo.png)

# Installation

Once you have [Registered your game](/RegisteringyourGame.md) on the Fizzyo Hub, you can download the latest Fizzyo Framework unity package from [here](https://github.com/Fizzyo/FizzyoFramework-Unity/releases)

Once downloaded, importing the asset in to your project is simple:

- Start your new project in Unity (or open your existing solution)
- From Unity **select Assets** > **Import Package** > **Custom Package**
- Select the **FizzyoFramework unitypackage** that you have downloaded.
- In the window that pops up select **ok** to import the package.



## Configure your Fizzyo Settings

After you have imported the FizzyoFramework asset, you first need to configure the Fizzyo Framework for your project using the Game ID and Game Secret codes from the [Fizzyo UCL site](http://fizzyo-ucl.co.uk).  

To do this:

- Go to the Fizzyo Folder
- Select **FizzyoConfigurationProfile** file, which can be found in the "**Assets/Fizzyo**" folder.

![](/Images/FizzyoConfigurationProfile.png)

- Insert your Game ID and Game Secret from the [Fizzyo UCL site](http://fizzyo-ucl.co.uk)

## Setup your scenes and Unity settings

With Fizzyo configured, you then need to update your project settings for Windows 10 and add the Fizzyo calibration scene to your project.

- Open the **Build Settings Window** using **File** > **Build Settings** from the Unity editor menu.
- Add the **Calibration** scene from **Assets/Fizzyo/FizzyoFramework/Scenes** folder to your project's build settings. (either drag and drop, or open the calibration scene and use the "Add open scenes" button in the build settings window)

> You own scenes can be placed before the Calibration scene.  it's recommended to be the last scene in your settings.

- Set the **Platform** to the **Universal Windows Platform** by selecting it in the Build Settings list and clicking on **Switch Platform**.

> If the Universal Windows Platform selection is "greyed out" you do not have the Unity option for that platform installed, simply run the Unity setup again and ensure the WSA / Universal Windows platform options are included in your install.

- Click on the **Player Settings** button in the Build settings window.
- In the inspector, you should now see the following window:

![](/Images/WindowsCapabilities.png)

- Ensure that the Windows Store tab (as indicated above) is selected and locate the **Capabilities** section under the **Publishing Settings** group.
- Check the four following options:

    * BlueTooth
    * InternetClient
    * UserAccountInformation
    * HumanInterfaceDevice

Now your project is configured and ready to start building your Fizzyo Project.

> Check the details in the [Fizzyo API](/FizzyoAPI.md) documentation for consuming the Fizzyo Framework in your project.

## Next Steps

* [Building your Project for UWP](/BuildingYourProject.md)