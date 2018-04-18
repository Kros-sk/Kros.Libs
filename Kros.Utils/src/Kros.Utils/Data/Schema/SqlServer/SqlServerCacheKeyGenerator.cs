using Kros.Utils;
using System;
using System.Data.SqlClient;

namespace Kros.Data.Schema.SqlServer
{
    /// <summary>
    /// Generátor kľúča pre SQL Server databázu.
    /// </summary>
    public class SqlServerCacheKeyGenerator
        : ISchemaCacheKeyGenerator<SqlConnection>
    {
        /// <summary>
        /// Vygeneruje kľúč pre spojenie <paramref name="connection"/>. Kľúč je vygenerovaný v tvare <b>SERVER\databáza</b>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Reťazec.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        public string GenerateKey(SqlConnection connection)
        {
            Check.NotNull(connection, nameof(connection));
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connection.ConnectionString);
            return "SqlServer:" + builder.DataSource.ToUpper() + @"\" + builder.InitialCatalog.ToLower();
        }

        /// <summary>
        /// Vygeneruje kľúč pre spojenie <paramref name="connection"/>. Kľúč je vygenerovaný v tvare <b>SERVER\databáza</b>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Reťazec.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        string ISchemaCacheKeyGenerator.GenerateKey(object connection)
        {
            return GenerateKey(connection as SqlConnection);
        }
    }
}
