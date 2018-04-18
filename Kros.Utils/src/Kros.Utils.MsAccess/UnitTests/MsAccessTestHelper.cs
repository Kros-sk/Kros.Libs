using Kros.Data.MsAccess;
using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;

namespace Kros.UnitTests
{
    /// <summary>
    /// Pomocná trieda pre unit testy, ak je potrebné mať pri testovaní reálnu databázu. Umožňuje jednoducho
    /// vytvoriť dočasnú databázu v ktorej sa bude testovať. Táto databáza je automaticky po skončení práce vymazaná.
    /// </summary>
    public class MsAccessTestHelper
        : IDisposable
    {
        #region Fields

        private readonly ProviderType _provider;
        private readonly Stream _sourceDatabaseStream = null;
        private readonly string _sourceDatabaseStreamPath = null;
        private string _databasePath;
        private OleDbConnection _connection = null;
        private readonly IEnumerable<string> _initDatabaseScripts = null;

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Nový <c>mdb</c> súbor je vytvorený ako kópia
        /// <paramref name="sourceDatabasePath"/>.
        /// </summary>
        /// <param name="provider">MS Access provider, ktorý sa má pre spojenie použiť.</param>
        /// <param name="sourceDatabasePath">Zdrojová databáza - nová databáza sa vytvorí ako jej kópia.</param>
        public MsAccessTestHelper(ProviderType provider, string sourceDatabasePath)
            : this(provider, sourceDatabasePath, null as IEnumerable<string>)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Nový <c>mdb</c> súbor je vytvorený ako kópia súboru
        /// <paramref name="sourceDatabasePath"/> a inicializovaný skriptom <paramref name="initDatabaseScript"/>.
        /// </summary>
        /// <param name="provider">MS Access provider, ktorý sa má pre spojenie použiť.</param>
        /// <param name="sourceDatabasePath">Zdrojová databáza - nová databáza sa vytvorí ako jej kópia.</param>
        /// <param name="initDatabaseScript">Skript, ktorý sa spustí a inicializuje tak novovytvorenú databázu.
        /// Môže to byť napríklad skript na vytvorenie a naplnenie potrebných tabuliek.</param>
        public MsAccessTestHelper(ProviderType provider, string sourceDatabasePath, string initDatabaseScript)
            : this(provider, sourceDatabasePath,
                  string.IsNullOrWhiteSpace(initDatabaseScript) ? null : new string[] { initDatabaseScript })
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Nový <c>mdb</c> súbor je vytvorený ako kópia súboru
        /// <paramref name="sourceDatabasePath"/> a inicializovaný skriptami <paramref name="initDatabaseScripts"/>.
        /// </summary>
        /// <param name="provider">MS Access provider, ktorý sa má pre spojenie použiť.</param>
        /// <param name="sourceDatabasePath">Zdrojová databáza - nová databáza sa vytvorí ako jej kópia.</param>
        /// <param name="initDatabaseScripts">Zoznam skriptov, ktoré sa spustia a inicializujú tak novovytvorenú databázu.
        /// Môžu to byť napríklad skripty na vytvorenie a naplnenie potrebných tabuliek.</param>
        public MsAccessTestHelper(ProviderType provider, string sourceDatabasePath, IEnumerable<string> initDatabaseScripts)
        {
            Check.NotNullOrWhiteSpace(sourceDatabasePath, nameof(sourceDatabasePath));

            _provider = provider;
            _sourceDatabaseStreamPath = sourceDatabasePath;
            _initDatabaseScripts = initDatabaseScripts;
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Nový <c>mdb</c> súbor je vytvorený ako kópia
        /// <paramref name="sourceDatabaseStream"/>.
        /// </summary>
        /// <param name="provider">MS Access provider, ktorý sa má pre spojenie použiť.</param>
        /// <param name="sourceDatabaseStream">Zdrojová databáza - nová databáza sa vytvorí ako jej kópia.</param>
        public MsAccessTestHelper(ProviderType provider, Stream sourceDatabaseStream)
            : this(provider, sourceDatabaseStream, null as IEnumerable<string>)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Nový <c>mdb</c> súbor je vytvorený ako kópia
        /// <paramref name="sourceDatabaseStream"/> a inicializovaný skriptom <paramref name="initDatabaseScript"/>.
        /// </summary>
        /// <param name="provider">MS Access provider, ktorý sa má pre spojenie použiť.</param>
        /// <param name="sourceDatabaseStream">Zdrojová databáza - nová databáza sa vytvorí ako jej kópia.</param>
        /// <param name="initDatabaseScript">Skript, ktorý sa spustí a inicializuje tak novovytvorenú databázu.
        /// Môže to byť napríklad skript na vytvorenie a naplnenie potrebných tabuliek.</param>
        public MsAccessTestHelper(ProviderType provider, Stream sourceDatabaseStream, string initDatabaseScript)
            : this(provider, sourceDatabaseStream, new string[] { initDatabaseScript })
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Nový <c>mdb</c> súbor je vytvorený ako kópia
        /// <paramref name="sourceDatabaseStream"/> a inicializovaný skriptami <paramref name="initDatabaseScripts"/>.
        /// </summary>
        /// <param name="provider">MS Access provider, ktorý sa má pre spojenie použiť.</param>
        /// <param name="sourceDatabaseStream">Zdrojová databáza - nová databáza sa vytvorí ako jej kópia.</param>
        /// <param name="initDatabaseScripts">Zoznam skriptov, ktoré sa spustia a inicializujú tak novovytvorenú databázu.
        /// Môžu to byť napríklad skripty na vytvorenie a naplnenie potrebných tabuliek.</param>
        public MsAccessTestHelper(ProviderType provider, Stream sourceDatabaseStream, IEnumerable<string> initDatabaseScripts)
        {
            Check.NotNull(sourceDatabaseStream, nameof(sourceDatabaseStream));

            _provider = provider;
            _sourceDatabaseStream = sourceDatabaseStream;
            _initDatabaseScripts = initDatabaseScripts;
        }

        #endregion


        #region Test helpers

        /// <summary>
        /// Cesta k vytvorenej databáze.
        /// </summary>
        public string DatabasePath { get => _databasePath; }

        /// <summary>
        /// Spojenie na vytvorenú databázu.
        /// </summary>
        public OleDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    CreateDatabase();
                }
                return _connection;
            }
        }

        #endregion


        #region Helpers

        /// <summary>
        /// Vygeneruje názov k súboru, kde sa vytovrí databáza. Štandardne je to náhodný súbor v dočasnom (Temp) priečinku.
        /// </summary>
        /// <returns>Názov databázy.</returns>
        protected virtual string GenerateDatabaseName()
        {
            return Path.GetTempFileName();
        }

        /// <summary>
        /// Inicializuje databázu. Metóda je volaná po vytvorení databázy a štandardne spustí skript(y), ktoré
        /// boli zadané v konštruktore. Metóda sa volá iba raz, po vytvorení databázy.
        /// </summary>
        protected virtual void InitDatabase()
        {
            if (_initDatabaseScripts != null)
            {
                using (OleDbCommand cmd = Connection.CreateCommand())
                {
                    foreach (string script in _initDatabaseScripts)
                    {
                        cmd.CommandText = script;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void CreateDatabase()
        {
            if (_connection == null)
            {
                CreateConnection();
                InitDatabase();
            }
        }

        private void CreateConnection()
        {
            _connection = new OleDbConnection(InitConnectionString(_provider));
        }

        private string InitConnectionString(ProviderType provider)
        {
            if (!MsAccessDataHelper.HasProvider(provider))
            {
                throw new InvalidOperationException($"Provider {provider.ToString()} is not installed.");
            }

            _databasePath = GenerateDatabaseName();
            if (_sourceDatabaseStream == null)
            {
                File.Copy(_sourceDatabaseStreamPath, _databasePath);
            }
            else
            {
                using (FileStream writer = new FileStream(_databasePath, FileMode.Create, FileAccess.Write))
                {
                    _sourceDatabaseStream.CopyTo(writer);
                }
            }
            return MsAccessDataHelper.CreateConnectionString(_databasePath, provider);
        }

        private void RemoveDatabase()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }
                string folder = Path.GetDirectoryName(_databasePath);
                string fileName = Path.GetFileNameWithoutExtension(_databasePath);
                string ldbFilePath = Path.Combine(folder, fileName + ".ldb");
                if (File.Exists(ldbFilePath))
                {
                    File.Delete(ldbFilePath);
                }
            }
        }

        #endregion


        #region IDisposable

        private bool disposedValue = false;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RemoveDatabase();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion
    }
}
