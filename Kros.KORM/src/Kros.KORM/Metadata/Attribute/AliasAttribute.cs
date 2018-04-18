using System;

namespace Kros.KORM.Metadata.Attribute
{
    /// <summary>
    /// Attribute which describe database name of property / class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class AliasAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasAttribute"/> class.
        /// </summary>
        /// <param name="alias">The database alias.</param>
        public AliasAttribute(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentNullException("alias", "Attribute 'alias' is required");
            }

            this.Alias = alias;
        }

        /// <summary>
        /// Database alias
        /// </summary>
        public string Alias { get; private set; }
    }
}
