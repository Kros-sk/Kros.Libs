namespace Kros.KORM.Extensions.Api.UnitTests
{
    internal static class IntegrationTestConfig
    {
        internal static string ConnectionString
        {
            get => "Server=(local)\\SQL2016; UID=sa;PWD=Password12!; Persist Security Info = 'TRUE'";
        }
    }
}
