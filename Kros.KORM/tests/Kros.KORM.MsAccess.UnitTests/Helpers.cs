using Kros.Data.MsAccess;
using Kros.KORM.Query.MsAccess;
using Kros.UnitTests;
using Kros.Utils.MsAccess;
using System;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests
{
    internal static class Helpers
    {
        internal class KormHelper : IDisposable
        {
            public KormHelper(MsAccessTestHelper helper)
            {
                Helper = helper;
                Korm = new Database(helper.Connection);
            }

            public MsAccessTestHelper Helper { get; }
            public IDatabase Korm { get; }

            public void Dispose()
            {
                Korm.Dispose();
                Helper.Dispose();
            }
        }

        static Helpers()
        {
            LibraryInitializer.InitLibrary();
            MsAccessQueryProviderFactory.Register();
        }

        public static KormHelper CreateDatabase(ProviderType provider, params string[] initDatabaseScripts)
            => new KormHelper(new MsAccessTestHelper(provider, initDatabaseScripts));

        private const string ProviderNotAvailableMessage = "MS Access provider {0} is not available.";

        public static void SkipTestIfAceProviderNotAvailable()
            => Skip.If(!MsAccessDataHelper.HasProvider(ProviderType.Ace),
                string.Format(ProviderNotAvailableMessage, MsAccessDataHelper.AceProviderBase));

        public static void SkipTestIfJetProviderNotAvailable()
            => Skip.If(!MsAccessDataHelper.HasProvider(ProviderType.Jet),
                string.Format(ProviderNotAvailableMessage, MsAccessDataHelper.JetProviderBase));
    }
}
