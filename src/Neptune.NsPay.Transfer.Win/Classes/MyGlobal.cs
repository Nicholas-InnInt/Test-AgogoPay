using DotNetBrowser.Input.Keyboard.Events;
using DotNetBrowser.Input.Keyboard;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Neptune.NsPay.Transfer.Win.Models;

namespace Neptune.NsPay.Transfer.Win.Classes
{
    public static class MyGlobal
    {
        public static void InvokeOnUiThreadIfRequired(this Control control, Action action)
        {
            if (control.Disposing || control.IsDisposed || !control.IsHandleCreated)
            {
                return;
            }

            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        public static Bitmap ToBitmap(this DotNetBrowser.Ui.Bitmap bitmap)
        {
            int width = (int)bitmap.Size.Width;
            int height = (int)bitmap.Size.Height;

            byte[] data = bitmap.Pixels.ToArray();
            Bitmap bmp = new Bitmap(width,
                                    height,
                                    PixelFormat.Format32bppRgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                              ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public static KeyCodeModel ToMyKeyCode(this char character)
        {
            return new KeyCodeModel()
            {
                Key = KeyCode.Oem1,
                KeyChar = character.ToString(),
                Modifiers = null,
            };
        }

        public static void SendKeyboardKey(this IKeyboard keyboard, KeyCode key, string keyChar, KeyModifiers modifiers = null)
        {
            modifiers = modifiers ?? new KeyModifiers();
            KeyPressedEventArgs keyDownEventArgs = new KeyPressedEventArgs
            {
                KeyChar = keyChar,
                VirtualKey = key,
                Modifiers = modifiers
            };

            KeyTypedEventArgs keyPressEventArgs = new KeyTypedEventArgs
            {
                KeyChar = keyChar,
                VirtualKey = key,
                Modifiers = modifiers
            };
            KeyReleasedEventArgs keyUpEventArgs = new KeyReleasedEventArgs
            {
                VirtualKey = key,
                Modifiers = modifiers
            };

            keyboard.KeyPressed.Raise(keyDownEventArgs);
            keyboard.KeyTyped.Raise(keyPressEventArgs);
            keyboard.KeyReleased.Raise(keyUpEventArgs);
        }

        public static DotNetBrowser.Geometry.Point ToDpiPoint(this DotNetBrowser.Geometry.Point oldPoint, int dpi)
        {
            var dpiRate = 1 / (96 / (float)dpi);
            return new DotNetBrowser.Geometry.Point((int)(oldPoint.X * dpiRate), (int)(oldPoint.Y * dpiRate));
        }
    }
}
