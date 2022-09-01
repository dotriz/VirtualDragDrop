
using System;
using System.Drawing;
using System.IO;
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
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }


        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            // Attach to interesting events
            LocalFile.MouseLeftButtonDown += new MouseButtonEventHandler(LocalFile_MouseButtonDown);
            VirtualFile.MouseDown += new MouseButtonEventHandler(VirtualFile_MouseButtonDown);
            VirtualFile2.MouseDown += new MouseButtonEventHandler(VirtualFile2_MouseButtonDown);
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
            //var virtualFileDataObject = new VirtualFileDataObject();
            var virtualFileDataObject = new VirtualFileDataObject(
                // BeginInvoke ensures UI operations happen on the right thread
                (vfdo) => Dispatcher.BeginInvoke((Action)(() => BusyScreen.Visibility = Visibility.Visible)),
                (vfdo) => Dispatcher.BeginInvoke((Action)(() => BusyScreen.Visibility = Visibility.Collapsed)));

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

        private static void DoDragDropOrClipboardSetDataObject(MouseButton button, DependencyObject dragSource, VirtualFileDataObject virtualFileDataObject, DragDropEffects allowedEffects)
        {
            try
            {
                if (button == MouseButton.Left)
                {
                    // Left button is used to start a drag/drop operation
                    VirtualFileDataObject.DoDragDrop(dragSource, virtualFileDataObject, allowedEffects);
                }
                else if (button == MouseButton.Right)
                {
                    // Right button is used to copy to the clipboard
                    // Communicate the preferred behavior to the destination
                    virtualFileDataObject.PreferredDropEffect = allowedEffects;
                    Clipboard.SetDataObject(virtualFileDataObject);

                    Instance.txtMessage.Text = "File copied to clipboard. Use Ctrl+V to paste";
                    System.Threading.Tasks.Task.Delay(3000).ContinueWith((_) => {
                        Application.Current.Dispatcher.Invoke(
                        () =>
                        {
                            Instance.txtMessage.Text = "";
                        });
                        
                    });
                }
            }
            catch (COMException)
            {
                // Failure; no way to recover
            }
        }
    }
}
