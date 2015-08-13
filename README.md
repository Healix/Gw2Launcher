See the [wiki](https://github.com/Healix/Gw2Launcher/wiki) for more information. Simply [download](https://github.com/Healix/Gw2Launcher/blob/master/Gw2Launcher/bin/Release/Gw2Launcher.exe?raw=true) and place the executable wherever you'd like to use this program.

## Gw2Launcher
GW2 uses a mutex to prevent multiple instances to be opened at the same time. In addition, GW2 also locks access to GW2.dat, which prevents other processes from reading it. By killing the mutex and enabling -sharedArchive, multiple clients can be launched simultaneously.

### Local.dat
Local.dat is the file used by GW2 to store settings related to your computer, such as your login information and video options. Only 1 Local.dat file can be active on a single Windows user account at a time. In order to run multiple clients with different settings, each client will need to use a different user's account. This program will assist in the creation and management of additional users if needed.

When -sharedArchive is enabled, GW2 will not be able to modify any files. In order to update the game or modify your settings, the game must be launched normally.

### Preview
![Preview](https://github.com/Healix/Gw2Launcher/wiki/images/preview.jpg)
