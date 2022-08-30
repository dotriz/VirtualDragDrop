
using System;
using System.Collections.Generic;

namespace Advent.Common.IO
{
    public class MemoryLibrary : IResourceLibrary, IDisposable
    {
        private readonly IDictionary<object, IDictionary<string, IResource>> resources;

        public ushort? DefaultLanguage { get; private set; }

        public IEnumerable<IResource> this[object resourceType]
        {
            get
            {
                IDictionary<string, IResource> dictionary;
                if (this.resources.TryGetValue(resourceType, out dictionary))
                    return (IEnumerable<IResource>)dictionary.Values;
                else
                    return (IEnumerable<IResource>)new IResource[0];
            }
        }

        public IEnumerable<object> ResourceTypes
        {
            get
            {
                return (IEnumerable<object>)this.resources.Keys;
            }
        }

        public MemoryLibrary(ushort defaultLanguage)
        {
            this.DefaultLanguage = new ushort?(defaultLanguage);
            this.resources = (IDictionary<object, IDictionary<string, IResource>>)new Dictionary<object, IDictionary<string, IResource>>();
        }

        public void Dispose()
        {
        }

        public IResource GetResource(string name, object type)
        {
            IResource resource = (IResource)null;
            IDictionary<string, IResource> dictionary;
            if (this.resources.TryGetValue(type, out dictionary))
                dictionary.TryGetValue(name, out resource);
            return resource ?? (IResource)new MemoryResource(this, name, type);
        }

        public string GetStringResource(int id, ushort? language)
        {
            throw new NotSupportedException();
        }

        internal void AddResource(MemoryResource resource)
        {
            IDictionary<string, IResource> dictionary;
            if (!this.resources.TryGetValue(resource.Type, out dictionary))
            {
                dictionary = (IDictionary<string, IResource>)new Dictionary<string, IResource>();
                this.resources[resource.Type] = dictionary;
            }
            dictionary[resource.Name] = (IResource)resource;
        }
    }
}
