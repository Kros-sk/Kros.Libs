using Kros.Utils;
using System;
using System.Data;

namespace Kros.Data
{
    /// <summary>
    /// Pomocná trieda pre otváranie spojenia na databázu. Trieda pri použití spojenie otvorí, ak je to potrebné a zase
    /// zatvorí, ak ho otvorila. V prípade, že spojenie už otvorené bolo, nerobí nič.
    /// </summary>
    public class ConnectionHelper
        : Suspender
    {
        /// <summary>
        /// Otvorí spojenie na databázu <paramref name="connection"/>, ak nie je otvorené. Po uvoľnení vráteného objektu
        /// je spojenie opäť uzatvorené v prípade, že bolo na začiatku otvorené. Ak spojenie už bolo otvorené, nerobí sa nič.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, ktoré sa otvorí.</param>
        /// <returns>
        /// Objekt, ktorého uvoľnením sa spojenie na databázu zatvorí v prípade, že bolo na začitku otvorené.
        /// </returns>
        public static IDisposable OpenConnection(IDbConnection connection)
        {
            Check.NotNull(connection, nameof(connection));
            var helper = new ConnectionHelper(connection);
            return helper.Suspend();
        }

        private readonly IDbConnection _connection;
        private bool _closeConnection = false;

        /// <summary>
        /// Vytvorí inštanciu so zadaným psojením na databázu <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <exception cref="ArgumentNullException">
        /// Parameter <paramref name="connection"/> má hodnotu <see langword="null"/>.
        /// </exception>
        private ConnectionHelper(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Otvorí spojenie na databázu, ak je zatvorené.
        /// </summary>
        protected override void SuspendCore()
        {
            base.SuspendCore();
            if (!_connection.IsOpened())
            {
                _closeConnection = true;
                _connection.Open();
            }
        }

        /// <summary>
        /// Zatvorí spojenie na databázu, ak bolo otvorené v <see cref="SuspendCore"/>.
        /// </summary>
        protected override void ResumeCore()
        {
            base.ResumeCore();
            if (_closeConnection)
            {
                _connection.Close();
                _closeConnection = false;
            }
        }
    }
}
