using Kros.Utils;
using Kros.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Implementácia <see cref="IDatabaseSchemaCache"/>. Po pridaní loader-ov umožňuje načítavať schému databázy.
    /// Načítanie schémy je pomalá záležitosť a preto sa načítaná schéma kešuje. Pri ďalšej požiadavke na rovnakú schému
    /// sa už nenačíta z databázy, ale je vrátená z keše.
    /// </summary>
    /// <remarks>
    /// <para><c>DatabaseSchemaCache</c> je potrebné inicializovať potrebným loaderom <see cref="IDatabaseSchemaLoader"/>,
    /// ktorý načíta schému. Zároveň každý loader musí mať špecifikovaný generátor kľúča <see cref="ISchemaCacheKeyGenerator"/>.
    /// Generátory kľúča pre rôzne typy databáz musia vytvárať navzájom rôzne kľúče, aby sa nestalo, že dva rôzne generátory
    /// vygenerujú rovnaký kľúč.</para>
    /// <para>Pre jednoduché použitie je implementovaná vlastnosť <see cref="Default">DatabaseSchemaCache.Default</see>
    /// a nie je tak nutné si vytvárať vlastnú inštanciu keše.</para>
    /// </remarks>
    public class DatabaseSchemaCache
        : IDatabaseSchemaCache
    {
        #region Nested types

        private class LoaderInfo
        {
            public LoaderInfo(IDatabaseSchemaLoader loader, ISchemaCacheKeyGenerator keyGenerator)
            {
                Loader = loader;
                KeyGenerator = keyGenerator;
            }
            public IDatabaseSchemaLoader Loader;
            public ISchemaCacheKeyGenerator KeyGenerator;
        }

        #endregion

        #region Static

        /// <summary>
        /// Inštancia <c>DatabaseSchemaCache</c> určená na bežné použitie. Štandardne obsahuje loader pre SQL Server
        /// (<see cref="SqlServer.SqlServerSchemaLoader">SqlServerSchemaLoader</see>).
        /// </summary>
        public static DatabaseSchemaCache Default { get; } = InitDefault();

        private static DatabaseSchemaCache InitDefault()
        {
            var cache = new DatabaseSchemaCache();
            cache.AddSchemaLoader(new SqlServer.SqlServerSchemaLoader(), new SqlServer.SqlServerCacheKeyGenerator());

            return cache;
        }

        #endregion

        #region Private fields

        private readonly List<LoaderInfo> _loaders = new List<LoaderInfo>();
        private readonly ConcurrentDictionary<string, DatabaseSchema> _cache =
            new ConcurrentDictionary<string, DatabaseSchema>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region IDatabaseSchemaCache

        /// <summary>
        /// Vráti schému databázy pre spojenie <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Schéma danej databázy.</returns>
        /// <exception cref="InvalidOperationException">Keš neobsahuje loader pre spojenie na databázu
        /// <paramref name="connection"/>.</exception>
        public DatabaseSchema GetSchema(object connection)
        {
            LoaderInfo linfo = GetLoaderInfo(connection);
            return _cache.GetOrAdd(linfo.KeyGenerator.GenerateKey(connection), (k) => linfo.Loader.LoadSchema(connection));
        }

        /// <summary>
        /// Zruší z keše schému databázy načítanú pre spojenie <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <exception cref="InvalidOperationException">Keš neobsahuje loader pre spojenie na databázu
        /// <paramref name="connection"/>.</exception>
        public void ClearSchema(object connection)
        {
            LoaderInfo linfo = GetLoaderInfo(connection);
            _cache.TryRemove(linfo.KeyGenerator.GenerateKey(connection), out DatabaseSchema schema);
        }

        /// <summary>
        /// Vyčistí celú keš - vymaže všetky načítané schémy.
        /// </summary>
        public void ClearAllSchemas()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Načíta schému databázy pre spojenie <paramref name="connection"/>. Schéma je načítaná priamo z databázy aj v prípade,
        /// že už je uložená v keši.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Schéma danej databázy.</returns>
        /// <exception cref="InvalidOperationException">Keš neobsahuje loader pre spojenie na databázu
        /// <paramref name="connection"/>.</exception>
        public DatabaseSchema RefreshSchema(object connection)
        {
            LoaderInfo linfo = GetLoaderInfo(connection);
            DatabaseSchema schema = linfo.Loader.LoadSchema(connection);
            _cache.AddOrUpdate(linfo.KeyGenerator.GenerateKey(connection), schema, (k, v) => schema);
            return schema;
        }

        #endregion

        #region Loaders

        /// <summary>
        /// Pridá do keše <paramref name="loader"/> na načítavanie schémy databázy, spolu s generátorom kľúčov pre načítanú
        /// shému <paramref name="keyGenerator"/>.
        /// </summary>
        /// <param name="loader">Loader pre načítavanie schémy databázy.</param>
        /// <param name="keyGenerator">Generátor kľúča pre schému databázy. Databáza sa interne drží v keši pod vygenerovaným
        /// kľúčom.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="loader"/> alebo <paramref name="keyGenerator"/>
        /// je <c>null</c>.</exception>
        public void AddSchemaLoader(IDatabaseSchemaLoader loader, ISchemaCacheKeyGenerator keyGenerator)
        {
            Check.NotNull(loader, nameof(loader));
            Check.NotNull(keyGenerator, nameof(keyGenerator));
            _loaders.Add(new LoaderInfo(loader, keyGenerator));
        }

        /// <summary>
        /// Vymaže zadaný <paramref name="loader"/> databázovej schémy.
        /// </summary>
        /// <param name="loader">Loader databázovej schémy, ktorý sa má vymazať.</param>
        public void RemoveSchemaLoader(IDatabaseSchemaLoader loader)
        {
            _loaders.Remove(_loaders.FirstOrDefault((linfo) => linfo.Loader == loader));
        }

        /// <summary>
        /// Vymaže všetky loader-y schém.
        /// </summary>
        public void ClearSchemaLoaders()
        {
            _loaders.Clear();
        }

        #endregion

        #region Helpers

        private LoaderInfo GetLoaderInfo(object connection)
        {
            LoaderInfo linfo = _loaders.FirstOrDefault((tmpLoader) => tmpLoader.Loader.SupportsConnectionType(connection));
            if (linfo == null)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.DatabaseSchemaCache_UnsupportedDatabaseType, connection.GetType().FullName));
            }
            return linfo;
        }

        #endregion
    }
}
