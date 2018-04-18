using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Štandardné parsery pre predvolené hodnoty stĺpcov databáz. Predvolené hodnoty sú v databáze uložené ako reťazec
    /// a je potrebné ich konvertovať na typ daného stĺpca. Ak sa hodnotu nepodarilo skonvertovať na určený typ,
    /// vždy je vrátená hodnota <c>null</c>.
    /// </summary>
    public static class DefaultValueParsers
    {

        /// <summary>
        /// Hlavička funkcie, ktorá parsuje predvolenú hodnotu stĺpca.
        /// </summary>
        /// <param name="defaultValue">Predvolená hodnota stĺpca ako reťazec.</param>
        /// <returns>Vráti hodnotu skonvertovanú na potrebný dátový typ, alebo <c>null</c>, ak sa konverzia nepodarila.</returns>
        public delegate object ParseDefaultValueFunction(string defaultValue);

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static object ParseInt64(string defaultValue)
        {
            if (long.TryParse(defaultValue, out long value))
            {
                return value;
            }
            return null;
        }

        public static object ParseInt32(string defaultValue)
        {
            if (int.TryParse(defaultValue, out int value))
            {
                return value;
            }
            return null;
        }

        public static object ParseInt16(string defaultValue)
        {
            if (short.TryParse(defaultValue, out short value))
            {
                return value;
            }
            return null;
        }

        public static object ParseByte(string defaultValue)
        {
            if (byte.TryParse(defaultValue, out byte value))
            {
                return value;
            }
            return null;
        }

        public static object ParseUInt64(string defaultValue)
        {
            if (ulong.TryParse(defaultValue, out ulong value))
            {
                return value;
            }
            return null;
        }

        public static object ParseUInt32(string defaultValue)
        {
            if (uint.TryParse(defaultValue, out uint value))
            {
                return value;
            }
            return null;
        }

        public static object ParseUInt16(string defaultValue)
        {
            if (ushort.TryParse(defaultValue, out ushort value))
            {
                return value;
            }
            return null;
        }

        public static object ParseSByte(string defaultValue)
        {
            if (sbyte.TryParse(defaultValue, out sbyte value))
            {
                return value;
            }
            return null;
        }

        public static object ParseDecimal(string defaultValue)
        {
            if (decimal.TryParse(defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out decimal value))
            {
                return value;
            }
            return null;
        }

        public static object ParseDouble(string defaultValue)
        {
            if (double.TryParse(defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out double value))
            {
                return value;
            }
            return null;
        }

        public static object ParseSingle(string defaultValue)
        {
            if (float.TryParse(defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out float value))
            {
                return value;
            }
            return null;
        }

        public static object ParseBool(string defaultValue)
        {
            if (int.TryParse(defaultValue, out int parsedInt))
            {
                return (parsedInt != 0);
            }
            else if (bool.TryParse(defaultValue, out bool parsedBool))
            {
                return parsedBool;
            }
            else if (defaultValue.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return null;
        }

        public static object ParseGuid(string defaultValue)
        {
            if (Guid.TryParse(defaultValue, out Guid value))
            {
                return value;
            }
            return null;
        }

        public static object ParseDate(string defaultValue)
        {
            if (defaultValue.StartsWith("#") && defaultValue.EndsWith("#"))
            {
                // Date in format #month/day/year# - i. e. #12/31/2107#
                int year = int.Parse(defaultValue.Substring(7, 4));
                int month = int.Parse(defaultValue.Substring(1, 2));
                int day = int.Parse(defaultValue.Substring(4, 2));
                return new DateTime(year, month, day);
            }
            return null;
        }


        private static Regex reDateTime =
            new Regex(@"(?<year>\d\d\d\d)-(?<month>\d\d)-(?<day>\d\d).(?<hour>\d\d):(?<min>\d\d):(?<sec>\d\d)",
                RegexOptions.Compiled);

        public static object ParseDateSql(string defaultValue)
        {
            Match match = reDateTime.Match(defaultValue);
            if (match.Success)
            {
                return new DateTime(
                    int.Parse(match.Groups["year"].Value),
                    int.Parse(match.Groups["month"].Value),
                    int.Parse(match.Groups["day"].Value),
                    int.Parse(match.Groups["hour"].Value),
                    int.Parse(match.Groups["min"].Value),
                    int.Parse(match.Groups["sec"].Value)
                );
            }
            return null;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
