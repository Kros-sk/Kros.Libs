using System;

namespace Kros.Extensions
{
    /// <summary>
    /// Rozšírenia pre dátum a čas <see cref="System.DateTime"/>.
    /// </summary>
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Vráti dátum, ktorý predstavuje prvý deň v mesiaci a roku vstupného dátumu <paramref name="value" />.
        /// Časová zložka je vynulovaná.
        /// </summary>
        /// <param name="value">Dátum, ku ktorému je vrátený prvý deň v mesiaci.</param>
        /// <returns>Dátum.</returns>
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        /// <summary>
        /// Vráti dátum, ktorý predstavuje prvý deň v aktuálnom mesiaci.
        /// </summary>
        /// <returns>Dátum.</returns>
        public static DateTime FirstDayOfCurrentMonth()
        {
            return DateTime.Now.FirstDayOfMonth();
        }

        /// <summary>
        /// Vráti dátum, ktorý predstavuje posledný deň v mesiaci a roku vstupného dátumu <paramref name="value" />.
        /// Časová zložka je vynulovaná.
        /// </summary>
        /// <param name="value">Dátum, ku ktorému je vrátený posledný deň v mesiaci.</param>
        /// <returns>Dátum.</returns>
        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        }

        /// <summary>
        /// Vráti dátum, ktorý predstavuje posledný deň v aktuálnom mesiaci.
        /// </summary>
        /// <returns>Dátum.</returns>
        public static DateTime LastDayOfCurrentMonth()
        {
            return DateTime.Now.LastDayOfMonth();
        }

    }
}
