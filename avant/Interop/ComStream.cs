
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Advent.Common.Interop
{
    public class ComStream : Stream
    {
        private IStream originalStream;

        public IStream UnderlyingStream
        {
            get
            {
                return this.originalStream;
            }
        }

        public override long Length
        {
            get
            {
                if (this.originalStream == null)
                    throw new ObjectDisposedException("originalStream");
                STATSTG pstatstg;
                this.originalStream.Stat(out pstatstg, 1);
                return pstatstg.cbSize;
            }
        }

        public override long Position
        {
            get
            {
                return this.Seek(0L, SeekOrigin.Current);
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public ComStream(IStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            this.originalStream = stream;
        }

        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            if (this.originalStream == null)
                throw new ObjectDisposedException("originalStream");
            if (offset != 0)
                throw new NotSupportedException("Only 0 offset is supported.");
            int num;
            IntPtr pcbRead = new IntPtr((void*)&num);
            this.originalStream.Read(buffer, count, pcbRead);
            return num;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.originalStream == null)
                throw new ObjectDisposedException("originalStream");
            if (offset != 0)
                throw new NotSupportedException("Only 0 offset is supported.");
            this.originalStream.Write(buffer, count, IntPtr.Zero);
        }

        public override unsafe long Seek(long offset, SeekOrigin origin)
        {
            if (this.originalStream == null)
                throw new ObjectDisposedException("originalStream");
            long num = 0L;
            IntPtr plibNewPosition = new IntPtr((void*)&num);
            this.originalStream.Seek(offset, (int)origin, plibNewPosition);
            return num;
        }

        public override void SetLength(long value)
        {
            if (this.originalStream == null)
                throw new ObjectDisposedException("originalStream");
            this.originalStream.SetSize(value);
        }

        public override void Close()
        {
            if (this.originalStream == null)
                return;
            this.originalStream.Commit(0);
            this.originalStream = (IStream)null;
        }

        public override void Flush()
        {
            this.originalStream.Commit(0);
        }
    }
}
