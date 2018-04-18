using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Implementácia rozhrania <see cref="IBulkActionDataReader"/> nad ľubovoľným zoznamom objektov.
    /// </summary>
    /// <typeparam name="T">Typ dátového objektu.</typeparam>
    /// <remarks>
    /// Trieda implementuje rozhranie <see cref="IBulkActionDataReader"/> nad ľubovoľným zoznamom objektov,
    /// aby bolo jednoduché takýto zoznam hromadne vložiť do databázy pomocou <c>BulkInsert</c>-u.
    /// </remarks>
    public class EnumerableDataReader<T> : IBulkActionDataReader
    {
        #region Fields

        private IEnumerator<T> _dataEnumerator;
        private readonly List<string> _columnNames;
        private readonly Dictionary<string, PropertyInfo> _propertyCache;

        #endregion

        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu reader-a nad dátami <paramref name="data"/> so zoznamom sĺpcov <paramref name="columnNames"/>.
        /// </summary>
        /// <param name="data">Dáta, nad ktorými je vytvorený reader.</param>
        /// <param name="columnNames">Zoznam stĺpcov s ktorými reader pracuje. Pre všetky stĺpce v zozname musí
        /// existovať property s rovnakým menom v objekte <c>T</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// Hodnota <paramref name="data"/>, alebo <paramref name="columnNames"/> je <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Zoznam <paramref name="columnNames"/> je prázdny, tzn. neobsahuje ani jednu hodnotu.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Trieda <c>T</c> nemá všetky vlastnosti určené zoznamom sĺpcov <paramref name="columnNames"/>.
        /// </exception>
        public EnumerableDataReader(IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            Check.NotNull(data, nameof(data));
            Check.NotNull(columnNames, nameof(columnNames));

            _columnNames = columnNames.ToList();
            Check.GreaterOrEqualThan(_columnNames.Count, 1, nameof(columnNames));

            _propertyCache = LoadProperties(_columnNames);
            _dataEnumerator = data.GetEnumerator();
        }

        #endregion

        #region IBulkActionDataReader

        /// <summary>
        /// Počet stĺpcov.
        /// </summary>
        public int FieldCount => _columnNames.Count;

        /// <summary>
        /// Meno stĺpca na indexe <paramref name="i"/>.
        /// </summary>
        /// <param name="i">Index stĺpca.</param>
        /// <returns>Reťazec.</returns>
        public string GetName(int i) => _columnNames[i];

        /// <summary>
        /// Index stĺpca určeného menom <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <returns>Číslo.</returns>
        public int GetOrdinal(string name) => _columnNames.IndexOf(_columnNames.First(column => column == name));

        /// <summary>
        /// Vráti hodnotu stĺpca na indexe <paramref name="i"/>.
        /// </summary>
        /// <param name="i">Index stĺpca.</param>
        /// <returns>Hodnota stĺpca.</returns>
        public object GetValue(int i) => _propertyCache[GetName(i)].GetValue(_dataEnumerator.Current, null);

        /// <summary>
        /// Posunie sa na ďalšiu položku v zozname.
        /// </summary>
        /// <returns>Vráti <see langword="true"/>, ak sa podarilo posunúť na ďalšiu položku, <see langword="false"/>,
        /// ak už žiadna položka nie je.</returns>
        public bool Read() => _dataEnumerator.MoveNext();

        #endregion

        #region Helpers

        private Dictionary<string, PropertyInfo> LoadProperties(IEnumerable<string> columnNames)
        {
            var properties = new Dictionary<string, PropertyInfo>();

            foreach (string columnName in columnNames)
            {
                properties[columnName] = typeof(T).GetProperty(columnName);
                if (properties[columnName] == null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).FullName} does not have property \"{columnName}\".");
                }
            }

            return properties;
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dataEnumerator.Dispose();
                    _dataEnumerator = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion
    }
}
