# Installation

Download the latest Fizzyo Framework unity package from [here](https://github.com/Fizzyo/FizzyoFramework-Unity/releases)

- Go to <http://fizzyo-ucl.co.uk>
- Register and create a new game
- Start your new project in Unity
- From Unity **select Assets** > **Import Package** > **Custom Package**
- Select the **FizzyoFramework.unitypackage** that you have downloaded.
- In the window that pops up select **ok** to import the package.
- Go to the Fizzyo Folder
- Select FizzyoConfigurationProfile
- Insert your Game ID and Game Secret from <http://fizzyo-ucl.co.uk>
- Add all scenes from **Assets/Fizzyo/Scenes** to your project build settings by opening them and going to **File** > **Build Settings** > **Add Cablibtation Scene in Scenes in Build Window**.
- Start Building your Project
- Edit the Package.appxmanifest and include the capbilities to support the Fizzyo Controller

*the Unity package is also able to be cloned directly from GitHub and the fizzyo folder placed in your projects Assets folder.*

### Configuration

The FizzyoFramework script can be attached to a Unity GameObject to configure though the Unity Editor.


1: Set WSA build
1: Set config profile
2: Set Capabilities (player Properties, WSA, Capabilities, select - BlueTooth, InternetClient, UserAccountInformation, HumanInterfaceDevice)