
namespace mpvgui.Windows.Misc;

public class MenuItemDefinition
{
    public string Path { get; init; } = "";
    public string Name { get; init; } = "";
    public string Command { get; init; } = "";
    
    MenuItemDefinition[] GetDefinitions()
    {
        MenuItemDefinition[] ret =
        {
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Open Files..."),
                Command = "script-message-to mpvgui open-files",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Open URL or file from clipboard"),
                Command = "script-message-to mpvgui open-clipboard",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Open DVD/Blu-ray Drive/Folder..."),
                Command = "script-message-to mpvgui open-optical-media",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Load external audio files..."),
                Command = "script-message-to mpvgui load-audio",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Load external subtitle files..."),
                Command = "script-message-to mpvgui load-sub",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Add files to playlist..."),
                Command = "script-message-to mpvgui open-files append",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("File"),
                Name = _("Recent"),
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Name = _("Play/Pause"),
                Command = "script-message-to mpvgui play-pause",
            },
            new MenuItemDefinition()
            {
                Name = _("Stop"),
                Command = "stop",
            },
            new MenuItemDefinition()
            {
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Name = _("Toggle Fullscreen"),
                Command = "cycle fullscreen",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Previous File"),
                Command = "script-message-to mpvgui playlist-add",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Next File"),
                Command = "script-message-to mpvgui playlist-add",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Next Chapter"),
                Command = "add chapter 1",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Previous Chapter"),
                Command = "add chapter -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump Next Frame"),
                Command = "frame-step",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump Previous Frame"),
                Command = "frame-back-step",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump 5 sec forward"),
                Command = "seek 5",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump 5 sec backward"),
                Command = "seek -5",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump 30 sec forward"),
                Command = "seek 30",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump 30 sec backward"),
                Command = "seek -30",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump 5 min forward"),
                Command = "seek 300",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Jump 5 min backward"),
                Command = "seek -300",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Titles"),
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Navigate"),
                Name = _("Chapters"),
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Increase Size"),
                Command = "add video-zoom 0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Decrease Size"),
                Command = "add video-zoom -0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Move Left"),
                Command = "add video-pan-x -0.01",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Move Right"),
                Command = "add video-pan-x 0.01",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Move Up"),
                Command = "add video-pan-y -0.01",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Move Down"),
                Command = "add video-pan-y 0.01",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Decrease Height"),
                Command = "add panscan -0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Increase Height"),
                Command = "add panscan 0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Pan & Scan"),
                Name = _("Reset"),
                Command = "set video-zoom 0; set video-pan-x 0; set video-pan-y 0",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Decrease Contrast"),
                Command = "add contrast -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Increase Contrast"),
                Command = "add contrast 1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Decrease Brightness"),
                Command = "add brightness -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Increase Brightness"),
                Command = "add brightness 1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Decrease Gamma"),
                Command = "add gamma -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Increase Gamma"),
                Command = "add gamma 1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Decrease Saturation"),
                Command = "add saturation -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Increase Saturation"),
                Command = "add saturation 1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Take Screenshot"),
                Command = "async screenshot",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Take Screenshot without subtitles"),
                Command = "async screenshot video",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Toggle Deinterlace"),
                Command = "cycle deinterlace",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Cycle Aspect Ratio"),
                Command = "cycle-values video-aspect 16:9 4:3 2.35:1 -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Video"),
                Name = _("Rotate Video"),
                Command = "cycle-values video-rotate 90 180 270 0",
            },
            new MenuItemDefinition()
            {
                Path = _("Audio"),
                Name = _("Cycle/Next"),
                Command = "cycle audio",
            },
            new MenuItemDefinition()
            {
                Path = _("Audio"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Audio"),
                Name = _("Delay +0.1"),
                Command = "add audio-delay 0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Audio"),
                Name = _("Delay -0.1"),
                Command = "add audio-delay -0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Cycle/Next"),
                Command = "cycle sub",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Toggle Visibility"),
                Command = "cycle sub-visibility",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Delay -0.1"),
                Command = "add sub-delay -0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Delay +0.1"),
                Command = "add sub-delay 0.1",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Move Up"),
                Command = "add sub-pos -1",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Move Down"),
                Command = "add sub-pos 1",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = "-",
                Command = "ignore",
            },
            new MenuItemDefinition()
            {
                Path = _("Subtitle"),
                Name = _("Decrease Subtitle Font Size"),
                Command = "add sub-scale -0.1",
            },
            new MenuItemDefinition()
            {
                Path = _(""),
                Name = _(""),
                Command = "",
            },
            new MenuItemDefinition()
            {
                Path = _(""),
                Name = _(""),
                Command = "",
            },
            new MenuItemDefinition()
            {
                Path = _(""),
                Name = _(""),
                Command = "",
            },
            new MenuItemDefinition()
            {
                Path = _(""),
                Name = _(""),
                Command = "",
            },
            new MenuItemDefinition()
            {
                Path = _(""),
                Name = _(""),
                Command = "",
            },
            new MenuItemDefinition()
            {
                Path = _(""),
                Name = _(""),
                Command = "",
            },
        };
        
        return ret;
    }
}

