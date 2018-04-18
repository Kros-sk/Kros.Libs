namespace Kros.Data.Schema
{
    /// <summary>
    /// Rozhranie pre triedy, načítavajúce schému databázy.
    /// </summary>
    public interface IDatabaseSchemaLoader
    {
        /// <summary>
        /// Kontroluje, či trieda dokáže načítať schému zo zadaného spojenia <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je.</returns>
        bool SupportsConnectionType(object connection);

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        DatabaseSchema LoadSchema(object connection);

        /// <summary>
        /// Načíta schému tabuľky <paramref name="tableName"/> z databázy <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <param name="tableName">Meno tabuľky, ktorej schéma sa načíta.</param>
        /// <returns>Vráti načítanú schému tabuľky, alebo hodnotu <c>null</c>, ak taká tabuľka neexistuje.</returns>
        TableSchema LoadTableSchema(object connection, string tableName);
    }

    /// <summary>
    /// Rozhranie pre triedy, načítavajúce schému databázy.
    /// </summary>
    /// <typeparam name="T">Typ spojenia na databázu, s ktorým pracuje daná trieda.</typeparam>
    public interface IDatabaseSchemaLoader<T>
        : IDatabaseSchemaLoader
    {
        /// <summary>
        /// Kontroluje, či trieda dokáže načítať schému zo zadaného spojenia <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je.</returns>
        bool SupportsConnectionType(T connection);

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        DatabaseSchema LoadSchema(T connection);

        /// <summary>
        /// Načíta schému tabuľky <paramref name="tableName"/> z databázy <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <param name="tableName">Meno tabuľky, ktorej schéma sa načíta.</param>
        /// <returns>Vráti načítanú schému tabuľky, alebo hodnotu <c>null</c>, ak taká tabuľka neexistuje.</returns>
        TableSchema LoadTableSchema(T connection, string tableName);
    }
}
