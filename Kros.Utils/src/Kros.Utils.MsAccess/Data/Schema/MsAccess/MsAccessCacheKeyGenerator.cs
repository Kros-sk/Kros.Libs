using Kros.Utils;
using System;
using System.Data.OleDb;

namespace Kros.Data.Schema.MsAccess
{
    /// <summary>
    /// Generátor kľúča pre MS Access databázu.
    /// </summary>
    public class MsAccessCacheKeyGenerator
        : ISchemaCacheKeyGenerator<OleDbConnection>
    {
        /// <summary>
        /// Vygeneruje kľúč pre spojenie <paramref name="connection"/>. Vygenerovaný kľúč je cesta k databázovému súboru.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Reťazec.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        public string GenerateKey(OleDbConnection connection)
        {
            Check.NotNull(connection, nameof(connection));
            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(connection.ConnectionString);
            return "MsAccess:" + builder.DataSource.ToLower();
        }

        /// <summary>
        /// Vygeneruje kľúč pre spojenie <paramref name="connection"/>. Vygenerovaný kľúč je cesta k databázovému súboru.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Reťazec.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        string ISchemaCacheKeyGenerator.GenerateKey(object connection)
        {
            return GenerateKey(connection as OleDbConnection);
        }
    }
}
