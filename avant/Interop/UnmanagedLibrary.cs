
using Advent.Common.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Advent.Common.Interop
{
    public enum UnmanagedLibraryAccess
    {
        Read,
        Write,
    }

    public class UnmanagedLibrary : IResourceLibrary, IDisposable
    {
        private static readonly Dictionary<string, UnmanagedLibrary.LibraryInfo> librariesDict = new Dictionary<string, UnmanagedLibrary.LibraryInfo>();
        private readonly UnmanagedLibrary.LibraryInfo libraryInfo;
        private readonly string fileName;
        private readonly UnmanagedLibraryAccess mode;
        private readonly IntPtr resourceHandle;
        private readonly Dictionary<int, UnmanagedLibrary.StringTable> stringTables;
        private ushort? languageID;

        public ushort? DefaultLanguage
        {
            get
            {
                return this.languageID;
            }
            set
            {
                this.languageID = value;
            }
        }

        public string File
        {
            get
            {
                return this.fileName;
            }
        }

        internal IntPtr Handle
        {
            get
            {
                if (this.mode == UnmanagedLibraryAccess.Read)
                    return this.libraryInfo.Handle;
                else
                    return this.resourceHandle;
            }
        }

        public IEnumerable<IResource> this[object resourceType]
        {
            get
            {
                return (IEnumerable<IResource>)new ResourceCollection(this, resourceType);
            }
        }

        protected UnmanagedLibraryAccess Mode
        {
            get
            {
                return this.mode;
            }
        }

        public IEnumerable<object> ResourceTypes
        {
            get
            {
                UnmanagedLibrary.ResourceTypeHelper resourceTypeHelper = new UnmanagedLibrary.ResourceTypeHelper();
                if (!NativeMethods.EnumResourceTypes(this.Handle, new NativeMethods.EnumResTypeProc(resourceTypeHelper.EnumResourceTypesCallback), new IntPtr(0)))
                    throw new Win32Exception();
                else
                    return resourceTypeHelper.ResourceTypes;
            }
        }

        static UnmanagedLibrary()
        {
        }

        public UnmanagedLibrary(string file)
            : this(file, UnmanagedLibraryAccess.Read)
        {
        }

        public UnmanagedLibrary(string file, UnmanagedLibraryAccess mode)
            : this(file, mode, new ushort?())
        {
        }

        public UnmanagedLibrary(string file, UnmanagedLibraryAccess mode, ushort? languageID)
        {
            this.fileName = file;
            this.mode = mode;
            this.languageID = languageID;
            this.libraryInfo = (UnmanagedLibrary.LibraryInfo)null;
            lock (UnmanagedLibrary.librariesDict)
            {
                if (!UnmanagedLibrary.librariesDict.TryGetValue(file, out this.libraryInfo) && mode == UnmanagedLibraryAccess.Read)
                {
                    this.libraryInfo = new UnmanagedLibrary.LibraryInfo();
                    this.libraryInfo.File = file;
                    this.libraryInfo.Handle = NativeMethods.LoadLibraryEx(file, IntPtr.Zero, 2U);
                    if (this.libraryInfo.Handle == IntPtr.Zero)
                        throw new Win32Exception();
                    UnmanagedLibrary.librariesDict[file] = this.libraryInfo;
                }
                if (this.libraryInfo != null)
                {
                    if (mode != UnmanagedLibraryAccess.Read)
                        throw new InvalidOperationException(string.Format("Cannot open {0} for updating as it is already open for reading.", (object)file));
                    ++this.libraryInfo.ReferenceCount;
                }
            }
            if (mode != UnmanagedLibraryAccess.Write)
                return;
            this.stringTables = new Dictionary<int, UnmanagedLibrary.StringTable>();
            this.resourceHandle = NativeMethods.BeginUpdateResource(file, false);
            if (this.resourceHandle == IntPtr.Zero)
                throw new Win32Exception();
        }

        ~UnmanagedLibrary()
        {
            this.Dispose(false);
        }

        public string GetStringResource(int id, ushort? language)
        {
            int blockID;
            int index;
            this.GetStringTableFromStringID(id, out blockID, out index);
            byte[] bytes = this.GetResource(blockID, (object)6).GetBytes(language);
            if (bytes != null)
                return new UnmanagedLibrary.StringTable(bytes)[index];
            else
                return (string)null;
        }

        public IResource GetResource(string name, object type)
        {
            return (IResource)new Resource(this, name, type);
        }

        public IResource GetResource(int id, object resType)
        {
            return (IResource)new Resource(this, id, resType);
        }

        public void UpdateStringResource(int id, string value)
        {
            this.UpdateStringResource(id, value, this.DefaultLanguage);
        }

        public void UpdateStringResource(int id, string value, ushort? language)
        {
            this.CheckIsWriteMode();
            int blockID;
            int index;
            this.GetStringTableFromStringID(id, out blockID, out index);
            UnmanagedLibrary.StringTable stringTable;
            if (!this.stringTables.TryGetValue(blockID, out stringTable))
            {
                byte[] bytes;
                using (new UnmanagedLibrary(this.fileName))
                    bytes = this.GetResource(blockID, (object)6).GetBytes(language);
                stringTable = new UnmanagedLibrary.StringTable(bytes);
                this.stringTables[blockID] = stringTable;
            }
            stringTable[index] = value;
        }

        public Resource GetMUI()
        {
            return new Resource(this, 1, (object)"MUI");
        }

        public string GetMUIFile()
        {
            foreach (string path in this.GetMUIFilePaths())
            {
                if (System.IO.File.Exists(path))
                    return path;
            }
            return (string)null;
        }

        public string[] GetMUIFilePaths()
        {
            List<string> list = new List<string>();
            long pululEnumerator = 0L;
            StringBuilder pwszLanguage = new StringBuilder(85);
            StringBuilder pwszFileMUIPath = new StringBuilder(260);
            int pcchFileMUIPath = 260;
            for (int pcchLanguage = 85; NativeMethods.GetFileMUIPath(16, this.File, pwszLanguage, ref pcchLanguage, pwszFileMUIPath, ref pcchFileMUIPath, ref pululEnumerator); pcchLanguage = 85)
            {
                list.Add(((object)pwszFileMUIPath).ToString());
                pwszLanguage = new StringBuilder(85);
                pwszFileMUIPath = new StringBuilder(260);
                pcchFileMUIPath = 260;
            }
            if (list.Count == 0)
            {
                Win32Exception win32Exception = new Win32Exception();
                if (win32Exception.NativeErrorCode != 18)
                    throw win32Exception;
            }
            return list.ToArray();
        }

        public void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                switch (this.mode)
                {
                    case UnmanagedLibraryAccess.Read:
                        lock (UnmanagedLibrary.librariesDict)
                        {
                            --this.libraryInfo.ReferenceCount;
                            if (this.libraryInfo.ReferenceCount != 0)
                                break;
                            NativeMethods.FreeLibrary(this.libraryInfo.Handle);
                            UnmanagedLibrary.librariesDict.Remove(this.libraryInfo.File);
                            break;
                        }
                    case UnmanagedLibraryAccess.Write:
                        foreach (KeyValuePair<int, UnmanagedLibrary.StringTable> keyValuePair in this.stringTables)
                        {
                            byte[] buffer = keyValuePair.Value.GetBuffer();
                            ResourceExtensions.Update(this.GetResource(keyValuePair.Key, (object)6), buffer);
                        }
                        if (NativeMethods.EndUpdateResource(this.resourceHandle, false))
                            break;
                        else
                            throw new Win32Exception();
                }
            }
            else
                NativeMethods.EndUpdateResource(this.resourceHandle, true);
        }

        internal void CheckIsReadMode()
        {
            if (this.mode != UnmanagedLibraryAccess.Read)
                throw new InvalidOperationException();
        }

        internal void CheckIsWriteMode()
        {
            if (this.mode != UnmanagedLibraryAccess.Write)
                throw new InvalidOperationException();
        }

        private void GetStringTableFromStringID(int id, out int blockID, out int index)
        {
            blockID = id / 16 + 1;
            int num = (blockID - 1) * 16;
            index = id - num;
        }

        private class ResourceTypeHelper
        {
            private List<object> types;

            public IEnumerable<object> ResourceTypes
            {
                get
                {
                    return (IEnumerable<object>)this.types;
                }
            }

            public ResourceTypeHelper()
            {
                this.types = new List<object>();
            }

            public bool EnumResourceTypesCallback(IntPtr module, IntPtr type, IntPtr param)
            {
                int id;
                string name;
                if (NativeMethods.GET_RESOURCE_NAME(type, out id, out name))
                    this.types.Add((object)name);
                else
                    this.types.Add((object)id);
                return true;
            }
        }

        public class LibraryInfo
        {
            public string File { get; set; }

            public IntPtr Handle { get; set; }

            public int ReferenceCount { get; set; }
        }

        private class StringTable
        {
            private string[] stringList = new string[16];

            public string this[int i]
            {
                get
                {
                    return this.stringList[i];
                }
                set
                {
                    this.stringList[i] = value;
                }
            }

            public StringTable(byte[] buffer)
            {
                int startIndex = 0;
                int index1 = 0;
                while (startIndex < buffer.Length)
                {
                    int count = (int)BitConverter.ToUInt16(buffer, startIndex) * 2;
                    int index2 = startIndex + 2;
                    this.stringList[index1] = Encoding.Unicode.GetString(buffer, index2, count);
                    startIndex = index2 + count;
                    ++index1;
                }
            }

            public byte[] GetBuffer()
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream))
                    {
                        for (int index = 0; index < this.stringList.Length; ++index)
                        {
                            if (this.stringList[index] != null)
                            {
                                byte[] bytes = Encoding.Unicode.GetBytes(this.stringList[index]);
                                binaryWriter.Write((ushort)this.stringList[index].Length);
                                binaryWriter.Write(bytes);
                            }
                            else
                                binaryWriter.Write((ushort)0);
                        }
                        binaryWriter.Flush();
                        byte[] numArray = new byte[memoryStream.Length];
                        Array.Copy((Array)memoryStream.GetBuffer(), (Array)numArray, numArray.Length);
                        return numArray;
                    }
                }
            }
        }

        
    }
}
