using Kros.Utils;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Schéma databázy.
    /// </summary>
    public class DatabaseSchema
    {

        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu schémy databázy s menom <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno databázy.</param>
        public DatabaseSchema(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            Name = name;
            Tables = new TableSchemaCollection(this);
        }

        #endregion


        #region Common

        /// <summary>
        /// Názov databázy.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Schéma tabuliek databázy.
        /// </summary>
        public TableSchemaCollection Tables { get; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            return $"Database {Name}";
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
