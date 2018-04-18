using Kros.Utils;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kros.Extensions
{
    /// <summary>
    /// Všeobecné rozšírenia pre reťazec (<see cref="System.String">String</see>).
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// Vráti, či zadaný reťazec <paramref name="value"/> je <c>null</c>, alebo prázdny reťazec <c>string.Empty</c>.
        /// </summary>
        /// <param name="value">Testovaný reťazec.</param>
        /// <returns><see langword="true"/>, ak testovaný reťazec je <c>null</c>, alebo <c>string.Empty</c>.</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Vráti, či zadaný reťazec <paramref name="value"/> je <c>null</c>, alebo prázdny reťazec <c>string.Empty</c>,
        /// alebo pozostáva iba z bielych znakov.
        /// </summary>
        /// <param name="value">Testovaný reťazec.</param>
        /// <returns><see langword="true"/>, ak testovaný reťazec je <c>null</c>, alebo <c>string.Empty</c>, alebo pozostáva
        /// iba z bielych znakov.</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Odstráni diakritiku z reťazca.
        /// </summary>
        public static string RemoveDiacritics(this string value)
        {
            string normalizedString = value.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(normalizedString.Length);

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }


        private static readonly Regex _reRemoveNewLines = new Regex(@"[\n\r]");

        /// <summary>
        /// Removes new lines from string. Removed are <c>line feed</c> (<c>\n</c>) and <c>carriage return</c> (<c>\r</c>)
        /// characters.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>String without new lines.</returns>
        public static string RemoveNewLines(this string value)
        {
            return value == null ? null : _reRemoveNewLines.Replace(value, string.Empty);
        }

        /// <summary>
        /// Vráti prvých <paramref name="length"/> znakov zo vstupného reťazca <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Vstupný reťazec.</param>
        /// <param name="length">Dĺžka reťazca.</param>
        /// <returns>Reťazec.</returns>
        public static string Left(this string value, int length)
        {
            Check.GreaterOrEqualThan(length, 0, nameof(length));

            if (value == null)
            {
                return string.Empty;
            }
            else if (length >= value.Length)
            {
                return value;
            }
            return value.Substring(0, length);
        }

        /// <summary>
        /// Vráti posledných <paramref name="length"/> znakov zo vstupného reťazca <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Vstupný reťazec.</param>
        /// <param name="length">Dĺžka reťazca.</param>
        /// <returns>Reťazec.</returns>
        public static string Right(this string value, int length)
        {
            Check.GreaterOrEqualThan(length, 0, nameof(length));

            if (value == null)
            {
                return string.Empty;
            }
            else if (length >= value.Length)
            {
                return value;
            }
            return value.Substring(value.Length - length);
        }
    }
}
