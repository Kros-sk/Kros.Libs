using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kros.IO
{
    /// <summary>
    /// Trieda na formátovanie ciest k súborom. Stará sa o to, aby celková cesta nepresiahla maximálnu
    /// povolenú dĺžku a aj o to, aby bola vygenerovaná cesta k súboru, ktorý ešte neexistuje.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Použitie triedy je vhodné napríklad pri exportoch, ak sa pri exporte generuje viacero súborov. Používateľ programu
    /// zadá len výstupný adresár a názvy súborov sa generujú na základe exportovaných dát. Trieda zabezpečí, aby vygenerované
    /// názvy boli platné, tzn. aby neobsahovali nepovolené znaky a aj aby náhodou nebola vygenerovaná príliš dlhá cesta
    /// k súboru. Zároveň kontroluje existenciu súboru. Ak už existuje spbor, ktorý vygenerovala, je k vygenerovanej ceste
    /// pridané číslo.
    /// </para>
    /// <para>K vytvoreným cestám je možné pridať ešte vlastný informačný reťazec.</para>
    /// <para>Pre jednoduché priame použitie je vytvorená statická inštancia
    /// <see cref="PathFormatter.Default">Default</see>.</para>
    /// </remarks>
    public class PathFormatter
    {
        /// <summary>
        /// Inštancia pre jednoduchšie priame použitie.
        /// </summary>
        public static PathFormatter Default { get; } = new PathFormatter();

        #region Fields

        private string _infoOpeningString = "(";
        private string _infoClosingString = ")";
        private string _counterOpeningString = "(";
        private string _counterClosingString = ")";

        #endregion

        #region Path formatting

        /// <summary>
        /// Naformátuje dátum do podoby, ako bude použitý v názve súboru/zložky.
        /// </summary>
        /// <param name="value">Formátovaný dátum.</param>
        /// <remarks>Minimálna (<see cref="DateTime.MinValue"/>) a maximálna (<see cref="DateTime.MaxValue"/>) hodnota dátumu
        /// sa neformátuje a je vrátený prázdny reťazec.</remarks>
        protected virtual string FormatDateForPath(DateTime value)
        {
            if ((value == DateTime.MinValue) || (value == DateTime.MaxValue))
            {
                return string.Empty;
            }

            return value.ToString("yyyy_MM_dd");
        }

        /// <summary>
        /// Naformátuje obdobie <paramref name="from"/> - <paramref name="to"/> do podoby, ako bude použité
        /// v názve súboru/zložky.
        /// </summary>
        /// <param name="from">Začiatok obdobia.</param>
        /// <param name="to">Koniec obdobia.</param>
        /// <remarks>Ak hodnota niektorého parametra je minimálny alebo maximálny dátum, parameter sa nepoužije.
        /// Na samotné formátovanie dátumu sa použije metóda <see cref="FormatDateForPath"/>.</remarks>
        public virtual string FormatSeasonForPath(DateTime from, DateTime to)
        {
            var sb = new StringBuilder(30);

            if ((from != DateTime.MinValue) && (from != DateTime.MaxValue))
            {
                sb.Append(FormatDateForPath(from));
            }
            if ((to != DateTime.MinValue) && (to != DateTime.MaxValue))
            {
                if (sb.Length > 0)
                {
                    sb.Append(" - ");
                }
                sb.Append(FormatDateForPath(to));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Spojí zadanú zložku <paramref name="folder"/> a názov súboru <paramref name="fileName"/> do cieľovej cesty.
        /// Nekontroluje, či taký súbor existuje.
        /// </summary>
        /// <param name="folder">Zložka.</param>
        /// <param name="fileName">Názov súboru.</param>
        /// <remarks>Ak je výsledná cesta príliš dlhá, názov súboru je skrátený tak, aby bola platná.</remarks>
        public string FormatPath(string folder, string fileName)
        {
            return FormatPathCore(folder, fileName, null, false);
        }

        /// <summary>
        /// Spojí zadanú zložku <paramref name="folder"/> a názov súboru <paramref name="fileName"/>
        /// do cieľovej cesty. Vrátená je cesta k takému súboru, ktorý ešte na disku neexistuje.
        /// </summary>
        /// <param name="folder">Zložka.</param>
        /// <param name="fileName">Názov súboru.</param>
        /// <remarks>V prípade existencie súboru je k názvu pridané také číslo (<b>(1)...</b>),
        /// aby zadaný súbor neexistoval. Ak je výsledná cesta príliš dlhá, názov súboru je skrátený aby cesta bola platná.
        /// Skráti sa pôvodný názov súboru, v prípade, že bolo k názvu dodané číslo, toto je zachované.</remarks>
        public string FormatNewPath(string folder, string fileName)
        {
            return FormatPathCore(folder, fileName, null, true);
        }

        /// <summary>
        /// Spojí zadanú zložku <paramref name="folder"/> a názov súboru <paramref name="fileName"/> do cieľovej cesty,
        /// pričom k názvu súboru pridá zadanú informáciu <paramref name="info"/>. Nekontroluje, či taký súbor existuje.
        /// </summary>
        /// <param name="folder">Zložka.</param>
        /// <param name="fileName">Názov súboru.</param>
        /// <param name="info">Informácia, ktorá je pridaná k názvu súboru.</param>
        /// <remarks>Informácia <paramref name="info"/> je pridaná k názvu súboru do zátvoriek.
        /// Teda ak <paramref name="fileName"/> je <c>exported.xml</c> a <paramref name="info"/> je <c>Lorem Ipsum</c>,
        /// výsledný názov súboru bude <c>exported (Lorem Ipsum).xml</c>. Ak je výsledná cesta príliš dlhá, názov súboru
        /// je skrátený tak, aby bola platná. Pričom je skrátený pôvodný názov súboru, teda dodatočná informácia
        /// <paramref name="info"/> ostane zachovaná.</remarks>
        public string FormatPath(string folder, string fileName, string info)
        {
            return FormatPathCore(folder, fileName, info, false);
        }

        /// <summary>
        /// Spojí zadanú zložku <paramref name="folder"/> a názov súboru <paramref name="fileName"/> do cieľovej cesty,
        /// pričom k názvu súboru pridá zadanú informáciu <paramref name="info"/>. Vrátená je cesta k takému súboru,
        /// ktorý ešte na disku neexistuje.
        /// </summary>
        /// <param name="folder">Zložka.</param>
        /// <param name="fileName">Názov súboru.</param>
        /// <param name="info">Informácia, ktorá je pridaná k názvu súboru.</param>
        /// <remarks>Informácia <paramref name="info"/> je pridaná k názvu súboru do zátvoriek.
        /// Teda ak <paramref name="fileName"/> je <c>exported.xml</c> a <paramref name="info"/> je <c>Lorem Ipsum</c>,
        /// výsledný názov súboru bude <c>exported (Lorem Ipsum).xml</c>. V prípade existencie súboru je k názvu pridané
        /// také číslo (<b>(1)...</b>), aby zadaný súbor neexistoval. Ak je výsledná cesta príliš dlhá, názov súboru
        /// je skrátený tak, aby bola platná. Pričom je skrátený pôvodný názov súboru, teda dodatočná informácia
        /// <paramref name="info"/> a prípadné pridané číslo ostanú zachované.</remarks>
        public string FormatNewPath(string folder, string fileName, string info)
        {
            return FormatPathCore(folder, fileName, info, true);
        }

        /// <summary>
        /// Vráti zoznam ciest k súborom. Cesty sú vytvorené na základe zadanej zložky <paramref name="baseFolder"/>,
        /// základného mena súboru <paramref name="baseFileName"/> a informácií pre jednotlivé súbory
        /// <paramref name="fileInfos"/>.
        /// </summary>
        /// <typeparam name="TKey">Typ kľúča vo vstupnom slovníku <paramref name="fileInfos"/>. Rovnaké kľúče sú
        /// vo vrátenom zozname ciest k súborom.</typeparam>
        /// <param name="baseFolder">Základná zložka pre cesty k súborom.</param>
        /// <param name="baseFileName">Základný názov súboru.</param>
        /// <param name="fileInfos">Informácie, podľa ktorých sa vytvárajú cesty jednotlivým výstupným súborom.</param>
        /// <returns>Slovník s rovnakými kľúčmi ako sú v <paramref name="fileInfos"/>, kde pre každý kľúč je vytvorená
        /// cesta k výstupnému súboru.</returns>
        /// <remarks>
        /// <para>Vo výstupnom slovníku je vytvorená cesta pre každú hodnodu z vstupného <paramref name="fileInfos"/>,
        /// pričom kľúče sú rovnaké.</para>
        /// <para>Zložka v ktorej budú súbory má základ <paramref name="baseFolder"/>.</para>
        /// <para>Názvy súborov sú vytvorené spôsobom ako to robí metóda <see cref="FormatPath(string, string)">FormatPath</see>
        /// s parametrom <c>info</c>, teda k základnému názvu <paramref name="baseFileName"/> je pridaná informácia
        /// zo zoznamu <paramref name="fileInfos"/>.</para>
        /// <para>Výsledná cesty sú skrátené tak, aby nepresiahli maximálnu dĺžky cesty pre súborový systém.
        /// Ak je nejaká cesta príliš dlhá, sú všetky skrátené rovnakým spôsobom tak, aby najdlahšia z nich bola platná.
        /// Skrátený je základný názov súboru o príslušný počet znakov. Preto v tomto prípade je skrátený aj názov
        /// podzložky aj základný názov pre súbory v nej. Dodatočné informácie pre jednotlivé súbory
        /// <paramref name="fileInfos"/> nie sú orezávané.</para>
        /// </remarks>
        /// <example>
        /// <para>Ak napríklad <paramref name="baseFolder"/> je <c>C:\lorem\ipsum</c>, <paramref name="baseFileName"/>
        /// je <c>filename.xml</c> a hodnoty v <paramref name="fileInfos"/> sú:</para>
        /// <list type="table">
        /// <listheader><term>kľúč</term><description>informácia k súboru</description></listheader>
        /// <item><term>1</term><description>some info 1</description></item>
        /// <item><term>2</term><description>some info 2</description></item>
        /// <item><term>3</term><description>some info 3</description></item>
        /// </list>
        /// <para>Výsledný zoznam vytvorených ciest bude:</para>
        /// <list type="table">
        /// <listheader><term>kľúč</term><description>cesta</description></listheader>
        /// <item><term>1</term><description>C:\lorem\ipsum\filename (some info 1).xml</description></item>
        /// <item><term>2</term><description>C:\lorem\ipsum\filename (some info 2).xml</description></item>
        /// <item><term>3</term><description>C:\lorem\ipsum\filename (some info 3).xml</description></item>
        /// </list>
        /// </example>
        public Dictionary<TKey, string> FormatPaths<TKey>(
            string baseFolder,
            string baseFileName,
            Dictionary<TKey, string> fileInfos)
        {
            return FormatPathsCore(baseFolder, baseFileName, false, null, fileInfos);
        }

        /// <summary>
        /// Vráti zoznam ciest k súborom. Cesty sú vytvorené na základe zadanej zložky <paramref name="baseFolder"/>,
        /// základného mena súboru <paramref name="baseFileName"/> a informácií pre jednotlivé súbory
        /// <paramref name="fileInfos"/>.
        /// </summary>
        /// <typeparam name="TKey">Typ kľúča vo vstupnom slovníku <paramref name="fileInfos"/>. Rovnaké kľúče sú
        /// vo vrátenom zozname ciest k súborom.</typeparam>
        /// <param name="baseFolder">Základná zložka pre cesty k súborom. K nej sa pridá pod zložka s rovnakým názovm,
        /// ako je základné meno súboru <paramref name="baseFileName"/>.</param>
        /// <param name="baseFileName">Základný názov súboru. Podľa neho sa vytvorí vo výslednej ceste podzložka
        /// a zároveň je to základ pre názvy výstupných súborov.</param>
        /// <param name="fileInfos">Informácie, podľa ktorých sa vytvárajú cesty jednotlivým výstupným súborom.</param>
        /// <returns>Slovník s rovnakými kľúčmi ako sú v <paramref name="fileInfos"/>, kde pre každý kľúč je vytvorená
        /// cesta k výstupnému súboru.</returns>
        /// <remarks>
        /// <para>Vo výstupnom slovníku je vytvorená cesta pre každú hodnodu z vstupného <paramref name="fileInfos"/>,
        /// pričom kľúče sú rovnaké.</para>
        /// <para>Zložka v ktorej budú súbory má základ <paramref name="baseFolder"/>, ku ktorému je pridaná podzložka
        /// vytvorená z názvu súboru <paramref name="baseFileName"/>. Vždy sa vytvorí taká cesta, ktorá ešte neexistuje
        /// na disku. Ak zadaná zložka už existuje, je k nej ešte pridané číslo (<b>(1)</b>...).</para>
        /// <para>Názvy súborov sú vytvorené spôsobom ako to robí metóda <see cref="FormatPath(string, string)">FormatPath</see>
        /// s parametrom <c>info</c>, teda k základnému názvu <paramref name="baseFileName"/> je pridaná informácia
        /// zo zoznamu <paramref name="fileInfos"/>.</para>
        /// <para>Výsledná cesty sú skrátené tak, aby nepresiahli maximálnu dĺžky cesty pre súborový systém.
        /// Ak je nejaká cesta príliš dlhá, sú všetky skrátené rovnakým spôsobom tak, aby najdlahšia z nich bola platná.
        /// Skrátený je základný názov súboru o príslušný počet znakov. Preto v tomto prípade je skrátený aj názov
        /// podzložky aj základný názov pre súbory v nej. Dodatočné informácie pre jednotlivé súbory
        /// <paramref name="fileInfos"/> nie sú orezávané.</para>
        /// </remarks>
        /// <example>
        /// <para>Ak napríklad <paramref name="baseFolder"/> je <c>C:\lorem\ipsum</c>, <paramref name="baseFileName"/>
        /// je <c>filename.xml</c> a hodnoty v <paramref name="fileInfos"/> sú:</para>
        /// <list type="table">
        /// <listheader><term>kľúč</term><description>informácia k súboru</description></listheader>
        /// <item><term>1</term><description>some info 1</description></item>
        /// <item><term>2</term><description>some info 2</description></item>
        /// <item><term>3</term><description>some info 3</description></item>
        /// </list>
        /// <para>Výsledný zoznam vytvorených ciest bude:</para>
        /// <list type="table">
        /// <listheader><term>kľúč</term><description>cesta</description></listheader>
        /// <item><term>1</term><description>C:\lorem\ipsum\filename\filename (some info 1).xml</description></item>
        /// <item><term>2</term><description>C:\lorem\ipsum\filename\filename (some info 2).xml</description></item>
        /// <item><term>3</term><description>C:\lorem\ipsum\filename\filename (some info 3).xml</description></item>
        /// </list>
        /// </example>
        public Dictionary<TKey, string> FormatPathsInSubfolder<TKey>(
            string baseFolder,
            string baseFileName,
            Dictionary<TKey, string> fileInfos)
        {
            return FormatPathsCore(baseFolder, baseFileName, true, null, fileInfos);
        }

        /// <summary>
        /// Vráti zoznam ciest k súborom. Cesty sú vytvorené na základe zadanej zložky <paramref name="baseFolder"/>,
        /// základného mena súboru <paramref name="baseFileName"/> a informácií pre jednotlivé súbory
        /// <paramref name="fileInfos"/>.
        /// </summary>
        /// <typeparam name="TKey">Typ kľúča vo vstupnom slovníku <paramref name="fileInfos"/>. Rovnaké kľúče sú
        /// vo vrátenom zozname ciest k súborom.</typeparam>
        /// <param name="baseFolder">Základná zložka pre cesty k súborom. K nej sa pridá pod zložka s rovnakým názovm,
        /// ako je základné meno súboru <paramref name="baseFileName"/>.</param>
        /// <param name="baseFileName">Základný názov súboru. Podľa neho sa vytvorí vo výslednej ceste podzložka
        /// a zároveň je to základ pre názvy výstupných súborov.</param>
        /// <param name="subfolderInfo">Informácia, pridaná k názvu podzložky,</param>
        /// <param name="fileInfos">Informácie, podľa ktorých sa vytvárajú cesty jednotlivým výstupným súborom.</param>
        /// <returns>Slovník s rovnakými kľúčmi ako sú v <paramref name="fileInfos"/>, kde pre každý kľúč je vytvorená
        /// cesta k výstupnému súboru.</returns>
        /// <remarks>
        /// <para>Vo výstupnom slovníku je vytvorená cesta pre každú hodnodu z vstupného <paramref name="fileInfos"/>,
        /// pričom kľúče sú rovnaké.</para>
        /// <para>Zložka v ktorej budú súbory má základ <paramref name="baseFolder"/>, ku ktorému je pridaná podzložka
        /// vytvorená z názvu súboru <paramref name="baseFileName"/> ku ktorému je pridaná informácia
        /// <paramref name="subfolderInfo"/>. Vždy sa vytvorí taká cesta, ktorá ešte neexistuje
        /// na disku. Ak zadaná zložka už existuje, je k nej ešte pridané číslo (<b>(1)</b>...).</para>
        /// <para>Názvy súborov sú vytvorené spôsobom ako to robí metóda <see cref="FormatPath(string, string)">FormatPath</see>
        /// s parametrom <c>info</c>, teda k základnému názvu <paramref name="baseFileName"/> je pridaná informácia
        /// zo zoznamu <paramref name="fileInfos"/>.</para>
        /// <para>Výsledná cesty sú skrátené tak, aby nepresiahli maximálnu dĺžky cesty pre súborový systém.
        /// Ak je nejaká cesta príliš dlhá, sú všetky skrátené rovnakým spôsobom tak, aby najdlahšia z nich bola platná.
        /// Skrátený je základný názov súboru o príslušný počet znakov. Preto v tomto prípade je skrátený aj názov
        /// podzložky aj základný názov pre súbory v nej. Dodatočné informácie pre zložku <paramref name="subfolderInfo"/>
        /// a pre jednotlivé súbory <paramref name="fileInfos"/> nie sú orezávané.</para>
        /// </remarks>
        /// <example>
        /// <para>Ak napríklad <paramref name="baseFolder"/> je <c>C:\lorem\ipsum</c>, <paramref name="baseFileName"/>
        /// je <c>filename.xml</c> a hodnoty v <paramref name="fileInfos"/> sú:</para>
        /// <list type="table">
        /// <listheader><term>kľúč</term><description>informácia k súboru</description></listheader>
        /// <item><term>1</term><description>some info 1</description></item>
        /// <item><term>2</term><description>some info 2</description></item>
        /// <item><term>3</term><description>some info 3</description></item>
        /// </list>
        /// <para>Výsledný zoznam vytvorených ciest bude:</para>
        /// <list type="table">
        /// <listheader><term>kľúč</term><description>cesta</description></listheader>
        /// <item><term>1</term><description>C:\lorem\ipsum\filename\filename (some info 1).xml</description></item>
        /// <item><term>2</term><description>C:\lorem\ipsum\filename\filename (some info 2).xml</description></item>
        /// <item><term>3</term><description>C:\lorem\ipsum\filename\filename (some info 3).xml</description></item>
        /// </list>
        /// </example>
        public Dictionary<TKey, string> FormatPathsInSubfolder<TKey>(
            string baseFolder,
            string baseFileName,
            string subfolderInfo,
            Dictionary<TKey, string> fileInfos)
        {
            return FormatPathsCore(baseFolder, baseFileName, true, subfolderInfo, fileInfos);
        }

        #endregion

        #region Settings

        /// <summary>
        /// Otvárací reťazec pre dodatočnú informáciu, ktorý sa vloží do názvu súboru/zložky pred túto informáciu.
        /// Dodatočná informácia je uzavretá medzi <see cref="InfoOpeningString"/> a <see cref="InfoClosingString"/>.
        /// Štandardne je to ľavá okrúhla zátvorka: <b>(</b>.
        /// </summary>
        /// <remarks>Pri nastavení hodnoty sú v nej nahradené znaky, ktoré nie sú platné pre názov súboru/zložky
        /// (<see cref="PathHelper.ReplaceInvalidPathChars(string)"/>).</remarks>
        public string InfoOpeningString
        {
            get
            {
                return _infoOpeningString;
            }
            set
            {
                _infoOpeningString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        /// <summary>
        /// Uzatvárací reťazec pre dodatočnú informáciu, ktorý sa vloží do názvu súboru/zložky za túto informáciu.
        /// Dodatočná informácia je uzavretá medzi <see cref="InfoOpeningString"/> a <see cref="InfoClosingString"/>.
        /// Štandardne je to pravá okrúhla zátvorka: <b>)</b>.
        /// </summary>
        /// <remarks>Pri nastavení hodnoty sú v nej nahradené znaky, ktoré nie sú platné pre názov súboru/zložky
        /// (<see cref="PathHelper.ReplaceInvalidPathChars(string)"/>).</remarks>
        public string InfoClosingString
        {
            get
            {
                return _infoClosingString;
            }
            set
            {
                _infoClosingString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        /// <summary>
        /// Otvárací reťazec pre počítadlo, ktorý sa vloží do názvu súboru/zložky pred toto počítadlo.
        /// Počítadlo je uzavreté medzi <see cref="CounterOpeningString"/> a <see cref="CounterClosingString"/>.
        /// Štandardne je to ľavá okrúhla zátvorka: <b>(</b>.
        /// </summary>
        /// <remarks>Pri nastavení hodnoty sú v nej nahradené znaky, ktoré nie sú platné pre názov súboru/zložky
        /// (<see cref="PathHelper.ReplaceInvalidPathChars(string)"/>).</remarks>
        public string CounterOpeningString
        {
            get
            {
                return _counterOpeningString;
            }
            set
            {
                _counterOpeningString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        /// <summary>
        /// Uzatvárací reťazec pre počítadlo, ktorý sa vloží do názvu súboru/zložky za toto počítadlo.
        /// Počítadlo je uzavreté medzi <see cref="CounterOpeningString"/> a <see cref="CounterClosingString"/>.
        /// Štandardne je to pravá okrúhla zátvorka: <b>)</b>.
        /// </summary>
        /// <remarks>Pri nastavení hodnoty sú v nej nahradené znaky, ktoré nie sú platné pre názov súboru/zložky
        /// (<see cref="PathHelper.ReplaceInvalidPathChars(string)"/>).</remarks>
        public string CounterClosingString
        {
            get
            {
                return _counterClosingString;
            }
            set
            {
                _counterClosingString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        #endregion

        #region Helpers

        // 260 znakov - maximálna dĺžka cesty na disku, prevzatá z .NET.
        // 12 znakov si nechám rezervu, ak by sa k názvu súboru/adresára pridalo číslo, v prípade, že taký už existuje.
        private int _maxPathLength = 260 - 12;

        /// <summary>
        /// Maximálna dĺžka cesty. Je určená na interné použitie.
        /// </summary>
        protected internal int MaxPathLength
        {
            get
            {
                return _maxPathLength;
            }
            set
            {
                _maxPathLength = value;
            }
        }

        /// <summary>
        /// Vráti, či existuje zadaný súbor. Metóda je určená na interné použitie.
        /// </summary>
        /// <param name="filePath">Cesta k súboru.</param>
        /// <returns><see langword="true"/>, ak súbor existuje, <see langword="false"/> ak neexistuje.</returns>
        public virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Vráti, či existuje zadaná zložka. Metóda je určená na interné použitie.
        /// </summary>
        /// <param name="folderPath">Cesta k zložke.</param>
        /// <returns><see langword="true"/>, ak zložka existuje, <see langword="false"/> ak neexistuje.</returns>
        public virtual bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        private string FormatPathCore(string folder, string fileName, string info, bool fileMustNotExist)
        {
            var filePath = FormatPathCore(folder, fileName, info, 0);

            if (fileMustNotExist && FileExists(filePath))
            {
                filePath = FormatNewPathCore(folder, fileName, info);
            }
            return filePath;
        }

        private string FormatPathCore(string folder, string fileName, string info, int counter)
        {
            var ext = Path.GetExtension(fileName);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var filePath = BuildFilePath(folder, baseName, ext, info, counter);

            if (filePath.Length > MaxPathLength)
            {
                var overflow = filePath.Length - MaxPathLength;
                baseName = baseName.Substring(0, baseName.Length - overflow);
                filePath = BuildFilePath(folder, baseName, ext, info, counter);
            }

            return filePath;
        }

        private string FormatNewPathCore(string folder, string fileName, string info)
        {
            string filePath;
            int counter = 0;
            do
            {
                counter += 1;
                filePath = FormatPathCore(folder, fileName, info, counter);
            } while (FileExists(filePath));

            return filePath;
        }

        private Dictionary<TKey, string> FormatPathsCore<TKey>(
            string baseFolder,
            string baseFileName,
            bool useSubfolder,
            string subfolderInfo,
            Dictionary<TKey, string> fileInfos)
        {
            string ext = Path.GetExtension(baseFileName);
            string baseName = Path.GetFileNameWithoutExtension(baseFileName);
            int overflow = 0;

            string outputFolder = baseFolder;
            if (useSubfolder)
            {
                outputFolder = BuildSubfolderPath(baseFolder, baseName, subfolderInfo);
            }
            foreach (var item in fileInfos)
            {
                string filePath = BuildFilePath(outputFolder, baseName, ext, item.Value, 0);
                overflow = Math.Max(overflow, filePath.Length - MaxPathLength);
            }

            if (overflow > 0)
            {
                if (useSubfolder)
                {
                    // Polovicu z pretečenia odrežem z adresára a polovicu z názvu súboru.
                    overflow = (int)Math.Ceiling(overflow / 2.0);
                }
                baseName = baseName.Substring(0, baseName.Length - overflow);
                if (useSubfolder)
                {
                    outputFolder = BuildSubfolderPath(baseFolder, baseName, subfolderInfo);
                }
            }
            if (useSubfolder)
            {
                outputFolder = GetNonExistingOutputFolder(outputFolder);
            }

            var result = new Dictionary<TKey, string>(fileInfos.Count);
            var counters = new Dictionary<string, int>(fileInfos.Count);
            foreach (var item in fileInfos)
            {
                string filePath = BuildFilePath(outputFolder, baseName, ext, item.Value, 0);
                if (counters.ContainsKey(filePath))
                {
                    counters[filePath] += 1;
                    filePath = BuildFilePath(outputFolder, baseName, ext, item.Value, counters[filePath]);
                }
                else
                {
                    counters.Add(filePath, 0);
                }
                result[item.Key] = filePath;
            }

            return result;
        }

        private string BuildFilePath(string folder, string fileName, string extension, string info, int counter)
        {
            var sbFileName = new StringBuilder(fileName);

            if (!string.IsNullOrEmpty(info))
            {
                sbFileName.Append(" ");
                sbFileName.Append(GetInfoString(info));
            }
            if (counter > 0)
            {
                sbFileName.Append(" ");
                sbFileName.Append(GetCounterString(counter));
            }
            sbFileName.Append(extension);
            return PathHelper.BuildPath(folder, sbFileName.ToString());
        }

        private string BuildSubfolderPath(string baseFolder, string subfolder, string info)
        {
            if (!string.IsNullOrEmpty(info))
            {
                subfolder = string.Format("{0} {1}", subfolder, PathHelper.ReplaceInvalidPathChars(info));
            }
            return PathHelper.BuildPath(baseFolder, subfolder);
        }

        private string GetNonExistingOutputFolder(string folder)
        {
            string newFolder = folder;
            int counter = 0;

            while (FolderExists(newFolder))
            {
                counter += 1;
                newFolder = $"{folder} {GetCounterString(counter)}";
            }

            return newFolder;
        }

        private string GetCounterString(int counter)
        {
            return string.Concat(CounterOpeningString, counter, CounterClosingString);
        }

        private string GetInfoString(string info)
        {
            return string.Concat(InfoOpeningString, PathHelper.ReplaceInvalidPathChars(info), InfoClosingString);
        }

        #endregion
    }
}
