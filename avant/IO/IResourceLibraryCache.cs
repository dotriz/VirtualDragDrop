
namespace Advent.Common.IO
{
    public interface IResourceLibraryCache
    {
        IResourceLibrary this[string name] { get; }

        void Clear();
    }
}
