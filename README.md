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

### Roadmap

##### 1.1.0
* Allow multiple downloads at once, each download in its own card panel

##### 1.2.0
* Implement settings profiles
* Allow using different settings profiles for different downloads
* Allow saving settings profile as default

##### 2.0.0
* Refactor with Angular (relies on Electron.NET update allowing application-level node packages)

### Contributing
All contributions are welcome. Electron.NET is still a very new framework and I'm new to Electron in general, so I'm sure I haven't written idiomatic Electron code. Any style corrections are welcome.

Development environment consists of:

* Visual Studio 2017 / Visual Studio Code
* NodeJS 8.6.x
* .NET Core 2.x

##### Building
Use Visual Studio or the following .NET Core CLI commands.

    dotnet restore
    dotnet electronize start

This will run the application in debug mode.

    dotnet electronize build <platform>

This will build and package the application for the specified platform.  
Platform is optional and will default to the currently running system.  
Platform options are `win`, `osx`, or `linux`.

**NOTE:** Building for the macOS platform does not currently work on Windows. Use Linux or macOS itself.

##### Debugging
Debug the UI itself using Electron's built-in debugging tools. These can be opened with `Ctrl+Shift+I` while running the application in debug mode.

Debug the .NET code by attaching the debugger to the `Yodel.exe` (or similarly named) process.