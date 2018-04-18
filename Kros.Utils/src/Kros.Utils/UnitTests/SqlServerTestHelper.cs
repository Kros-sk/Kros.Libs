using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Kros.UnitTests
{
    /// <summary>
    /// Pomocná trieda na testovanie databázových vecí v reálnej SQL Server databáze.
    /// </summary>
    /// <remarks>
    /// Štandardne sú testy robené tak, aby nebežali nad reálnou databázou, ale iba nad nejakým rozhraním, ktoré sa ako
    /// databáza tvári. Pre niektoré testy je však potrebné reálne otestovať funkčnosť voči skutočnej databáze.
    /// Trieda sa stará o to, že vytvorí novú databázu s náhodným menom a vráti spojenie na ňu, ktoré je možné používať
    /// (<see cref="Connection"/>).
    /// </remarks>
    /// <example>
    /// <code language = "cs" source="..\Examples\Kros.Utils\SqlServerTestHelperExamples.cs" region="SqlServerTestHelper" />
    /// </example>
    public class SqlServerTestHelper
        : IDisposable
    {

        #region Constants

        private const string MasterDatabaseName = "master";

        #endregion


        #region Fields

        private SqlConnection _connection = null;
        private readonly IEnumerable<string> _initDatabaseScripts = null;

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami. Vytvorená databáza bude prázdna.
        /// </summary>
        /// <param name="baseConnectionString">Základný connection string na SQL Server, kde sa vytvorí databáza.</param>
        /// <param name="baseDatabaseName">Základné meno databázy, ku ktorému sa pridá náhodný GUID. Nemusí byť zadané.</param>
        public SqlServerTestHelper(string baseConnectionString, string baseDatabaseName)
            : this(baseConnectionString, baseDatabaseName, null as IEnumerable<string>)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami a vytvorenú databázu inicializuje skriptom
        /// <paramref name="initDatabaseScript"/>.
        /// </summary>
        /// <param name="baseConnectionString">Základný connection string na SQL Server, kde sa vytvorí databáza.</param>
        /// <param name="baseDatabaseName">Základné meno databázy, ku ktorému sa pridá náhodný GUID. Nemusí byť zadané.</param>
        /// <param name="initDatabaseScript">Skript, ktorý sa spustí a inicializuje tak novovytvorenú databázu.
        /// Môže to byť napríklad skript na vytvorenie a naplnenie potrebných tabuliek.</param>
        public SqlServerTestHelper(string baseConnectionString, string baseDatabaseName, string initDatabaseScript)
            : this(baseConnectionString, baseDatabaseName,
                  string.IsNullOrWhiteSpace(initDatabaseScript) ? null : new string[] { initDatabaseScript })
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami a vytvorenú databázu inicializuje skriptami
        /// <paramref name="initDatabaseScripts"/>.
        /// </summary>
        /// <param name="baseConnectionString">Základný connection string na SQL Server, kde sa vytvorí databáza.</param>
        /// <param name="baseDatabaseName">Základné meno databázy, ku ktorému sa pridá náhodný GUID. Nemusí byť zadané.</param>
        /// <param name="initDatabaseScripts">Zoznam skriptov, ktoré sa spustia a inicializujú tak novovytvorenú databázu.
        /// Môžu to byť napríklad skripty na vytvorenie a naplnenie potrebných tabuliek.</param>
        public SqlServerTestHelper(string baseConnectionString, string baseDatabaseName, IEnumerable<string> initDatabaseScripts)
        {
            Check.NotNullOrWhiteSpace(baseConnectionString, nameof(baseConnectionString));

            BaseConnectionString = baseConnectionString;
            BaseDatabaseName = baseDatabaseName?.Trim();
            _initDatabaseScripts = initDatabaseScripts;
        }

        #endregion


        #region Test helpers

        /// <summary>
        /// Základný connection string na SQL Server, kde bude vytvorená dočasná databáza. Connection string nemusí mať zdané
        /// meno databázy, pretože to bude aj tak vygenerované vlastné.
        /// </summary>
        public string BaseConnectionString { get; }

        /// <summary>
        /// Základný názov databázy. K tomuto názvu sa pridá náhdoný GUID, aby bola databáza jednoznačná.
        /// Ak tento názov nebol zadaný, meno databázy bude iba náhodný GUID.
        /// </summary>
        public string BaseDatabaseName { get; }

        /// <summary>
        /// Spojenie na vytvorenú databázu.
        /// </summary>
        public SqlConnection Connection
        {
            get {
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
        /// Vygeneruje názov databázy, ktorá sa na serveri vytvorí. Názov je zložený z <see cref="BaseDatabaseName"/>
        /// a pridaný je k nemu GUID.
        /// </summary>
        /// <returns>Názov databázy.</returns>
        protected virtual string GenerateDatabaseName()
        {
            string unique = Guid.NewGuid().ToString();
            return string.IsNullOrWhiteSpace(BaseDatabaseName) ? unique : $"{BaseDatabaseName}_{unique}";
        }

        /// <summary>
        /// Inicializuje databázu. Metóda je volaná po vytvorení databázy a štandardne spustí skript(y), ktoré
        /// boli zadané v konštruktore. Metóda sa volá iba raz, po vytvorení databázy.
        /// </summary>
        protected virtual void InitDatabase()
        {
            if (_initDatabaseScripts != null)
            {
                using (SqlCommand cmd = Connection.CreateCommand())
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
            string databaseName = GenerateDatabaseName(); ;
            using (SqlConnection masterConnection = GetConnectionCore(MasterDatabaseName))
            {
                using (SqlCommand cmd = masterConnection.CreateCommand())
                {
                    cmd.CommandText = $"CREATE DATABASE [{databaseName}]";
                    cmd.ExecuteNonQuery();
                }
            }
            _connection = GetConnectionCore(databaseName);
        }

        private SqlConnection GetConnectionCore(string databaseName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(BaseConnectionString)
            {
                InitialCatalog = databaseName,
                Pooling = false,
                PersistSecurityInfo = true
            };
            SqlConnection connection = new SqlConnection(builder.ToString());
            connection.Open();

            return connection;
        }

        private void RemoveDatabase()
        {
            if (_connection != null)
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connection.ConnectionString);
                _connection.Dispose();
                _connection = null;
                using (SqlConnection connection = GetConnectionCore(MasterDatabaseName))
                {
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"DROP DATABASE [{builder.InitialCatalog}]";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        #endregion


        #region IDisposable Support

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
