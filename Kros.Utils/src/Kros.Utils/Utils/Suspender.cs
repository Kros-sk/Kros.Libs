using System;

namespace Kros.Utils
{
    /// <summary>
    /// Trieda, umožňujúca jednoduché pozastavenie práce. Pozastaviť prácu (<see cref="Suspender.Suspend"/> je možné
    /// viackrát/vnorene. V takom prípade je nutné obnoviť prácu toľkokrát, koľkokrát bola pozastavená.
    /// Ideálne je používať <c>using</c> blok.
    /// </summary>
    /// <remarks>
    /// Vhodné použitie je napríklad pri inicialziáciách objektov. Počas inicializácie je častokrát potrebné nevykonávať určité
    /// funkcie. Rieši sa to príznakom, či beží inicializácia. Trieda <c>Suspender</c> zapúzdruje udržiavanie tohto príznaku,
    /// pričom je možné ho nastavovať viackrát za sebou - vnorene.
    /// </remarks>
    /// <example>
    /// <code language = "cs" source="..\Examples\Kros.Utils\SuspenderExamples.cs" region="Init" />
    /// </example>
    public class Suspender
    {
        #region Nested typed

        private class SuspenderInternal : IDisposable
        {
            private Suspender _suspender;

            public SuspenderInternal(Suspender suspender)
            {
                _suspender = suspender;
            }

            bool _disposed = false;

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    _suspender.Resume();
                }
                _disposed = true;
            }
        }

        #endregion


        private int _counter = 0;

        /// <summary>
        /// Pozastaví prácu. Príznak <see cref="IsSuspended"/> je nastavený na <see langword="true"/>. Ak je práca pozastavená viackrát,
        /// je potrebné ju aj rovnaký počet krát obnoviť.
        /// </summary>
        /// <returns>
        /// Vráti objekt, ktorého uvoľnením (<c>Dispose()</c>) sa práca obnoví. Vhodné je používať <c>using</c> blok.
        /// </returns>
        /// <example>
        /// <code language = "cs" source="..\Examples\Kros.Utils\SuspenderExamples.cs" region="Init" />
        /// </example>
        public IDisposable Suspend()
        {
            if (_counter == 0)
            {
                SuspendCore();
            }
            _counter++;
            return new SuspenderInternal(this);
        }

        /// <summary>
        /// Metóda sa zavolá pri prvom volaní <see cref="Suspend"/>, tzn. pri zmene stavu <see cref="IsSuspended"/>
        /// z <see langword="false"/> na <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// Metóda je určená pre vlastné suspender-y, ktoré dedia z tejto triedy. V jej implementácii si vlastný suspender spraví
        /// potrebné veci pri suspendovaní. Metóda sa volá iba pri prvom suspendovaní, tzn. každé ďalšie volanie
        /// <see cref="Suspend"/> už nevolá <see cref="SuspendCore"/>. Metóda sa volá <legacyBold>pred</legacyBold>
        /// zmenou stavu suspendera, čiže hodnota <see cref="IsSuspended"/> počas jej vykonávania je <see langword="false"/>.
        /// </remarks>
        protected virtual void SuspendCore()
        {
        }

        private void Resume()
        {
            _counter--;
            if (_counter == 0)
            {
                ResumeCore();
            }
        }

        /// <summary>
        /// Metóda sa zavolá pri poslednom volaní <c>Resume</c>, tzn. pri zmene stavu <see cref="IsSuspended"/>
        /// z <see langword="true"/> na <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// Metóda je určená pre vlastné suspender-y, ktoré dedia z tejto triedy. V jej implementácii si vlastný suspender spraví
        /// potrebné veci pri obnovení stavu. Metóda sa volá iba raz, pri poslednom obnovení, tzn. až posledné volanie
        /// <c>Resume</c> zavolá <see cref="ResumeCore"/>. Metóda sa volá <legacyBold>po</legacyBold>
        /// zmene stavu suspendera, čiže hodnota <see cref="IsSuspended"/> počas jej vykonávania je <see langword="false"/>.
        /// </remarks>
        protected virtual void ResumeCore()
        {
        }

        /// <summary>
        /// Vráti príznak, či je práca pozastavená (<see langword="true"/>), alebo nie (<see langword="false"/>).
        /// </summary>
        public bool IsSuspended => _counter > 0;
    }
}