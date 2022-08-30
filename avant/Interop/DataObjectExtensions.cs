
using Advent.Common.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Advent.Common.Interop
{
    public enum DropImageType
    {
        Invalid = -1,
        None = 0,
        Copy = 1,
        Move = 2,
        Link = 4,
        Label = 6,
        Warning = 7,
    }

    public static class DataObjectExtensions
    {
        private static readonly IDictionary<UIElement, DataObjectExtensions.DragSourceEntry> dataContext = (IDictionary<UIElement, DataObjectExtensions.DragSourceEntry>)new Dictionary<UIElement, DataObjectExtensions.DragSourceEntry>();
        private static readonly IDictionary<System.Windows.IDataObject, DataObjectExtensions.DropDescriptionFlags> dropDescriptions = (IDictionary<System.Windows.IDataObject, DataObjectExtensions.DropDescriptionFlags>)new Dictionary<System.Windows.IDataObject, DataObjectExtensions.DropDescriptionFlags>();
        private static readonly IDropTargetHelper instance = (IDropTargetHelper)new DragDropHelper();
        private static readonly Guid ManagedDataStamp = new Guid("D98D9FD6-FA46-4716-A769-F3451DFBE4B4");

        static DataObjectExtensions()
        {
        }

        public static DragDropEffects DoDragDrop(this System.Windows.IDataObject dataObject, UIElement dragSource, System.Windows.Point cursorOffset, DragDropEffects allowedEffects)
        {
            BitmapSource imageFromElement = DataObjectExtensions.GetImageFromElement(dragSource);
            return DataObjectExtensions.DoDragDrop(dataObject, dragSource, imageFromElement, cursorOffset, allowedEffects);
        }

        public static DragDropEffects DoDragDrop(this System.Windows.IDataObject dataObject, UIElement dragSource, BitmapSource dragImage, System.Windows.Point cursorOffset, DragDropEffects allowedEffects)
        {
            DataObjectExtensions.RegisterDefaultDragSource(dragSource, dataObject, cursorOffset, dragImage);
            EnhancedDragEventArgs e = new EnhancedDragEventArgs()
            {
                DataObject = dataObject,
                Effects = allowedEffects
            };
            try
            {
                Advent.Common.UI.DragDrop.OnDragStarted(e);
                e.Effects = System.Windows.DragDrop.DoDragDrop((DependencyObject)dragSource, (object)dataObject, allowedEffects);
                return e.Effects;
            }
            finally
            {
                DataObjectExtensions.DragLeave();
                DataObjectExtensions.UnregisterDefaultDragSource(dragSource);
                Advent.Common.UI.DragDrop.OnDragEnded(e);
            }
        }

        public static void DefaultGiveFeedbackHandler(object sender, GiveFeedbackEventArgs e)
        {
            UIElement key = sender as UIElement;
            if (key == null || !DataObjectExtensions.dataContext.ContainsKey(key))
                return;
            DataObjectExtensions.DefaultGiveFeedback(DataObjectExtensions.dataContext[key].Data, e);
        }

        public static void DefaultGiveFeedback(System.Windows.IDataObject data, GiveFeedbackEventArgs e)
        {
            bool flag1 = false;
            bool flag2 = DataObjectExtensions.IsDropDescriptionDefault(data);
            DropImageType dropImageType = DropImageType.Invalid;
            if (!DataObjectExtensions.IsDropDescriptionValid(data) || flag2)
            {
                dropImageType = DataObjectExtensions.GetDropImageType(data);
                flag1 = true;
            }
            if (DataObjectExtensions.IsShowingLayered(data))
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Arrow);
            }
            else
                e.UseDefaultCursors = true;
            if (DataObjectExtensions.InvalidateRequired(data) || !flag2 || dropImageType != DropImageType.None)
            {
                DataObjectExtensions.InvalidateDragImage(data);
                DataObjectExtensions.SetInvalidateRequired(data, false);
            }
            if (flag1 && e.Effects != (DragDropEffects)dropImageType)
            {
                if (e.Effects == DragDropEffects.Copy)
                    DataObjectExtensions.SetDropDescription(data, DropImageType.Copy, "Copy", string.Empty);
                else if (e.Effects == DragDropEffects.Link)
                    DataObjectExtensions.SetDropDescription(data, DropImageType.Link, "Link", string.Empty);
                else if (e.Effects == DragDropEffects.Move)
                    DataObjectExtensions.SetDropDescription(data, DropImageType.Move, "Move", string.Empty);
                else if (e.Effects == DragDropEffects.None)
                    DataObjectExtensions.SetDropDescription(data, DropImageType.None, (string)null, (string)null);
                DataObjectExtensions.SetDropDescriptionIsDefault(data, true);
                DataObjectExtensions.SetInvalidateRequired(data, true);
            }
            e.Handled = true;
        }

        public static void DefaultQueryContinueDragHandler(object sender, QueryContinueDragEventArgs e)
        {
            DataObjectExtensions.DefaultQueryContinueDrag(e);
        }

        public static void DefaultQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            if (!e.EscapePressed)
                return;
            e.Action = DragAction.Cancel;
            e.Handled = true;
        }

        public static void AllowDropDescription(bool allow)
        {
            ((IDragSourceHelper2)new DragDropHelper()).SetFlags(allow ? 1 : 0);
        }

        public static void InvalidateDragImage(System.Windows.IDataObject dataObject)
        {
            if (!dataObject.GetDataPresent("DragWindow"))
                return;
            Advent.Common.Interop.NativeMethods.PostMessage(DataObjectExtensions.GetIntPtrFromData(dataObject.GetData("DragWindow")), 1027U, IntPtr.Zero, IntPtr.Zero);
        }

        public static void SetDropDescription(this System.Windows.IDataObject dataObject, DropDescription dropDescription)
        {
            FORMATETC formatETC;
            DataObjectExtensions.FillFormatETC("DropDescription", TYMED.TYMED_HGLOBAL, out formatETC);
            IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DropDescription)));
            try
            {
                Marshal.StructureToPtr((object)dropDescription, num, false);
                STGMEDIUM medium;
                medium.pUnkForRelease = (object)null;
                medium.tymed = TYMED.TYMED_HGLOBAL;
                medium.unionmember = num;
                ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(ref formatETC, ref medium, true);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
                throw;
            }
            if (dropDescription.szMessage == null)
                return;
            DataObjectExtensions.SetIsShowingText(dataObject, true);
        }

        public static void SetIsDragTextEnabled(this System.Windows.IDataObject dataObject, bool isDragTextEnabled)
        {
            DataObjectExtensions.SetBoolFormat(dataObject, "DisableDragText", !isDragTextEnabled);
        }

        public static void SetIsShowingText(this System.Windows.IDataObject dataObject, bool isShowingText)
        {
            DataObjectExtensions.SetBoolFormat(dataObject, "IsShowingText", isShowingText);
        }

        public static object GetDropDescription(this System.Windows.IDataObject dataObject)
        {
            FORMATETC formatETC;
            DataObjectExtensions.FillFormatETC("DropDescription", TYMED.TYMED_HGLOBAL, out formatETC);
            System.Runtime.InteropServices.ComTypes.IDataObject dataObject1 = (System.Runtime.InteropServices.ComTypes.IDataObject)dataObject;
            if (dataObject1.QueryGetData(ref formatETC) != 0)
                return (object)null;
            STGMEDIUM medium;
            dataObject1.GetData(ref formatETC, out medium);
            try
            {
                return (object)(DropDescription)Marshal.PtrToStructure(medium.unionmember, typeof(DropDescription));
            }
            finally
            {
                Advent.Common.Interop.NativeMethods.ReleaseStgMedium(ref medium);
            }
        }

        public static int Advise(this System.Windows.IDataObject dataObject, IAdviseSink sink, string format, ADVF advf)
        {
            FORMATETC formatETC;
            DataObjectExtensions.FillFormatETC(format, TYMED.TYMED_HGLOBAL | TYMED.TYMED_FILE | TYMED.TYMED_ISTREAM | TYMED.TYMED_ISTORAGE | TYMED.TYMED_GDI | TYMED.TYMED_MFPICT | TYMED.TYMED_ENHMF, out formatETC);
            int connection;
            int errorCode = ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).DAdvise(ref formatETC, advf, sink, out connection);
            if (errorCode != 0)
                Marshal.ThrowExceptionForHR(errorCode);
            return connection;
        }

        public static VirtualFile[] GetVirtualFiles(this System.Windows.IDataObject dataObject)
        {
            return DataObjectExtensions.GetVirtualFiles(dataObject, true);
        }

        public static VirtualFile[] GetVirtualFiles(this System.Windows.IDataObject dataObject, bool getContents)
        {
            System.Runtime.InteropServices.ComTypes.IDataObject dataObject1 = (System.Runtime.InteropServices.ComTypes.IDataObject)dataObject;
            FORMATETC formatETC1;
            DataObjectExtensions.FillFormatETC("FileGroupDescriptorW", TYMED.TYMED_HGLOBAL, out formatETC1);
            STGMEDIUM medium1;
            dataObject1.GetData(ref formatETC1, out medium1);
            int length = Marshal.ReadInt32(medium1.unionmember);
            IntPtr ptr = new IntPtr(medium1.unionmember.ToInt32() + 4);
            VirtualFile[] virtualFileArray = new VirtualFile[length];
            for (int index = 0; index < length; ++index)
            {
                VirtualFile virtualFile = VirtualFile.FromFileDescriptor((FILEDESCRIPTOR)Marshal.PtrToStructure(ptr, typeof(FILEDESCRIPTOR)));
                if (getContents)
                {
                    FORMATETC formatETC2;
                    DataObjectExtensions.FillFormatETC("FileContents", TYMED.TYMED_HGLOBAL, out formatETC2);
                    formatETC2.lindex = index;
                    formatETC2.tymed = TYMED.TYMED_ISTREAM;
                    STGMEDIUM medium2;
                    dataObject1.GetData(ref formatETC2, out medium2);
                    if (medium2.tymed == TYMED.TYMED_ISTREAM)
                    {
                        System.Runtime.InteropServices.ComTypes.IStream stream = (System.Runtime.InteropServices.ComTypes.IStream)Marshal.GetObjectForIUnknown(medium2.unionmember);
                        if (stream != null)
                        {
                            try
                            {
                                using (ComStream comStream = new ComStream(stream))
                                {
                                    byte[] buffer = new byte[comStream.Length];
                                    comStream.Read(buffer, 0, buffer.Length);
                                    virtualFile.Contents = buffer;
                                }
                            }
                            finally
                            {
                                Marshal.ReleaseComObject((object)stream);
                            }
                        }
                    }
                }
                virtualFileArray[index] = virtualFile;
                ptr = new IntPtr(ptr.ToInt32() + Marshal.SizeOf(typeof(FILEDESCRIPTOR)));
            }
            return virtualFileArray;
        }

        private static STGMEDIUM WriteBytesToMedium(byte[] buffer)
        {
            IntPtr num = Marshal.AllocHGlobal(buffer.Length);
            System.Runtime.InteropServices.ComTypes.IStream ppstm;
            try
            {
                Marshal.Copy(buffer, 0, num, buffer.Length);
                Advent.Common.Interop.NativeMethods.CreateStreamOnHGlobal(num, true, out ppstm);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
                throw;
            }
            return new STGMEDIUM()
            {
                pUnkForRelease = (object)null,
                tymed = TYMED.TYMED_ISTREAM,
                unionmember = Marshal.GetIUnknownForObject((object)ppstm)
            };
        }

        public static void SetVirtualFiles(this Advent.Common.Interop.DataObject dataObject, VirtualFile[] files)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(BitConverter.GetBytes(files.Length), 0, 4);
            for (int index = 0; index < files.Length; ++index)
            {
                VirtualFile file = files[index];
                if (file.ContentsFunc == null)
                {
                    FORMATETC formatIn = new FORMATETC()
                    {
                        cfFormat = (short)DataFormats.GetDataFormat("FileContents").Id,
                        dwAspect = DVASPECT.DVASPECT_CONTENT,
                        lindex = index,
                        ptd = IntPtr.Zero,
                        tymed = TYMED.TYMED_ISTREAM
                    };
                    STGMEDIUM medium = DataObjectExtensions.WriteBytesToMedium(file.Contents);
                    dataObject.SetData(ref formatIn, ref medium, true);
                }
                DataObjectExtensions.WriteFileDescriptor((Stream)memoryStream, file);
            }
            dataObject.DataRequested += (EventHandler<DataRequestedEventArgs>)((sender, args) =>
            {
                int local_0 = (int)(short)DataFormats.GetDataFormat("FileContents").Id;
                if ((int)args.Format.cfFormat != local_0 || args.Format.lindex < 0 || (args.Format.lindex >= files.Length || files[args.Format.lindex].ContentsFunc == null))
                    return;
                args.Medium = DataObjectExtensions.WriteBytesToMedium(files[args.Format.lindex].ContentsFunc());
                args.IsHandled = true;
            });
            IntPtr num = Marshal.AllocHGlobal((int)memoryStream.Length);
            try
            {
                Marshal.Copy(memoryStream.GetBuffer(), 0, num, (int)memoryStream.Length);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
                throw;
            }
            STGMEDIUM medium1;
            medium1.pUnkForRelease = (object)null;
            medium1.tymed = TYMED.TYMED_HGLOBAL;
            medium1.unionmember = num;
            FORMATETC formatIn1 = new FORMATETC()
            {
                cfFormat = (short)DataFormats.GetDataFormat("FileGroupDescriptorW").Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero,
                tymed = TYMED.TYMED_HGLOBAL
            };
            dataObject.SetData(ref formatIn1, ref medium1, true);
        }

        public static void SetVirtualFiles(this System.Windows.IDataObject dataObject, VirtualFile[] files)
        {
            if (files == null)
                throw new ArgumentNullException();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(BitConverter.GetBytes(files.Length), 0, 4);
            for (int index = 0; index < files.Length; ++index)
            {
                VirtualFile file = files[index];
                if (file.Contents == null || file.Contents.Length == 0)
                    throw new ArgumentException("VirtualFile does not have any contents.");
                FORMATETC formatIn = new FORMATETC()
                {
                    cfFormat = (short)DataFormats.GetDataFormat("FileContents").Id,
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = index,
                    ptd = IntPtr.Zero,
                    tymed = TYMED.TYMED_ISTREAM
                };
                STGMEDIUM medium = DataObjectExtensions.WriteBytesToMedium(file.Contents);
                ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(ref formatIn, ref medium, true);
                DataObjectExtensions.WriteFileDescriptor((Stream)memoryStream, file);
            }
            IntPtr num = Marshal.AllocHGlobal((int)memoryStream.Length);
            try
            {
                Marshal.Copy(memoryStream.GetBuffer(), 0, num, (int)memoryStream.Length);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
                throw;
            }
            STGMEDIUM medium1;
            medium1.pUnkForRelease = (object)null;
            medium1.tymed = TYMED.TYMED_HGLOBAL;
            medium1.unionmember = num;
            FORMATETC formatIn1 = new FORMATETC()
            {
                cfFormat = (short)DataFormats.GetDataFormat("FileGroupDescriptorW").Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero,
                tymed = TYMED.TYMED_HGLOBAL
            };
            ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(ref formatIn1, ref medium1, true);
        }

        public static void SetManagedData(this System.Windows.IDataObject dataObject, string format, object data)
        {
            FORMATETC formatETC;
            DataObjectExtensions.FillFormatETC(format, TYMED.TYMED_HGLOBAL, out formatETC);
            STGMEDIUM medium;
            DataObjectExtensions.GetMediumFromObject(data, out medium);
            try
            {
                ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(ref formatETC, ref medium, true);
            }
            catch
            {
                Advent.Common.Interop.NativeMethods.ReleaseStgMedium(ref medium);
                throw;
            }
        }

        public static object GetManagedData(this System.Windows.IDataObject dataObject, string format)
        {
            FORMATETC formatETC;
            DataObjectExtensions.FillFormatETC(format, TYMED.TYMED_HGLOBAL, out formatETC);
            STGMEDIUM medium;
            ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).GetData(ref formatETC, out medium);
            System.Runtime.InteropServices.ComTypes.IStream ppstm;
            try
            {
                if (Advent.Common.Interop.NativeMethods.CreateStreamOnHGlobal(medium.unionmember, true, out ppstm) != 0)
                    return (object)null;
            }
            finally
            {
                Advent.Common.Interop.NativeMethods.ReleaseStgMedium(ref medium);
            }
            System.Runtime.InteropServices.ComTypes.STATSTG pstatstg;
            ppstm.Stat(out pstatstg, 0);
            if (pstatstg.cbSize > (long)int.MaxValue)
                throw new NotSupportedException();
            byte[] numArray1 = new byte[pstatstg.cbSize];
            ppstm.Read(numArray1, (int)pstatstg.cbSize, IntPtr.Zero);
            MemoryStream memoryStream = new MemoryStream(numArray1);
            int count = Marshal.SizeOf(typeof(Guid));
            byte[] numArray2 = new byte[count];
            if (memoryStream.Length >= (long)count && count == memoryStream.Read(numArray2, 0, count))
            {
                Guid g = new Guid(numArray2);
                if (DataObjectExtensions.ManagedDataStamp.Equals(g))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Type type = (Type)binaryFormatter.Deserialize((Stream)memoryStream);
                    object obj = binaryFormatter.Deserialize((Stream)memoryStream);
                    if (obj.GetType() == type)
                        return obj;
                    if (obj is string)
                        return DataObjectExtensions.ConvertDataFromString((string)obj, (ICustomAttributeProvider)type);
                    else
                        return (object)null;
                }
            }
            if (memoryStream.CanSeek)
                memoryStream.Position = 0L;
            return (object)null;
        }

        public static void SetDragImage(this System.Windows.IDataObject dataObject, BitmapSource image, System.Windows.Point cursorOffset)
        {
            Bitmap fromBitmapSource = DataObjectExtensions.GetBitmapFromBitmapSource(image, Colors.Magenta);
            DataObjectExtensions.SetDragImage(dataObject, fromBitmapSource, cursorOffset);
        }

        public static void SetDropDescription(this System.Windows.IDataObject dataObject, DropImageType type, string format, string insert)
        {
            if (format != null && format.Length > 259)
                throw new ArgumentException("Format string exceeds the maximum allowed length of 259.", "format");
            if (insert != null && insert.Length > 259)
                throw new ArgumentException("Insert string exceeds the maximum allowed length of 259.", "insert");
            DropDescription dropDescription;
            dropDescription.type = type;
            dropDescription.szMessage = format;
            dropDescription.szInsert = insert;
            DataObjectExtensions.SetDropDescription(dataObject, dropDescription);
        }

        public static void SetDataEx(this System.Windows.IDataObject dataObject, string format, object data)
        {
            DataFormat dataFormat = DataFormats.GetDataFormat(format);
            FORMATETC formatetc = new FORMATETC()
            {
                cfFormat = (short)dataFormat.Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero
            };
            TYMED compatibleTymed = DataObjectExtensions.GetCompatibleTymed(format, data);
            if (compatibleTymed != TYMED.TYMED_NULL)
            {
                formatetc.tymed = compatibleTymed;
                System.Windows.IDataObject dataObject1 = new System.Windows.DataObject();
                
                dataObject1.SetData(format, data, true);
                
                STGMEDIUM medium;
                
                ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject1).GetData(ref formatetc, out medium);
                
                try
                {
                    ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(ref formatetc, ref medium, true);
                }
                catch
                {
                    Advent.Common.Interop.NativeMethods.ReleaseStgMedium(ref medium);
                    throw;
                }
            }
            else
                DataObjectExtensions.SetManagedData(dataObject, format, data);
        }

        public static object GetDataEx(this System.Windows.IDataObject dataObject, string format)
        {
            object data = dataObject.GetData(format, true);
            if (data is Stream)
            {
                object managedData = DataObjectExtensions.GetManagedData(dataObject, format);
                if (managedData != null)
                    return managedData;
            }
            return data;
        }

        public static void DragEnter(this IDropTargetHelper dropHelper, Window window, System.Windows.IDataObject data, System.Windows.Point cursorOffset, DragDropEffects effect)
        {
            IntPtr hwndTarget = IntPtr.Zero;
            if (window != null)
                hwndTarget = new WindowInteropHelper(window).Handle;
            POINT pt = NativeUtilities.ToWin32Point(cursorOffset);
            dropHelper.DragEnter(hwndTarget, (System.Runtime.InteropServices.ComTypes.IDataObject)data, ref pt, (int)effect);
        }

        public static void DragOver(this IDropTargetHelper dropHelper, System.Windows.Point cursorOffset, DragDropEffects effect)
        {
            POINT pt = NativeUtilities.ToWin32Point(cursorOffset);
            dropHelper.DragOver(ref pt, (int)effect);
        }

        public static void Drop(this IDropTargetHelper dropHelper, System.Windows.IDataObject data, System.Windows.Point cursorOffset, DragDropEffects effect)
        {
            POINT pt = NativeUtilities.ToWin32Point(cursorOffset);
            dropHelper.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)data, ref pt, (int)effect);
        }

        public static void DragEnter(Window window, System.Windows.IDataObject data, System.Windows.Point cursorOffset, DragDropEffects effect)
        {
            DataObjectExtensions.DragEnter(window, data, cursorOffset, effect, (string)null, (string)null);
        }

        public static void DragEnter(Window window, System.Windows.IDataObject data, System.Windows.Point cursorOffset, DragDropEffects effect, string descriptionMessage, string descriptionInsert)
        {
            if (descriptionMessage != null)
            {
                DataObjectExtensions.AllowDropDescription(true);
                DataObjectExtensions.SetDropDescription(data, (DropImageType)effect, descriptionMessage, descriptionInsert);
            }
            DataObjectExtensions.DragEnter(DataObjectExtensions.instance, window, data, cursorOffset, effect);
        }

        public static void DragOver(System.Windows.Point cursorOffset, DragDropEffects effect)
        {
            DataObjectExtensions.DragOver(DataObjectExtensions.instance, cursorOffset, effect);
        }

        public static void DragLeave()
        {
            DataObjectExtensions.instance.DragLeave();
        }

        public static void DragLeave(System.Windows.IDataObject data)
        {
            DataObjectExtensions.SetDropDescription(data, DropImageType.None, (string)null, (string)null);
            DataObjectExtensions.DragLeave();
        }

        public static void Drop(System.Windows.IDataObject data, System.Windows.Point cursorOffset, DragDropEffects effect)
        {
            DataObjectExtensions.Drop(DataObjectExtensions.instance, data, cursorOffset, effect);
        }

        public static void Show(bool show)
        {
            DataObjectExtensions.instance.Show(show);
        }

        private static void WriteFileDescriptor(Stream stream, VirtualFile file)
        {
            FILEDESCRIPTOR filedescriptor = file.ToFileDescriptor();
            int length = Marshal.SizeOf((object)filedescriptor);
            byte[] numArray = new byte[length];
            IntPtr num = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr((object)filedescriptor, num, true);
                Marshal.Copy(num, numArray, 0, length);
            }
            finally
            {
                Marshal.FreeHGlobal(num);
            }
            stream.Write(numArray, 0, numArray.Length);
        }

        private static void RegisterDefaultDragSource(UIElement dragSource, System.Windows.IDataObject data)
        {
            DataObjectExtensions.DragSourceEntry dragSourceEntry = new DataObjectExtensions.DragSourceEntry(data);
            if (!DataObjectExtensions.dataContext.ContainsKey(dragSource))
                DataObjectExtensions.dataContext.Add(dragSource, dragSourceEntry);
            else
                DataObjectExtensions.dataContext[dragSource] = dragSourceEntry;
            dragSourceEntry.AdviseConnection = DataObjectExtensions.Advise(data, (IAdviseSink)new DataObjectExtensions.AdviseSink(data), "DropDescription", (ADVF)0);
            dragSource.GiveFeedback += new GiveFeedbackEventHandler(DataObjectExtensions.DefaultGiveFeedbackHandler);
            dragSource.QueryContinueDrag += new QueryContinueDragEventHandler(DataObjectExtensions.DefaultQueryContinueDragHandler);
        }

        private static BitmapSource GetImageFromElement(UIElement element)
        {
            System.Windows.Size renderSize = element.RenderSize;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)renderSize.Width, (int)renderSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
            renderTargetBitmap.Render((Visual)element);
            return (BitmapSource)renderTargetBitmap;
        }

        private static void RegisterDefaultDragSource(UIElement dragSource, System.Windows.IDataObject data, System.Windows.Point cursorOffset, BitmapSource dragImage)
        {
            DataObjectExtensions.AllowDropDescription(true);
            DataObjectExtensions.SetDragImage(data, dragImage, cursorOffset);
            DataObjectExtensions.RegisterDefaultDragSource(dragSource, data);
        }

        private static void UnregisterDefaultDragSource(UIElement dragSource)
        {
            DataObjectExtensions.DragSourceEntry dragSourceEntry;
            if (!DataObjectExtensions.dataContext.TryGetValue(dragSource, out dragSourceEntry))
                return;
            ((System.Runtime.InteropServices.ComTypes.IDataObject)dragSourceEntry.Data).DUnadvise(dragSourceEntry.AdviseConnection);
            dragSource.GiveFeedback -= new GiveFeedbackEventHandler(DataObjectExtensions.DefaultGiveFeedbackHandler);
            dragSource.QueryContinueDrag -= new QueryContinueDragEventHandler(DataObjectExtensions.DefaultQueryContinueDragHandler);
            DataObjectExtensions.dataContext.Remove(dragSource);
            DataObjectExtensions.dropDescriptions.Remove(dragSourceEntry.Data);
        }

        private static IntPtr GetIntPtrFromData(object data)
        {
            byte[] buffer = (byte[])null;
            if (data is MemoryStream)
            {
                buffer = new byte[4];
                if (4 != ((Stream)data).Read(buffer, 0, 4))
                    throw new ArgumentException("Could not read an IntPtr from the MemoryStream");
            }
            if (data is byte[])
            {
                buffer = (byte[])data;
                if (buffer.Length < 4)
                    throw new ArgumentException("Could not read an IntPtr from the byte array");
            }
            if (buffer == null)
                throw new ArgumentException("Could not read an IntPtr from the " + (object)data.GetType());
            else
                return new IntPtr((int)buffer[3] << 24 | (int)buffer[2] << 16 | (int)buffer[1] << 8 | (int)buffer[0]);
        }

        private static bool IsShowingLayered(System.Windows.IDataObject dataObject)
        {
            if (dataObject.GetDataPresent("IsShowingLayered", true))
            {
                object data = dataObject.GetData("IsShowingLayered");
                if (data != null)
                    return DataObjectExtensions.GetBooleanFromData(data);
            }
            return false;
        }

        private static bool GetBooleanFromData(object data)
        {
            if (data is Stream)
                return new BinaryReader(data as Stream).ReadBoolean();
            else
                return false;
        }

        private static bool IsDropDescriptionValid(System.Windows.IDataObject dataObject)
        {
            object dropDescription = DataObjectExtensions.GetDropDescription(dataObject);
            if (dropDescription is DropDescription)
                return ((DropDescription)dropDescription).type != DropImageType.Invalid;
            else
                return false;
        }

        private static bool IsDropDescriptionDefault(System.Windows.IDataObject dataObject)
        {
            if (DataObjectExtensions.dropDescriptions.ContainsKey(dataObject))
                return (DataObjectExtensions.dropDescriptions[dataObject] & DataObjectExtensions.DropDescriptionFlags.IsDefault) == DataObjectExtensions.DropDescriptionFlags.IsDefault;
            else
                return false;
        }

        private static bool InvalidateRequired(System.Windows.IDataObject dataObject)
        {
            if (DataObjectExtensions.dropDescriptions.ContainsKey(dataObject))
                return (DataObjectExtensions.dropDescriptions[dataObject] & DataObjectExtensions.DropDescriptionFlags.InvalidateRequired) == DataObjectExtensions.DropDescriptionFlags.InvalidateRequired;
            else
                return false;
        }

        private static void SetDropDescriptionIsDefault(System.Windows.IDataObject dataObject, bool isDefault)
        {
            if (isDefault)
                DataObjectExtensions.SetDropDescriptionFlag(dataObject, DataObjectExtensions.DropDescriptionFlags.IsDefault);
            else
                DataObjectExtensions.UnsetDropDescriptionFlag(dataObject, DataObjectExtensions.DropDescriptionFlags.IsDefault);
        }

        private static void SetInvalidateRequired(System.Windows.IDataObject dataObject, bool required)
        {
            if (required)
                DataObjectExtensions.SetDropDescriptionFlag(dataObject, DataObjectExtensions.DropDescriptionFlags.InvalidateRequired);
            else
                DataObjectExtensions.UnsetDropDescriptionFlag(dataObject, DataObjectExtensions.DropDescriptionFlags.InvalidateRequired);
        }

        private static void SetDropDescriptionFlag(System.Windows.IDataObject dataObject, DataObjectExtensions.DropDescriptionFlags flag)
        {
            if (DataObjectExtensions.dropDescriptions.ContainsKey(dataObject))
            {
                IDictionary<System.Windows.IDataObject, DataObjectExtensions.DropDescriptionFlags> dictionary;
                System.Windows.IDataObject index;
                (dictionary = DataObjectExtensions.dropDescriptions)[index = dataObject] = dictionary[index] | flag;
            }
            else
                DataObjectExtensions.dropDescriptions.Add(dataObject, flag);
        }

        private static void UnsetDropDescriptionFlag(System.Windows.IDataObject dataObject, DataObjectExtensions.DropDescriptionFlags flag)
        {
            if (!DataObjectExtensions.dropDescriptions.ContainsKey(dataObject))
                return;
            DataObjectExtensions.DropDescriptionFlags descriptionFlags = DataObjectExtensions.dropDescriptions[dataObject];
            DataObjectExtensions.dropDescriptions[dataObject] = (descriptionFlags | flag) ^ flag;
        }

        private static DropImageType GetDropImageType(System.Windows.IDataObject dataObject)
        {
            object dropDescription = DataObjectExtensions.GetDropDescription(dataObject);
            if (dropDescription is DropDescription)
                return ((DropDescription)dropDescription).type;
            else
                return DropImageType.Invalid;
        }

        private static void SetBoolFormat(this System.Windows.IDataObject dataObject, string format, bool val)
        {
            FORMATETC formatETC;
            DataObjectExtensions.FillFormatETC(format, TYMED.TYMED_HGLOBAL, out formatETC);
            IntPtr num = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.Copy(BitConverter.GetBytes(val ? 1 : 0), 0, num, 4);
                STGMEDIUM medium;
                medium.pUnkForRelease = (object)null;
                medium.tymed = TYMED.TYMED_HGLOBAL;
                medium.unionmember = num;
                ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(ref formatETC, ref medium, true);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
                throw;
            }
        }

        private static void FillFormatETC(string format, TYMED tymed, out FORMATETC formatETC)
        {
            formatETC.cfFormat = (short)Advent.Common.Interop.NativeMethods.RegisterClipboardFormat(format);
            formatETC.dwAspect = DVASPECT.DVASPECT_CONTENT;
            formatETC.lindex = -1;
            formatETC.ptd = IntPtr.Zero;
            formatETC.tymed = tymed;
        }

        private static void GetMediumFromObject(object data, out STGMEDIUM medium)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(DataObjectExtensions.ManagedDataStamp.ToByteArray(), 0, Marshal.SizeOf(typeof(Guid)));
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize((Stream)memoryStream, (object)data.GetType());
            binaryFormatter.Serialize((Stream)memoryStream, DataObjectExtensions.GetAsSerializable(data));
            byte[] buffer = memoryStream.GetBuffer();
            IntPtr num = Marshal.AllocHGlobal(buffer.Length);
            try
            {
                Marshal.Copy(buffer, 0, num, buffer.Length);
            }
            catch
            {
                Marshal.FreeHGlobal(num);
                throw;
            }
            medium.unionmember = num;
            medium.tymed = TYMED.TYMED_HGLOBAL;
            medium.pUnkForRelease = (object)null;
        }

        private static object GetAsSerializable(object obj)
        {
            if (obj.GetType().IsSerializable)
                return obj;
            TypeConverter converterForType = DataObjectExtensions.GetTypeConverterForType((ICustomAttributeProvider)obj.GetType());
            if (converterForType != null && converterForType.CanConvertTo(typeof(string)) && converterForType.CanConvertFrom(typeof(string)))
                return (object)converterForType.ConvertToInvariantString(obj);
            else
                throw new NotSupportedException("Cannot serialize the object");
        }

        private static object ConvertDataFromString(string data, ICustomAttributeProvider dataType)
        {
            TypeConverter converterForType = DataObjectExtensions.GetTypeConverterForType(dataType);
            if (converterForType != null && converterForType.CanConvertFrom(typeof(string)))
                return converterForType.ConvertFromInvariantString(data);
            else
                throw new NotSupportedException("Cannot convert data");
        }

        private static TypeConverter GetTypeConverterForType(ICustomAttributeProvider dataType)
        {
            TypeConverterAttribute[] converterAttributeArray = (TypeConverterAttribute[])dataType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            if (converterAttributeArray.Length > 0)
                return (TypeConverter)Activator.CreateInstance(Type.GetType(converterAttributeArray[0].ConverterTypeName));
            else
                return (TypeConverter)null;
        }

        private static void SetDragImage(this System.Windows.IDataObject dataObject, Bitmap bitmap, System.Windows.Point cursorOffset)
        {
            ShDragImage dragImage = new ShDragImage();
            Win32Size win32Size;
            win32Size.cx = bitmap.Width;
            win32Size.cy = bitmap.Height;
            dragImage.sizeDragImage = win32Size;
            POINT point;
            point.X = (int)Math.Max(Math.Min(cursorOffset.X, (double)bitmap.Width * 0.8), (double)bitmap.Width * 0.2);
            point.Y = (int)Math.Max(Math.Min(cursorOffset.Y, (double)bitmap.Height * 0.8), (double)bitmap.Height * 0.2);
            dragImage.ptOffset = point;
            dragImage.crColorKey = System.Drawing.Color.Magenta.ToArgb();
            IntPtr hbitmap = bitmap.GetHbitmap();
            dragImage.hbmpDragImage = hbitmap;
            try
            {
                IDragSourceHelper dragSourceHelper = (IDragSourceHelper)new DragDropHelper();
                try
                {
                    dragSourceHelper.InitializeFromBitmap(ref dragImage, (System.Runtime.InteropServices.ComTypes.IDataObject)dataObject);
                }
                catch (NotImplementedException ex)
                {
                    throw new Exception("A NotImplementedException was caught. This could be because you forgot to construct your DataObject using a DragDropLib.DataObject", (Exception)ex);
                }
            }
            catch
            {
                Advent.Common.Interop.NativeMethods.DeleteObject(hbitmap);
            }
        }

        private static TYMED GetCompatibleTymed(string format, object data)
        {
            if (DataObjectExtensions.IsFormatEqual(format, DataFormats.Bitmap) && (data is Bitmap || data is BitmapSource))
                return TYMED.TYMED_GDI;
            if (DataObjectExtensions.IsFormatEqual(format, DataFormats.EnhancedMetafile))
                return TYMED.TYMED_ENHMF;
            if (DataObjectExtensions.IsFormatEqual(format, StrokeCollection.InkSerializedFormat))
                return TYMED.TYMED_ISTREAM;
            return data is Stream || DataObjectExtensions.IsFormatEqual(format, DataFormats.Html) || (DataObjectExtensions.IsFormatEqual(format, DataFormats.Xaml) || DataObjectExtensions.IsFormatEqual(format, DataFormats.Text)) || (DataObjectExtensions.IsFormatEqual(format, DataFormats.Rtf) || DataObjectExtensions.IsFormatEqual(format, DataFormats.OemText) || (DataObjectExtensions.IsFormatEqual(format, DataFormats.UnicodeText) || DataObjectExtensions.IsFormatEqual(format, "ApplicationTrust"))) || (DataObjectExtensions.IsFormatEqual(format, DataFormats.FileDrop) || DataObjectExtensions.IsFormatEqual(format, "FileName") || DataObjectExtensions.IsFormatEqual(format, "FileNameW")) || (!DataObjectExtensions.IsFormatEqual(format, DataFormats.Dib) || !(data is Image)) && (DataObjectExtensions.IsFormatEqual(format, typeof(BitmapSource).FullName) || DataObjectExtensions.IsFormatEqual(format, typeof(Bitmap).FullName) || !DataObjectExtensions.IsFormatEqual(format, DataFormats.EnhancedMetafile) && !(data is Metafile) && (DataObjectExtensions.IsFormatEqual(format, DataFormats.Serializable) || data is ISerializable || data != null && data.GetType().IsSerializable)) ? TYMED.TYMED_HGLOBAL : TYMED.TYMED_NULL;
        }

        private static bool IsFormatEqual(string formatA, string formatB)
        {
            return string.CompareOrdinal(formatA, formatB) == 0;
        }

        private static Bitmap GetBitmapFromBitmapSource(BitmapSource source, System.Windows.Media.Color transparencyKey)
        {
            Int32Rect int32Rect = new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);
            System.Drawing.Imaging.PixelFormat format = DataObjectExtensions.ConvertPixelFormat(source.Format);
            Bitmap bitmap = new Bitmap(int32Rect.Width, int32Rect.Height, format);
            if ((format & System.Drawing.Imaging.PixelFormat.Indexed) == System.Drawing.Imaging.PixelFormat.Indexed)
                DataObjectExtensions.ConvertColorPalette(bitmap.Palette, source.Palette);
            System.Drawing.Color transKey = DataObjectExtensions.ToDrawingColor(transparencyKey);
            BitmapData bitmapData = bitmap.LockBits(DataObjectExtensions.ToDrawingRectangle(int32Rect), ImageLockMode.ReadWrite, format);
            source.CopyPixels(int32Rect, bitmapData.Scan0, bitmapData.Stride * int32Rect.Height, bitmapData.Stride);
            if ((format & System.Drawing.Imaging.PixelFormat.Alpha) == System.Drawing.Imaging.PixelFormat.Alpha)
                DataObjectExtensions.ReplaceTransparentPixelsWithTransparentKey(bitmapData, transKey);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private static unsafe void ReplaceTransparentPixelsWithTransparentKey(BitmapData bmpData, System.Drawing.Color transKey)
        {
            System.Drawing.Imaging.PixelFormat pixelFormat = bmpData.PixelFormat;
            if (System.Drawing.Imaging.PixelFormat.Format32bppArgb == pixelFormat || System.Drawing.Imaging.PixelFormat.Format32bppPArgb == pixelFormat)
            {
                int num1 = transKey.ToArgb();
                byte* numPtr1 = (byte*)bmpData.Scan0.ToPointer();
                int num2 = 0;
                while (num2 < bmpData.Height)
                {
                    int* numPtr2 = (int*)numPtr1;
                    int num3 = 0;
                    while (num3 < bmpData.Width)
                    {
                        if (((long)*numPtr2 & 4278190080L) == 0L)
                            *numPtr2 = num1;
                        ++num3;
                        ++numPtr2;
                    }
                    ++num2;
                    numPtr1 += bmpData.Stride;
                }
            }
            else
                Trace.TraceWarning("Not converting transparent colors to transparency key.");
        }

        private static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb((int)color.A, (int)color.R, (int)color.G, (int)color.B);
        }

        private static Rectangle ToDrawingRectangle(this Int32Rect rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        private static void ConvertColorPalette(ColorPalette destPalette, BitmapPalette bitmapPalette)
        {
            System.Drawing.Color[] entries = destPalette.Entries;
            IList<System.Windows.Media.Color> colors = bitmapPalette.Colors;
            if (entries.Length < colors.Count)
                throw new ArgumentException("Destination palette has less entries than the source palette");
            int index = 0;
            for (int count = colors.Count; index < count; ++index)
                entries[index] = DataObjectExtensions.ToDrawingColor(colors[index]);
        }

        private static System.Drawing.Imaging.PixelFormat ConvertPixelFormat(System.Windows.Media.PixelFormat pixelFormat)
        {
            if (PixelFormats.Bgr24 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            if (PixelFormats.Bgr32 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            if (PixelFormats.Bgr555 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
            if (PixelFormats.Bgr565 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
            if (PixelFormats.Bgra32 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            if (PixelFormats.BlackWhite == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
            if (PixelFormats.Gray16 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
            if (PixelFormats.Indexed1 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
            if (PixelFormats.Indexed4 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
            if (PixelFormats.Indexed8 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            if (PixelFormats.Pbgra32 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format32bppPArgb;
            if (PixelFormats.Prgba64 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format64bppPArgb;
            if (PixelFormats.Rgb24 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            if (PixelFormats.Rgb48 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format48bppRgb;
            if (PixelFormats.Rgba64 == pixelFormat)
                return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
            else
                throw new NotSupportedException("The pixel format of the source bitmap is not supported.");
        }

        [System.Flags]
        private enum DropDescriptionFlags
        {
            IsDefault = 1,
            InvalidateRequired = 2,
        }

        private class DragSourceEntry
        {
            public readonly System.Windows.IDataObject Data;

            public int AdviseConnection { get; set; }

            public DragSourceEntry(System.Windows.IDataObject data)
            {
                this.Data = data;
            }
        }

        private class AdviseSink : IAdviseSink
        {
            private readonly System.Windows.IDataObject data;

            public AdviseSink(System.Windows.IDataObject data)
            {
                this.data = data;
            }

            public void OnDataChange(ref FORMATETC format, ref STGMEDIUM stgmedium)
            {
                if (DataObjectExtensions.GetDropDescription(this.data) == null)
                    return;
                DataObjectExtensions.SetDropDescriptionIsDefault(this.data, false);
            }

            public void OnClose()
            {
                throw new NotImplementedException();
            }

            public void OnRename(IMoniker moniker)
            {
                throw new NotImplementedException();
            }

            public void OnSave()
            {
                throw new NotImplementedException();
            }

            public void OnViewChange(int aspect, int index)
            {
                throw new NotImplementedException();
            }
        }
    }
}
