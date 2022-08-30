
using System;
using System.Collections.Generic;

namespace Advent.Common.IO
{
    public interface IResourceLibrary : IDisposable
    {
        ushort? DefaultLanguage { get; }

        IEnumerable<IResource> this[object resourceType] { get; }

        IEnumerable<object> ResourceTypes { get; }

        IResource GetResource(string name, object type);

        string GetStringResource(int id, ushort? language);
    }
}
