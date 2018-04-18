using System;

namespace Kros.Utils
{
    /// <summary>
    /// Trieda sprístupňujúca dátum a čas, aby bolo možné "fixovať" čas v testoch
    /// - <see cref="DateTimeProvider.InjectActualDateTime(DateTime)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Aktuálny čas je dostupný vlastnosťou <see cref="DateTimeProvider.Now"/>, vlastný čas je možné podhodiť volaním
    /// <see cref="DateTimeProvider.InjectActualDateTime(DateTime)"/>:
    /// <code language="cs" source="..\Examples\Kros.Utils\DateTimeProviderExamples.cs" region="BasicExample"/>
    /// </para>
    /// <para>
    /// Natavená hodnota je platná pre aktuálne vlákno, tzn. v dvoch rôznych vláknach je možné mať rôzne nastavené
    /// hodnoty (<see cref="System.ThreadStaticAttribute"/>).
    /// </para>
    /// </remarks>
    /// <seealso cref="System.IDisposable" />
    public class DateTimeProvider : IDisposable
    {
        [ThreadStatic]
        private static DateTime? _injectedDateTime;

        private DateTimeProvider()
        {
        }

        /// <summary>
        /// Ak bol čas fixne nastavený metódou <see cref="InjectActualDateTime(DateTime)"/>, je vrátený ten. Inak je vrátený
        /// reálny čas <see cref="DateTime.Now">DateTime.Now</see>.
        /// </summary>
        public static DateTime Now
        {
            get
            {
                return _injectedDateTime ?? DateTime.Now;
            }
        }

        /// <summary>
        /// Nastaví fixný čas, ktorý provider potom vracia vo vlastnosti <see cref="Now"/>. Používa sa v <c>using</c> bloku.
        /// </summary>
        /// <param name="actualDateTime">Hodnota, ktorú bude provider vracať.</param>
        public static IDisposable InjectActualDateTime(DateTime actualDateTime)
        {
            _injectedDateTime = actualDateTime;

            return new DateTimeProvider();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void Dispose()
        {
            _injectedDateTime = null;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
