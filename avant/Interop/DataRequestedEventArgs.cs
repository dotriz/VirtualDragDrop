
using System;
using System.Runtime.InteropServices.ComTypes;

namespace Advent.Common.Interop
{
    public class DataRequestedEventArgs : EventArgs
    {
        public FORMATETC Format { get; set; }

        public STGMEDIUM Medium { get; set; }

        public bool IsHandled { get; set; }

        public DataRequestedEventArgs(FORMATETC format, STGMEDIUM medium)
        {
            this.Format = format;
            this.Medium = medium;
        }
    }
}
