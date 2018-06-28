using Kros.Data.MsAccess;
using Kros.KORM.Query.MsAccess;
using Kros.Utils.MsAccess;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests
{
    internal static class Helpers
    {
        private static bool _inited = false;

        public static void InitLibrary()
        {
            if (!_inited)
            {
                _inited = true;
                LibraryInitializer.InitLibrary();
                MsAccessQueryProviderFactory.Register();
            }
        }

        private const string ProviderNotAvailableMessage = "MS Access provider {0} is not available.";

        public static void SkipTestIfAceProviderNotAvailable()
            => Skip.If(!MsAccessDataHelper.HasProvider(ProviderType.Ace),
                string.Format(ProviderNotAvailableMessage, MsAccessDataHelper.AceProviderBase));

        public static void SkipTestIfJetProviderNotAvailable()
            => Skip.If(!MsAccessDataHelper.HasProvider(ProviderType.Jet),
                string.Format(ProviderNotAvailableMessage, MsAccessDataHelper.JetProviderBase));
    }
}
