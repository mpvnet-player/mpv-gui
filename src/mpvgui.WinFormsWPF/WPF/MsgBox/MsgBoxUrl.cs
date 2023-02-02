﻿
using System;
using System.Windows.Media;

namespace MsgBoxEx
{
    public class MsgBoxUrl
    {
        public Uri URL { get; set; }
        public string DisplayName { get; set; }
        public System.Windows.Media.Color Foreground { get; set; }

        public MsgBoxUrl()
        {
            Foreground = MessageBoxEx.DefaultUrlForegroundColor;
        }
    }
}
