using Macrophotography.ViewModel;

namespace Macrophotography.Layouts
{
    /// <summary>
    /// Lógica de interacción para ReviewLayout.xaml
    /// </summary>
    public partial class ReviewLayout : LayoutBaseMacro
    {
        public ReviewLayout()
        {
            InitializeComponent();
            ZoomAndPanControlMacro = zoomAndPanControl;
            ImageListViewModel = (ImageListViewModel)ZoomAndPanControlMacro.DataContext;
            content = Image;
            //UnInit();
            //InitServices();
        }
    }
}
