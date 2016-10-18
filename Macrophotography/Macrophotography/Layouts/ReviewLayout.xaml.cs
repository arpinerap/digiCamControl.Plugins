using Macrophotography.ViewModel;

namespace Macrophotography.Layouts
{
    /// <summary>
    /// Lógica de interacción para ReviewLayout.xaml
    /// </summary>
    public partial class ReviewLayout : LayoutBase
    {
        public ReviewLayout()
        {
            InitializeComponent();
            ZoomAndPanControl = zoomAndPanControl;
            ImageListViewModel = (ImageListViewModel)ZoomAndPanControl.DataContext;
            content = Image;
            //InitServices();
        }
    }
}
