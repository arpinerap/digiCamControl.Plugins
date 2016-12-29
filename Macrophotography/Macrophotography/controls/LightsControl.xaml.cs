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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for LightsControl.xaml
    /// </summary>
    public partial class LightsControl : UserControl
    {
        public LightsControl()
        {
            InitializeComponent();
        }

        #region Light1

        private void ToggleButton_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON)
            {
                /*LightUp.Visibility = System.Windows.Visibility.Visible;
                LightDown.Visibility = System.Windows.Visibility.Visible;
                LightSlider.Visibility = System.Windows.Visibility.Visible;
                LightFlash.Visibility = System.Windows.Visibility.Visible;
                LightControls1.Visibility = System.Windows.Visibility.Visible;*/
                ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
            }
            if (!StepperManager.Instance.IsLightON)
            {
                /*LightUp.Visibility = System.Windows.Visibility.Hidden;
                LightDown.Visibility = System.Windows.Visibility.Hidden;
                LightSlider.Visibility = System.Windows.Visibility.Hidden;
                LightFlash.Visibility = System.Windows.Visibility.Hidden;
                LightControls1.Visibility = System.Windows.Visibility.Hidden;*/
                ArduinoPorts.Instance.SendCommand(8, 0, 0);
            }
        }

        private void LightUp_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON)
            {
                if (StepperManager.Instance.LightValue < 250) StepperManager.Instance.LightValue += 10;
                ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
            }
        }

        private void LightDown_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON)
            {
                if (StepperManager.Instance.LightValue > 10) StepperManager.Instance.LightValue -= 10;
                ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
            }

        }

        private void Flash_Click(object sender, RoutedEventArgs e)
        {
            ArduinoPorts.Instance.SendCommandFlash(7, 200, 254, 254);
        }

        private void LightSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
        }
        
        #endregion

        #region Light2

        private void ToggleButton2_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON2)
            {
                /*LightUp2.Visibility = System.Windows.Visibility.Visible;
                LightDown2.Visibility = System.Windows.Visibility.Visible;
                LightSlider2.Visibility = System.Windows.Visibility.Visible;
                LightFlash2.Visibility = System.Windows.Visibility.Visible;
                LightControls2.Visibility = System.Windows.Visibility.Visible;*/
                ArduinoPorts.Instance.SendCommand(9, 1, StepperManager.Instance.LightValue2);
            }
            if (!StepperManager.Instance.IsLightON2)
            {
                /*LightUp2.Visibility = System.Windows.Visibility.Hidden;
                LightDown2.Visibility = System.Windows.Visibility.Hidden;
                LightSlider2.Visibility = System.Windows.Visibility.Hidden;
                LightFlash2.Visibility = System.Windows.Visibility.Hidden;
                LightControls2.Visibility = System.Windows.Visibility.Hidden;*/
                ArduinoPorts.Instance.SendCommand(9, 0, 0);
            }
        }

        private void LightUp2_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON2)
            {
                if (StepperManager.Instance.LightValue2 < 250) StepperManager.Instance.LightValue2 += 10;
                ArduinoPorts.Instance.SendCommand(9, 1, StepperManager.Instance.LightValue2);
            }
        }

        private void LightDown2_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON2)
            {
                if (StepperManager.Instance.LightValue2 > 10) StepperManager.Instance.LightValue2 -= 10;
                ArduinoPorts.Instance.SendCommand(9, 1, StepperManager.Instance.LightValue2);
            }

        }

        private void Flash2_Click(object sender, RoutedEventArgs e)
        {
            ArduinoPorts.Instance.SendCommandFlash(7, 1000, 254, 254);
        }

        private void LightSlider2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ArduinoPorts.Instance.SendCommand(9, 1, StepperManager.Instance.LightValue2);
        }

        #endregion

        
    }
}
