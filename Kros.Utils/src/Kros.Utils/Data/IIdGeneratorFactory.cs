namespace Kros.Data
{
    /// <summary>
    /// Interface popisujúci factory triedu, ktorá vie vytvoriť inštanciu <see cref="IIdGenerator"/>.
    /// </summary>
    /// <seealso cref="Kros.Data.SqlServer.SqlServerIdGeneratorFactory"/>
    /// <seealso cref="IdGeneratorFactories"/>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\IdGeneratorExamples.cs" region="IdGeneratorFactory"/>
    /// </example>
    public interface IIdGeneratorFactory
    {
        /// <summary>
        /// Vytvorí inštanciu <see cref="IIdGenerator"/> pre generovanie unikátnych identifikátorov pre tabuľku
        /// <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">Názov tabuľky pre ktorú sa budú generovať identifikátory.</param>
        /// <returns>
        /// Inštancia <see cref="IIdGenerator"/>.
        /// </returns>
        IIdGenerator GetGenerator(string tableName);

        /// <summary>
        /// Vytvorí inštanciu <see cref="IIdGenerator"/> pre generovanie unikátnych identifikátorov pre tabuľku
        /// <paramref name="tableName"/>.
        /// Nastaví mu aj dávku, ktorú ma rezervovať. (aby v prípade väčšieho počtu nemusel ísť zakaždým do databázy)
        /// </summary>
        /// <param name="tableName">Názov tabuľky pre ktorú sa budú generovať identifikátory.</param>
        /// <param name="batchSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        /// <returns>
        /// Inštancia <see cref="IIdGenerator"/>.
        /// </returns>
        IIdGenerator GetGenerator(string tableName, int batchSize);
    }
}
