using System;
using System.Runtime.InteropServices;

namespace Haley.Enums
{
    //reference: https://stackoverflow.com/questions/28040646/transparent-blurred-background-to-canvas
    //reference: https://gist.github.com/walterlv/752669f389978440d344941a5fcd5b00
    //reference: https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-rounded-corners

    //Below are the native enum values required for blur
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
    // Copied from dwmapi.h
    internal enum DWMWINDOWATTRIBUTE {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
    // what value of the enum to set.
    // Copied from dwmapi.h
    internal enum DWM_WINDOW_CORNER_PREFERENCE {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public AccentFlags AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    [Flags]
    internal enum AccentFlags
    {
        DrawLeftBorder = 0x20,
        DrawTopBorder = 0x40,
        DrawRightBorder = 0x80,
        DrawBottomBorder = 0x100,
        DrawAllBorders = DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder
    }

}
