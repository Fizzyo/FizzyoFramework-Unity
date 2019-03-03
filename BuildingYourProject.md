![](/Images/FizzyoFrameworkLogo.png)

# Building UWP application

Once your project is built / tested and ready to go, you need to package your solution to the Windows 10 Universal platform.

## Building your Unity project

Building your Unity project for the Universal Windows Platform (Windows 10 is fairly easy, to do this:

- From Unity **select File** > **Build Settings**

![](/Images/BuildSetingsBuild.png)

> If the Universal Windows Platform selection is "greyed out" you do not have the Unity option for that platform installed, simply run the Unity setup again and ensure the WSA / Universal Windows platform options are included in your install.

- Ensure the project is set to the Universal Windows Platform (as indicated by the Unity logo located next to the name, if not, select it and click "Switch Platform")
- Check you have both the "Calibration" and "Error" scenes in your scenes selection.  They should be at the end.
- Set the build configuration to **master (as highlighted in the Image above)
- Click on **Build** and Choose / create a folder to export the project to.

## Building the Windows Store project in Visual Studio

Once your project is built, you need to open the project that Unity generated in Visual studio (recommended, Visual Studio 2017) and then create a package for the Windows Store.
You also need to do some project customisation to set the artwork and prepare it for submission.

- Open the exported solution in Visual Studio by  going to the exported folder from the previous step and double clicking on the ".sln" file.
- Double click on **Package.appxmanifest** file in the Solution Explorer.
- Check the game details are correct and select the correct orientation options for your project.
- on the Visual Assets tab, you need to define all your artwork.  Alternatively, create a 400x400 image and use the "Tile Generator" options on the screen
- Double check the Capabilities tab and validate the "Bluetooth, Internet (Client) & UserAccountinformation" capabilities are checked.  If not, follow the instructions in the **Manually configuring your "Package.appxmanifest"** section below.
- Select the Declarations tab - Select "Protocol" in the **Available Declarations** drop down and click **Add**
![](/Images/ProtocolSettings.png)

- Set the **Name** to the name of your game as entered on the Fizzyo-UCL site (prefixed with "fizzyo" and only using lowercase letters, no spaces)

> for example, the game "Qubi" would be registered with a protocol name of "fizzyoqubi"
> ![](/Images/ProtocolDeclarationsExample.png)

- Save everything and close the Package.appxmanifest window.

## Unity build bug fix.

Due to an issue with the Unity Build Templates for UWP, a few things (it seems) were left out that prevent Fizzyo games working with the hub.

You can either:

### Update your Build settings in the Build Settings window. Change the Build Type from D3D to **XAML**

> Note this may incur a slight performance drain due to the overhead of XAML rendering. Try it and see if your game still functions as expected.

### Update the final D3D build project and add the proper activation code.
To do this:

* Build your project as normal
* Open the built solution in Visual Studio
* Open the "App.cpp" file in the Solution Explorer
* Find the function called "void App::OnActivated(CoreApplicationView^ sender, IActivatedEventArgs^ args)"
* Replace the function with the following code

```cpp
void App::OnActivated(CoreApplicationView^ sender, IActivatedEventArgs^ args)
{
    if (args->Kind == Windows::ApplicationModel::Activation::ActivationKind::Protocol)
    {
        Windows::ApplicationModel::Activation::ProtocolActivatedEventArgs^ eventArgs =
            dynamic_cast<Windows::ApplicationModel::Activation::ProtocolActivatedEventArgs^>(args);

        m_AppCallbacks->SetAppArguments(eventArgs->Uri->AbsoluteUri);
    }

    m_CoreWindow->Activate();
}
```

That will patch your built project and ensure the hub parameters it is sent at launch are passed as expected. 
(Else you will just see the Framework Error screen)

## Testing your game

With everything configured, it's recommended you test your build on your local machine, to do this:

![](/Images/VSBuildSettings.png)

- Open the exported solution open in Visual Studio
- Select Solution Configurations to Master in the Menu toolbar (as shown above)
- Select Solution Platform to x86
- Select Run on "Local Machine"

So long as everything is ok, you can then proceed to package your project.

> Any issues, return to the Unity build to ensure your game is running as expected and re-export the solution if necessary.  All the settings done previously in Visual Studio will be retained.

## Packaging your Solution

With everything tested, its time to package up your build project so it can be installed on to the Fizzyo laptops for use by the kids.

![](/Images/BuildVSProject.png)

- Open the exported solution open in Visual Studio
- Right click on the Project (Not the Solution)
- Select **Store** -> **Create App Package**
- Select **I want to create packages for sideloading**
- Leave version information untouched (Visual Studio manages this for you)
- Ensure **Master** Build is selected in the build configuration mappings and uncheck the **ARM** option (as shown below)
- Uncheck the **Include full PDB symbol files** checkbox
- Click on **Create**

![](/Images/VSPackagingWindow.png)

Once the solution is built (which may take some time), you can package up the folder that is output to the "AppPackages" folder for submission.

# Additional Information

## Manually configuring your "Package.appxmanifest"

If you either forgot or didn't configure the project capabilities in Unity, you also then need to manually edit the "Package.appxmanifest" file in visual studio to ensure it has all the Windows 10 "Capabilities" enabled, else **the project simply will not run**

- Open the Windows 10 Visual Studio output solution (not the Unity project)
- Double click on Package.appxmanifest to open the file
- Select the Capabilities tab and ensure the following options are checked:

    * Select Bluetooth
    * Internet (Client)
    * UserAccountinformation

- Save and close the GUI editor (close the tab)
- Right-click on the "Package.appxmanifest" and click "View Code" to see the xml

> Then manually update the capabilities section if your interested in available capabilities with the details below for more details see <https://docs.microsoft.com/en-us/uwp/schemas/appxpackage/how-to-specify-device-capabilities-in-a-package-manifest>

Ensure that the "Capabilities section" looks as it does below, if not, copy this XML and paste it in to the correct place (replacing the current Capabilities section, if one doesn't exist, repeat the steps above, save and return)


``` XML

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
