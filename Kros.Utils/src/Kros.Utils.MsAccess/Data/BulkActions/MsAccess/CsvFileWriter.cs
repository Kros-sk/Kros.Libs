using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Kros.Data.BulkActions.MsAccess
{

    /// <summary>
    /// Trieda na vytváranie CSV súborov, tzn. textových súborov, kde dáta sú oddelene čiarkou, prípadne iným znakom.
    /// </summary>
    /// <remarks></remarks>
    internal class CsvFileWriter : IDisposable
    {

        #region Static

        private const string StringTrue = "1";
        private const string StringFalse = "0";
        private const string DecimalNumberFormat = "0.0###########";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const char DefaultValueDelimiter = ',';
        private const char DefaultStringQuote = '"';
        private static readonly Encoding DefaultFileEncoding = Encoding.UTF8;

        protected static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu so zadaným výstupným názvom súboru <paramref name="filePath" /> a kódovaním súboru
        /// <paramref name="encoding" />.
        /// </summary>
        /// <param name="filePath">Názov výstupného súboru, do ktorého sa zapisujú dáta.</param>
        /// <param name="encoding">Kódovanie výstupného súboru.</param>
        /// <param name="append">Ak výstupnú súbor už existuje, určuje, či sa bude do neho pridávať (<see langword="true"/>),
        /// alebo súbor bude prepísaný novým (<see langword="false"/>). Ak súbor neexistuje, je vytvorený nový.
        /// Štandardná hodnota je <see langword="true"/>, čiže do súboru sa pridáva.</param>
        /// <exception cref="ArgumentException">Vyvolaná, ak cesta k súboru <paramref name="filePath" />
        /// je zložená iba z bielych znakov, prípadne jej hodnota je <see langword="null"/>.</exception>
        /// <remarks></remarks>
        public CsvFileWriter(string filePath, Encoding encoding, bool append)
        {
            Check.NotNullOrWhiteSpace(filePath, nameof(filePath));
            FilePath = filePath;
            FileEncoding = encoding;
            _writer = new StreamWriter(FilePath, append, FileEncoding);
            StringQuote = DefaultStringQuote;
        }

        /// <summary>
        /// Vytvorí inštanciu so zadaným výstupným názvom súboru <paramref name="filePath" /> a kódovaním UTF-8.
        /// </summary>
        /// <param name="filePath">Názov výstupného súboru, do ktorého sa zapisujú dáta.</param>
        /// <param name="append">Ak výstupnú súbor už existuje, určuje, či sa bude do neho pridávať (<see langword="true"/>),
        /// alebo súbor bude prepísaný novým (<see langword="false"/>). Ak súbor neexistuje, je vytvorený nový.
        /// Štandardná hodnota je <see langword="true"/>, čiže do súboru sa pridáva.</param>
        /// <exception cref="ArgumentException">Vyvolaná, ak cesta k súboru <paramref name="filePath" />
        /// je zložená iba z bielych znakov, prípadne jej hodnota je <see langword="null"/>.</exception>
        /// <remarks></remarks>
        public CsvFileWriter(string filePath, bool append)
            : this(filePath, DefaultFileEncoding, append)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu so zadaným výstupným názvom súboru <paramref name="filePath" /> a kódovaním UTF-8.
        /// </summary>
        /// <param name="filePath">Názov výstupného súboru, do ktorého sa zapisujú dáta.</param>
        /// <param name="codePage">Číslo kódovej stránky pre kódovanie výstupného súboru. Napríklad pre stredoeurópske
        /// Windows kódovanie je hodnota 1250.</param>
        /// <param name="append">Ak výstupnú súbor už existuje, určuje, či sa bude do neho pridávať (<see langword="true"/>),
        /// alebo súbor bude prepísaný novým (<see langword="false"/>). Ak súbor neexistuje, je vytvorený nový.
        /// Štandardná hodnota je <see langword="true"/>, čiže do súboru sa pridáva.</param>
        /// <exception cref="ArgumentException">Vyvolaná, ak cesta k súboru <paramref name="filePath" />
        /// je zložená iba z bielych znakov, prípadne jej hodnota je <see langword="null"/>.</exception>
        /// <remarks></remarks>
        public CsvFileWriter(string filePath, int codePage, bool append)
            : this(filePath, Encoding.GetEncoding(codePage), append)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu s náhodne vygenerovanou cestou k súboru v systémovej zložke pre dočasné súbory.
        /// Kódovanie súboru je UTF-8.
        /// </summary>
        /// <remarks>Výstupný súbor má náhodný názov je vytvorený v systémovej zložke pre dočasné súbory.
        /// Na jeho vytvorenie je použitá metóda <see cref="System.IO.Path.GetTempFileName">Path.GetTempFileName</see>.
        /// Cesta k výstupnému súboru je dostupná vo vlastnosti <see cref="FilePath">FilePath</see>
        /// a je dostupná aj po uvoľnení (<see cref="Dispose()">Dispose()</see>) objektu.</remarks>
        public CsvFileWriter()
            : this(Path.GetTempFileName(), DefaultFileEncoding, true)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu s náhodne vygenerovanou cestou k súboru v systémovej zložke pre dočasné súbory.
        /// Kódovanie súboru je UTF-8.
        /// </summary>
        /// <param name="encoding">Kódovanie výstupného súboru.</param>
        /// <remarks>Výstupný súbor má náhodný názov je vytvorený v systémovej zložke pre dočasné súbory.
        /// Na jeho vytvorenie je použitá metóda <see cref="System.IO.Path.GetTempFileName">Path.GetTempFileName</see>.
        /// Cesta k výstupnému súboru je dostupná vo vlastnosti <see cref="FilePath">FilePath</see>
        /// a je dostupná aj po uvoľnení (<see cref="Dispose()">Dispose()</see>) objektu.</remarks>
        public CsvFileWriter(Encoding encoding)
            : this(Path.GetTempFileName(), encoding, true)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu s náhodne vygenerovanou cestou k súboru v systémovej zložke pre dočasné súbory.
        /// Kódovanie súboru je UTF-8.
        /// </summary>
        /// <param name="encoding">Kódovanie výstupného súboru.</param>
        /// <param name="append">Ak výstupnú súbor už existuje, určuje, či sa bude do neho pridávať (<see langword="true"/>),
        /// alebo súbor bude prepísaný novým (<see langword="false"/>). Ak súbor neexistuje, je vytvorený nový.</param>
        /// <remarks>Výstupný súbor má náhodný názov je vytvorený v systémovej zložke pre dočasné súbory.
        /// Na jeho vytvorenie je použitá metóda <see cref="System.IO.Path.GetTempFileName">Path.GetTempFileName</see>.
        /// Cesta k výstupnému súboru je dostupná vo vlastnosti <see cref="FilePath">FilePath</see>
        /// a je dostupná aj po uvoľnení (<see cref="Dispose()">Dispose()</see>) objektu.</remarks>
        public CsvFileWriter(Encoding encoding, bool append)
            : this(Path.GetTempFileName(), encoding, append)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu s náhodne vygenerovanou cestou k súboru v systémovej zložke pre dočasné súbory.
        /// Kódovanie súboru je UTF-8.
        /// </summary>
        /// <param name="codePage">Číslo kódovej stránky pre kódovanie výstupného súboru. Napríklad pre stredoeurópske
        /// Windows kódovanie je hodnota 1250.</param>
        /// <remarks>Výstupný súbor má náhodný názov je vytvorený v systémovej zložke pre dočasné súbory.
        /// Na jeho vytvorenie je použitá metóda <see cref="System.IO.Path.GetTempFileName">Path.GetTempFileName</see>.
        /// Cesta k výstupnému súboru je dostupná vo vlastnosti <see cref="FilePath">FilePath</see>
        /// a je dostupná aj po uvoľnení (<see cref="Dispose()">Dispose()</see>) objektu.</remarks>
        public CsvFileWriter(int codePage)
            : this(Path.GetTempFileName(), Encoding.GetEncoding(codePage), true)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu s náhodne vygenerovanou cestou k súboru v systémovej zložke pre dočasné súbory.
        /// Kódovanie súboru je UTF-8.
        /// </summary>
        /// <param name="codePage">Číslo kódovej stránky pre kódovanie výstupného súboru. Napríklad pre stredoeurópske
        /// Windows kódovanie je hodnota 1250.</param>
        /// <param name="append">Ak výstupnú súbor už existuje, určuje, či sa bude do neho pridávať (<see langword="true"/>),
        /// alebo súbor bude prepísaný novým (<see langword="false"/>). Ak súbor neexistuje, je vytvorený nový.</param>
        /// <remarks>Výstupný súbor má náhodný názov je vytvorený v systémovej zložke pre dočasné súbory.
        /// Na jeho vytvorenie je použitá metóda <see cref="System.IO.Path.GetTempFileName">Path.GetTempFileName</see>.
        /// Cesta k výstupnému súboru je dostupná vo vlastnosti <see cref="FilePath">FilePath</see>
        /// a je dostupná aj po uvoľnení (<see cref="Dispose()">Dispose()</see>) objektu.</remarks>
        public CsvFileWriter(int codePage, bool append)
            : this(Path.GetTempFileName(), Encoding.GetEncoding(codePage), append)
        {
        }

        #endregion


        #region Common

        /// <summary>
        /// Oddeľovač hodnôt v súbore.
        /// </summary>
        /// <value>Znak, štandardná hodnota je čiarka (<b>,</b>).</value>
        /// <remarks>Hodnotu je potrebné nastaviť pred sápisom do súboru, aby sa nestalo, že sa nejaké dáta zapíšu
        /// s jedným oddeľovačom a ďalšie dáta s iným.</remarks>
        public char ValueDelimiter { get; set; } = DefaultValueDelimiter;

        private char _stringQuote;
        private string _stringQuoteSubstitute;

        /// <summary>
        /// Znak na uzatváranie reťazcov.
        /// </summary>
        /// <value>Znak, štandardná hodnota sú dvojité úvodzovky (<b>"</b>).</value>
        /// <remarks>Hodnotu je potrebné nastaviť pred sápisom do súboru, aby sa nestalo, že sa nejaké dáta sú
        /// uzatvorené jedným znakom a ďalšie dáta iným.</remarks>
        public char StringQuote
        {
            get { return _stringQuote; }
            set
            {
                _stringQuote = value;
                _stringQuoteSubstitute = new string(_stringQuote, 2);
            }
        }

        /// <summary>
        /// Kódovanie výstupného súboru.
        /// </summary>
        /// <value>Objekt <see cref="System.Text.Encoding">Encoding</see>.</value>
        /// <remarks>Hodnota je nastavená v konštruktore pri vytvorení inštancie a nie je možné ju neskôr zmeniť.</remarks>
        public Encoding FileEncoding { get; } = DefaultFileEncoding;

        /// <summary>
        /// Cesta k výstupnému súboru.
        /// </summary>
        /// <value>Reťazec.</value>
        /// <remarks>Hodnota je v konštruktore pri vytvorení inštancie. Zadaná je buď explicitne, alebo sa vygeneruje
        /// náhodný súbor v systémovej zložke pre dočasné súbory.</remarks>
        public string FilePath { get; }

        #endregion


        #region Writing CSV

        private readonly TextWriter _writer;

        /// <summary>
        /// Do súboru zapíše jeden záznam, ktorý predstavujú všetky dáta v zozname
        /// <paramref name="data">data</paramref>.
        /// </summary>
        /// <param name="data">Zoznam dátových hodnôt.</param>
        /// <remarks>Jedným volaním sa do súboru zapíše celý jeden dátový riadok. Jednotlivé hodnoty musia byť
        /// správneho .NET dátového typu, pretože podľa neho sa určuje, ako budú dáta zapísané. <b>Teda napríklad
        /// je nesprávne zapisovať rôzne typy dát ako reťazce.</b> Na konverziu dát na správny reťazec sú použité
        /// virtuálne metódy <c>ProcessXxxValue</c>. Štandardná konverzia na reťazce z dátových typov je:
        /// <list type="bullet">
        /// <item><b>Reťazec (<see cref="System.String">String</see>):</b> Celý reťazec sa uzavrie medzi znaky
        /// <see cref="StringQuote">StringQuote</see> a v reťazci sú tieto znaky zdvojené.
        /// Takže napríklad reťazec <b><c>Lorem "ipsum" dolor sit amet.</c></b> bude zapísaný ako
        /// <b><c>"Lorem ""ipsum"" dolor sit amet."</c></b> (v prípade, že hodnota
        /// <see cref="StringQuote">StringQuote</see> je dvojitá úvodzovka.</item>
        /// <item><b>Celé číslo (<see cref="int" />, <see cref="long" /> a ostatné celočíselné typy):</b>
        /// Zapísané je ako obyčajné číslo, bez oddeľovačov tisícov.</item>
        /// <item><b>Desatinné číslo (<see cref="double" />, <see cref="decimal" /> a ostatné desatinné typy):</b>
        /// Zapísané je ako číslo s desatinnou <b>bodkou</b>, bez oddeľovačov tisícov.</item>
        /// <item><b><see cref="System.Guid">GUID</see>:</b> Zapísaný je vo formáte
        /// <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>. Efektívne je to len volanie <c>ToString()</c>
        /// nad <c>Guid</c> objektom.</item>
        /// <item><b>Dátum a čas (<see cref="DateTime">DateTime</see>):</b> Zapísaný je fo formáte ISO 8601,
        /// kde oddeľovač dátumovej a časovej časti je medzera. Celý reťazec je uzavretý medzi znakmi
        /// <see cref="StringQuote">StringQuote</see> (napríklad <c>"1978-12-10 06:31:28"</c>).</item>
        /// <item><b>Booelan hodnota (<see cref="bool">Boolean</see>):</b> Ak je <see langword="true"/>, zapísaná je <b>1</b>,
        /// v prípade <see langword="false"/> je zapísaná <b>0</b>.</item>
        /// </list>
        /// </remarks>
        public void Write(IEnumerable<object> data)
        {
            CheckDisposed();

            bool addValueDelimiter = false;
            foreach (var value in data)
            {
                if (addValueDelimiter)
                {
                    _writer.Write(ValueDelimiter);
                    _writer.Write(" ");
                }
                else
                {
                    addValueDelimiter = true;
                }

                if ((value != null) && (!object.ReferenceEquals(value, DBNull.Value)))
                {
                    WriteValue(value);
                }
            }
            _writer.WriteLine();
        }

        public void Write(IDataReader data)
        {
            CheckDisposed();

            while (data.Read())
            {
                WriteRecord(data);
            }
        }

        private void WriteRecord(IDataReader data)
        {
            bool addValueDelimiter = false;

            for (var i = 0; i < data.FieldCount; i++)
            {
                var value = data.GetValue(i);

                if (addValueDelimiter)
                {
                    _writer.Write(ValueDelimiter);
                    _writer.Write(" ");
                }
                else
                {
                    addValueDelimiter = true;
                }

                if ((value != null) && (!object.ReferenceEquals(value, DBNull.Value)))
                {
                    WriteValue(value);
                }
            }
            _writer.WriteLine();
        }

        /// <summary>
        /// Zapíše jednu hodnotu do výstupného súboru.
        /// </summary>
        /// <param name="value">Zapisované dáta.</param>
        /// <remarks></remarks>
        protected void WriteValue(object value)
        {
            TypeCode valueTypeCode = default(TypeCode);

            if (value is bool)
            {
                _writer.Write(ProcessBooleanValue((bool)value));
            }
            else if (value is DateTime)
            {
                _writer.Write(ProcessDateTimeValue((DateTime)value));
            }
            else if ((value is string) || (value is char))
            {
                _writer.Write(StringQuote);
                _writer.Write(ProcessStringValue(Convert.ToString(value)));
                _writer.Write(StringQuote);
            }
            else if (value is Guid)
            {
                _writer.Write(ProcessGuidValue((Guid)value));
            }
            else
            {
                valueTypeCode = Type.GetTypeCode(value.GetType());
                if (value.GetType().IsEnum)
                {
                    if (valueTypeCode == TypeCode.Int64)
                    {
                        _writer.Write(Convert.ToInt64(value));
                    }
                    else
                    {
                        _writer.Write(Convert.ToInt32(value));
                    }
                }
                else
                {
                    switch (valueTypeCode)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Int16:
                        case TypeCode.Byte:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.UInt16:
                        case TypeCode.SByte:
                            _writer.Write(value);
                            break;
                        case TypeCode.Single:
                            _writer.Write(ProcessSingleValue(Convert.ToSingle(value)));
                            break;
                        case TypeCode.Double:
                            _writer.Write(ProcessDoubleValue(Convert.ToDouble(value)));
                            break;
                        case TypeCode.Decimal:
                            _writer.Write(ProcessDecimalValue(Convert.ToDecimal(value)));
                            break;

                        default:
                            throw new ArgumentException(string.Format("Neznámy typ dát pre zápis. Typ dát: {0}, hodnota: {1}.",
                                                                      value.GetType().FullName, value.ToString()));
                    }
                }
            }
        }

        protected virtual string ProcessStringValue(string value)
        {
            if (value.Contains(StringQuote))
            {
                return value.Replace(StringQuote.ToString(), _stringQuoteSubstitute);
            }
            return value;
        }

        protected virtual string ProcessGuidValue(Guid value)
        {
            return value.ToString();
        }

        protected virtual string ProcessDoubleValue(double value)
        {
            return value.ToString(DecimalNumberFormat, Culture.NumberFormat);
        }

        protected virtual string ProcessSingleValue(float value)
        {
            return value.ToString(DecimalNumberFormat, Culture.NumberFormat);
        }

        protected virtual string ProcessDecimalValue(decimal value)
        {
            return value.ToString(DecimalNumberFormat, Culture.NumberFormat);
        }

        protected virtual string ProcessDateTimeValue(DateTime value)
        {
            return StringQuote + value.ToString(DateTimeFormat) + StringQuote;
        }

        protected virtual string ProcessBooleanValue(bool value)
        {
            return value ? StringTrue : StringFalse;
        }

        #endregion


        #region IDisposable

        protected void CheckDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Dispose();
                }
            }
            disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
