
using System.Collections.Generic;

namespace Advent.Common.IO
{
    public class MemoryLibraryCache : IResourceLibraryCache
    {
        private readonly IDictionary<string, IResourceLibrary> libraries;
        private readonly ushort defaultLanguage;

        public IResourceLibrary this[string name]
        {
            get
            {
                IResourceLibrary resourceLibrary;
                if (!this.libraries.TryGetValue(name, out resourceLibrary))
                {
                    resourceLibrary = (IResourceLibrary)new MemoryLibrary(this.defaultLanguage);
                    this.libraries[name] = resourceLibrary;
                }
                return resourceLibrary;
            }
        }

        public IEnumerable<string> Libraries
        {
            get
            {
                return (IEnumerable<string>)this.libraries.Keys;
            }
        }

        public MemoryLibraryCache(ushort language)
        {
            this.libraries = (IDictionary<string, IResourceLibrary>)new Dictionary<string, IResourceLibrary>();
            this.defaultLanguage = language;
        }

        public void Clear()
        {
            this.libraries.Clear();
        }

        public void ApplyTo(IResourceLibraryCache libraryCache)
        {
            foreach (string index in this.Libraries)
            {
                IResourceLibrary resourceLibrary1 = this[index];
                IResourceLibrary resourceLibrary2 = libraryCache[index];
                foreach (int num in resourceLibrary1.ResourceTypes)
                {
                    foreach (IResource resource in resourceLibrary1[(object)num])
                        ResourceExtensions.Update(resourceLibrary2.GetResource(resource.Name, (object)num), ResourceExtensions.GetBytes(resource));
                }
            }
        }
    }
}
