
using Advent.Common.Interop;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Advent.Common.UI
{
    public class WindowPreview : IDisposable
    {
        private readonly IntPtr sourceHandle;
        private readonly Window window;
        private IntPtr thumbnailHandle;
        private bool clientAreaOnly;
        private double opacity;
        private bool isVisible;

        public FrameworkElement Destination { get; private set; }

        public bool ClientAreaOnly
        {
            get
            {
                return this.clientAreaOnly;
            }
            set
            {
                this.clientAreaOnly = value;
                this.OnPropertyUpdated();
            }
        }

        public double Opacity
        {
            get
            {
                return this.opacity;
            }
            set
            {
                this.opacity = value;
                this.OnPropertyUpdated();
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }
            set
            {
                this.isVisible = value;
                this.UpdateThumbnail();
            }
        }

        public WindowPreview(FrameworkElement destination, IntPtr sourceHandle)
        {
            this.sourceHandle = sourceHandle;
            this.window = UIExtensions.GetAncestor<Window>((DependencyObject)destination);
            Marshal.ThrowExceptionForHR(Advent.Common.Interop.NativeMethods.DwmRegisterThumbnail(new WindowInteropHelper(this.window).Handle, sourceHandle, ref this.thumbnailHandle));
            this.Destination = destination;
            this.opacity = 1.0;
            this.clientAreaOnly = false;
            this.isVisible = false;
            destination.LayoutUpdated += new EventHandler(this.Destination_LayoutUpdated);
        }

        public static BitmapSource GetWindowImage(IntPtr windowHandle, bool clientOnly)
        {
            Advent.Common.Interop.RECT lpRect;
            if (clientOnly)
                Advent.Common.Interop.NativeMethods.GetClientRect(windowHandle, out lpRect);
            else
                Advent.Common.Interop.NativeMethods.GetWindowRect(windowHandle, out lpRect);
            using (Bitmap bitmap = new Bitmap(lpRect.Width, lpRect.Height))
            {
                using (Graphics graphics = Graphics.FromImage((Image)bitmap))
                {
                    IntPtr hdc = graphics.GetHdc();
                    try
                    {
                        Advent.Common.Interop.NativeMethods.PrintWindow(windowHandle, hdc, clientOnly ? 1U : 0U);
                    }
                    finally
                    {
                        graphics.ReleaseHdc(hdc);
                    }
                    IntPtr hbitmap = bitmap.GetHbitmap();
                    try
                    {
                        return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        Advent.Common.Interop.NativeMethods.DeleteObject(hbitmap);
                    }
                }
            }
        }

        public BitmapSource TakeScreenshot(bool clientOnly)
        {
            return WindowPreview.GetWindowImage(this.sourceHandle, clientOnly);
        }

        public void Dispose()
        {
            if (!(this.thumbnailHandle != IntPtr.Zero))
                return;
            this.Destination.LayoutUpdated -= new EventHandler(this.Destination_LayoutUpdated);
            Advent.Common.Interop.NativeMethods.DwmUnregisterThumbnail(this.thumbnailHandle);
            this.thumbnailHandle = IntPtr.Zero;
        }

        private void OnPropertyUpdated()
        {
            if (!this.IsVisible)
                return;
            this.UpdateThumbnail();
        }

        private void UpdateThumbnail()
        {
            DwmThumbnailProperties props = new DwmThumbnailProperties();
            props.Flags = DwmThumbnailFlags.RectDestination | DwmThumbnailFlags.Opacity | DwmThumbnailFlags.Visible | DwmThumbnailFlags.SourceClientAreaOnly;
            props.SourceClientAreaOnly = this.ClientAreaOnly;
            props.Opacity = (byte)(this.Opacity * (double)byte.MaxValue);
            props.Visible = this.IsVisible;
            System.Windows.Point point1 = this.Destination.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement)this.window);
            System.Windows.Point point2 = this.Destination.TranslatePoint(new System.Windows.Point(this.Destination.ActualWidth, this.Destination.ActualHeight), (UIElement)this.window);
            props.rcDestination = new Advent.Common.Interop.RECT((int)point1.X, (int)point1.Y, (int)point2.X, (int)point2.Y);
            Marshal.ThrowExceptionForHR(Advent.Common.Interop.NativeMethods.DwmUpdateThumbnailProperties(this.thumbnailHandle, ref props));
        }

        private void Destination_LayoutUpdated(object sender, EventArgs e)
        {
            try
            {
                this.OnPropertyUpdated();
            }
            catch (Exception)
            {
            }
        }
    }
}
