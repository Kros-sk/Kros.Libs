using Kros.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kros.IO
{
    /// <summary>
    /// Pomocná trieda na prácu s cestami k súborom/zložkám.
    /// </summary>
    public static class PathHelper
    {

        #region Helpers

        private static readonly Regex _reReplacePathChars = new Regex(CreatePathReplacePattern(), RegexOptions.Compiled);

        private static string CreatePathReplacePattern()
        {
            HashSet<char> invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            invalidChars.UnionWith(Path.GetInvalidPathChars());

            System.Text.StringBuilder result = new System.Text.StringBuilder(invalidChars.Count + 3);
            result.Append("[");
            foreach (char c in invalidChars)
            {
                result.Append(Regex.Escape(Convert.ToString(c)));
            }
            result.Append("]+");

            return result.ToString();
        }

        #endregion

        /// <summary>
        /// Spojí zadané časti <paramref name="parts" /> do reťazca predstavujúceho cestu k súboru/zložke.
        /// </summary>
        /// <param name="parts">Časti cesty.</param>
        /// <returns>Vytvorená cesta.</returns>
        /// <exception cref="ArgumentNullException">
        /// Vstupný parameter <paramref name="parts" /> má hodnotu <see langword="null"/>,
        /// alebo niektorá z jeho častí je <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Niektorý z reťazcov v zozname <paramref name="parts" />
        /// obsahuje nepovolené znaky, definované v <see cref="Path.GetInvalidPathChars">Path.GetInvalidPathChars</see>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Metóda pracuje podobne ako štandardná .NET metóda
        /// <see cref="Path.Combine(string[])" autoUpgrade="true">Path.Combine</see>, s pár upravenými detailami.
        /// <list type="bullet">
        /// <item>Ak niektorá časť začína lomítkom (normálnym, alebo spätným), správa sa k nej rovnako ako keby tak nebolo.
        /// <see cref="Path.Combine(string[])" autoUpgrade="true">Path.Combine</see> výslednú cestu začne vytvárať od poslednej
        /// časti začínajúcej lomítkom (ak taká je v zozname <paramref name="parts"/>), teda všetky časti predtým ignoruje.
        /// <example>
        /// <c>Path.Combine("lorem", "\ipsum", "dolor")</c> vráti <c>\ipsum\dolor</c><br />
        /// <c>PathHelper.BuildPath("lorem", "\ipsum", "dolor")</c> vráti <c>lorem\ipsum\dolor</c>
        /// </example>
        /// </item>
        /// <item>Ak časť končí oddeľovačom disku (dvojbodka), oddeľovač adresárov (lomítko) je vložený aj za ňou.
        /// <example>
        /// <c>Path.Combine("c:", "lorem", "ipsum", "dolor")</c> vráti <b>c:lorem\ipsum\dolor</b><br />
        /// <c>PathHelper.Build("c:", "lorem", "ipsum", "dolor")</c> vráti <b>c:\lorem\ipsum\dolor</b>
        /// </example>
        /// Niektoré funkcie .NET nedokážu s cestou v tvare <c>c:lorem</c> pracovať a vyvolávajú výnimky.
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public static string BuildPath(params string[] parts)
        {
            Check.NotNull(parts, nameof(parts));

            int capacity = CheckBuildPathParts(parts);
            System.Text.StringBuilder sb = new System.Text.StringBuilder(capacity);
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        char firstChar = part[0];
                        char lastChar = sb[sb.Length - 1];

                        if (((firstChar == Path.DirectorySeparatorChar) || (firstChar == Path.AltDirectorySeparatorChar)) &&
                            ((lastChar == Path.DirectorySeparatorChar) || (lastChar == Path.AltDirectorySeparatorChar)))
                        {
                            sb.Length -= 1;
                        }
                        else if ((firstChar != Path.DirectorySeparatorChar) && (firstChar != Path.AltDirectorySeparatorChar) &&
                            (lastChar != Path.DirectorySeparatorChar) && (lastChar != Path.AltDirectorySeparatorChar))
                        {
                            sb.Append(Path.DirectorySeparatorChar);
                        }
                    }
                    sb.Append(part);
                }
            }

            return sb.ToString();
        }

        private static int CheckBuildPathParts(string[] parts)
        {
            int capacity = parts.Length;
            int partIndex = 0;
            char[] invalidChars = Path.GetInvalidPathChars();

            foreach (string part in parts)
            {
                Check.NotNull(part, nameof(parts));

                if (part.IndexOfAny(invalidChars) >= 0)
                {
                    throw new ArgumentException(
                        $"Časť cesty obsahuje nepovolené znaky. Chybná časť je na indexe {partIndex}: {part}");
                }

                capacity += part.Length;
                partIndex++;
            }
            return capacity;
        }

        /// <summary>
        /// V reťazci <paramref name="pathName" /> nahradí zakázané znaky pre názov súboru za pomlčku (<b>-</b>).
        /// Ak je vo vstupnom reťazci viacero zakázaných znakov za sebou, sú všetky ako skupina nahradené jednou pomlčkou.
        /// </summary>
        /// <param name="pathName">Vstupný reťazec, predstavujúci názov súboru.</param>
        /// <returns><inheritdoc cref="ReplaceInvalidPathChars(string, string)"/></returns>
        public static string ReplaceInvalidPathChars(string pathName)
        {
            return ReplaceInvalidPathChars(pathName, "-");
        }

        /// <summary>
        /// V reťazci <paramref name="pathName" /> nahradí zakázané znaky pre názov súboru za reťazec
        /// <paramref name="replacement" />. Ak je vo vstupnom reťazci viacero zakázaných znakov za sebou,
        /// sú všetky ako skupina nahradené jedným znakom.
        /// </summary>
        /// <param name="pathName">Vstupný reťazec, predstavujúci názov súboru.</param>
        /// <param name="replacement">Reťazec, ktorým sa nahrádzajú zakázané znaky. Ak je <see langword="null"/>,
        /// použije sa prázdny reťazec, tzn. zakázané znaky sa zo vstupného reťazca odstránia.</param>
        /// <returns>Reťazec s nahradenými zakázanými znakmi. Ak vstupná hodnota <paramref name="pathName"/>
        /// je <see langword="null"/>, je vrátený prázdny reťazec (<c>string.Empty</c>).</returns>
        public static string ReplaceInvalidPathChars(string pathName, string replacement)
        {
            if (pathName == null)
            {
                return string.Empty;
            }
            if (replacement == null)
            {
                replacement = string.Empty;
            }
            return _reReplacePathChars.Replace(pathName, replacement);
        }

        /// <summary>
        /// Adresárová cesta ku systémovej zložke pre dočasné súbory <legacyBold>bez</legacyBold> lomítka na konci.
        /// </summary>
        public static string GetTempPath()
        {
            return Path.GetTempPath().TrimEnd(new char[] { Path.DirectorySeparatorChar });
        }

        /// <summary>
        /// Zistí, či dané umiestnenie, adresár alebo súbor sídli na zdieľanom adresári na sieti.
        /// </summary>
        /// <param name="path">Cesta, ktorá sa testuje.</param>
        /// <returns><see langword="true"/>, ak <paramref name="path"/> je sieťová cesta, inak <see langword="false"/>.</returns>
        public static bool IsNetworkPath(string path)
        {
            return (GetDriveTypeFromPath(path) == DriveType.Network);
        }

        private static DriveType GetDriveTypeFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return DriveType.Unknown;
            }
            if ((path.Length >= 2) && (path[0] == '\\') && (path[1] == '\\'))
            {
                return DriveType.Network;
            }

            string driveName = path.Length > 3 ? path.Substring(0, 3) : path;
            DriveInfo drive = DriveInfo.GetDrives()
                .Where(item => item.Name.Equals(driveName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (drive != null)
            {
                return drive.DriveType;
            }

            return DriveType.Unknown;
        }
    }
}
