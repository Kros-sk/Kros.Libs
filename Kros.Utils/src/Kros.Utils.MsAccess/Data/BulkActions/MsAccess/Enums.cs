namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// Typ stĺpca pre hromadné vkladanie dát do databázy zo súboru.
    /// </summary>
    /// <remarks></remarks>
    public enum BulkInsertColumnType
    {
        /// <summary>
        /// Typ stĺpca nie definovaný.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Stĺpec je textový.
        /// </summary>
        Text = 1
    }
}
