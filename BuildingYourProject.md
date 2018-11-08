# Building UWP application

From Unity **select File** > **Build Setting** > **Universial Windows Platform**
- Set the build configuration = master

### Export game from Unity as a UWP app and make sure the following is added to the Package.appxmanifest file

At present Unity doesnt allows you to specific VID & PID's so to you need to have to manuually add the following to Package.appxmanifest after exporting to ensure the game will support the Fizzyo Device. See <https://docs.microsoft.com/en-gb/windows/uwp/packaging/packaging-uwp-apps>

- Open the build solution in Visual Studio
- Double click on Package.appxmanifest to open the file
- Check the game details to ensure correct oritentation
- Visual Assets tab - Add Game Logo
- Capabilities tab - Select Bluetooth, Internet (Client), UserAccountinformation
- Declarations tab - Add "Protocol", set name to your game (prefixed with "fizzyo" and only using lowercase letters, no spaces)

To Edit the package.manifest to add the capbiliies simply have to right-click on the "Package.appxmanifest" and click "View Code" to see the xml

Then manually update the capabilities section if your interested in available capabilities with the details below for more details see <https://docs.microsoft.com/en-us/uwp/schemas/appxpackage/how-to-specify-device-capabilities-in-a-package-manifest>


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

## Testing your game

- Open the Build Solution in Visual Studio
- Select Solution Configurations to Master
- Select Solution Platform to x86
- Select Run on Local Machine


## Packaging your Solution

Open the Build Solution Visual Studio
Right click on the Project (Not the Solution)
Select **Store** **Create App Package**
Select **I want to create packages for sideloading**
Leave version information
Ensure your on Master Build and uncheck ARM
uncheck the PDB checkbox
Select **create**
