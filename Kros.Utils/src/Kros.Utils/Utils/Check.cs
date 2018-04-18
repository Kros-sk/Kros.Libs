using Kros.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Kros.Utils
{
    /// <summary>
    /// Pomocná trieda na kontrolu argumentov metód. Každá kontrola, ak zlyhá, vyvolá nejaký druh výnimky
    /// <c>ArgumentException</c> (<see cref="ArgumentException"/>, <see cref="ArgumentNullException"/>,
    /// <see cref="ArgumentOutOfRangeException"/>).
    /// </summary>
    /// <remarks>
    /// Štandardný spôsob kontroly argumentov je:
    /// <code language="cs" source="..\Examples\Kros.Utils\CheckExamples.cs" region="CheckArgumentsOld"/>
    /// S triedou <c>Check</c> je to jednoduché. Ak je to možné, jednotlivé kontroly vracajú vstupnú hodnotu,
    /// takže je možné na jednom riadku argument skontrolovať, aj priradiť:
    /// <code language = "cs" source="..\Examples\Kros.Utils\CheckExamples.cs" region="CheckArgumentsNew"/>
    /// </remarks>
    public static class Check
    {
        #region Object

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť <c>null</c>. Vyvolaná výnimka má nastavené meno
        /// parametra.
        /// </summary>
        /// <typeparam name="T">Typ vstupného parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentNullException">Hodnota parametra <paramref name="param"/> je <c>null</c>.
        /// </exception>
        [DebuggerStepThrough]
        public static T NotNull<T>(T param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName, Resources.Check_NotNull);
            }
            return param;
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť <c>null</c>. Vyvolaná výnimka má nastavený text
        /// a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ vstupného parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentNullException">Hodnota parametra <paramref name="param"/> je <c>null</c>.
        /// </exception>
        [DebuggerStepThrough]
        public static T NotNull<T>(T param, string paramName, string message)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName, message);
            }
            return param;
        }

        #endregion

        #region Type

        /// <summary>
        /// Parameter <paramref name="param"/> musí byť zadaného typu <typeparamref name="T"/>. Vyvolaná výnimka má nastavené
        /// meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ, ktorého musí parameter byť.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <exception cref="ArgumentException">Parameter nie je požadovaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsOfType<T>(object param, string paramName)
        {
            IsOfType(param, typeof(T), paramName);
        }

        /// <summary>
        /// Parameter <paramref name="param"/> musí byť zadaného typu <typeparamref name="T"/>. Vyvolaná výnimka má nastavený text
        /// a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ, ktorého musí parameter byť.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <exception cref="ArgumentException">Parameter nie je požadovaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsOfType<T>(object param, string paramName, string message)
        {
            IsOfType(param, typeof(T), paramName, message);
        }

        /// <summary>
        /// Parameter <paramref name="param"/> musí byť zadaného typu <paramref name="expectedType"/>. Vyvolaná výnimka má
        /// nastavené meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="expectedType">Typ, ktorého musí parameter byť.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <exception cref="ArgumentException">Parameter nie je požadovaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsOfType(object param, Type expectedType, string paramName)
        {
            if (param.GetType() != expectedType)
            {
                throw new ArgumentException(
                    string.Format(Resources.Check_IsOfType, expectedType.FullName, param.GetType().FullName), paramName);
            }
        }

        /// <summary>
        /// Parameter <paramref name="param"/> musí byť zadaného typu <paramref name="expectedType"/>. Vyvolaná výnimka má
        /// nastavený text a meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="expectedType">Typ, ktorého musí parameter byť.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <exception cref="ArgumentException">Parameter nie je požadovaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsOfType(object param, Type expectedType, string paramName, string message)
        {
            if (param.GetType() != expectedType)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť zadaného typu <typeparamref name="T"/>. Vyvolaná výnimka má
        /// nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Parameter nesmie byť tohto typu.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <exception cref="ArgumentException">Parameter je zadaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsNotOfType<T>(object param, string paramName)
        {
            IsNotOfType(param, typeof(T), paramName);
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť zadaného typu <typeparamref name="T"/>. Vyvolaná výnimka má
        /// nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Parameter nesmie byť tohto typu.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <exception cref="ArgumentException">Parameter je zadaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsNotOfType<T>(object param, string paramName, string message)
        {
            IsNotOfType(param, typeof(T), paramName, message);
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť zadaného typu <paramref name="notExpectedType"/>. Vyvolaná výnimka má
        /// nastavené meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="notExpectedType">Parameter nesmie byť tohto typu.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <exception cref="ArgumentException">Parameter je zadaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsNotOfType(object param, Type notExpectedType, string paramName)
        {
            if (param.GetType() == notExpectedType)
            {
                throw new ArgumentException(
                    string.Format(Resources.Check_IsNotOfType, notExpectedType.FullName, param.GetType().FullName), paramName);
            }
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť zadaného typu <paramref name="notExpectedType"/>. Vyvolaná výnimka má
        /// nastavený text a meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="notExpectedType">Parameter nesmie byť tohto typu.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <exception cref="ArgumentException">Parameter je zadaného typu.</exception>
        [DebuggerStepThrough]
        public static void IsNotOfType(object param, Type notExpectedType, string paramName, string message)
        {
            if (param.GetType() == notExpectedType)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        #endregion

        #region String

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť <c>null</c>, ani prázdny reťazec.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentNullException">Parameter má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Parameter je prázdny reťazec.</exception>
        [DebuggerStepThrough]
        public static string NotNullOrEmpty(string param, string paramName)
        {
            NotNull(param, paramName);
            if (string.IsNullOrEmpty(param))
            {
                throw new ArgumentException(Resources.Check_StringNotNullOrEmpty, paramName);
            }
            return param;
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť <c>null</c>, ani prázdny reťazec.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentNullException">Parameter má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Parameter je prázdny reťazec.</exception>
        [DebuggerStepThrough]
        public static string NotNullOrEmpty(string param, string paramName, string message)
        {
            NotNull(param, paramName, message);
            if (string.IsNullOrEmpty(param))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť <c>null</c>, ani prázdny reťazec, ani reťazec
        /// zložený iba z bielych znakov. Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentNullException">Parameter má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Parameter je prázdny reťazec, alebo reťazec zložený iba z bielych znakov.
        /// </exception>
        [DebuggerStepThrough]
        public static string NotNullOrWhiteSpace(string param, string paramName)
        {
            NotNull(param, paramName);
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException(Resources.Check_StringNotNullOrWhiteSpace, paramName);
            }
            return param;
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť <c>null</c>, ani prázdny reťazec, ani reťazec
        /// zložený iba z bielych znakov. Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentNullException">Parameter má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Parameter je prázdny reťazec, alebo reťazec zložený iba z bielych znakov.
        /// </exception>
        [DebuggerStepThrough]
        public static string NotNullOrWhiteSpace(string param, string paramName, string message)
        {
            NotNull(param, paramName, message);
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        #endregion

        #region Values

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Požadovaná hodnota parametera.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Parameter nemá požadovanú hodnotu.</exception>
        [DebuggerStepThrough]
        public static T Equal<T>(T param, T value, string paramName)
        {
            if (!param.Equals(value))
            {
                throw new ArgumentException(string.Format(Resources.Check_Equal, value, param), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Požadovaná hodnota parametera.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Parameter nemá požadovanú hodnotu.</exception>
        [DebuggerStepThrough]
        public static T Equal<T>(T param, T value, string paramName, string message)
        {
            if (!param.Equals(value))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> nesmie byť <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Rozdielna hodnota parametera.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Parameter má rovnakú hodnotu.</exception>
        [DebuggerStepThrough]
        public static T NotEqual<T>(T param, T value, string paramName)
        {
            if (param.Equals(value))
            {
                throw new ArgumentException(string.Format(Resources.Check_NotEqual, value), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> nesmie byť <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Rozdielna hodnota parametera.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Parameter má rovnakú hodnotu.</exception>
        [DebuggerStepThrough]
        public static T NotEqual<T>(T param, T value, string paramName, string message)
        {
            if (param.Equals(value))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť menšia ako <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je väčšia, alebo rovná zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T LessThan<T>(T param, T value, string paramName) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) < 0))
            {
                throw new ArgumentException(string.Format(Resources.Check_LessThan, value, param), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť menšia ako <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je väčšia, alebo rovná zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T LessThan<T>(T param, T value, string paramName, string message) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) < 0))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť menšia, alebo rovná <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je väčšia zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T LessOrEqualThan<T>(T param, T value, string paramName) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) <= 0))
            {
                throw new ArgumentException(string.Format(Resources.Check_LessOrEqualThan, value, param), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť menšia, alebo rovná <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je väčšia zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T LessOrEqualThan<T>(T param, T value, string paramName, string message) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) <= 0))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť väčšia ako <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je menšia, alebo rovná zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T GreaterThan<T>(T param, T value, string paramName) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) > 0))
            {
                throw new ArgumentException(string.Format(Resources.Check_GreaterThan, value, param), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť väčšia ako <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je menšia, alebo rovná zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T GreaterThan<T>(T param, T value, string paramName, string message) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) > 0))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť väčšia, alebo rovná <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je menšia zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T GreaterOrEqualThan<T>(T param, T value, string paramName) where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) >= 0))
            {
                throw new ArgumentException(string.Format(Resources.Check_GreaterOrEqualThan, value, param), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť väčšia, alebo rovná <paramref name="value"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Parameter.</param>
        /// <param name="value">Hodnota, s ktorou sa parameter porovnáva.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je menšia zadanej hodnote.</exception>
        [DebuggerStepThrough]
        public static T GreaterOrEqualThan<T>(T param, T value, string paramName, string message)
            where T : IComparable<T>
        {
            if (!((param as IComparable<T>).CompareTo(value) >= 0))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť v zozname <paramref name="list"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Hodnota, ktorá musí byť v zozname <paramref name="list"/>.</param>
        /// <param name="list">Zoznam hodnôt, medzi ktorými sa musí nachádzať hodnota <paramref name="param"/>.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra nie je v zadanom zozname.</exception>
        [DebuggerStepThrough]
        public static T IsInList<T>(T param, IEnumerable<T> list, string paramName)
        {
            if (!list.Contains(param))
            {
                throw new ArgumentException(GetIsInListDefaultMessage(param, list), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> musí byť v zozname <paramref name="list"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Hodnota, ktorá musí byť v zozname <paramref name="list"/>.</param>
        /// <param name="list">Zoznam hodnôt, medzi ktorými sa musí nachádzať hodnota <paramref name="param"/>.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra nie je v zadanom zozname.</exception>
        [DebuggerStepThrough]
        public static T IsInList<T>(T param, IEnumerable<T> list, string paramName, string message)
        {
            if (!list.Contains(param))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> nesmie byť v zozname <paramref name="list"/>.
        /// Vyvolaná výnimka má nastavené meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Hodnota, ktorá nesmie byť v zozname <paramref name="list"/>.</param>
        /// <param name="list">Zoznam hodnôt, medzi ktorými sa hodnota <paramref name="param"/> nesmie nachádzať.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je v zadanom zozname.</exception>
        [DebuggerStepThrough]
        public static T IsNotInList<T>(T param, IEnumerable<T> list, string paramName)
        {
            if (list.Contains(param))
            {
                throw new ArgumentException(GetIsNotInListDefaultMessage(param, list), paramName);
            }
            return param;
        }

        /// <summary>
        /// Hodnota parametra <paramref name="param"/> nesmie byť v zozname <paramref name="list"/>.
        /// Vyvolaná výnimka má nastavený text a meno parametra.
        /// </summary>
        /// <typeparam name="T">Typ parametra.</typeparam>
        /// <param name="param">Hodnota, ktorá nesmie byť v zozname <paramref name="list"/>.</param>
        /// <param name="list">Zoznam hodnôt, medzi ktorými sa hodnota <paramref name="param"/> nesmie nachádzať.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Hodnota parametra je v zadanom zozname.</exception>
        [DebuggerStepThrough]
        public static T IsNotInList<T>(T param, IEnumerable<T> list, string paramName, string message)
        {
            if (list.Contains(param))
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        [DebuggerStepThrough]
        private static string GetIsInListDefaultMessage<T>(T param, IEnumerable<T> list)
        {
            return string.Format(Resources.Check_IsInListDefaultMessage, param, GetValuesInListAsString(list));
        }

        [DebuggerStepThrough]
        private static string GetIsNotInListDefaultMessage<T>(T param, IEnumerable<T> list)
        {
            return string.Format(Resources.Check_IsNotInListDefaultMessage, param, GetValuesInListAsString(list));
        }

        [DebuggerStepThrough]
        private static string GetValuesInListAsString<T>(IEnumerable<T> list)
        {
            const int maxItemsInString = 10;
            bool hasMoreValues = false;
            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (T item in list)
            {
                i++;
                if (i > maxItemsInString)
                {
                    hasMoreValues = true;
                    break;
                }
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(item);
            }
            if (hasMoreValues)
            {
                sb.Append("...");
            }

            return sb.ToString();
        }

        #endregion

        #region Guid

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť prázdny GUID (<see cref="Guid.Empty"/>). Vyvolaná výnimka má nastavené
        /// meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Paramere je prázdny GUID (<see cref="Guid.Empty"/>).</exception>
        [DebuggerStepThrough]
        public static Guid NotEmptyGuid(Guid param, string paramName)
        {
            if (param == Guid.Empty)
            {
                throw new ArgumentException(Resources.Check_NotEmptyGuid, paramName);
            }
            return param;
        }

        /// <summary>
        /// Parameter <paramref name="param"/> nesmie byť prázdny GUID (<see cref="Guid.Empty"/>). Vyvolaná výnimka má nastavený
        /// text a meno parametra.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <param name="paramName">Meno parametra.</param>
        /// <param name="message">Text výnimky.</param>
        /// <returns>Vstupný parameter <paramref name="param"/>.</returns>
        /// <exception cref="ArgumentException">Paramere je prázdny GUID (<see cref="Guid.Empty"/>).</exception>
        [DebuggerStepThrough]
        public static Guid NotEmptyGuid(Guid param, string paramName, string message)
        {
            if (param == Guid.Empty)
            {
                throw new ArgumentException(message, paramName);
            }
            return param;
        }

        #endregion
    }
}
