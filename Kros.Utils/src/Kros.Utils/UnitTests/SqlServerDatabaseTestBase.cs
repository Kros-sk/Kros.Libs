using System;
using System.Collections.Generic;

namespace Kros.UnitTests
{
    /// <summary>
    /// Základná trieda pre databázové integračné testy nad SQL Server-om. Trieda sa stará o vytvorenie a inicializáciu
    /// databázy, v ktorej bežia testy. V potomkoch sa už len používa spojenie na vytvorenú databázu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Trieda vytvorí databázu pre testy vo svojom konštruktore a pri volaní <see cref="Dispose()"/> je táto databáza
    /// zmazaná. Databázu je možné aj inicializovať nastavením <see cref="DatabaseInitScripts"/>, aby bolo pre testy
    /// pripravené všetko potrebné.
    /// </para>
    /// <para>
    /// V potomkoch je potrebné prepísať metódy <see cref="BaseConnectionString"/> a nastaviť tak spojenie na server,
    /// kde sa databáza vytvorí. V jednej testovacej triede je možné mať ľubovoľné množstvo testov.
    /// <c>xUnit</c> pre každý test vytvára novú inštanciu triedy, takže každý test bude mať svoju vlastnú inicializovanú
    /// databázu.
    /// </para>
    /// <code language="cs" source="..\Examples\Kros.Utils\SqlServerDatabaseTestBaseExamples.cs" region="SqlServerDatabaseTestBase"/>
    /// </remarks>
    public abstract class SqlServerDatabaseTestBase
        : IDisposable
    {
        private readonly SqlServerTestHelper _serverHelper;

        /// <summary>
        /// Vytvorí inštanciu triedy a inicializuje <see cref="ServerHelper"/>.
        /// </summary>
        public SqlServerDatabaseTestBase()
        {
            _serverHelper = new SqlServerTestHelper(BaseConnectionString, BaseDatabaseName, DatabaseInitScripts);
        }

        /// <summary>
        /// Základný názov databázy, ktorá sa vytvorí. K tomuto názvu je ešte pridaný náhodný GUID, aby bolo meno
        /// databázy unikátne. Štandardne vráti plný názov aktuálnej triedy (<c>GetType().FullName</c>) s pridaným
        /// podčiarovníkom na konci (napr. <c>Kros.Utils.UnitTests.SomeTestClass_</c>).
        /// </summary>
        /// <remarks>
        /// Pozri tiež <see cref="SqlServerTestHelper.BaseDatabaseName"/>,
        /// resp. celú triedu <see cref="SqlServerTestHelper"/>.
        /// </remarks>
        protected virtual string BaseDatabaseName => GetType().FullName.Replace("+", "__") + "_";

        /// <summary>
        /// Základný connection string na server, kde sa vytvorí databáza. Connection string neobsahuje meno konkrétnej
        /// databázy, pretože to je generované, aby každý test išiel izolovane vo vlastnej databáze.
        /// </summary>
        /// <remarks>
        /// Pozri tiež <see cref="SqlServerTestHelper.BaseConnectionString"/>,
        /// resp. celú triedu <see cref="SqlServerTestHelper"/>.
        /// </remarks>
        protected abstract string BaseConnectionString { get; }

        /// <summary>
        /// Skripty pre inicializáciu databázy do počiatočného stavu pre testy.
        /// </summary>
        /// <remarks>
        /// Trieda automaticky vytvorí prázdnu databázu. Ak je potrebné mať databázu nejako konkrétne inicializovanú
        /// (vytvorné tabuľky, pripravené dáta...), v tejto vlatnosti je zoznam skriptov, ktoré sa po vytvorení databázy
        /// spustia.
        /// </remarks>
        protected virtual IEnumerable<string> DatabaseInitScripts => null;

        /// <summary>
        /// Helper pre testy na sql serveri. Obsahuje hlavne spojenie na databázu, v ktorej testy bežia
        /// (<c>ServerHelper.Connection</c>).
        /// </summary>
        protected SqlServerTestHelper ServerHelper
        {
            get
            {
                CheckDisposed();
                return _serverHelper;
            }
        }

        /// <summary>
        /// Kontroluje, či inštancia nebola uvoľnená. Ak už bola (bola zavolaná jej metóda <see cref="Dispose()"/>),
        /// vyvolá výnimku <see cref="ObjectDisposedException"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Vyvolaná pri použití triedy, ak už bola na nej zavolaná
        /// metóda <see cref="Dispose()"/>.</exception>
        protected void CheckDisposed()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #region IDisposable Support

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _serverHelper.Dispose();
                }
                _disposedValue = true;
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
