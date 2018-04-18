using System;
using System.Data;
﻿using Kros.Data.SqlServer;
using System.Data.SqlClient;
using System.Linq;

namespace Kros.Data
{
    /// <summary>
    /// Rozne rozšírenia dátových tried.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Vráti, či je spojenie na databázu otvorené.
        /// </summary>
        /// <param name="cn">Spojenie na databázu, ktoré sa testuje.</param>
        /// <returns><see langword="true"/>, ak spojenie na databázu je otvorené, <see langword="false"/> ak nie je.</returns>
        public static bool IsOpened(this IDbConnection cn)
        {
            return cn.State.HasFlag(ConnectionState.Open);
        }

        /// <summary>
        /// Vráti chybový kód výnimky pre SQL Server.
        /// </summary>
        /// <param name="ex">Výnimka, ktorej chybový kód sa kontroluje.</param>
        /// <returns>Vráti chybový kód ako hodnotu enumerátu <see cref="SqlServerErrorCode" />. Ak chybový kód
        /// je neznámy, alebo nie je definovaný, je vrátená hodnota <see cref="SqlServerErrorCode.Unknown" />.</returns>
        /// <remarks>
        /// Metóda pozerá hodnotu <see cref="SqlError.Number">Number</see> prvej chyby v zozname
        /// <see cref="SqlException.Errors">SqlException.Errors</see>.
        /// </remarks>
        public static SqlServerErrorCode SqlServerErrorCode(this SqlException ex)
        {
            if (ex.Errors.Count > 0)
            {
                int[] values = (int[])Enum.GetValues(typeof(SqlServerErrorCode));
                if (values.Contains(ex.Errors[0].Number))
                {
                    return (SqlServerErrorCode)ex.Errors[0].Number;
                }
            }

            return SqlServer.SqlServerErrorCode.Unknown;
        }
    }
}
