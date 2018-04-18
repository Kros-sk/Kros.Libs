namespace Kros.Data.Schema
{
    /// <summary>
    /// Rozhranie pre triedy načítavajúce a kešujúce schému databázy.
    /// </summary>
    public interface IDatabaseSchemaCache
    {
        /// <summary>
        /// Vráti schému databázy pre spojenie <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Schéma danej databázy.</returns>
        DatabaseSchema GetSchema(object connection);

        /// <summary>
        /// Zruší z keše schému databázy načítanú pre spojenie <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        void ClearSchema(object connection);

        /// <summary>
        /// Vyčistí celú keš - vymaže všetky načítané schémy.
        /// </summary>
        void ClearAllSchemas();

        /// <summary>
        /// Načíta schému databázy pre spojenie <paramref name="connection"/>. Schéma je načítaná priamo z databázy aj v prípade,
        /// že už je uložená v keši.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Schéma danej databázy.</returns>
        DatabaseSchema RefreshSchema(object connection);
    }
}
