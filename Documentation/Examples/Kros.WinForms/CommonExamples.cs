using Kros.WinForms.DataBinding;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kros.WinForms.Examples
{
    class CommonExamples
    {
        public void CapturingImages()
        {
            #region CaptureControlImages

            // Zachytenie/uloženie obrázku formulára.
            Form frm = new Form();

            Bitmap formImage = frm.CaptureImage();

            frm.SaveImage(@"C:\images\formImage.jpg");

            // Zachytenie obrázku klientskej časti formulára, tzn. bez hlavičky a orámovania.
            Bitmap formClientImage = frm.CaptureImage(true);


            // Zachytenie/uloženie obrázku formulárového prvku.
            Control ctrl = new Panel();

            Bitmap controlImage = ctrl.CaptureImage();

            ctrl.SaveImage(@"C:\images\controlImage.jpg");

            #endregion

            #region CaptureScreenImages

            // Zachytenie/uloženie obrázku primárnej obrazovky.
            Bitmap primaryScreenImage = WinFormsUtils.CaptureScreenImage();

            WinFormsUtils.SaveScreenImage(@"C:\images\primaryScreenImage.jpg");


            // Zachytenie/uloženie obrázku konkrétnej obrazovky.
            Bitmap secondaryScreenImage = WinFormsUtils.CaptureScreenImage(Screen.AllScreens[1]);

            WinFormsUtils.SaveScreenImage(Screen.AllScreens[1], @"C:\images\secondaryScreenImage.jpg");


            // Zachytenie/uloženie obrázkov všetkých obrazoviek naraz.
            Bitmap[] allScreenImages = WinFormsUtils.CaptureAllScreenImages();

            WinFormsUtils.SaveAllScreenImages(@"C:\images\screenImage.jpg"); ;

            #endregion
        }

        public void CommonFormExtensions()
        {
            #region FormMoveToView
            Form mainForm = new Form();
            Form frm = new Form();

            // Presunie formulár tak, aby bol celý zobrazený na monitore, tzn. aby nezasahoval mimo plochu.
            // Ak treba, zmenší jeho veľkosť.
            frm.MoveToView();

            // Presunie formulár tak, aby bol umiestnený na stred formulára "mainForm".
            // Vo všeobecnosti ako "mainForm" môže byť zadaný ľubovoľný prvok, nielen formulár.
            frm.MoveToView(mainForm);
            #endregion
        }

        public void CommonControlExtensions()
        {
            #region ControlPlacement
            Control lbl = new Label();
            Control editor = new TextBox();

            // Presunie TextBox, aby bol naľavo od Label-u.
            editor.PlaceToTheLeftOf(lbl);

            // Presunie TextBox, aby bol napravo od Label-u.
            editor.PlaceToTheRightOf(lbl);

            // Presunie TextBox, aby bol nad Label-om.
            editor.PlaceAbove(lbl);

            // Presunie TextBox, aby bol pod Label-om.
            editor.PlaceUnder(lbl);
            #endregion

            Control ctrl = null;

            #region ControlExtensions
            if (ctrl.IsVisible())
            {
                // ...
            }

            if (!ctrl.IsInDesignMode())
            {
                // ...
            }
            #endregion
        }

        public void WaitCursorExamples()
        {
            #region WaitCursor
            using (new WaitCursor())
            {
                // Do some action.
            }
            #endregion
        }

        public class BindingExamplesForm
            : Form
        {
            private Control _lblInfo = null;

            #region BindingConverters
            public class ExampleViewModel
            {
                public string Name { get; set; }
                public bool DetailsSaved { get; set; }
            }


            private ExampleViewModel _viewModel = new ExampleViewModel();

            // Bindovanie v metóde OnLoad() formulára.
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                // Štandardné bindovanie vlastnosti "Name" v dátach na "Text" prvku "_lblInfo".
                _lblInfo.DataBindings.Add(nameof(Control.Text), _viewModel, nameof(ExampleViewModel.Name));

                // Bindovanie vlastnosti "DetailsSaved" v dátach na "Visible" prvku "_lblInfo",
                // s použitím konvertera "InversionBooleanConverter".
                // Ak hodnota v dátach je true, prvok bude skrytý (Visible bude false) a naopak.
                _lblInfo.DataBindings.Add(nameof(Control.Visible), _viewModel, nameof(ExampleViewModel.DetailsSaved), new InversionBooleanConverter());
            }
            #endregion

            public void bindingStandardProperties()
            {
                #region BindingStandardProperties
                // Bindovanie vlastnosti "Text".
                _lblInfo.DataBindings.BindText(_viewModel, nameof(ExampleViewModel.Name));

                // Bindovanie vlastnosti "Visible".
                _lblInfo.DataBindings.BindVisible(_viewModel, nameof(ExampleViewModel.DetailsSaved), new InversionBooleanConverter());
                #endregion
            }

        }

        #region BindingActions
        public class BindingActionsViewModel
        {
            public void Save()
            {
            }
        }

        public class BindingActionsForm
            : Form
        {
            private Button _btnOk = null; // Nejaké formulárové tlačítko.

            private UiCommandManager _commandManager = new UiCommandManager();
            private BindingActionsViewModel _viewModel = new BindingActionsViewModel();

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                // Nabinduje tlačítko "_btnOk" na akciu "Save" viewmodelu - tzn. po kliku na tlačítko sa vyvolá daná metóda.
                // Prvý parameter určuje, za akých okolností sa akcia vyvolá - v tomto prípade vždy.
                var okCommand = new DelegateCommand(() => true, _viewModel.Save);
                _commandManager.Bind(okCommand, _btnOk);


                // Pre jednoduchšie bindovanie existuje extension metóda.
                _commandManager.BindClick(_btnOk, _viewModel.Save);
            }


            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _commandManager.Dispose();
                }

                base.Dispose(disposing);
            }
        }
        #endregion
    }


    public class ItemClickEventArgs
    {
        public object Item { get; }
    }

    public class BarButtonItem
    {
        public event EventHandler<ItemClickEventArgs> ItemClick;

        protected virtual void OnItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClick?.Invoke(sender, e);
        }
    }

    #region BindingDX
    public class BarButtonItemBinder
        : UiCommandBinder<BarButtonItem>
    {
        protected override void BindCore(BarButtonItem source)
        {
            source.ItemClick += ItemClickEventHandler;
        }

        protected override void UnbindCore(BarButtonItem source)
        {
            source.ItemClick -= ItemClickEventHandler;
        }

        private void ItemClickEventHandler(object sender, ItemClickEventArgs e)
        {
            var button = e.Item as BarButtonItem;
            IUiCommand command = GetCommand(button);

            if (command != null)
            {
                command.Execute();
            }
        }
    }
    #endregion

    #region BindingDXForm
    public class DXBaseForm
        : Form
    {
        private UiCommandManager _commandManager = new UiCommandManager();

        public DXBaseForm()
        {
            // Štandardný konštruktor formulára.
            _commandManager.AddBinder(new BarButtonItemBinder());
        }
    }
    #endregion
}
