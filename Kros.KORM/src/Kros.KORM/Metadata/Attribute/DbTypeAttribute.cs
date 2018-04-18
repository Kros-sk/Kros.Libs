using System;
using System.Data;

namespace Kros.KORM.Metadata.Attribute
{
    /// <summary>
    /// Attribute which describe database column type of property.
    /// </summary>
    /// <example>
    ///   <para>
    ///     You can use DbTypeAttribute for defined custom DbType.
    ///   </para>
    /// </example>
    [Obsolete("Atribút Alias sa už nepoužíva. KORM si databázový typ načíta priamo zo schémy databázy.")]
    [AttributeUsage(AttributeTargets.Property)]
    public class DbTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Database type.
        /// </summary>
        public DbType Type { get; set; }
    }
}
