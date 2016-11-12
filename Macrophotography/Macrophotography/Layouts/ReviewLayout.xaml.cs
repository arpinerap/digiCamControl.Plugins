using System;
using System.Windows.Controls;
using CameraControl.Core;
using CameraControl.Core.Classes;
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
            Loaded += ReviewLayout_Loaded;
            //UnInit();
            //InitServices();
        }

        private void ReviewLayout_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ZoomAndPanControlMacro = zoomAndPanControl;
                ImageListViewModel = (ImageListViewModel)ZoomAndPanControlMacro.DataContext;
                content = Image;
                ImageLIst = new ListBox();
                UnInit();
                InitServices();
                ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.Zoom_Image_Fit);

            }
            catch (Exception ex)
            {
                
                
            }
        }
    }
}
