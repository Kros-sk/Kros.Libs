using Kros.Utils;
using System;
using System.Data;
using System.Collections;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Obálka, ktorá jednoduché rozhranie <see cref="IBulkActionDataReader"/>, zverejní ako komplikovanejší
    /// <see cref="IDataReader"/>.
    /// </summary>
    public class BulkActionDataReader : System.Data.Common.DbDataReader
    {

        #region Fields

        private readonly IBulkActionDataReader _reader;

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí <see cref="IDataReader"/> nad zadaným reader-om <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Vstupný reader.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> má hodnotu <c>null</c>.</exception>
        public BulkActionDataReader(IBulkActionDataReader reader)
        {
            Check.NotNull(reader, nameof(reader));
            _reader = reader;
        }

        #endregion


        #region IDataReader

        /// <summary>
        /// Počet stĺpcov v dátovom riadku.
        /// </summary>
        public override int FieldCount => _reader.FieldCount;

        /// <summary>
        /// Vráti názov stĺpca.
        /// </summary>
        /// <param name="i">Index hľadaného stĺpca.</param>
        /// <returns>Meno stĺpca.</returns>
        /// <exception cref="IndexOutOfRangeException">Zadaný index bol mimo rozsah stĺpcov 0 až <see cref="FieldCount"/>.
        /// </exception>
        public override string GetName(int i) => _reader.GetName(i);

        /// <summary>
        /// Vráti index stĺpca s menom <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno hľadaného stĺpca.</param>
        /// <returns>Index hľadaného stĺpca.</returns>
        public override int GetOrdinal(string name) => _reader.GetOrdinal(name);

        /// <summary>
        /// Vráti hodnotu zadaného stĺpca.
        /// </summary>
        /// <param name="i">Index stĺpca, ktorého hodnota sa vracia.</param>
        /// <returns>Objekt - hodnota daného stĺpca.</returns>
        /// <exception cref="IndexOutOfRangeException">Zadaný index bol mimo rozsah stĺpcov 0 až <see cref="FieldCount"/>.
        /// </exception>
        public override object GetValue(int i) => _reader.GetValue(i);

        /// <summary>
        /// Posunie reader na ďalší záznam.
        /// </summary>
        /// <returns><see langword="true"/>, ak existuje ďalší záznam a reader bol posunutý, <see langword="false"/> ak už ďalší záznam neexistuje.
        /// </returns>
        public override bool Read() => _reader.Read();


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        // Close je implementované iba kvôli tomu, že trieda je zdedená z DbDataReader a to je kvôli tomu,
        // že v .NET Core je chyba v implementácii SqlBulkCopy. Ak reader nededí z DbDataReader, tak v určitých prípadoch
        // padne na NullReferenceException.
        // Keď bude chyba v .NET Core opravená (https://github.com/dotnet/corefx/issues/24638), môže sa zrušť dedenie
        // z DbDataReader, namiesto neho bude len implementovať IDataReader a metóda Close môže ostať neimplementovaná
        // v regióne nižšie.
        public override void Close() { }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion


        #region NotImplemented

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override bool GetBoolean(int i) => throw new NotImplementedException();
        public override bool HasRows => throw new NotImplementedException();
        public override bool IsClosed => throw new NotSupportedException();
        public override bool IsDBNull(int i) => throw new NotImplementedException();
        public override bool NextResult() => throw new NotImplementedException();
        public override byte GetByte(int i) => throw new NotImplementedException();
        public override DataTable GetSchemaTable() => throw new NotImplementedException();
        public override DateTime GetDateTime(int i) => throw new NotImplementedException();
        public override decimal GetDecimal(int i) => throw new NotImplementedException();
        public override double GetDouble(int i) => throw new NotImplementedException();
        public override float GetFloat(int i) => throw new NotImplementedException();
        public override Guid GetGuid(int i) => throw new NotImplementedException();
        public override char GetChar(int i) => throw new NotImplementedException();
        public override IEnumerator GetEnumerator() => throw new NotImplementedException();
        public override int Depth => throw new NotSupportedException();
        public override int GetInt32(int i) => throw new NotImplementedException();
        public override int GetValues(object[] values) => throw new NotImplementedException();
        public override int RecordsAffected => throw new NotSupportedException();
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
            throw new NotImplementedException();
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
            throw new NotImplementedException();
        public override long GetInt64(int i) => throw new NotImplementedException();
        public override object this[int i] => throw new NotSupportedException();
        public override object this[string name] => throw new NotSupportedException();
        public override short GetInt16(int i) => throw new NotImplementedException();
        public override string GetDataTypeName(int i) => throw new NotImplementedException();
        public override string GetString(int i) => throw new NotImplementedException();
        public override Type GetFieldType(int i) => throw new NotImplementedException();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion
    }
}
