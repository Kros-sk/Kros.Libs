using System;
using System.IO;
using System.Reflection;

namespace Kros.Utils
{
    /// <summary>
    /// Helper for getting content of file resources embedded into assemblies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// .NET Core is not able to load content of resources added as files. With this helper, it is easy to get these resources.
    /// Just make an instance of helper for desired assembly and use <see cref="ResourceHelper.GetString(string)"/> method.
    /// If you need to read binary resources, use <see cref="ResourceHelper.GetStream(string)"/> method.
    /// </para>
    /// <para>
    /// Resources in assembly are identified with full name, that means root namespace for assembly and folder if file resources
    /// are in some. So for example if root namespace of the assembly is <c>Kros.Utils</c> and resource file is <c>data.txt</c>
    /// located in <c>Resources</c> folder, the full resource name will be <c>Kros.Utils.Resources.data.txt</c>.
    /// </para>
    /// <code lang="csharp">
    /// ResourceHelper helper = new ResourceHelper(Assembly.GetExecutingAssembly());
    /// string resourceValue = helper.GetString("Kros.Utils.Resources.data.txt");
    /// </code>
    /// <para>
    /// If all the file resources are located in one base folder, the base namespace for resource names can be set for easier
    /// writing of resource names. Let's assume, ther are these resources:
    /// <list type="bullet">
    /// <item>Resources\data.txt</item>
    /// <item>Resources\Templates\template.txt</item>
    /// </list>
    /// They can be loaded using shorter names:
    /// </para>
    /// <code lang="csharp">
    /// ResourceHelper helper = new ResourceHelper(Assembly.GetExecutingAssembly(), "Kros.Utils.Resources");
    /// string dataValue = helper.GetString("data.txt");
    /// string templateValue = helper.GetString(@"Templates\template.txt");
    /// </code>
    /// </remarks>
    public class ResourceHelper
    {
        internal static ResourceHelper Instance { get; } = new ResourceHelper(Assembly.GetExecutingAssembly(), "Kros.Resources");

        private readonly Assembly _assembly;

        /// <inheritdoc cref="ResourceHelper.ResourceHelper(Assembly, string)"/>
        public ResourceHelper(Assembly resourcesAssembly) : this(resourcesAssembly, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <c>ResourceHelper</c> for assembly <paramref name="resourcesAssembly"/> and with
        /// base namespace <paramref name="baseNamespace"/>.
        /// </summary>
        /// <param name="resourcesAssembly">Assembly with the resources.</param>
        /// <param name="baseNamespace">Base namespace for resource names.</param>
        /// <exception cref="ArgumentNullException">
        /// The value of <paramref name="resourcesAssembly"/> is <see langword="null"/>.
        /// </exception>
        public ResourceHelper(Assembly resourcesAssembly, string baseNamespace)
        {
            _assembly = Check.NotNull(resourcesAssembly, nameof(resourcesAssembly));
            BaseNamespace = string.IsNullOrWhiteSpace(baseNamespace) ? null : baseNamespace;
        }

        /// <summary>
        /// Base namespace for resource names. It it is set, resource names are appended to it.
        /// </summary>
        public string BaseNamespace { get; }

        /// <summary>
        /// Returns full name of resource <paramref name="resourceName"/>. If <see cref="BaseNamespace"/> is set,
        /// <paramref name="resourceName"/> is appended to it. Otherwise just <paramref name="resourceName"/> is returned.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns>Full name of the resource.</returns>
        public string GetResourceName(string resourceName)
        {
            Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName));
            return BaseNamespace == null ? resourceName : BaseNamespace + '.' + resourceName;
        }

        /// <summary>
        /// Returns the names of all the resources in assembly used by <c>ResourceHelper</c>.
        /// </summary>
        /// <returns>An array that contains the names of all the resources.</returns>
        public string[] GetManifestResourceNames() => _assembly.GetManifestResourceNames();

        /// <summary>
        /// Returns content of the resource <paramref name="resourceName"/> as string. Can be used for text file resources.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>String, or <see langword="null"/> if resource <paramref name="resourceName"/> does not exist.</returns>
        public string GetString(string resourceName)
        {
            using (Stream stream = _assembly.GetManifestResourceStream(GetResourceName(resourceName)))
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

        /// <summary>
        /// Returns stream for resource <paramref name="resourceName"/>. Can be used for binary file resources.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>Stream for the resource, or value <see langword="null"/> if resource <paramref name="resourceName"/>
        /// does not exist.</returns>
        public Stream GetStream(string resourceName) => _assembly.GetManifestResourceStream(GetResourceName(resourceName));
    }
}
