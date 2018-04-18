using System;
using System.Data.OleDb;
using System.Linq;

namespace Kros.Data.MsAccess
{
    /// <summary>
    /// Všeobecné rozšírenia pre MS Access.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Vráti chybový kód výnimky pre MsAccess.
        /// </summary>
        /// <param name="ex">Výnimka, ktorej chybový kód sa kontroluje.</param>
        /// Metóda pozerá hodnotu <see cref="OleDbError.SQLState">SQLState</see> prvej chyby v zozname
        /// <see cref="OleDbException.Errors">OleDbException.Errors</see>.
        public static MsAccessErrorCode MsAccessErrorCode(this OleDbException ex)
        {
            return MsAccessErrorCode(ex, out string sqlState);
        }

        /// <summary>
        /// Vráti chybový kód výnimky pre MsAccess.
        /// </summary>
        /// <param name="ex">Výnimka, ktorej chybový kód sa kontroluje.</param>
        /// <param name="sqlState">Skutočná hodnota vlastnosti <see cref="OleDbError.SQLState">SQLState</see>.</param>
        /// <returns>Vráti chybový kód ako hodnotu enumerátu <see cref="Kros.Data.MsAccess.MsAccessErrorCode" />. Ak chybový kód
        /// je neznámy, alebo nie je definovaný, je vrátená hodnota <see cref="MsAccessErrorCode.Unknown" />.</returns>
        /// <remarks>
        /// Metóda pozerá hodnotu <see cref="OleDbError.SQLState">SQLState</see> prvej chyby v zozname
        /// <see cref="OleDbException.Errors">OleDbException.Errors</see>.
        /// </remarks>
        public static MsAccessErrorCode MsAccessErrorCode(this OleDbException ex, out string sqlState)
        {
            MsAccessErrorCode result = MsAccess.MsAccessErrorCode.Unknown;
            sqlState = string.Empty;

            if (ex.Errors.Count > 0)
            {
                sqlState = ex.Errors[0].SQLState;
                if ((!string.IsNullOrEmpty(sqlState)) && int.TryParse(sqlState, out int intState))
                {
                    int[] values = (int[])Enum.GetValues(typeof(MsAccessErrorCode));
                    if (values.Contains(intState))
                    {
                        result = (MsAccessErrorCode)intState;
                    }
                }
            }

            return result;
        }
    }
}
