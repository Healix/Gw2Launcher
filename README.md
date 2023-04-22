**Last release:** January 1, 2023
<br>**Download:** [Gw2Launcher.exe](/Gw2Launcher/bin64/Release/Gw2Launcher.exe?raw=true) (nonfunctional due to CEF)
<br>**Beta:** [Gw2Launcher.exe](/Gw2Launcher/bin64/Beta/Gw2Launcher.exe?raw=true)

See the [**wiki**](https://github.com/Healix/Gw2Launcher/wiki) for more information and recent [**changes**](https://github.com/Healix/Gw2Launcher/wiki/Changes). Simply [**download**](/Gw2Launcher/bin64/Release/Gw2Launcher.exe?raw=true) (64-bit) and place the executable wherever you'd like it.

## CEF Update
Use the [beta version](/Gw2Launcher/bin64/Beta/Gw2Launcher.exe?raw=true) for compatibility.

To continue using CoherentUI, add the following option under "Settings > Guild Wars 2 > Launch options > Arguments":

-usecoherent

## Gw2Launcher
GW2 uses a mutex to prevent multiple instances from being opened at the same time. In addition, GW2 also locks access to Gw2.dat, which prevents other processes from reading it. By killing the mutex and enabling -shareArchive, multiple clients can be launched simultaneously.

When -shareArchive is enabled, GW2 will not be able to modify any files. In order to update the game or modify your settings, the game must be launched normally, which will be handled for you.

### Preview
![Preview](https://github.com/Healix/Gw2Launcher/wiki/images/preview.jpg)
