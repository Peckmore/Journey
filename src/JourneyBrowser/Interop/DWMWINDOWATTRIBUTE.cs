namespace JourneyBrowser.Interop
{
    /// <summary>
    /// Options used by the DwmGetWindowAttribute and DwmSetWindowAttribute functions.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute" />
    [Flags]
    internal enum DWMWINDOWATTRIBUTE
    {
        /// <summary>
        /// <para>Use with DwmSetWindowAttribute. Allows the window frame for this window to be drawn in dark mode colors when the dark mode system setting is enabled. For compatibility reasons, all windows default to light mode regardless of the system setting. The pvAttribute parameter points to a value of type BOOL. TRUE to honor dark mode for the window, FALSE to always use light mode.</para>
        /// <para>This value is supported starting with Windows 11 Build 22000.</para>
        /// </summary>
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,

        /// <summary>
        /// <para>Use with DwmGetWindowAttribute or DwmSetWindowAttribute. Retrieves or specifies the system-drawn backdrop material of a window, including behind the non-client area. The pvAttribute parameter points to a value of type DWM_SYSTEMBACKDROP_TYPE.</para>
        /// <para>This value is supported starting with Windows 11 Build 22621.</para>
        /// </summary>
        DWMWA_SYSTEMBACKDROP_TYPE = 38
    }
}