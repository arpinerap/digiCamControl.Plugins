using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Macrophotography;
using MahApps.Metro.Controls;

namespace Macrophotography.Windows
{
    /// <summary>
    /// Interaction logic for MagniWin.xaml
    /// </summary>
    public partial class MagniWin : MetroWindow
    {
        public MagniWin()
        {
            InitializeComponent();
        }

        private void Close_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            colorbmpbx.Source = LiveViewViewModel.Instance.ColorBitmapSource;
        }

        private void Max_btn_Click(object sender, RoutedEventArgs e)
        {
            StepperManager.Instance.MagniMax = StepperManager.Instance.Magni;
            MessageBox.Show("Maximun Magnification Saved");
        }

        private void Min_btn_Click(object sender, RoutedEventArgs e)
        {
            StepperManager.Instance.MagniMin = StepperManager.Instance.Magni;
            MessageBox.Show("Minimun Magnification Saved");
        }
    }
}
