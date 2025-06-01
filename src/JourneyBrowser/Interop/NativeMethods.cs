using System.Runtime.InteropServices;

namespace JourneyBrowser.Interop
{
    public class NativeMethods
    {
        #region Methods

        #region dwmapi.dll
        
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

        #endregion

        #region Public Static

        public static int ExtendFrame(IntPtr hwnd, MARGINS margins)
        {
            return DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter)
        {
            return DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
        }

        #endregion

        #endregion
    }
}