**Last release:** January 7, 2021

See the [**wiki**](https://github.com/Healix/Gw2Launcher/wiki) for more information and recent [**changes**](https://github.com/Healix/Gw2Launcher/wiki/Changes). Simply download the [**64-bit**](https://github.com/Healix/Gw2Launcher/raw/master/Gw2Launcher/bin64/Release/Gw2Launcher.exe) or [**32-bit**](https://github.com/Healix/Gw2Launcher/raw/master/Gw2Launcher/bin/Release/Gw2Launcher.exe) version and place the executable wherever you'd like to use this program.

## Gw2Launcher
GW2 uses a mutex to prevent multiple instances from being opened at the same time. In addition, GW2 also locks access to Gw2.dat, which prevents other processes from reading it. By killing the mutex and enabling -shareArchive, multiple clients can be launched simultaneously.

When -shareArchive is enabled, GW2 will not be able to modify any files. In order to update the game or modify your settings, the game must be launched normally, which will be handled for you.

### Problems when using d912pxy
Using d912pxy will cause launches to stall until the previous account is closed. This is due to d912pxy requiring exclusive access to its files, preventing the game from loading while they're in use. To avoid this, "localized execution" must be enabled and set to "full" under Settings > Guild Wars 2 > Management. This will give each account its own copy of the Guild Wars 2 folder - note these files are essentially shortcuts and don't actually take up any space.

### Preview
![Preview](https://github.com/Healix/Gw2Launcher/wiki/images/preview.jpg)
