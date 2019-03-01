namespace Kros.KORM.Migrations.Providers
{
    /// <summary>
    /// Information about migration script.
    /// </summary>
    public class ScriptInfo
    {
        /// <summary>
        /// Migration Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name of migration script.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to migration script.
        /// </summary>
        public string Path { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{Id}_{Name}";
    }
}
