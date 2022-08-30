
using System.Text;

namespace Advent.Common.IO
{
    public static class ResourceExtensions
    {
        public static string GetString(this IResource resource, Encoding encoding)
        {
            byte[] bytes = ResourceExtensions.GetBytes(resource);
            if (bytes != null)
                return encoding.GetString(bytes);
            else
                return (string)null;
        }

        public static byte[] GetBytes(this IResource resource)
        {
            return resource.GetBytes(resource.Library.DefaultLanguage);
        }

        public static void Update(this IResource resource, string data, Encoding encoding)
        {
            ResourceExtensions.Update(resource, data, encoding, resource.Library.DefaultLanguage);
        }

        public static void Update(this IResource resource, string data, Encoding encoding, ushort? language)
        {
            byte[] bytes = encoding.GetBytes(data);
            resource.Update(bytes, language);
        }

        public static void Update(this IResource resource, byte[] data)
        {
            resource.Update(data, resource.Library.DefaultLanguage);
        }

        public static string GetStringResource(this IResourceLibrary library, int id)
        {
            return library.GetStringResource(id, library.DefaultLanguage);
        }
    }
}
