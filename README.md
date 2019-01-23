**Last release:** December 24, 2018

See the [wiki](https://github.com/Healix/Gw2Launcher/wiki) for more information and recent [changes](https://github.com/Healix/Gw2Launcher/wiki/Changes). Simply download the [64-bit](https://github.com/Healix/Gw2Launcher/blob/master/Gw2Launcher/bin64/Release/Gw2Launcher.exe?raw=true) or [32-bit](https://github.com/Healix/Gw2Launcher/blob/master/Gw2Launcher/bin/Release/Gw2Launcher.exe?raw=true) version and place the executable wherever you'd like to use this program.

## Notice: -nopatchui disabled
ArenaNet has disabled the ability to automatically login directly to the character select on January 22, 2019. The "automatically login to character select" option can no longer be used and will be removed in a future update. To launch multiple accounts and have their credentials saved, each account must use its own Local.dat file, which can be configured under the account's settings. Check the auto play option on the game's launcher if you want it to automatically play (to character select). 

Remember, GW2 cannot modify its data files when multiple clients are allowed to be opened, so you'll need to choose to launch an account normally (right click > selected > launch (normal) if you need to change anything, such as remembering your email/password or selecting auto play on the launcher.

## Gw2Launcher
GW2 uses a mutex to prevent multiple instances from being opened at the same time. In addition, GW2 also locks access to Gw2.dat, which prevents other processes from reading it. By killing the mutex and enabling -shareArchive, multiple clients can be launched simultaneously.

When -shareArchive is enabled, GW2 will not be able to modify any files. In order to update the game or modify your settings, the game must be launched normally, which will be handled for you.

### Preview
![Preview](https://github.com/Healix/Gw2Launcher/wiki/images/preview.jpg)
