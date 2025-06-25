using System.Runtime.InteropServices;

namespace JourneyBrowser.Interop
{
    internal static class NativeMethods
    {
        #region Methods

        #region dwmapi.dll

        /// <summary>
        /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window. For programming guidance, and code examples, see Controlling non-client region rendering.
        /// </summary>
        /// <param name="hwnd">The handle to the window for which the attribute value is to be set.</param>
        /// <param name="dwAttribute">A flag describing which value to set, specified as a value of the <see cref="DWMWINDOWATTRIBUTE" /> enumeration. This parameter specifies which attribute to set, and the pvAttribute parameter points to an object containing the attribute value.</param>
        /// <param name="pvAttribute">A pointer to an object containing the attribute value to set. The type of the value set depends on the value of the dwAttribute parameter. The <see cref="DWMWINDOWATTRIBUTE" /> enumeration topic indicates, in the row for each flag, what type of value you should pass a pointer to in the pvAttribute parameter.</param>
        /// <param name="cbAttribute">The size, in bytes, of the attribute value being set via the pvAttribute parameter. The type of the value set, and therefore its size in bytes, depends on the value of the dwAttribute parameter.</param>
        /// <returns>
        /// <para>If the function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</para>
        /// <para>If Desktop Composition has been disabled(Windows 7 and earlier), then this function returns DWM_E_COMPOSITIONDISABLED.</para>
        /// </returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

        #endregion

        #region Public Static

        /// <summary>
        /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window. For programming guidance, and code examples, see Controlling non-client region rendering.
        /// </summary>
        /// <param name="hwnd">The handle to the window for which the attribute value is to be set.</param>
        /// <param name="dwAttribute">A flag describing which value to set, specified as a value of the <see cref="DWMWINDOWATTRIBUTE" /> enumeration. This parameter specifies which attribute to set, and the pvAttribute parameter points to an object containing the attribute value.</param>
        /// <param name="pvAttribute">A pointer to an object containing the attribute value to set. The type of the value set depends on the value of the dwAttribute parameter. The <see cref="DWMWINDOWATTRIBUTE" /> enumeration topic indicates, in the row for each flag, what type of value you should pass a pointer to in the pvAttribute parameter.</param>
        /// <returns>
        /// <para>If the function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</para>
        /// <para>If Desktop Composition has been disabled(Windows 7 and earlier), then this function returns DWM_E_COMPOSITIONDISABLED.</para>
        /// </returns>
        public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, int pvAttribute)
        {
            // A simple wrapper for the DwmSetWindowAttribute method that automatically populates "cbAttribute".

            return DwmSetWindowAttribute(hwnd, dwAttribute, ref pvAttribute, Marshal.SizeOf<int>());
        }

        #endregion

        #endregion
    }
}