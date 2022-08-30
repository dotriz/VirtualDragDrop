
using System.Runtime.InteropServices.ComTypes;

namespace Advent.Common.Interop
{
    internal class AdviseEntry
    {
        public FORMATETC Format;
        public ADVF Advf;
        public IAdviseSink Sink;

        public AdviseEntry(ref FORMATETC format, ADVF advf, IAdviseSink sink)
        {
            this.Format = format;
            this.Advf = advf;
            this.Sink = sink;
        }
    }
}
