
using System.Collections.Generic;

namespace Advent.Common.IO
{
    public interface IResource
    {
        IResourceLibrary Library { get; }

        IEnumerable<ushort> Languages { get; }

        string Name { get; }

        object Type { get; }

        bool Exists(ushort? language);

        void Update(byte[] data, ushort? language);

        byte[] GetBytes(ushort? language);
    }
}
