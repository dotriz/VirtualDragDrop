
using Advent.Common.Interop;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;


namespace Advent.Common.UI
{
    public static class WindowUtil
    {
        public static readonly DependencyProperty GlassMarginProperty = DependencyProperty.RegisterAttached("GlassMargin", typeof(Thickness), typeof(WindowUtil), (PropertyMetadata)new FrameworkPropertyMetadata((object)new Thickness(), new PropertyChangedCallback(WindowUtil.OnGlassMarginChanged)));
        public static readonly DependencyProperty IsDialogProperty = DependencyProperty.RegisterAttached("IsDialogProperty", typeof(bool), typeof(WindowUtil), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, new PropertyChangedCallback(WindowUtil.OnIsDialogChanged)));

        static WindowUtil()
        {
        }

        public static void SetGlassMargin(DependencyObject element, Thickness value)
        {
            element.SetValue(WindowUtil.GlassMarginProperty, (object)value);
        }

        public static Thickness GetGlassMargin(DependencyObject element)
        {
            return (Thickness)element.GetValue(WindowUtil.GlassMarginProperty);
        }

        public static void SetIsDialog(DependencyObject element, bool value)
        {
            element.SetValue(WindowUtil.IsDialogProperty, value);
        }

        public static bool GetIsDialog(DependencyObject element)
        {
            return (bool)element.GetValue(WindowUtil.IsDialogProperty);
        }

        public static void RemoveCaption(this Window w)
        {
            IntPtr handle = new WindowInteropHelper(w).Handle;
            int newStyle = -65537 & (-131073 & (-524289 & NativeMethods.GetWindowLong(handle, -16)));
            NativeMethods.SetWindowLong(handle, -16, newStyle);
            NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 39U);
        }

        private static void SetIsDialog(this Window w, bool isDialog)
        {
            IntPtr handle = new WindowInteropHelper(w).Handle;
            int windowLong = NativeMethods.GetWindowLong(handle, -20);
            int newStyle = !isDialog ? -2 & windowLong : windowLong | 1;
            NativeMethods.SetWindowLong(handle, -20, newStyle);
            NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 39U);
        }

        private static bool SetGlassMargin(this Window window, Thickness margin)
        {
            if (!NativeMethods.DwmIsCompositionEnabled())
                return false;
            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("The Window must be loaded before extending glass.");
            window.Background = (Brush)Brushes.Transparent;
            HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Colors.Transparent;
            MARGINS margins = new MARGINS(margin);
            NativeMethods.DwmExtendFrameIntoClientArea(handle, ref margins);
            return true;
        }

        private static void OnGlassMarginChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Thickness margin = (Thickness)args.NewValue;
            Window window = (Window)obj;
            if (margin.Top != 0.0 || margin.Left != 0.0 || (margin.Bottom != 0.0 || margin.Right != 0.0))
            {
                window.MouseDown += new MouseButtonEventHandler(WindowUtil.Window_MouseDown);
                if (!window.IsLoaded)
                    window.Loaded += new RoutedEventHandler(WindowUtil.Window_GlassMargin_Loaded);
            }
            else
            {
                window.MouseDown -= new MouseButtonEventHandler(WindowUtil.Window_MouseDown);
                window.Loaded -= new RoutedEventHandler(WindowUtil.Window_GlassMargin_Loaded);
            }
            if (!window.IsLoaded)
                return;
            WindowUtil.SetGlassMargin(window, margin);
        }

        private static void OnIsDialogChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            bool isDialog = (bool)args.NewValue;
            
            Window w = obj as Window;
            // TODO During design this crashes because either the window instance 
            // doesn't exist, or something funky either way, this may need to be looked at
            if (w == null)
                return;
            if (isDialog && !w.IsLoaded)
                w.Loaded += new RoutedEventHandler(WindowUtil.Window_IsDialog_Loaded);
            if (!w.IsLoaded)
                return;
            WindowUtil.SetIsDialog(w, isDialog);
        }

        private static void Window_GlassMargin_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = (Window)sender;
            WindowUtil.SetGlassMargin(window, WindowUtil.GetGlassMargin((DependencyObject)window));
            window.Loaded -= new RoutedEventHandler(WindowUtil.Window_GlassMargin_Loaded);
        }

        private static void Window_IsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)sender;
            WindowUtil.SetIsDialog(w, WindowUtil.GetIsDialog((DependencyObject)w));
            w.Loaded -= new RoutedEventHandler(WindowUtil.Window_IsDialog_Loaded);
        }

        private static void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            IntPtr handle = new WindowInteropHelper((Window)sender).Handle;
            NativeMethods.ReleaseCapture(handle);
            NativeMethods.SendMessage(handle, 161U, 2, 0);
        }
    }
}
