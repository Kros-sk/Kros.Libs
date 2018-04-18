namespace Kros.Data.Schema
{
    /// <summary>
    /// Typ indexu tabuľky.
    /// </summary>
    public enum IndexType
    {
        /// <summary>
        /// Obyčajný index.
        /// </summary>
        Index = 0,

        /// <summary>
        /// Unikátny index.
        /// </summary>
        UniqueKey = 1,

        /// <summary>
        /// Primárny kľúč tabuľky.
        /// </summary>
        PrimaryKey = 2
    }

    /// <summary>
    /// Zoradenie indexového stĺpca.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Vzostupne - od najmenšej hodnoty po najväčšiu.
        /// </summary>
        Ascending = 0,

        /// <summary>
        /// Zostupne - od najväčšej hodnoty po najmenšiu.
        /// </summary>
        Descending = 1
    }

    /// <summary>
    /// Pravidlo cudzieho kľúča - ako sa zachovať k detským záznamom po zmenení/zmazaní hlavného nadradeného.
    /// </summary>
    public enum ForeignKeyRule
    {
        /// <summary>
        /// Žiadna akcia.
        /// </summary>
        NoAction = 0,

        /// <summary>
        /// Zmena v hlavnej tabuľke sa propaguje do detských záznamov.
        /// </summary>
        Cascade = 1,

        /// <summary>
        /// Pri zmene hlavného záznamu sa cudzí kľúč v detských záznamoch nastaví na <c>NULL</c>.
        /// </summary>
        SetNull = 2,

        /// <summary>
        /// Pri zmene hlavného záznamu sa cudzí kľúč v detských záznamoch nastaví na predvolenú hodnotu stĺpca.
        /// </summary>
        SetDefault = 3
    }
}
