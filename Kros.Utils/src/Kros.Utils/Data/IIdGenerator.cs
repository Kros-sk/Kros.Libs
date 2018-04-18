using System;

namespace Kros.Data
{
    /// <summary>
    /// Interfejs popisujúci tiedu, ktorá generuje unikátne identifikátory pre záznamy v tabuľke.
    /// </summary>
    /// <remarks>Jedna inštancia generuje vždy pre jednu tabuľku.</remarks>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="Kros.Data.SqlServer.SqlServerIdGenerator"/>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\IdGeneratorExamples.cs" region="IdGeneratorFactory"/>
    /// </example>
    public interface IIdGenerator
        : IDisposable
    {
        /// <summary>
        /// Vráti identifikátor pre ďalší záznam.
        /// </summary>
        /// <returns>
        /// Unikátny identifikátor pre záznam v tabuľke.
        /// </returns>
        int GetNext();
    }
}
