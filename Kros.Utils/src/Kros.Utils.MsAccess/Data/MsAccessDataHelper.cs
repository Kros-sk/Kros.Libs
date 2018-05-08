using Kros.Utils;
using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;

namespace Kros.Data.MsAccess
{
    /// <summary>
    /// Všeobecné pomocné metódy pre praácu s MS Access.
    /// </summary>
    public static class MsAccessDataHelper
    {
        /// <summary>
        /// Identifikácia MS Access ACE providera: <c>Microsoft.ACE.OLEDB</c>.
        /// </summary>
        public const string AceProviderBase = "Microsoft.ACE.OLEDB";

        /// <summary>
        /// Identifikácia MS Access JET providera: <c>Microsoft.Jet.OLEDB</c>.
        /// </summary>
        public const string JetProviderBase = "Microsoft.Jet.OLEDB";

        private const string BaseConnectionString = "Provider={0};Data Source={1};";

        private const string ResourcesNamespace = "Kros.MsAccess.Resources";
        private const string AccdbResourceName = "EmptyDatabase.accdb";
        private const string MdbResourceName = "EmptyDatabase.mdb";

        private static string _msAccessAceProvider = null;
        private static string _msAccessJetProvider = null;

        /// <summary>
        /// Vytvorí prázdnu MS Access databázu na umiestnení <paramref name="path"/>. Typ databázy (<c>.accdb</c>, <c>.mdb</c>)
        /// je určený parametrom <paramref name="provider"/>.
        /// </summary>
        /// <param name="path">Cesta, kde sa databáza vytvorí. Cesta je úplná, aj s názvom vytváraného súboru.</param>
        /// <param name="provider">Typ databázy, ktorá sa vytovrí.</param>
        /// <remarks>
        /// <alert class="warning">
        /// <para>Ak súbor <paramref name="path"/> už existuje, bude prepísaný.</para>
        /// </alert>
        /// <para>
        /// Na základe hodnoty <paramref name="provider"/> sa len vytvorí daný typ databázy, buď databáza typu <c>.accdb</c>,
        /// alebo staršieho typu <c>.mdb</c>. S príponou súboru sa však nič nerobí, tzn. databázový súbor sa bude volať tak,
        /// ako je zadané v <paramref name="path"/>. Je tak možné vytvoriť databázový súbor s príponou <c>.mdb</c>, ktorý
        /// ale v skutočnosti bude databáza <c>.accdb</c>. Správna prípona je tak plne v kompetencii toho, kto metódu volá.
        /// </para>
        /// </remarks>
        public static void CreateEmptyDatabase(string path, ProviderType provider)
        {
            string resourceFileName = (provider == ProviderType.Ace ? AccdbResourceName : MdbResourceName);
            string resourceName = $"{ResourcesNamespace}.{resourceFileName}";

            using (Stream sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                sourceStream.CopyTo(writer);
            }
        }

        /// <summary>
        /// Vráti nainštalovaný provider pre Microsoft Access. Preferovaný je ACE provider pred JET providerom.
        /// Ak nie je nainštalovaný žiadny ACE, alebo JET provider, je vrátený prázdny reťazec.
        /// </summary>
        public static string MsAccessProvider
        {
            get
            {
                if (MsAccessAceProvider == string.Empty)
                {
                    return MsAccessJetProvider;
                }
                return MsAccessAceProvider;
            }
        }

        /// <summary>
        /// Vráti, či je dostupný zadaný MS Access provider.
        /// </summary>
        /// <param name="provider">Typ providera, ktorý sa kontroluje.</param>
        /// <returns><see langword="true"/> ak zadaný provider je dostupný, <see langword="false"/> ak nie je.</returns>
        public static bool HasProvider(ProviderType provider) =>
            provider == ProviderType.Ace
                ? !string.IsNullOrEmpty(MsAccessAceProvider)
                : !string.IsNullOrEmpty(MsAccessJetProvider);

        /// <summary>
        /// Pre zadaný typ MS Access providera <paramref name="provider"/> vráti konkrétny reťazec predstavujúci
        /// tento provider pre použitie v connection string-u. Ak nie je dostupný žiadny provider, je vrátený prázdny reťazec.
        /// </summary>
        /// <param name="provider">Typ MS Access pridera.</param>
        /// <returns>
        /// Vráti hodnoty vlastností:
        /// <list type="table">
        /// <item>
        ///   <term><see cref="ProviderType.Ace">ProviderType.Ace</see></term>
        ///   <description><see cref="MsAccessAceProvider"/></description>
        /// </item>
        /// <item>
        ///   <term><see cref="ProviderType.Jet">ProviderType.Jet</see></term>
        ///   <description><see cref="MsAccessJetProvider"/></description>
        /// </item>
        /// </list>
        /// </returns>
        public static string GetProviderString(ProviderType provider)
        {
            return provider == ProviderType.Jet ? MsAccessJetProvider : MsAccessAceProvider;
        }

        /// <summary>
        /// Vráti nainštalovaný ACE provider pre Microsoft Access (napríklad <b>Microsoft.ACE.OLEDB.12.0</b>).
        /// Ak ACE provider nie je nainštalovaný, vráti prázdny reťazec.
        /// </summary>
        public static string MsAccessAceProvider
        {
            get
            {
                if (_msAccessAceProvider == null)
                {
                    _msAccessAceProvider = GetMsAccessProvider(AceProviderBase);
                }
                return _msAccessAceProvider;
            }
        }

