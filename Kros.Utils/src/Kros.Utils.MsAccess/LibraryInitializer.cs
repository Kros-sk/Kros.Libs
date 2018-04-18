using Kros.Data;
using Kros.Data.MsAccess;
using Kros.Data.Schema;
using Kros.Data.Schema.MsAccess;

namespace Kros.Utils.MsAccess
{
    /// <summary>
    /// Inicialiácia knižnice.
    /// </summary>
    public static class LibraryInitializer
    {
        /// <summary>
        /// Inicializuje knižnicu - zavolať raz pri štarte programu.
        /// </summary>
        /// <remarks>
        /// Inicializácia vykoná:
        /// <list type="bullet">
        /// <item>Pridanie <see cref="MsAccessSchemaLoader"/> do zoznamu
        /// <see cref="DatabaseSchemaLoader.Default">DatabaseSchemaLoader.Default</see>.</item>
        /// <item>Pridanie <see cref="MsAccessSchemaLoader"/> do zoznamu
        /// <see cref="DatabaseSchemaCache.Default">DatabaseSchemaCache.Default</see>.</item>
        /// <item>Zaregistruje <see cref="MsAccessIdGeneratorFactory"/> do <see cref="IdGeneratorFactories"/>.</item>
        /// </list>
        /// </remarks>
        public static void InitLibrary()
        {
            DatabaseSchemaLoader.Default.AddSchemaLoader(new MsAccessSchemaLoader());
            DatabaseSchemaCache.Default.AddSchemaLoader(new MsAccessSchemaLoader(), new MsAccessCacheKeyGenerator());
            MsAccessIdGeneratorFactory.Register();
        }
    }
}
