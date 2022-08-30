
using Advent.Common.Interop;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DragVirtual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // From Windows SDK header files
        private const string CFSTR_INETURLA = "UniformResourceLocator";

        public MainWindow()
        {
            InitializeComponent();

            // Attach to interesting events
            LocalFile.MouseLeftButtonDown += new MouseButtonEventHandler(LocalFile_MouseButtonDown);
            Text.MouseLeftButtonDown += new MouseButtonEventHandler(Text_MouseButtonDown);
            TextUrl.MouseLeftButtonDown += new MouseButtonEventHandler(TextUrl_MouseButtonDown);
            VirtualFile.MouseLeftButtonDown += new MouseButtonEventHandler(VirtualFile_MouseButtonDown);
            VirtualFile2.MouseLeftButtonDown += new MouseButtonEventHandler(VirtualFile2_MouseButtonDown);
            VirtualFileAvant.MouseLeftButtonDown += new MouseButtonEventHandler(VirtualFileAvant_MouseButtonDown);
            TextUrlVirtualFile.MouseLeftButtonDown += new MouseButtonEventHandler(TextUrlVirtualFile_MouseButtonDown);
        }

        private void LocalFile_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            string filePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\flower.jpg";
            // get IDataObject from the Shell so it can handle more formats
            var dataObject = DragDataObject.GetFileDataObject(filePath);

            // add the thumbnail to the data object
            DragDataObject.AddPreviewImage(dataObject, filePath);

            var result = DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
        }
        
        private void Text_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var virtualFileDataObject = new VirtualFileDataObject();

            // Provide simple text (in the form of a NULL-terminated ANSI string)
            virtualFileDataObject.SetData(
                (short)(DataFormats.GetDataFormat(DataFormats.Text).Id),
                Encoding.Default.GetBytes("This is some sample text\0"));

            DoDragDropOrClipboardSetDataObject(e.ChangedButton, Text, virtualFileDataObject, DragDropEffects.Copy);
        }

        private void TextUrl_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var virtualFileDataObject = new VirtualFileDataObject();

            // Provide simple text and an URL in priority order
            // (both in the form of a NULL-terminated ANSI string)
            virtualFileDataObject.SetData(
                (short)(DataFormats.GetDataFormat(CFSTR_INETURLA).Id),
                Encoding.Default.GetBytes("https://dotriz.com/wp-content/uploads/2019/07/vc-runtime1-40-dll-missing-3.png\0"));
            virtualFileDataObject.SetData(
                (short)(DataFormats.GetDataFormat(DataFormats.Text).Id),
                Encoding.Default.GetBytes("https://dotriz.com/wp-content/uploads/2019/07/vc-runtime1-40-dll-missing-3.png\0"));

            DoDragDropOrClipboardSetDataObject(e.ChangedButton, TextUrl, virtualFileDataObject, DragDropEffects.Copy);
        }

        private void VirtualFile_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var virtualFileDataObject = new VirtualFileDataObject(
                null,
                (vfdo) =>
                {
                    if (DragDropEffects.Move == vfdo.PerformedDropEffect)
                    {
                        // Hide the element that was moved (or cut)
                        // BeginInvoke ensures UI operations happen on the right thread
                        Dispatcher.BeginInvoke((Action)(() => VirtualFile.Visibility = Visibility.Hidden));
                    }
                });

            // Provide a virtual file (generated on demand) containing the letters 'a'-'z'
            virtualFileDataObject.SetData(new VirtualFileDataObject.FileDescriptor[]
            {
                new VirtualFileDataObject.FileDescriptor
                {
                    Name = "Alphabet.txt",
                    //Length = 26,
                    ChangeTimeUtc = DateTime.Now.AddDays(-1),
                    StreamContents = stream =>
                        {
                            var contents = Enumerable.Range('a', 26).Select(i => (byte)i).ToArray();
                            stream.Write(contents, 0, contents.Length);
                        }
                },
            });

            DoDragDropOrClipboardSetDataObject(e.ChangedButton, VirtualFile, virtualFileDataObject, DragDropEffects.Copy);
        }

        private void VirtualFile2_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var virtualFileDataObject = new VirtualFileDataObject();

            virtualFileDataObject.SetData(new VirtualFileDataObject.FileDescriptor[]
            {
                new VirtualFileDataObject.FileDescriptor
                {
                    Name = "test.zip",
                    ChangeTimeUtc = DateTime.Now.AddDays(-1),
                    StreamContents = stream =>
                        {
                            using(var webClient = new WebClient())
                            {
                                var data = webClient.DownloadData("https://dotriz.com/tools/test.zip");
                                stream.Write(data, 0, data.Length);
                            }

                        }
                },
            });

            DoDragDropOrClipboardSetDataObject(e.ChangedButton, VirtualFile2, virtualFileDataObject, DragDropEffects.Copy);
        }

        private void VirtualFileAvant_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {

            byte[] numArray = Enumerable.Range('a', 26).Select(i => (byte)i).ToArray();
            IDataObject dataObject1 = Advent.Common.Interop.DataObject.CreateDataObject();
            VirtualFile[] virtualFileArray = new VirtualFile[1];
            virtualFileArray[0] = new VirtualFile()
            {
                Name = "test.txt",
                LastWriteTime = DateTime.Now,
                Contents = numArray
            };
            VirtualFile[] files = virtualFileArray;
            DataObjectExtensions.SetVirtualFiles(dataObject1, files);
            try
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\flower.jpg"; ;
                var bitmapImage = new Bitmap(path);
                //VmcStudioUtil.DragDropObject = (object)this;
                DataObjectExtensions.DoDragDrop(dataObject1, VirtualFileAvant, Convert(bitmapImage), new System.Windows.Point(), DragDropEffects.Copy);
            }
            finally
            {
                //VmcStudioUtil.DragDropObject = (object)null;
            }

        }
        

        private void TextUrlVirtualFile_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var virtualFileDataObject = new VirtualFileDataObject(
                // BeginInvoke ensures UI operations happen on the right thread
                (vfdo) => Dispatcher.BeginInvoke((Action)(() => BusyScreen.Visibility = Visibility.Visible)),
                (vfdo) => Dispatcher.BeginInvoke((Action)(() => BusyScreen.Visibility = Visibility.Collapsed)));

            // Provide a virtual file (downloaded on demand), its URL, and descriptive text
            virtualFileDataObject.SetData(new VirtualFileDataObject.FileDescriptor[]
            {
                new VirtualFileDataObject.FileDescriptor
                {
                    Name = "test.data",
                    StreamContents = stream =>
                        {
                            using(var webClient = new WebClient())
                            {
                                var data = webClient.DownloadData("https://dotriz.com/tools/test.zip");
                                stream.Write(data, 0, data.Length);
                            }
                        }
                },
            });
            virtualFileDataObject.SetData(
                (short)(DataFormats.GetDataFormat(CFSTR_INETURLA).Id),
                Encoding.Default.GetBytes("https://dotriz.com/tools/test.zip\0"));
            virtualFileDataObject.SetData(
                (short)(DataFormats.GetDataFormat(DataFormats.Text).Id),
                Encoding.Default.GetBytes("[Test Data zip file]\0"));

            DoDragDropOrClipboardSetDataObject(e.ChangedButton, TextUrl, virtualFileDataObject, DragDropEffects.Copy);
        }

        private static void DoDragDropOrClipboardSetDataObject(MouseButton button, DependencyObject dragSource, VirtualFileDataObject virtualFileDataObject, DragDropEffects allowedEffects)
        {
            try
            {
                VirtualFileDataObject.DoDragDrop(dragSource, virtualFileDataObject, allowedEffects);
            }
            catch (COMException)
            {
                // Failure; no way to recover
            }
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
    }
}