// G           add sub-scale  0.1     #menu: Subtitle > Increase Subtitle Font Size
// 
// _           ignore                 #menu: Track
// 
// +           add volume  2          #menu: Volume > Up
// -           add volume -2          #menu: Volume > Down
// 0           add volume  2          #menu: Volume > Up
// 9           add volume -2          #menu: Volume > Down
// _           ignore                 #menu: Volume > -
// m           cycle mute             #menu: Volume > Mute
// 
// [           multiply speed 1/1.1   #menu: Speed > -10%
// ]           multiply speed 1.1     #menu: Speed > +10%
// _           ignore                 #menu: Speed > -
// {           multiply speed 0.5     #menu: Speed > Half
// }           multiply speed 2.0     #menu: Speed > Double
// _           ignore                 #menu: Speed > -
// BS          set speed 1            #menu: Speed > Reset
// 
// Alt++   script-message-to mpvgui scale-window 1.2     #menu: View > Zoom > Enlarge
// Alt+-   script-message-to mpvgui scale-window 0.8     #menu: View > Zoom > Shrink
// _       ignore                                        #menu: View > Zoom > -
// Alt+0   script-message-to mpvgui window-scale 0.5     #menu: View > Zoom > 50 %
// Alt+1   script-message-to mpvgui window-scale 1.0     #menu: View > Zoom > 100 %
// Alt+2   script-message-to mpvgui window-scale 2.0     #menu: View > Zoom > 200 %
// Alt+3   script-message-to mpvgui window-scale 3.0     #menu: View > Zoom > 300 %
// 
// Alt+Left  script-message-to mpvgui move-window left   #menu: View > Move > Left
// Alt+Right script-message-to mpvgui move-window right  #menu: View > Move > Right
// Alt+Up    script-message-to mpvgui move-window top    #menu: View > Move > Top
// Alt+Down  script-message-to mpvgui move-window bottom #menu: View > Move > Bottom
// Alt+BS    script-message-to mpvgui move-window center #menu: View > Move > Center
// 
// F8      script-message-to mpvgui show-playlist        #menu: View > Show Playlist
// Ctrl+p  script-message-to mpvgui select-profile       #menu: View > Show Profile Selection
// Ctrl+P  script-message-to mpvgui show-profiles        #menu: View > Show Profiles
// Ctrl+7  script-message-to mpvgui show-audio-tracks    #menu: View > Show Audio Tracks
// Ctrl+8  script-message-to mpvgui show-subtitle-tracks #menu: View > Show Subtitle Tracks
// Ctrl+c  script-message-to mpvgui show-chapters        #menu: View > Show Chapters
// b       cycle border                                  #menu: View > Toggle Border
// Ctrl+t  cycle ontop                                   #menu: View > Toggle On Top 
// t       script-binding stats/display-stats-toggle     #menu: View > Toggle Statistics
// Del     script-binding osc/visibility                 #menu: View > Toggle OSC Visibility
// i       script-message-to mpvgui show-info            #menu: View > Show Media Info
// Ctrl+m  script-message-to mpvgui show-media-info      #menu: View > Show Media Info Advanced
// p       show-progress                                 #menu: View > Show Progress
// Alt+r   script-message-to mpvgui show-recent          #menu: View > Show Recent
// 
// `       script-binding console/enable                 #menu: View > Advanced > Show Console
// _       script-message-to mpvgui show-audio-devices   #menu: View > Advanced > Show Audio Devices
// P       script-message-to mpvgui show-properties      #menu: View > Advanced > Show Properties
// C       script-message-to mpvgui show-commands        #menu: View > Advanced > Show Commands
// _       script-message-to mpvgui show-demuxers        #menu: View > Advanced > Show Demuxers
// _       script-message-to mpvgui show-decoders        #menu: View > Advanced > Show Decoders
// _       script-message-to mpvgui show-protocols       #menu: View > Advanced > Show Protocols
// _       script-message-to mpvgui show-keys            #menu: View > Advanced > Show Keys
// 
// _       ignore                                        #menu: Profile
// 
// Ctrl+,  script-message-to mpvgui show-conf-editor     #menu: Settings > Show Config Editor
// Ctrl+i  script-message-to mpvgui show-input-editor    #menu: Settings > Show Input Editor
// Ctrl+f  script-message-to mpvgui open-conf-folder     #menu: Settings > Open Config Folder
// 
// _       script-message-to mpvgui reg-file-assoc video #menu: Settings > Setup > Register video file associations
// _       script-message-to mpvgui reg-file-assoc audio #menu: Settings > Setup > Register audio file associations
// _       script-message-to mpvgui reg-file-assoc image #menu: Settings > Setup > Register image file associations
// _       script-message-to mpvgui reg-file-assoc unreg #menu: Settings > Setup > Unregister file associations
// 
// l       ab-loop                                       #menu: Tools > Set/clear A-B loop points
// L       cycle-values loop-file inf no                 #menu: Tools > Toggle infinite file looping
// _       playlist-shuffle                              #menu: Tools > Shuffle Playlist
// Ctrl+h  cycle-values hwdec auto no                    #menu: Tools > Toggle Hardware Decoding
// Q       quit-watch-later                              #menu: Tools > Exit Watch Later
// e       run powershell -command "explorer.exe '/select,' ( \"${path}\" -replace '/', '\\' )" #menu: Tools > Show current file in File Explorer
// 
// _ script-message-to mpvgui shell-execute https://mpv.io                    #menu: Help > Website mpv
// _ script-message-to mpvgui shell-execute https://github.com/mpvgui-player/mpv-gui #menu: Help > Website mpv-gui
// _ ignore                                                                   #menu: Help > -
// _ script-message-to mpvgui shell-execute https://mpv.io/manual/stable/     #menu: Help > Manual mpv
// _ script-message-to mpvgui shell-execute https://github.com/mpvgui-player/mpv-gui/blob/master/docs/Manual.md #menu: Help > Manual mpv-gui
// _ ignore                                                                   #menu: Help > -
// _ script-message-to mpvgui show-about                                      #menu: Help > About mpv-gui
// 
// F1 script-message-to mpvgui show-command-palette #menu: Command Palette
// _ ignore                                         #menu: -
// Esc quit                                         #menu: Exit
// 
// MBTN_Right    script-message-to mpvgui show-menu
// 6             script-message-to mpvgui show-progress
// KP6           script-message-to mpvgui show-progress
// KP9           ab-loop
// 7             cycle audio
// Sharp         cycle audio
// Ctrl+F11      script-message-to mpvgui playlist-add -10
// Ctrl+F12      script-message-to mpvgui playlist-add  10
// 8             cycle sub
// j             cycle sub
// q             quit
// Power         quit
// Play          cycle pause
// Pause         cycle pause
// PlayPause     cycle pause
// MBTN_Mid      cycle pause
// Stop          stop
// Forward       seek  60
// Rewind        seek -60
// Wheel_Up      add volume  2 
// Wheel_Down    add volume -2 
// Wheel_Left    add volume -2 
// Wheel_Right   add volume  2 
// Prev          playlist-prev
// Next          playlist-next
// MBTN_Forward  playlist-next
// MBTN_Back     playlist-prev
// >             playlist-next
// <             playlist-prev
// MBTN_Left     ignore
// f             cycle fullscreen
// MBTN_Left_DBL cycle fullscreen
// KP_Enter      cycle fullscreen
// Shift+Right   no-osd seek  1 exact
// Shift+Left    no-osd seek -1 exact
// Shift+Up      no-osd seek  5 exact
// Shift+Down    no-osd seek -5 exact
// Shift+BS      revert-seek      # undo the previous (or marked) seek
// Shift+Ctrl+BS revert-seek mark # mark the position for revert-seek
// Ctrl+Shift+Left  no-osd sub-seek -1 # seek to the previous subtitle
// Ctrl+Shift+Right no-osd sub-seek  1 # seek to the next subtitle
// Ctrl+Wheel_Up    no-osd seek  7
// Ctrl+Wheel_Down  no-osd seek -7
