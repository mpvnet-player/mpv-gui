
mpv.net manual
==============

**ENGLISH** | **[简体中文](Manual_chs.md)**

Table of contents
-----------------

* [About](#about)
* [Download](#download)
* [Installation](#installation)
* [Support](#support)
* [Settings](#settings)
* [Input and context menu](#input-and-context-menu)
* [Command Palette](#command-palette)
* [Command Line Interface](#command-line-interface)
* [Terminal](#terminal)
* [mpv.net specific commands](#mpvnet-specific-commands)
* [mpv.net specific options](#mpvnet-specific-options)
* [External Tools](#external-tools)
* [Scripting](#scripting)
* [Extensions](#extensions)
* [Color Theme](#color-theme)
* [Advanced Features](#advanced-features)
* [Hidden Features](#hidden-features)
* [Differences compared to mpv](#differences-compared-to-mpv)
* [Technical Overview](#technical-overview)
* [Context Menu Commands](#context-menu)


About
-----

mpv.net is a modern desktop media player for Windows based on the popular mpv player.

mpv.net is designed to be mpv compatible, almost all mpv features are available
because they are all contained in libmpv, this means the official
[mpv manual](https://mpv.io/manual/master/) applies to mpv.net.

mpv focuses on the usage of the command line and the terminal,
mpv.net retains the ability to be used from the command line and
the terminal and adds a modern Windows GUI on top of it.

Like mpv, mpv.net is designed for power users.


Download
--------

1. [Stable via Microsoft Store](https://www.microsoft.com/store/productId/9N64SQZTB3LM)

2. [Stable and beta via GitHub download](../../../releases)

3. `winget install mpv.net`

[Changelog](Changelog.md)

Installation
------------

mpv.net requires the .NET Framework 4.8 and Windows 7 or higher and a modern graphics card.

Internet streaming requires:

- Downloading [yt-dlp](https://github.com/yt-dlp/yt-dlp) and adding its folder
  to the [user environment variable PATH](https://www.google.com/search?q=user+environment+variable+PATH).
- In case of proxy server usage, [manual configuration](https://github.com/mpvnet-player/mpv.net/issues/401).

#### File Associations

File Associations can be registered using the context menu under 'Settings > Setup'.

After the file associations were registered, it might be necessary to change the
default app in the Windows settings (Win+I, ms-settings:defaultapps).

Another way to register file associations is using Windows File Explorer,
select a media file and select 'Open with > Choose another app' in the context menu.

[Open with++](#open-with) can be used to extend the File Explorer context menu
to get menu items for [Play with mpv.net](https://github.com/stax76/OpenWithPlusPlus#play-with-mpvnet) and
[Add to mpv.net playlist](https://github.com/stax76/OpenWithPlusPlus#add-to-mpvnet-playlist).

When multiple files are selected in File Explorer and enter is pressed then
the files are opened in mpv.net in random order, this works with maximum 15 files.


Support
-------

Before making a support request, please try the newest [beta version](../../../releases) first.

Support can be requested here:

Beginner questions:

https://www.reddit.com/r/mpv

mpv.net bug reports, feature requests and advanced questions:

https://github.com/mpvnet-player/mpv.net/issues

Advanced mpv questions:

https://github.com/mpv-player/mpv/issues


Settings
--------

mpv.net searches the config folder at:

1. startup\portable_config
2. %APPDATA%\mpv.net (`C:\Users\%USERNAME%\AppData\Roaming\mpv.net`)

mpv options are stored in the file mpv.conf,
mpv.net options are stored in the file mpvnet.conf,
mpv.net options are documented [here](#mpvnet-specific-options).


Input and context menu
----------------------

The input (key/mouse) bindings and the context menu definitions are stored in the
input.conf file, if it's missing mpv.net generates it with default values.

Please be aware that once input.conf exists, mpv.net cannot update it, this means
the menu becomes outdated when mpv.net is updated with new or changed default menu
items. The only way to get an up-to-date menu is either resetting the menu by
deleting input.conf or updating it by manually editing input.conf.

Global keyboard shortcuts are supported via global-input.conf file.

The config folder can be opened from the context menu: `Settings > Open Config Folder`

A input and config editor can be found in the context menu under 'Settings'.

The input test mode can be started via command line: --input-test

The input key list can be printed with --input-keylist or
shown from the context menu under: View > Advanced > Show Keys

mpv.net input.conf defaults:  
https://github.com/mpvnet-player/mpv.net/blob/master/src/Resources/input.conf.txt

mpv input.conf defaults:
https://github.com/mpv-player/mpv/blob/master/etc/input.conf

mpv input commands:
https://mpv.io/manual/master/#list-of-input-commands

mpv input options:
https://mpv.io/manual/master/#input


Command Palette
---------------

The command palette is designed to quickly find,
select and execute commands.

It can also be used to easily find shortcut keys.

The following functionality is presented with the Command Palette:

- Show media info in different ways.
- Show and select audio tracks.
- Show and select subtitle tracks.
- Show and select playlist files.
- Show and select recent files.
- Show available mpv properties.
- Show available decoders.
- Show available demuxers.
- Show available keys.
- Show available protocols.

| Key    | Action                      |
| ------ | --------------------------- |
| F1     | Shows the command palette.  |
| Escape | Hides the command palette.  |
| Enter  | Executes the selected item. |
| Up     | Moves the selection up.     |
| Down   | Moves the selection down.   |


Command Line Interface
----------------------

**mpvnet** [options] [file|URL|PLAYLIST|-]  
**mpvnet** [options] files


mpv properties can be set with the same syntax as mpv, that is:


To enable the border property:

`--border` or `--border=yes`


To disable the border property:

`--no-boder` or `--border=no`


Supported are all mpv properties, they are documented here:

https://mpv.io/manual/master/#properties


mpv.net has a feature to list all available properties:

_Context Menu > View > Advanced > Show Properties_


mpv has a few non property based switches which are generally not supported in mpv.net.


Terminal
--------

When mpv.net is started from a terminal it will output status,
error and debug messages to the terminal and accept input keys from the terminal.

A common task for the terminal is debugging scripts.


mpv.net specific commands
-------------------------

`script-message-to mpvnet <command> <arguments>`

mpv.net commands are used when mpv commands don't exist or lack a feature.

### load-audio
Shows a file browser dialog to open external audio files.

### load-sub
Shows a file browser dialog to open external subtitle files.

### move-window [left|top|right|bottom|center]
Moves the Window to the screen edge (Alt+Arrow) or center (Alt+BS).

### open-conf-folder
Opens the config folder with Windows File Explorer.

### open-files [\<flags\>]
**append**  
Appends files to the playlist.

Opens a file browser dialog in order to select files to be opened.
The file browser dialog supports multiselect to load multiple files
at once. Pressing CTRL appends the files to the playlist.

### open-optical-media
Shows a folder browser dialog to open a DVD or BD folder.
ISO images don't have to be mounted, but instead can be
opened directly with the open-files command.

### open-clipboard
Opens a single URL or filepath from the clipboard,
or multiple files in the file clipboard format.

### play-pause
Cycles the pause property. In case the playlist is empty,
the most recent file from the recent files list is loaded.

### playlist-add \<integer\>
Changes the playlist position by adding the supplied integer value.
If the position goes out of range, it jumpes to the opposite end.

### reg-file-assoc \<audio|video|image\>
Registers the file associations.

### scale-window \<factor\>
Decreases or increases the Window size.

### select-profile
Shows the command palette to select a profile.

### shell-execute \<file|URL\>
Shell executes a single file or URL.

### show-about
Shows the about dialog.

### show-audio-devices
Shows available audio devices in a message box.

### show-audio-tracks
Shows available audio tracks in the command palette
and allows to load the selected audio track.

### show-chapters
Shows chapters in the command palette.

### show-command-palette
Shows the command palette.

### show-commands
Shows available mpv input commands.

### show-conf-editor
Shows the conf editor.

### show-decoders
Shows available decoders.

### show-demuxers
Shows available demuxers.

### show-input-editor
Shows the input editor.

### show-keys
Shows available keys (as shown with `--input-keylist`) in the command palette.

### show-media-info [\<flags\>]
**msgbox**  
Shows media info in a messsage box.

**editor**  
Shows media info in the text editor.

**osd**
Displays media info on screen.

**full**  
Shows fully detailed media info.

**raw**  
Shows media info with raw property names.

### show-menu
Shows the context menu.

### show-playlist
Shows the playlist in the command palette
and allows to play the selected entry.

### show-profiles
Shows available profiles with a message box.

### show-progress
Shows a simple OSD progress message with time and date.

### show-properties
Shows available properties in the command palette and
allows to display the property value of the selected property.

### show-protocols
Shows available protocols in the command palette.

### show-recent
Shows recently played files and URLs in the
command palette and allows to select and play entries.

### show-subtitle-tracks
Shows available subtitles in the command palette
and allows to activate the selected subtitle.

### show-text \<text\> \<duration\> \<font-size\>
Shows a OSD message with given text, duration and font size.

### window-scale \<factor\>
Works similar as the [window-scale](https://mpv.io/manual/master/#command-interface-window-scale) mpv property.


mpv.net specific options
------------------------

mpv.net specific options can be found in the conf editor searching for 'mpv.net'.

The options are saved in the mpvnet.conf file.

#### --autofit-audio \<integer\>
Initial window height in percent for audio files. Default: 70

#### --autofit-image \<integer\>
Initial window height in percent for image files. Default: 80

#### --queue \<files\>

Adds files to the playlist, requires [--process-instance=single](#--process-instancevalue).
[Open with++](#open-with) can be used to add files to the playlist using File Explorer.

#### --command=\<input command\>

Sends a input command to a running mpv.net instance via command line, for instance
to create global keyboard shortcuts with AutoHotkey. Requires [process-instance=single](#--process-instancevalue).

### Audio

#### --remember-volume=\<yes|no\>

Save volume and mute on exit and restore it on start. Default: yes


### Screen

#### --start-size=\<value\>

Setting to remember the window size.

**width-session**  
Width is remembered in the current session.

**width-always**  
Width is always remembered.

**height-session**  
Height is remembered in the current session. Default

**height-always**  
Height is always remembered.

**video**  
Window size is set to video resolution.

**session**  
Window size is remembered in the current session.

**always**  
Window size is always remembered.

#### --minimum-aspect-ratio=\<float\>

Minimum aspect ratio of the window. Useful to force
a wider window and therefore a larger OSC.

#### --minimum-aspect-ratio-audio=\<float\>

Same as minimum-aspect-ratio but used for audio files.

#### --remember-window-position=\<yes|no\>

Save the window position on exit. Default: no

#### --start-threshold=\<milliseconds\>

Threshold in milliseconds to wait for libmpv returning the video
resolution before the window is shown, otherwise default dimensions
are used as defined by autofit and start-size. Default: 1500

### Playback

#### --auto-load-folder=\<yes|no\>

For single files automatically load the entire directory into the playlist.

#### --auto-play=\<yes|no\>

If the player is paused and another file is loaded,
playback automatically resumes.


### General

#### --process-instance=\<value\>

Defines if more then one mpv.net process is allowed.

Whenever the CTRL key is pressed when files or URLs are opened,
the playlist is not cleared but the files or URLs are appended to the playlist.
This not only works on process startup but in all mpv.net features that open files and URLs.

Multi can alternatively be enabled by pressing the SHIFT key.

**multi**  
Create a new process everytime the shell starts mpv.net.

**single**  
Force a single process everytime the shell starts mpv.net. Default

**queue**  
Force a single process and add files to playlist.

#### --recent-count=\<int\>

Amount of recent files to be remembered. Default: 15

#### --media-info=\<yes|no\>

Usage of the media info library instead of mpv to access media information. Default: yes (mpv.net specific option)

#### --video-file-extensions=\<string\>

Video file extensions used to create file associations and used by the auto-load-folder feature.

#### --audio-file-extensions=\<string\>

Audio file extensions used to create file associations and used by the auto-load-folder feature.

#### --image-file-extensions=\<string\>

Image file extensions used to create file associations and used by the auto-load-folder feature.

#### --debug-mode=\<yes|no\>

Enable this only when a developer asks for it. Default: no


### UI

#### --dark-mode=\<value\>

Enables a dark theme.

**always**  
Default

**system**  
Available on Windows 10 or higher.

**never**

#### --dark-theme=\<string\>

Color theme used in dark mode. Default: dark

[Color Themes](#color-theme)

#### --light-theme=\<string\>

Color theme used in light mode. Default: light

[Color Themes](#color-theme)

#### --show-logo=\<yes|no\>

Draws the blue mpv.net logo ontop of the native OSC logo. Default: yes

#### --show-santa-logo=\<yes|no\>

Draws the blue mpv.net logo with a santa hat in december,
the option is called greenandgrumpy in mpv. Default: yes

External Tools
--------------

### Play with mpv

In order to play videos from sites such as YouTube the Chrome Extension [Play with mpv](https://chrome.google.com/webstore/detail/play-with-mpv/hahklcmnfgffdlchjigehabfbiigleji) can be used.

Due to Chrome Extensions not being able to start a app, another app that communicates with the extension is required, this app can be downloaded [here](http://www.mediafire.com/file/lezj8lwqt5zf75v/play-with-mpvnet-server.7z/file). The extension works only when the app is running, to have the app always running a link can be created in the auto start folder located at:

`C:\Users\%username%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`

This will start the app on system start and have it running in the background. When the file association registration of mpv.net was executed then the app should find the location of mpv.net, alternativly the mpv.net folder can be added to the Path environment variable.


### Open With

Alternatively the Chrome/Firefox extension [Open With](../../../issues/119) can be used.


### Open with++

[Open with++](https://github.com/stax76/OpenWithPlusPlus) can be used to extend the File Explorer context menu to get menu items for [Play with mpv.net](https://github.com/stax76/OpenWithPlusPlus#play-with-mpvnet) and [Add to mpv.net playlist](https://github.com/stax76/OpenWithPlusPlus#add-to-mpvnet-playlist).


### External Application Button

Videos can be streamed or downloaded easily with the Chrome extension
External Application Button, for download (recommended):

path: `wt`

args: `-- pwsh -NoLogo -Command "yt-dlp --ignore-errors --download-archive 'C:\External Application Button.txt' --output 'C:\YouTube\%(channel)s - %(title)s.%(ext)s' ('[HREF]' -replace '&list=.+','')"`


Scripting
---------

#### Lua

A very large collection of Lua user scripts can be found in the mpv wiki [here](https://github.com/mpv-player/mpv/wiki/User-Scripts).

Lua scripting is documented in the mpv.net wiki [here](https://github.com/mpvnet-player/mpv.net/wiki/Extending-mpv-and-mpv.net-via-Lua-scripting).

#### JavaScript

[mpv JavaScript documentation](https://mpv.io/manual/master/#javascript)

#### PowerShell

Location: `<config folder>\scripts-ps`

The PowerShell scripting host is not initialized before media files are loaded.

[Example Scripts](../../../tree/master/src/Scripts)


#### C#

Location: `<config folder>\scripts-cs`

There are no compatibility guaranties.

Script code can be written within a C# [extension](../../../tree/master/src/Extensions),
that way full code completion and debugger support is available.
Once the code was developed and debugged, it can be moved
from the extension to a lightweight standalone script.
The script host uses an old C# version, modern features
like string interpolation are not available.

There are synchronous and asynchronous events, prefer asynchronous events
and don't block synchronous events and observed properties, as it would
block the main event loop.

The C# scripting host is like [extensions](../../../tree/master/src/Extensions)
not initialized before media files are loaded.

[Example Scripts](../../../tree/master/src/Scripts)


Extensions
----------

Extensions are located in a subfolder _extensions_ in the config folder
and the filename must have the same name as the directory:

```Text
<config folder>\extensions\ExampleExtension\ExampleExtension.dll
```

There are synchronous and asynchronous events, prefer asynchronous events
and don't block synchronous events and observed properties, as it would
block the main event loop.

### Walkthrough creating an extension

- Download and install [Visual Studio Community](https://visualstudio.microsoft.com).
- Create a new project of type **Class Library .NET Framework**
  and ensure the project name ends with **Extension**.
- Add a reference to **System.ComponentModel.Composition**.
- Add a reference to mpvnet.exe, select the mpvnet reference
  in the Solution Explorer, open the Properties window and set
  **Copy Local** to false to prevent mpvnet.exe being copied
  to the output directory when the project is built.
- Now open the project properties and set the output path in the Build tab,
  extensions are like scripts located in your config folder, example:
  `<config folder>\extensions\ExampleExtension\ExampleExtension.dll`
- Also in the project properties choose the option **Start external program**
  in the Debug tab and define the path to mpvnet.exe. In the Debug tab you may also
  define command line arguments like a video file to be played when you start debugging.


### Sample Code

#### RatingExtension

This extension writes a rating to the filename of rated videos when mpv.net shuts down.

The input.conf defaults contain key bindings for this extension to set ratings.

[Source Code](../../../tree/master/src/Extensions)


Color Theme
-----------

mpv.net supports custom color themes, the definition of the built-in themes can be found at:

[theme.txt](../../../tree/master/src/Resources/theme.txt)


Custom themes can be saved at:

`<conf folder>\theme.conf`

The theme.conf file may contain an unlimited amount of themes.

In the config editor under UI there are the settings dark-theme and
light-theme to define the themes used in dark and in light mode.


Advanced Features
-----------------

### Playback of VapourSynth scripts

vpy files are supported with following mpv.conf configuration:

```
[extension.vpy]
demuxer-lavf-format = vapoursynth
```

Python and VapourSynth must be in the path environment variable.


Hidden Features
---------------

Selecting multiple files in File Explorer and pressing enter will
open the files in mpv.net. Explorer restricts this to maximum 15 files
and the order will be random.

In fullscreen mode clicking the top right corner closes the player.


Differences compared to mpv
---------------------------

mpv.net is designed to work exactly like mpv, there are a few limitations:


### Window Limitations

mpv.net implements an own main window which means only mpv window
features are supported that have an own implementation in mpv.net.

A window free mode is currently not supported, the main window is always
visible, even when mpv.net is started from the terminal and music is played.

The documentation of mpv's window features can be found here:

https://mpv.io/manual/master/#window


**mpv.net has currently implemented the following window properties:**

- [border](https://mpv.io/manual/master/#options-border)
- [fullscreen](https://mpv.io/manual/master/#options-fullscreen)
- [keepaspect-window](https://mpv.io/manual/master/#options-keepaspect-window)
- [ontop](https://mpv.io/manual/master/#options-ontop)
- [screen](https://mpv.io/manual/master/#options-screen)
- [snap-window](https://mpv.io/manual/master/#options-snap-window)
- [title](https://mpv.io/manual/master/#options-title)
- [window-maximized](https://mpv.io/manual/master/#options-window-maximized)
- [window-minimized](https://mpv.io/manual/master/#options-window-minimized)
- [window-scale](https://mpv.io/manual/master/#options-window-scale)


**Partly implemented are:**

- [autofit-larger](https://mpv.io/manual/master/#options-autofit-larger)  
  Supported is a single integer value in the range 0-100.
- [autofit-smaller](https://mpv.io/manual/master/#options-autofit-smaller)  
  Supported is a single integer value in the range 0-100.
- [autofit](https://mpv.io/manual/master/#options-autofit)  
  Supported is a single integer value in the range 0-100.

mpv.net specific window features are documented in the [screen section](#screen).


### Command Line Limitations

mpv.net supports property based mpv command line options which means it supports
almost all mpv command line options.

What is not supported are non property bases options. Non property based options
need an own implementation in mpv.net, so far implemented are:

--ad=help  
--audio-device=help  
--input-keylist  
--profile=help  
--vd=help  
--version  

### Other Limitations

The mpv property [idle](https://mpv.io/manual/master/#options-idle) can be
used and mpv.net functions accordingly, but Lua scripts always see `idle=yes`
because mpv.net has to set it to function correctly, this is a difficult
to overcome libmpv limitation.


### mpv.net specific options

Options that are specific to mpv.net can be found by entering _mpv.net_
in the search field of the config editor, in the mpv.net manual they are
documented [here](#mpvnet-specific-options).

mpv.net specific options are saved in the file mpvnet.conf and are just
as mpv properties available on the command line.


Technical Overview
------------------

mpv.net is written in C# 7 and runs on the .NET Framework 4.8.

The Extension implementation is based on the
[Managed Extensibility Framework](https://docs.microsoft.com/en-us/dotnet/framework/mef/).

The main window is WinForms based because WinForms allows better libmpv integration
compared to WPF, all other windows are WPF based.

Third party components are:

- [libmpv provides the core functionality](https://mpv.io/)
- [MediaInfo](https://mediaarea.net/en/MediaInfo)


Context Menu
------------

The context menu of mpv.net is defined in the file input.conf which is
located in the config directory.

If the input.conf file does not exists mpv.net generates it with the following defaults:

<https://github.com/mpvnet-player/mpv.net/tree/master/src/Resources/input.conf.txt>

input.conf defines mpv's key and mouse bindings and mpv.net uses
comments to define the context menu.


### Open > Open Files

The Open Files menu entry is one way to open files in mpv.net, it supports multi selection.

Another way to open files is the command line which is used by
File Explorer for existing associations.

A third way is to drag and drop files on the main window.

Blu-ray and DVD ISO image files are supported.


### Open > Open URL or file path from clipboard

Opens files and URLs from the clipboard. How to open URLs directly
from the browser from sites like YouTube is described in the
[External Tools section](#external-tools).


### Open > Open DVD/Blu-ray Drive/Folder

Opens a DVD/Blu-ray Drive/Folder.


### Open > Load external audio files

Allows to load an external audio file. It's also possible to auto detect
external audio files based on the file name, the option for this can be
found in the settings under 'Settings > Show Config Editor > Audio > audio-file-auto'.


### Open > Load external subtitle files

Allows to load an external subtitle file. It's also possible to auto detect
external subtitle files based on the file name, the option for this can be
found in the settings under 'Settings > Show Config Editor > Subtitles > sub-auto'.


### Play/Pause

Play/Pause using the command:

`cycle pause`

[cycle command](https://mpv.io/manual/master/#command-interface-cycle-%3Cname%3E-[%3Cvalue%3E])

[pause property](https://mpv.io/manual/master/#options-pause)


### Stop

Stops the player and unloads the playlist using the command:

`stop`

[stop command](https://mpv.io/manual/master/#command-interface-stop)


### Toggle Fullscreen

Toggles fullscreen using the command:

`cycle fullscreen`

[cycle command](https://mpv.io/manual/master/#command-interface-cycle-%3Cname%3E-[%3Cvalue%3E])

[fullscreen property](https://mpv.io/manual/master/#options-fs)


### Navigate > Previous File

Navigates to the previous file in the playlist using the command:

`playlist-prev`

[playlist-prev command](https://mpv.io/manual/master/#command-interface-playlist-prev)


### Navigate > Next File

Navigates to the next file in the playlist using the command:

`playlist-next`

[playlist-next command](https://mpv.io/manual/master/#command-interface-playlist-next)


### Navigate > Next Chapter

Navigates to the next chapter using the command:

`add chapter 1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[chapter property](https://mpv.io/manual/master/#command-interface-chapter)


### Navigate > Previous Chapter

Navigates to the previous chapter using the command:

`add chapter -1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[chapter property](https://mpv.io/manual/master/#command-interface-chapter)


### Navigate > Jump Next Frame

Jumps to the next frame using the command:

`frame-step`

[frame-step command](https://mpv.io/manual/master/#command-interface-frame-step)


### Navigate > Jump Previous Frame

Jumps to the previous frame using the command:

`frame-back-step`

[frame-back-step command](https://mpv.io/manual/master/#command-interface-frame-back-step)


### Navigate > Jump

Seeking using the command:

`no-osd seek sec`

sec is the relative amount of seconds to jump, the no-osd prefix
is used because mpv.net includes a script that shows the position
when a seek operation is performed, the script uses a more simple
time format.

[no-osd command prefix](https://mpv.io/manual/master/#command-interface-no-osd)

[seek command](https://mpv.io/manual/master/#command-interface-seek-%3Ctarget%3E-[%3Cflags%3E])


### Pan & Scan > Increase Size

Adds video zoom using the command:

`add video-zoom  0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[video-zoom property](https://mpv.io/manual/master/#options-video-zoom)


### Pan & Scan > Decrease Size

Adds negative video zoom using the command:

`add video-zoom  -0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[video-zoom property](https://mpv.io/manual/master/#options-video-zoom)


### Pan & Scan > Move Left

`add video-pan-x -0.01`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[video-pan-x, video-pan-y property](https://mpv.io/manual/master/#options-video-pan-y)


### Pan & Scan > Move Right

`add video-pan-x 0.01`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[video-pan-x, video-pan-y property](https://mpv.io/manual/master/#options-video-pan-y)


### Pan & Scan > Move Up

`add video-pan-y -0.01`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[video-pan-x, video-pan-y property](https://mpv.io/manual/master/#options-video-pan-y)


### Pan & Scan > Move Down

`add video-pan-y 0.01`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[video-pan-x, video-pan-y property](https://mpv.io/manual/master/#options-video-pan-y)


### Pan & Scan > Decrease Height

`add panscan -0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[panscan property](https://mpv.io/manual/master/#options-panscan)


### Pan & Scan > Increase Height

`add panscan  0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[panscan property](https://mpv.io/manual/master/#options-panscan)


### Pan & Scan > Reset

Resets Pan & Scan, multiple commands in the same line are separated with semicolon.

`set video-zoom 0; set video-pan-x 0; set video-pan-y 0`

[video-zoom property](https://mpv.io/manual/master/#options-video-zoom)

[video-pan-x, video-pan-y property](https://mpv.io/manual/master/#options-video-pan-y)


### Video > Decrease Contrast

Decreases contrast with the following command:

`add contrast -1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[contrast property](https://mpv.io/manual/master/#options-contrast)


### Video > Increase Contrast

Increases contrast with the following command:

`add contrast 1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[contrast property](https://mpv.io/manual/master/#options-contrast)


### Video > Decrease Brightness

Decreases brightness using the following command:

`add brightness -1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[brightness property](https://mpv.io/manual/master/#options-brightness)


### Video > Increase Brightness

Increases brightness using the following command:

`add brightness 1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[brightness property](https://mpv.io/manual/master/#options-brightness)


### Video > Decrease Gamma

Decreases gamma using the following command:

`add gamma -1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[gamma property](https://mpv.io/manual/master/#options-gamma)


### Video > Increase Gamma

Increases gamma using the following command:

`add gamma 1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[gamma property](https://mpv.io/manual/master/#options-gamma)


### Video > Decrease Saturation

Decreases saturation using the following command:

`add saturation -1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[saturation property](https://mpv.io/manual/master/#options-saturation)


### Video > Increase Saturation

Increases saturation using the following command:

`add saturation 1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[saturation property](https://mpv.io/manual/master/#options-saturation)


### Video > Take Screenshot

`async screenshot`

[async command prefix](https://mpv.io/manual/master/#command-interface-async)

[screenshot command](https://mpv.io/manual/master/#command-interface-screenshot-%3Cflags%3E)


### Video > Toggle Deinterlace

Cycles the deinterlace property using the following command:

`cycle deinterlace`

[cycle command](https://mpv.io/manual/master/#command-interface-cycle-%3Cname%3E-[%3Cvalue%3E])

[deinterlace property](https://mpv.io/manual/master/#options-deinterlace)


### Video > Cycle Aspect Ratio

Cycles the aspect ratio using the following command:

`cycle-values video-aspect 16:9 4:3 2.35:1 -1`

[cycle-values command](https://mpv.io/manual/master/#command-interface-cycle-values)

[video-aspect property](https://mpv.io/manual/master/#command-interface-video-aspect)


### Audio > Cycle/Next

This uses a mpv.net command that shows better info then the mpv preset
and also has the advantage of not showing no audio.


### Audio > Delay +0.1

Adds a audio delay using the following command:

`add audio-delay 0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[audio-delay property](https://mpv.io/manual/master/#options-audio-delay)


### Audio > Delay -0.1

Adds a negative audio delay using the following command:

`add audio-delay -0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[audio-delay property](https://mpv.io/manual/master/#options-audio-delay)


### Subtitle > Toggle Visibility

Cycles the subtitle visibility using the following command:

`cycle sub-visibility`

[cycle command](https://mpv.io/manual/master/#command-interface-cycle-%3Cname%3E-[%3Cvalue%3E])

[sub-visibility property](https://mpv.io/manual/master/#options-no-sub-visibility)


### Subtitle > Delay -0.1

Adds a negative subtitle delay using the following command:

`add sub-delay -0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[sub-delay property](https://mpv.io/manual/master/#options-sub-delay)


### Subtitle > Delay 0.1

Adds a positive subtitle delay using the following command:

`add sub-delay 0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[sub-delay property](https://mpv.io/manual/master/#options-sub-delay)


### Subtitle > Move Up

Moves the subtitle up using the following command:

`add sub-pos -1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[sub-pos property](https://mpv.io/manual/master/#options-sub-pos)


### Subtitle > Move Down

Moves the subtitle down using the following command:

`add sub-pos 1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[sub-pos property](https://mpv.io/manual/master/#options-sub-pos)


### Subtitle > Decrease Subtitle Font Size

Decreases the subtitle font size using the following command:

`add sub-scale -0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[sub-scale property](https://mpv.io/manual/master/#options-sub-scale)


### Subtitle > Increase Subtitle Font Size

Increases the subtitle font size using the following command:

`add sub-scale 0.1`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[sub-scale property](https://mpv.io/manual/master/#options-sub-scale)


### Volume > Up

Increases the volume using the following command:

`add volume 10`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[volume property](https://mpv.io/manual/master/#options-volume)


### Volume > Down

Decreases the volume using the following command:

`add volume -10`

[add command](https://mpv.io/manual/master/#command-interface-add-%3Cname%3E-[%3Cvalue%3E])

[volume property](https://mpv.io/manual/master/#options-volume)


### Volume > Mute

Cycles the mute property using the following command:

`cycle mute`

[cycle command](https://mpv.io/manual/master/#command-interface-cycle-%3Cname%3E-[%3Cvalue%3E])

[mute property](https://mpv.io/manual/master/#options-mute)


### Speed > -10%

Decreases the speed by 10% using the following command:

`multiply speed 1/1.1`

[multiply command](https://mpv.io/manual/master/#command-interface-multiply-%3Cname%3E-%3Cvalue%3E)

[speed property](https://mpv.io/manual/master/#options-speed)


### Speed > 10%

Increases the speed by 10% using the following command:

`multiply speed 1.1`

[multiply command](https://mpv.io/manual/master/#command-interface-multiply-%3Cname%3E-%3Cvalue%3E)

[speed property](https://mpv.io/manual/master/#options-speed)


### Speed > Half

Halfs the speed using the following command:

`multiply speed 0.5`

[multiply command](https://mpv.io/manual/master/#command-interface-multiply-%3Cname%3E-%3Cvalue%3E)

[speed property](https://mpv.io/manual/master/#options-speed)


### Speed > Double

Doubles the speed using the following command:

`multiply speed 2`

[multiply command](https://mpv.io/manual/master/#command-interface-multiply-%3Cname%3E-%3Cvalue%3E)

[speed property](https://mpv.io/manual/master/#options-speed)


### Speed > Reset

Resets the speed using the following command:

`set speed 1`

[set command](https://mpv.io/manual/master/#command-interface-set-%3Cname%3E-%3Cvalue%3E)

[speed property](https://mpv.io/manual/master/#options-speed)


### Extensions > Rating > 0stars

A plugin the writes the rating to the filename.


### View > On Top > Enable

Forces the player to stay on top of other windows using the following command:

`set ontop yes`

[set command](https://mpv.io/manual/master/#command-interface-set-%3Cname%3E-%3Cvalue%3E)

[ontop property](https://mpv.io/manual/master/#options-ontop)


### View > On Top > Disable

Disables the player to stay on top of other windows using the following command:

`set ontop no`

[set command](https://mpv.io/manual/master/#command-interface-set-%3Cname%3E-%3Cvalue%3E)

[ontop property](https://mpv.io/manual/master/#options-ontop)


### View > File Info

Shows info using a mpv.net command about the current file, shows length, position, formats, size and filename.


### View > Show Statistics

Show statistics using the following command:

`script-binding stats/display-stats`

[script-binding command](https://mpv.io/manual/master/#command-interface-script-binding)


### View > Toggle Statistics

Toggles statistics using the following command:

`script-binding stats/display-stats-toggle`

[script-binding command](https://mpv.io/manual/master/#command-interface-script-binding)


### View > Toggle OSC Visibility

Toggles OSC Visibility using the following command:

`script-binding osc/visibility`

[script-binding command](https://mpv.io/manual/master/#command-interface-script-binding)


### View > Show Playlist

Shows the playlist for 5 seconds using the following command:

`show-text ${playlist} 5000`

[show-text command](https://mpv.io/manual/master/#command-interface-show-text)


### View > Show Audio/Video/Subtitle List

Shows the Audio/Video/Subtitle list for 5 seconds using the following command:

`show-text ${track-list} 5000`

[show-text command](https://mpv.io/manual/master/#command-interface-show-text)


### Settings > Show Config Editor

Shows mpv.net's config editor.


### Settings > Show Input Editor

Shows mpv.net's key binding editor.


### Settings > Open Config Folder

Opens the config folder which contains:

mpv.conf file containing mpv settings

mpvnet.conf file containing mpv.net settings

input.conf containing mpv key and mouse bindings

User scripts and user extensions


### Tools > Command Palette

Shows the command palette window which allows to quickly find and execute commands and key shortcuts.


### Tools > Set/clear A-B loop points

Enables to set loop start and end points using the following command:

`ab-loop`

[ab-loop command](https://mpv.io/manual/master/#command-interface-ab-loop)


### Tools > Toggle infinite file looping

Loops the current file infinitely using the following command:

`cycle-values loop-file "inf" "no"`

[cycle-values command](https://mpv.io/manual/master/#command-interface-cycle-values)

[loop-file command](https://mpv.io/manual/master/#options-loop)


### Tools > Toggle Hardware Decoding

Cycles the hwdec property to enable/disable hardware decoding using the following command:

`cycle-values hwdec "auto" "no"`

[cycle-values command](https://mpv.io/manual/master/#command-interface-cycle-values)

[hwdec property](https://mpv.io/manual/master/#options-hwdec)


### Tools > Setup

Allows to manage file associations.


### Help > Show mpv manual

Shows the [mpv manual](https://mpv.io/manual/stable/).


### Help > Show mpv.net web site

Shows the [mpv.net web site](https://mpv-net.github.io/mpv.net-web-site/).


### Help > Show mpv.net manual

Shows the [mpv.net manual](https://github.com/mpvnet-player/mpv.net/blob/master/Manual.md).


### Help > About mpv.net

Shows the mpv.net about dialog which shows a copyright notice, the versions of mpv.net and libmpv and a license notice (GPL v2).


### Exit

Exits mpv.net using the following command:

`quit`

[quit command](https://mpv.io/manual/master/#command-interface-quit-[%3Ccode%3E])


### Exit Watch Later

Exits mpv.net and remembers the position in the file using the following command:

`quit-watch-later`

[quit-watch-later command](https://mpv.io/manual/master/#command-interface-quit-watch-later)
