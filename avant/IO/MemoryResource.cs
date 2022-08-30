
using System.Collections.Generic;

namespace Advent.Common.IO
{
    public class MemoryResource : IResource
    {
        private readonly MemoryLibrary library;
        private readonly IDictionary<ushort, byte[]> values;

        public IResourceLibrary Library
        {
            get
            {
                return (IResourceLibrary)this.library;
            }
        }

        public IEnumerable<ushort> Languages
        {
            get
            {
                return (IEnumerable<ushort>)this.values.Keys;
            }
        }

        public string Name { get; private set; }

        public object Type { get; private set; }

        protected virtual ushort DefaultLanguage
        {
            get
            {
                return this.library.DefaultLanguage ?? (ushort)0;
            }
        }

        public MemoryResource(MemoryLibrary memoryLibrary, string name, object type)
        {
            this.library = memoryLibrary;
            this.Type = type;
            this.Name = name;
            this.values = (IDictionary<ushort, byte[]>)new Dictionary<ushort, byte[]>();
        }

        public bool Exists(ushort? language)
        {
            IDictionary<ushort, byte[]> dictionary = this.values;
            ushort? nullable = language;
            int num = nullable.HasValue ? (int)nullable.GetValueOrDefault() : (int)this.DefaultLanguage;
            return dictionary.ContainsKey((ushort)num);
        }

        public void Update(byte[] data, ushort? language)
        {
            IDictionary<ushort, byte[]> dictionary = this.values;
            ushort? nullable = language;
            int num = nullable.HasValue ? (int)nullable.GetValueOrDefault() : (int)this.DefaultLanguage;
            byte[] numArray = data;
            dictionary[(ushort)num] = numArray;
            this.library.AddResource(this);
        }

        public byte[] GetBytes(ushort? language)
        {
            IDictionary<ushort, byte[]> dictionary = this.values;
            ushort? nullable = language;
            int num = nullable.HasValue ? (int)nullable.GetValueOrDefault() : (int)this.DefaultLanguage;
            byte[] numArray;

            dictionary.TryGetValue((ushort)num, out numArray);
            return numArray;
        }
    }
}
