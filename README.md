**Last release:** May 15, 2023
<br>**Download:** [Gw2Launcher.exe](/Gw2Launcher/bin64/Release/Gw2Launcher.exe?raw=true) (build 7195)

See the [**wiki**](https://github.com/Healix/Gw2Launcher/wiki) for more information and recent [**changes**](https://github.com/Healix/Gw2Launcher/wiki/Changes). Simply [**download**](/Gw2Launcher/bin64/Release/Gw2Launcher.exe?raw=true) (64-bit) and place the executable wherever you'd like it.

## Notices
### CEF Update
To continue using CoherentUI, add the following option under "Settings > Guild Wars 2 > Launch options > Arguments":

-usecoherent

### Indefinite black screen while loading character select
This is a bug GW2 introduced back when DX11 was implemented. The slower GW2 is to load, the more likely it is to occur. There are a few options to help with this:
<br>
<br>Settings > Guild Wars 2 > Process priority while initializing the game: high.
<br>Settings > General > Launching > Timeout: relaunch if character select hasn't loaded within 15~30 seconds.
<br>Settings > General > Launching > Delay until the main window is loaded.

## Gw2Launcher
GW2 uses a mutex to prevent multiple instances from being opened at the same time. In addition, GW2 also locks access to Gw2.dat, which prevents other processes from reading it. By killing the mutex and enabling -shareArchive, multiple clients can be launched simultaneously.

When -shareArchive is enabled, GW2 will not be able to modify any files. In order to update the game or modify your settings, the game must be launched normally, which will be handled for you.

### Preview
![Preview](https://github.com/Healix/Gw2Launcher/wiki/images/preview.jpg)
