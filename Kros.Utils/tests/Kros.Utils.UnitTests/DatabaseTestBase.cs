namespace Kros.Utils.UnitTests
{
    /// <summary>
    /// Základná trieda pre databázové integračné testy.
    /// </summary>
    public class DatabaseTestBase
        : Kros.UnitTests.SqlServerDatabaseTestBase
    {
        protected override string BaseConnectionString => @"Data Source=CENSQL\SQL16ENT2;User ID=KrosPlus;Password=7040;";
    }
}