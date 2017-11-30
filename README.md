# Yodel
**CircleCI:** [![CircleCI](https://circleci.com/gh/BrianAllred/Yodel.svg?style=svg)](https://circleci.com/gh/BrianAllred/Yodel)

A cross-platform [Electron.NET](https://github.com/ElectronNET/Electron.NET) frontend for youtube-dl.

See the [main page](https://rg3.github.io/youtube-dl/) for youtube-dl for more information.

### Supported platforms
Theoretically, any Electron and .NET Core supported platform.

**NOTE:** All platforms require the appropriate [youtube-dl](https://rg3.github.io/youtube-dl/) binary in PATH or placed alongside the Yodel binary.

##### Windows
* .NET Core Runtime 2.x (or compatible .NET Framework)

##### Linux (check Issues for known problems)
* .NET Core Runtime 2.x
* GTK2
* libxss
* gconf

##### OS X
* Currently untested

### Screenshots
##### Main Window
![Main Window](../screenshots/Screenshots/main.png?raw=true)

##### Downloading a video
![Download](../screenshots/Screenshots/download.png?raw=true)

##### Console for extra information
![Console](../screenshots/Screenshots/console.png?raw=true)

### Contributing
All contributions are welcome. Electron.NET is still a very new framework and I'm new to Electron in general, so I'm sure I haven't written idiomatic Electron code. Any style corrections are welcome.

Development environment consists of:

* Visual Studio 2017 / Visual Studio Code
* NodeJS 8.6.x
* .NET Core 2.x.