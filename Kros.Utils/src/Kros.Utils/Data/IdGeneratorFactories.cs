using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Kros.Data
{
    /// <summary>
    /// Reprezentuje množinu registrovaných <see cref="IIdGeneratorFactory"/>. Umožňuje ich registráciu a získavanie.
    /// </summary>
    /// <seealso cref="IIdGeneratorFactory"/>
    /// <seealso cref="IIdGenerator"/>
    /// <seealso cref="IdGeneratorFactories"/>
    public static class IdGeneratorFactories
    {
        private static Dictionary<string, Func<string, IIdGeneratorFactory>> _factoryByAdoClientName =
            new Dictionary<string, Func<string, IIdGeneratorFactory>>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<Type, Func<DbConnection, IIdGeneratorFactory>> _factoryByConnection =
            new Dictionary<Type, Func<DbConnection, IIdGeneratorFactory>>();

        static IdGeneratorFactories()
        {
            SqlServer.SqlServerIdGeneratorFactory.Register();
        }

        /// <summary>
        /// Registruje factory metódu pre vytvorenie <see cref="IIdGeneratorFactory"/> na základe connection a connection stringu.
        /// </summary>
        /// <typeparam name="TConnection">Typ connection.</typeparam>
        /// <param name="adoClientName">
        /// Názov ado clienta. (napr. pre <see cref="System.Data.SqlClient.SqlConnection"/> to je: System.Data.SqlClient)
        /// </param>
        /// <param name="factoryByConnection">
        /// Factory metóda na vytvorenie <see cref="IIdGeneratorFactory"/> so špecifickým connection stringom.
        /// </param>
        /// <param name="factoryByConnectionString">
        /// Factory metóda na vytvorenie <see cref="IIdGeneratorFactory"/> so špecifickým connection stringom.
        /// </param>
        public static void Register<TConnection>(string adoClientName,
            Func<DbConnection, IIdGeneratorFactory> factoryByConnection,
            Func<string, IIdGeneratorFactory> factoryByConnectionString)
            where TConnection : DbConnection
        {
            Check.NotNullOrWhiteSpace(adoClientName, nameof(adoClientName));

            _factoryByAdoClientName[adoClientName] = Check.NotNull(factoryByConnectionString, nameof(factoryByConnectionString));
            _factoryByConnection[typeof(TConnection)] = Check.NotNull(factoryByConnection, nameof(factoryByConnection));
        }

        /// <summary>
        /// Získanie <see cref="IIdGeneratorFactory"/> so špecifickou connection.
        /// </summary>
        /// <param name="connection">Connection, ktorá sa použije pre vykonanie dotazu na získanie identifikátorov.</param>
        /// <returns>
        /// Inštancia <see cref="IIdGeneratorFactory"/>.
        /// </returns>
        public static IIdGeneratorFactory GetFactory(DbConnection connection)
        {
            if (_factoryByConnection.TryGetValue(connection.GetType(), out var factory))
            {
                return factory(connection);
            }
            else
            {
                throw new InvalidOperationException(
                    $"IIdGeneratorFactory for connection type '{connection.GetType().Name}' is not registered.");
            }
        }

        /// <summary>
        /// Získanie <see cref="IIdGeneratorFactory"/> so špecifickým connection stringom.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, na základe ktorého sa vytvorí connection pre vykonanie dotazu na získanie identifikátorov.
        /// </param>
        /// <param name="adoClientName">
        /// Názov ado clienta. (napr. pre <see cref="System.Data.SqlClient.SqlConnection"/> to je: System.Data.SqlClient)
        /// </param>
        /// <returns>
        /// Inštancia <see cref="IIdGeneratorFactory"/>.
        /// </returns>
        public static IIdGeneratorFactory GetFactory(string connectionString, string adoClientName)
        {
            if (_factoryByAdoClientName.TryGetValue(adoClientName, out var factory))
            {
                return factory(connectionString);
            }
            else
            {
                throw new InvalidOperationException(
                    $"IIdGeneratorFactory for ADO client '{adoClientName}' is not registered.");
            }
        }
    }
}
