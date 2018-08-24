**Last release:** August 24, 2018

See the [wiki](https://github.com/Healix/Gw2Launcher/wiki) for more information. Simply download the [64-bit](https://github.com/Healix/Gw2Launcher/blob/master/Gw2Launcher/bin64/Release/Gw2Launcher.exe?raw=true) or [32-bit](https://github.com/Healix/Gw2Launcher/blob/master/Gw2Launcher/bin/Release/Gw2Launcher.exe?raw=true) version and place the executable wherever you'd like to use this program.

## Gw2Launcher
GW2 uses a mutex to prevent multiple instances from being opened at the same time. In addition, GW2 also locks access to Gw2.dat, which prevents other processes from reading it. By killing the mutex and enabling -shareArchive, multiple clients can be launched simultaneously.

When -shareArchive is enabled, GW2 will not be able to modify any files. In order to update the game or modify your settings, the game must be launched normally, which will be handled for you.

### Preview
![Preview](https://github.com/Healix/Gw2Launcher/wiki/images/preview.jpg)
