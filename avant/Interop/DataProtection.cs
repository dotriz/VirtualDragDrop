
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Advent.Common.Interop
{
    public class DataProtection
    {
        private static readonly IntPtr NullPtr = (IntPtr)0;
        private static DataProtection.KeyType defaultKeyType = DataProtection.KeyType.UserKey;

        static DataProtection()
        {
        }

        public static string Encrypt(string plainText)
        {
            return DataProtection.Encrypt(DataProtection.defaultKeyType, plainText, string.Empty, string.Empty);
        }

        public static string Encrypt(DataProtection.KeyType keyType, string plainText)
        {
            return DataProtection.Encrypt(keyType, plainText, string.Empty, string.Empty);
        }

        public static string Encrypt(DataProtection.KeyType keyType, string plainText, string entropy)
        {
            return DataProtection.Encrypt(keyType, plainText, entropy, string.Empty);
        }

        public static string Encrypt(DataProtection.KeyType keyType, string plainText, string entropy, string description)
        {
            if (plainText == null)
                plainText = string.Empty;
            if (entropy == null)
                entropy = string.Empty;
            return Convert.ToBase64String(DataProtection.Encrypt(keyType, Encoding.UTF8.GetBytes(plainText), Encoding.UTF8.GetBytes(entropy), description));
        }

        public static byte[] Encrypt(DataProtection.KeyType keyType, byte[] plainTextBytes, byte[] entropyBytes, string description)
        {
            if (plainTextBytes == null)
                plainTextBytes = new byte[0];
            if (entropyBytes == null)
                entropyBytes = new byte[0];
            if (description == null)
                description = string.Empty;
            DATA_BLOB dataBlob1 = new DATA_BLOB();
            DATA_BLOB pCipherText = new DATA_BLOB();
            DATA_BLOB dataBlob2 = new DATA_BLOB();
            CRYPTPROTECT_PROMPTSTRUCT cryptprotectPromptstruct = new CRYPTPROTECT_PROMPTSTRUCT();
            DataProtection.InitPrompt(ref cryptprotectPromptstruct);
            try
            {
                try
                {
                    DataProtection.InitBLOB(plainTextBytes, ref dataBlob1);
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot initialize plaintext BLOB.", ex);
                }
                try
                {
                    DataProtection.InitBLOB(entropyBytes, ref dataBlob2);
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot initialize entropy BLOB.", ex);
                }
                int dwFlags = 1;
                if (keyType == DataProtection.KeyType.MachineKey)
                    dwFlags |= 4;
                if (!Advent.Common.Interop.NativeMethods.CryptProtectData(ref dataBlob1, description, ref dataBlob2, IntPtr.Zero, ref cryptprotectPromptstruct, dwFlags, ref pCipherText))
                    throw new Exception("CryptProtectData failed.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                byte[] destination = new byte[pCipherText.cbData];
                Marshal.Copy(pCipherText.pbData, destination, 0, pCipherText.cbData);
                return destination;
            }
            catch (Exception ex)
            {
                throw new Exception("DPAPI was unable to encrypt data.", ex);
            }
            finally
            {
                if (dataBlob1.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataBlob1.pbData);
                if (pCipherText.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pCipherText.pbData);
                if (dataBlob2.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataBlob2.pbData);
            }
        }

        public static string Decrypt(string cipherText)
        {
            string description;
            return DataProtection.Decrypt(cipherText, string.Empty, out description);
        }

        public static string Decrypt(string cipherText, out string description)
        {
            return DataProtection.Decrypt(cipherText, string.Empty, out description);
        }

        public static string Decrypt(string cipherText, string entropy, out string description)
        {
            if (entropy == null)
                entropy = string.Empty;
            return Encoding.UTF8.GetString(DataProtection.Decrypt(Convert.FromBase64String(cipherText), Encoding.UTF8.GetBytes(entropy), out description));
        }

        public static byte[] Decrypt(byte[] cipherTextBytes, byte[] entropyBytes, out string description)
        {
            DATA_BLOB pPlainText = new DATA_BLOB();
            DATA_BLOB dataBlob1 = new DATA_BLOB();
            DATA_BLOB dataBlob2 = new DATA_BLOB();
            CRYPTPROTECT_PROMPTSTRUCT cryptprotectPromptstruct = new CRYPTPROTECT_PROMPTSTRUCT();
            DataProtection.InitPrompt(ref cryptprotectPromptstruct);
            description = string.Empty;
            try
            {
                try
                {
                    DataProtection.InitBLOB(cipherTextBytes, ref dataBlob1);
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot initialize ciphertext BLOB.", ex);
                }
                try
                {
                    DataProtection.InitBLOB(entropyBytes, ref dataBlob2);
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot initialize entropy BLOB.", ex);
                }
                int dwFlags = 1;
                if (!Advent.Common.Interop.NativeMethods.CryptUnprotectData(ref dataBlob1, ref description, ref dataBlob2, IntPtr.Zero, ref cryptprotectPromptstruct, dwFlags, ref pPlainText))
                    throw new Exception("CryptUnprotectData failed.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                byte[] destination = new byte[pPlainText.cbData];
                Marshal.Copy(pPlainText.pbData, destination, 0, pPlainText.cbData);
                return destination;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to decrypt data.", ex);
            }
            finally
            {
                if (pPlainText.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pPlainText.pbData);
                if (dataBlob1.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataBlob1.pbData);
                if (dataBlob2.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataBlob2.pbData);
            }
        }

        private static void InitPrompt(ref CRYPTPROTECT_PROMPTSTRUCT ps)
        {
            ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
            ps.dwPromptFlags = 0;
            ps.hwndApp = DataProtection.NullPtr;
            ps.szPrompt = (string)null;
        }

        private static void InitBLOB(byte[] data, ref DATA_BLOB blob)
        {
            if (data == null)
                data = new byte[0];
            blob.pbData = Marshal.AllocHGlobal(data.Length);
            if (blob.pbData == IntPtr.Zero)
                throw new Exception("Unable to allocate data buffer for BLOB structure.");
            blob.cbData = data.Length;
            Marshal.Copy(data, 0, blob.pbData, data.Length);
        }

        public enum KeyType
        {
            UserKey = 1,
            MachineKey = 2,
        }
    }
}
