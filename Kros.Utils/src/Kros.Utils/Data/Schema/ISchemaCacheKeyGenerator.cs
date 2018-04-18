namespace Kros.Data.Schema
{
    /// <summary>
    /// Generátor kľúča pre spojenie na databázu. Používa ho <see cref="DatabaseSchemaCache"/>.
    /// </summary>
    public interface ISchemaCacheKeyGenerator
    {
        /// <summary>
        /// Vygeneruje kľúč pre spojenie <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Reťazec.</returns>
        string GenerateKey(object connection);
    }

    /// <summary>
    /// Generátor kľúča pre spojenie na databázu. Používa ho <see cref="DatabaseSchemaCache"/>.
    /// </summary>
    /// <typeparam name="T">Typ spojenia na databázu.</typeparam>
    public interface ISchemaCacheKeyGenerator<T>
        : ISchemaCacheKeyGenerator
    {
        /// <summary>
        /// Vygeneruje kľúč pre spojenie <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Reťazec.</returns>
        string GenerateKey(T connection);
    }
}
