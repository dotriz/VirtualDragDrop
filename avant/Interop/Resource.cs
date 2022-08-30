
using Advent.Common.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Advent.Common.Interop
{
    public class Resource : IResource
    {
        private readonly UnmanagedLibrary library;
        private readonly string stringName;
        private readonly string stringType;
        private readonly int intName;
        private readonly int intType;
        private ushort[] languages;

        public string Name
        {
            get
            {
                return this.stringName ?? this.intName.ToString();
            }
        }

        public object Type
        {
            get
            {
                if (this.stringType != null)
                    return (object)this.stringType;
                else
                    return (object)this.intType;
            }
        }

        public int? ID
        {
            get
            {
                if (this.stringName == null)
                    return new int?(this.intName);
                else
                    return new int?();
            }
        }

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
                if (this.languages == null)
                {
                    this.library.CheckIsReadMode();
                    Resource.LanguageHelper languageHelper = new Resource.LanguageHelper();
                    if (!(this.stringName == null || this.stringType == null ? (this.stringName == null ? (this.stringType == null ? Advent.Common.Interop.NativeMethods.EnumResourceLanguages(this.library.Handle, this.intType, this.intName, new Advent.Common.Interop.NativeMethods.EnumResLangProc(languageHelper.EnumLanguages), 0) : Advent.Common.Interop.NativeMethods.EnumResourceLanguages(this.library.Handle, this.stringType, this.intName, new Advent.Common.Interop.NativeMethods.EnumResLangProc(languageHelper.EnumLanguages), 0)) : Advent.Common.Interop.NativeMethods.EnumResourceLanguages(this.library.Handle, this.intType, this.stringName, new Advent.Common.Interop.NativeMethods.EnumResLangProc(languageHelper.EnumLanguages), 0)) : Advent.Common.Interop.NativeMethods.EnumResourceLanguages(this.library.Handle, this.stringType, this.stringName, new Advent.Common.Interop.NativeMethods.EnumResLangProc(languageHelper.EnumLanguages), 0)) && new Win32Exception().NativeErrorCode != 1813)
                        throw new Win32Exception();
                    this.languages = languageHelper.Languages;
                }
                return (IEnumerable<ushort>)this.languages;
            }
        }

        internal Resource(UnmanagedLibrary library, string name, object type)
            : this(library)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (type == null)
                throw new ArgumentNullException("type");
            this.stringName = name;
            if (type is string)
                this.stringType = (string)type;
            else
                this.intType = (int)type;
        }

        internal Resource(UnmanagedLibrary library, int name, object type)
            : this(library)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.intName = name;
            if (type is string)
                this.stringType = (string)type;
            else
                this.intType = (int)type;
        }

        private Resource(UnmanagedLibrary library)
        {
            if (library == null)
                throw new ArgumentNullException("library");
            this.library = library;
        }

        public bool Exists(ushort? language)
        {
            return this.GetResourceHandle(language) != IntPtr.Zero;
        }

        public byte[] GetBytes(ushort? language)
        {
            this.library.CheckIsReadMode();
            IntPtr resourceHandle = this.GetResourceHandle(language);
            if (resourceHandle == IntPtr.Zero)
                return (byte[])null;
            uint num = Advent.Common.Interop.NativeMethods.SizeofResource(this.library.Handle, resourceHandle);
            IntPtr source = Advent.Common.Interop.NativeMethods.LockResource(Advent.Common.Interop.NativeMethods.LoadResource(this.library.Handle, resourceHandle));
            //TODO  original new byte[(IntPtr)num];
            byte[] destination = new byte[num];
            Marshal.Copy(source, destination, 0, (int)num);
            return destination;
        }

        public void Update(byte[] data, ushort? language)
        {
            this.library.CheckIsWriteMode();
            ushort? nullable = language;
            ushort wLanguage = !(nullable.HasValue ? new int?((int)nullable.GetValueOrDefault()) : new int?()).HasValue ? LanguageUtils.GetUserDefaultUILanguage() : language.Value;
            int cbData = data == null ? 0 : data.Length;
            if (!(this.stringName == null || this.stringType == null ? (this.stringName == null ? (this.stringType == null ? Advent.Common.Interop.NativeMethods.UpdateResource(this.library.Handle, this.intType, this.intName, wLanguage, data, cbData) : Advent.Common.Interop.NativeMethods.UpdateResource(this.library.Handle, this.stringType, this.intName, wLanguage, data, cbData)) : Advent.Common.Interop.NativeMethods.UpdateResource(this.library.Handle, this.intType, this.stringName.ToUpper(), wLanguage, data, cbData)) : Advent.Common.Interop.NativeMethods.UpdateResource(this.library.Handle, this.stringType, this.stringName.ToUpper(), wLanguage, data, cbData)))
                throw new Win32Exception();
        }

        private IntPtr GetResourceHandle(ushort? language)
        {
            IntPtr num;
            if (this.stringName != null && this.stringType != null)
            {
                ushort? nullable = language;
                num = (nullable.HasValue ? new int?((int)nullable.GetValueOrDefault()) : new int?()).HasValue ? Advent.Common.Interop.NativeMethods.FindResourceEx(this.library.Handle, this.stringType, this.stringName, language.Value) : Advent.Common.Interop.NativeMethods.FindResource(this.library.Handle, this.stringName, this.stringType);
            }
            else if (this.stringName != null)
            {
                ushort? nullable = language;
                num = (nullable.HasValue ? new int?((int)nullable.GetValueOrDefault()) : new int?()).HasValue ? Advent.Common.Interop.NativeMethods.FindResourceEx(this.library.Handle, this.intType, this.stringName, language.Value) : Advent.Common.Interop.NativeMethods.FindResource(this.library.Handle, this.stringName, this.intType);
            }
            else if (this.stringType != null)
            {
                ushort? nullable = language;
                num = (nullable.HasValue ? new int?((int)nullable.GetValueOrDefault()) : new int?()).HasValue ? Advent.Common.Interop.NativeMethods.FindResourceEx(this.library.Handle, this.stringType, this.intName, language.Value) : Advent.Common.Interop.NativeMethods.FindResource(this.library.Handle, this.intName, this.stringType);
            }
            else
            {
                ushort? nullable = language;
                num = (nullable.HasValue ? new int?((int)nullable.GetValueOrDefault()) : new int?()).HasValue ? Advent.Common.Interop.NativeMethods.FindResourceEx(this.library.Handle, this.intType, this.intName, language.Value) : Advent.Common.Interop.NativeMethods.FindResource(this.library.Handle, this.intName, this.intType);
            }
            return num;
        }

        private class LanguageHelper
        {
            private readonly List<ushort> languages = new List<ushort>();

            public ushort[] Languages
            {
                get
                {
                    return this.languages.ToArray();
                }
            }

            public bool EnumLanguages(IntPtr module, int type, int name, ushort language, IntPtr param)
            {
                this.languages.Add(language);
                return true;
            }
        }
    }
}