        /// <summary>
        /// Vráti nainštalovaný JET provider pre Microsoft Access (napríklad <b>Microsoft.Jet.OLEDB.4.0</b>).
        /// Ak JET provider nie je nainštalovaný, vráti prázdny reťazec.
        /// </summary>
        public static string MsAccessJetProvider
        {
            get
            {
                if (_msAccessJetProvider == null)
                {
                    _msAccessJetProvider = GetMsAccessProvider(JetProviderBase);
                }
                return _msAccessJetProvider;
            }
        }

        private static string GetMsAccessProvider(string providerBase)
        {
            using (OleDbDataReader reader = OleDbEnumerator.GetRootEnumerator())
            {
                while (reader.Read())
                {
                    string providerName = reader.GetString(reader.GetOrdinal("SOURCES_NAME")).Trim();
                    if (providerName.StartsWith(providerBase, StringComparison.OrdinalIgnoreCase))
                    {
                        return providerName;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Vráti typ providera z <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Testované spojenie.</param>
        /// <returns>Typ providera.</returns>
        public static ProviderType GetProviderType(IDbConnection connection)
        {
            return GetProviderType(connection.ConnectionString);
        }

        /// <summary>
        /// Vráti typ providera z <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Testované spojenie.</param>
        /// <returns>Typ providera.</returns>
        public static ProviderType GetProviderType(string connectionString)
        {
            Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            return IsMsAccessConnection(connectionString)
                ? IsMsAccessJetProvider(connectionString)
                    ? ProviderType.Jet
                    : ProviderType.Ace
                : throw new InvalidOperationException("Zadaný connection string nie je spojenie na MS Access databázu.");
        }


        /// <summary>
        /// Určuje, či zadané spojenie je spojenie na <b>Microsoft Access</b>.
        /// </summary>
        /// <param name="connectionString">Testované spojenie.</param>
        /// <returns><see langword="true"/>, ak spojenie je na Microsoft Access, <see langword="false"/> ak nie je.</returns>
        public static bool IsMsAccessConnection(string connectionString) =>
            IsMsAccessAceProvider(connectionString) || IsMsAccessJetProvider(connectionString);

        /// <summary>
        /// Určuje, či zadané spojenie je spojenie na <b>Microsoft Access</b>.
        /// </summary>
        /// <param name="connection">Testované spojenie.</param>
        /// <returns><see langword="true"/>, ak spojenie je na Microsoft Access, <see langword="false"/> ak nie je.</returns>
        public static bool IsMsAccessConnection(IDbConnection connection)
        {
            return IsMsAccessConnection(connection.ConnectionString);
        }

        /// <summary>
        /// Vráti, či v zadanom <paramref name="connectionString"/> je použitý Ace provider.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns><see langword="true"/>, ak provider je Ace provider, <see langword="false"/> ak nie je.</returns>
        public static bool IsMsAccessAceProvider(string connectionString) =>
            GetProviderName(connectionString).StartsWith(AceProviderBase, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Vráti, či v zadanom <paramref name="connectionString"/> je použitý Jet provider.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns><see langword="true"/>, ak provider je Jet provider, <see langword="false"/> ak nie je.</returns>
        public static bool IsMsAccessJetProvider(string connectionString) =>
            GetProviderName(connectionString).StartsWith(JetProviderBase, StringComparison.OrdinalIgnoreCase);

        private static string GetProviderName(string connectionString)
        {
            string ret = string.Empty;
            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(connectionString);

            if (builder.Provider != null)
            {
                ret = builder.Provider.Trim();
            }

            return ret;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="connection"/> is exclusive; otherwise <see langword="false"/>.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <returns>
        /// <see langword="true"/> if connection string contains settings
        /// (<b>Share Deny Read</b> and <b>Share Deny Write</b>) or <b>Share Exclusive</b>.
        /// </returns>
        public static bool IsExclusiveMsAccessConnection(IDbConnection connection)
        {
            if (connection.GetType() == typeof(OleDbConnection))
            {
                return IsExclusiveMsAccessConnection(connection.ConnectionString);
            }
            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="connectionString"/> has exclusive connection to MS Access database;
        /// otherwise <see langword="false"/>.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// <see langword="true"/> if connection string contains settings
        /// (<b>Share Deny Read</b> and <b>Share Deny Write</b>) or <b>Share Exclusive</b>.
        /// </returns>
        public static bool IsExclusiveMsAccessConnection(string connectionString)
        {
            return
                ((connectionString.IndexOf("Share Deny Read", StringComparison.OrdinalIgnoreCase) >= 0) &&
                 (connectionString.IndexOf("Share Deny Write", StringComparison.OrdinalIgnoreCase) >= 0)) ||
                (connectionString.IndexOf("Share Exclusive", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Vytvorí connection string k databáze <paramref name="databasePath"/> s aktuálnym providerom
        /// typu <paramref name="provider"/>.
        /// </summary>
        /// <param name="databasePath">Cesta k databáze.</param>
        /// <param name="provider">Typ providera, ktorý sa má použiť. Konkrétny reťazec providera sa použije aktuálny,
        /// ktorý je v systéme, takže nie je potrebné starať sa o jeho verziu.</param>
        /// <returns>Vráti connection string k zadanej databáze, napríklad
        /// <c>Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\data\database.accdb;</c>.</returns>
        public static string CreateConnectionString(string databasePath, ProviderType provider)
            => string.Format(BaseConnectionString, GetProviderString(provider), databasePath);
    }
}
