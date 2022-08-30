
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows;

namespace Advent.Common.Interop
{
    [ComVisible(true)]
    public class DataObject : System.Runtime.InteropServices.ComTypes.IDataObject, IDisposable
    {
        private int nextConnectionId = 1;
        private readonly IList<KeyValuePair<FORMATETC, STGMEDIUM>> storage;
        private readonly IDictionary<int, AdviseEntry> connections;
        private EventHandler<DataRequestedEventArgs> dataRequested;

        public event EventHandler<DataRequestedEventArgs> DataRequested
        {
            add
            {
                EventHandler<DataRequestedEventArgs> eventHandler = this.dataRequested;
                EventHandler<DataRequestedEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<DataRequestedEventArgs>>(ref this.dataRequested, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<DataRequestedEventArgs> eventHandler = this.dataRequested;
                EventHandler<DataRequestedEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<DataRequestedEventArgs>>(ref this.dataRequested, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public DataObject()
        {
            this.storage = (IList<KeyValuePair<FORMATETC, STGMEDIUM>>)new List<KeyValuePair<FORMATETC, STGMEDIUM>>();
            this.connections = (IDictionary<int, AdviseEntry>)new Dictionary<int, AdviseEntry>();
        }

        ~DataObject()
        {
            this.Dispose(false);
        }

        public static System.Windows.IDataObject CreateDataObject()
        {
            return (System.Windows.IDataObject)new System.Windows.DataObject((object)new Advent.Common.Interop.DataObject());
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
        {
            throw Marshal.GetExceptionForHR(-2147221501);
        }

        public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
        {
            formatOut = formatIn;
            return -2147221404;
        }

        public int DAdvise(ref FORMATETC formatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
        {
            if (((advf | ADVF.ADVF_NODATA | ADVF.ADVF_PRIMEFIRST | ADVF.ADVF_ONLYONCE) ^ (ADVF.ADVF_NODATA | ADVF.ADVF_PRIMEFIRST | ADVF.ADVF_ONLYONCE)) != (ADVF)0)
            {
                connection = 0;
                return -2147221501;
            }
            else
            {
                this.connections.Add(this.nextConnectionId, new AdviseEntry(ref formatetc, advf, adviseSink));
                connection = this.nextConnectionId;
                ++this.nextConnectionId;
                KeyValuePair<FORMATETC, STGMEDIUM> dataEntry;
                if ((advf & ADVF.ADVF_PRIMEFIRST) == ADVF.ADVF_PRIMEFIRST && this.GetDataEntry(ref formatetc, out dataEntry))
                    this.RaiseDataChanged(connection, ref dataEntry);
                return 0;
            }
        }

        public void DUnadvise(int connection)
        {
            this.connections.Remove(connection);
        }

        public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
        {
            if (DATADIR.DATADIR_GET == direction)
                return (IEnumFORMATETC)new Advent.Common.Interop.DataObject.EnumFORMATETC(this.storage);
            else
                throw new NotImplementedException("OLE_S_USEREG");
        }

        public void GetData(ref FORMATETC format, out STGMEDIUM medium)
        {
            medium = new STGMEDIUM();
            this.GetDataHere(ref format, ref medium);
        }

        public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
        {
            DataRequestedEventArgs args = new DataRequestedEventArgs(format, medium);
            this.OnDataRequested(args);
            if (!args.IsHandled)
            {
                KeyValuePair<FORMATETC, STGMEDIUM> dataEntry;
                if (this.GetDataEntry(ref format, out dataEntry))
                {
                    STGMEDIUM medium1 = dataEntry.Value;
                    medium = Advent.Common.Interop.DataObject.CopyMedium(ref medium1);
                }
                else
                    medium = new STGMEDIUM();
            }
            else
            {
                format = args.Format;
                STGMEDIUM medium1 = args.Medium;
                medium = Advent.Common.Interop.DataObject.CopyMedium(ref medium1);
            }
        }

        public int QueryGetData(ref FORMATETC format)
        {
            if ((DVASPECT.DVASPECT_CONTENT & format.dwAspect) == (DVASPECT)0)
                return -2147221397;
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> keyValuePair in (IEnumerable<KeyValuePair<FORMATETC, STGMEDIUM>>)this.storage)
            {
                if ((int)keyValuePair.Key.cfFormat == (int)format.cfFormat)
                    return 0;
            }
            return -2147221399;
        }

        public void SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
        {
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> keyValuePair in (IEnumerable<KeyValuePair<FORMATETC, STGMEDIUM>>)this.storage)
            {
                if ((keyValuePair.Key.tymed & formatIn.tymed) > TYMED.TYMED_NULL && keyValuePair.Key.dwAspect == formatIn.dwAspect && ((int)keyValuePair.Key.cfFormat == (int)formatIn.cfFormat && keyValuePair.Key.lindex == formatIn.lindex))
                {
                    STGMEDIUM pmedium = keyValuePair.Value;
                    Advent.Common.Interop.NativeMethods.ReleaseStgMedium(ref pmedium);
                    this.storage.Remove(keyValuePair);
                    break;
                }
            }
            STGMEDIUM stgmedium = medium;
            if (!release)
                stgmedium = Advent.Common.Interop.DataObject.CopyMedium(ref medium);
            KeyValuePair<FORMATETC, STGMEDIUM> dataEntry = new KeyValuePair<FORMATETC, STGMEDIUM>(formatIn, stgmedium);
            this.storage.Add(dataEntry);
            this.RaiseDataChanged(ref dataEntry);
        }

        private static STGMEDIUM CopyMedium(ref STGMEDIUM medium)
        {
            STGMEDIUM pstgmedDest = new STGMEDIUM();
            int errorCode = Advent.Common.Interop.NativeMethods.CopyStgMedium(ref medium, ref pstgmedDest);
            if (errorCode != 0)
                throw Marshal.GetExceptionForHR(errorCode);
            else
                return pstgmedDest;
        }

        protected virtual void OnDataRequested(DataRequestedEventArgs args)
        {
            EventHandler<DataRequestedEventArgs> eventHandler = this.dataRequested;
            if (eventHandler == null)
                return;
            eventHandler((object)this, args);
        }

        private bool GetDataEntry(ref FORMATETC formatetc, out KeyValuePair<FORMATETC, STGMEDIUM> dataEntry)
        {
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> keyValuePair in (IEnumerable<KeyValuePair<FORMATETC, STGMEDIUM>>)this.storage)
            {
                FORMATETC key = keyValuePair.Key;
                if (this.IsFormatCompatible(ref formatetc, ref key) && formatetc.lindex == key.lindex)
                {
                    dataEntry = keyValuePair;
                    return true;
                }
            }
            dataEntry = new KeyValuePair<FORMATETC, STGMEDIUM>();
            return false;
        }

        private void RaiseDataChanged(int connection, ref KeyValuePair<FORMATETC, STGMEDIUM> dataEntry)
        {
            AdviseEntry adviseEntry = this.connections[connection];
            FORMATETC key = dataEntry.Key;
            STGMEDIUM stgmedium = (adviseEntry.Advf & ADVF.ADVF_NODATA) == ADVF.ADVF_NODATA ? new STGMEDIUM() : dataEntry.Value;
            adviseEntry.Sink.OnDataChange(ref key, ref stgmedium);
            if ((adviseEntry.Advf & ADVF.ADVF_ONLYONCE) != ADVF.ADVF_ONLYONCE)
                return;
            this.connections.Remove(connection);
        }

        private void RaiseDataChanged(ref KeyValuePair<FORMATETC, STGMEDIUM> dataEntry)
        {
            foreach (KeyValuePair<int, AdviseEntry> keyValuePair in (IEnumerable<KeyValuePair<int, AdviseEntry>>)this.connections)
            {
                if (this.IsFormatCompatible(keyValuePair.Value.Format, dataEntry.Key))
                    this.RaiseDataChanged(keyValuePair.Key, ref dataEntry);
            }
        }

        private bool IsFormatCompatible(FORMATETC format1, FORMATETC format2)
        {
            return this.IsFormatCompatible(ref format1, ref format2);
        }

        private bool IsFormatCompatible(ref FORMATETC format1, ref FORMATETC format2)
        {
            bool flag1 = (format1.tymed & format2.tymed) > TYMED.TYMED_NULL;
            bool flag2 = (int)format1.cfFormat == (int)format2.cfFormat;
            bool flag3 = (format1.dwAspect & format2.dwAspect) > (DVASPECT)0;
            if (flag1 && flag2)
                return flag3;
            else
                return false;
        }

        private void Dispose(bool disposing)
        {
            //int num = disposing ? 1 : 0;
            this.ClearStorage();
        }

        private void ClearStorage()
        {
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> keyValuePair in (IEnumerable<KeyValuePair<FORMATETC, STGMEDIUM>>)this.storage)
            {
                STGMEDIUM pmedium = keyValuePair.Value;
                Advent.Common.Interop.NativeMethods.ReleaseStgMedium(ref pmedium);
            }
            this.storage.Clear();
        }

        [ComVisible(true)]
        private class EnumFORMATETC : IEnumFORMATETC
        {
            private FORMATETC[] formats;
            private int currentIndex;

            internal EnumFORMATETC(IList<KeyValuePair<FORMATETC, STGMEDIUM>> storage)
            {
                this.formats = new FORMATETC[storage.Count];
                for (int index = 0; index < this.formats.Length; ++index)
                    this.formats[index] = storage[index].Key;
            }

            private EnumFORMATETC(FORMATETC[] formats)
            {
                this.formats = new FORMATETC[formats.Length];
                formats.CopyTo((Array)this.formats, 0);
            }

            public void Clone(out IEnumFORMATETC newEnum)
            {
                newEnum = (IEnumFORMATETC)new Advent.Common.Interop.DataObject.EnumFORMATETC(this.formats)
                {
                    currentIndex = this.currentIndex
                };
            }

            public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
            {
                if (pceltFetched != null && pceltFetched.Length > 0)
                    pceltFetched[0] = 0;
                int num = celt;
                if (celt <= 0 || rgelt == null || this.currentIndex >= this.formats.Length || (pceltFetched == null || pceltFetched.Length < 1) && celt != 1)
                    return 1;
                if (rgelt.Length < celt)
                    throw new ArgumentException("The number of elements in the return array is less than the number of elements requested");
                int index = 0;
                for (; this.currentIndex < this.formats.Length && num > 0; ++this.currentIndex)
                {
                    rgelt[index] = this.formats[this.currentIndex];
                    ++index;
                    --num;
                }
                if (pceltFetched != null && pceltFetched.Length > 0)
                    pceltFetched[0] = celt - num;
                return num != 0 ? 1 : 0;
            }

            public int Reset()
            {
                this.currentIndex = 0;
                return 0;
            }

            public int Skip(int celt)
            {
                if (this.currentIndex + celt > this.formats.Length)
                    return 1;
                this.currentIndex += celt;
                return 0;
            }
        }
    }
}
