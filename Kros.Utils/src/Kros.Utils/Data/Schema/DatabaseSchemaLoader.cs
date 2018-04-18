using Kros.Properties;
using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Pomocná trieda na jednoduché načítavanie schémy databáz. Obsahuje zoznam loader-ov pre konkrétne databázy a tak
    /// je možné použiť ju transparentne pre ľubovoľný typ databázy. Pri požiadavke je vždy načítaná aktuálna schéma,
    /// tzn. schémy sa nekešujú.
    /// </summary>
    /// <remarks>
    /// <para>Trieda obaľuje zoznam loader-ov rôznych typov databáz a umožňuje tak jednoduché načítanie schémy ľubovoľného
    /// známeho typu. Loadery je možné pridávať metódou <see cref="DatabaseSchemaLoader.AddSchemaLoader">AddSchemaLoader</see>.
    /// </para>
    /// <para>Trieda je určená na statické použitie pomocou vlatnosti <see cref="DatabaseSchemaLoader.Default"/>.
    /// Štandardne dokáže načítať schémy SQL Server-a a MS Access.</para>
    /// </remarks>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\SchemaExamples.cs" region="SchemaLoader"/>
    /// </example>
    public class DatabaseSchemaLoader
        : IDatabaseSchemaLoader
    {

        #region Static

        /// <summary>
        /// Inštancia <c>DatabaseSchemaLoader</c> určená na bežné použitie. Štandardne obsahuje loader pre SQL Server
        /// (<see cref="SqlServer.SqlServerSchemaLoader">SqlServerSchemaLoader</see>).
        /// </summary>
        public static DatabaseSchemaLoader Default { get; } = InitDefault();

        private static DatabaseSchemaLoader InitDefault()
        {
            DatabaseSchemaLoader loader = new DatabaseSchemaLoader();
            loader.AddSchemaLoader(new SqlServer.SqlServerSchemaLoader());

            return loader;
        }

        #endregion


        #region Private fields

        private readonly List<IDatabaseSchemaLoader> _loaders = new List<IDatabaseSchemaLoader>();

        #endregion


        #region Loaders

        /// <summary>
        /// Pridá <paramref name="loader"/> do zoznamu loader-ov.
        /// </summary>
        /// <param name="loader">Trieda na načítanie schémy konkrétneho typu databázy.</param>
        public void AddSchemaLoader(IDatabaseSchemaLoader loader)
        {
            Check.NotNull(loader, nameof(loader));
            _loaders.Add(loader);
        }

        /// <summary>
        /// Odstráni zo zoznamu loader-ov zadaný <paramref name="loader"/>.
        /// </summary>
        /// <param name="loader">Trieda na načítanie schémy databázy, ktorá sa z interného zoznamu odstráni.</param>
        public void RemoveSchemaLoader(IDatabaseSchemaLoader loader)
        {
            _loaders.Remove(loader);
        }

        /// <summary>
        /// Vymaže všetky loader-y v zozname.
        /// </summary>
        public void ClearSchemaLoaders()
        {
            _loaders.Clear();
        }

        #endregion


        #region IDatabaseSchemaLoader

        private IDatabaseSchemaLoader GetLoader(object connection)
        {
            return _loaders.FirstOrDefault((loader) => loader.SupportsConnectionType(connection));
        }

        private IDatabaseSchemaLoader CheckConnectionAndGetLoader(object connection)
        {
            Check.NotNull(connection, nameof(connection));

            IDatabaseSchemaLoader loader = GetLoader(connection);
            if (loader == null)
            {
                throw new ArgumentException(Resources.DatabaseSchemaLoader_UnsupportedConnectionType, nameof(connection));
            }

            return loader;
        }

        /// <summary>
        /// Kontroluje, či trieda dokáže načítať schému zo zadaného spojenia <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je. Kontroluje sa zoznam
        /// loader-ov a <see langword="true"/> sa vráti, ak ľubovoľný z nich vie načítať schému so zadaného spojenia.</returns>
        public bool SupportsConnectionType(object connection)
        {
            Check.NotNull(connection, nameof(connection));

            return GetLoader(connection) != null;
        }

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Nie je možné načítať schému databázy, pretože pre spojenie
        /// <paramref name="connection"/> neexistuje loader.</exception>
        public DatabaseSchema LoadSchema(object connection)
        {
            IDatabaseSchemaLoader loader = CheckConnectionAndGetLoader(connection);
            return loader.LoadSchema(connection);
        }

        /// <summary>
        /// Načíta schému tabuľky <paramref name="tableName"/> z databázy <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <param name="tableName">Meno tabuľky, ktorej schéma sa načíta.</param>
        /// <returns>Vráti načítanú schému tabuľky, alebo hodnotu <c>null</c>, ak taká tabuľka neexistuje.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Nie je možné načítať schému databázy, pretože pre spojenie
        /// <paramref name="connection"/> neexistuje loader.</exception>
        public TableSchema LoadTableSchema(object connection, string tableName)
        {
            Check.NotNullOrWhiteSpace(tableName, nameof(tableName));

            IDatabaseSchemaLoader loader = CheckConnectionAndGetLoader(connection);
            return loader.LoadTableSchema(connection, tableName);
        }

        #endregion

    }
}
