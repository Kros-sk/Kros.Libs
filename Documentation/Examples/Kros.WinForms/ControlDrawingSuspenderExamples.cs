using System.Windows.Forms;

namespace Kros.WinForms.Examples
{
    internal class ControlDrawingSuspenderExamples
    {
        private void SimpleExample()
        {
            #region SimpleExample
            Control ctrl1 = null;
            Control ctrl2 = null;

            using (ControlDrawingSuspender.SuspendDrawing(ctrl1, ctrl2))
            {
                // Akcia, počas ktorej je vypnuté vykresľovanie prvkov "ctrl1" a "ctrl2".
            }
            #endregion
        }
    }
}
