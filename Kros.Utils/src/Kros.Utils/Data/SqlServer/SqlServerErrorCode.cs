namespace Kros.Data.SqlServer
{
    /// <summary>
    /// Chybové kódy Microsoft Sql Server-a.
    /// </summary>
    /// <remarks>Chybový kód je vo výnimke <see cref="System.Data.SqlClient.SqlException">SqlException</see>
    /// vo vlastnosti <see cref="System.Data.SqlClient.SqlException.Number">Number</see>. Zoznam chybových kódov
    /// je na adrese <see href="https://msdn.microsoft.com/en-us/library/cc645603.aspx"/>.</remarks>
    public enum SqlServerErrorCode
    {

        /// <summary>
        /// Neznámy typ chyby.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A network-related or instance-specific error occurred while establishing a connection to SQL Server.
        /// The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
        /// is configured to allow remote connections.
        /// (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
        /// </summary>
        ServerNotAccessible = 53,

        /// <summary>
        /// Invalid column name 'name'.
        /// </summary>
        InvalidColumnName = 207,

        /// <summary>
        /// Invalid object name 'name'.
        /// </summary>
        InvalidObjectName = 208,

        /// <summary>
        /// Database 'name' already exists. Choose a different database name.
        /// </summary>
        DatabaseAlreadyExists = 1801,

        /// <summary>
        /// Cannot insert duplicate key row in object 'objectName' with unique index 'indexName'.
        /// </summary>
        CannotInsertDuplicateKeyRow = 2601,

        /// <summary>
        /// Violation of ... constraint 'constraintName'. Cannot insert duplicate key in object 'objectName'.
        /// </summary>
        DuplicateKeyViolation = 2627,

        /// <summary>
        /// The database could not be exclusively locked to perform the operation.
        /// </summary>
        TheDatabaseCouldNotBeExclusivelyLocked = 5030,

        /// <summary>
        /// The ... statement conflicted with the ... constraint. The conflict occurred in database ..., table ....
        /// </summary>
        ConstraintViolation = 547

    }
}
