using System.IO;
using System.Reflection;

namespace Kros.Utils
{
    /// <summary>
    /// Helper, ktorý slúži na získanie obsahu súborových resourcov.
    /// </summary>
    internal static class ResourceHelper
    {
        /// <summary>
        /// Gets the resource by name.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>
        /// Resource content if resource exist; otherwise <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// Používa sa to ako work around,
        /// pretože v súčasnosti má dotnet core CLI problém získať obsah resourcu štandardným spôsobom.
        /// </remarks>
        public static string GetResourceContent(string resourceName)
        {
            Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName));

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
