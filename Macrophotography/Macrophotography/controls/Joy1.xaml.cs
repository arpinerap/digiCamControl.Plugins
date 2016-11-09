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
using GalaSoft.MvvmLight;

namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for Joy1.xaml
    /// </summary>
    public partial class Joy1 : UserControl
    {
        public Joy1()
        {
            InitializeComponent();
            StepperManager.Instance.IsLightON = false;
        }

        public int DCsteps;

        #region Open/Close

        private void OpenJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOn_btn.IsOpen = false;
            LiveViewOff_btn.IsOpen = true;
            LiveViewOff2_btn.IsOpen = true;
            RotL.Visibility = System.Windows.Visibility.Visible;
            RotR.Visibility = System.Windows.Visibility.Visible;
            DCsteps_sld2.Visibility = System.Windows.Visibility.Visible;
            Light_swch.Visibility = System.Windows.Visibility.Visible;
            LightFlash.Visibility = System.Windows.Visibility.Visible;  
        }

        private void CloseJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOn_btn.IsOpen = true;
            LiveViewOff_btn.IsOpen = false;
            LiveViewOff2_btn.IsOpen = false;
            StepperManager.Instance.IsLightON = false;
            RotL.Visibility = System.Windows.Visibility.Hidden;
            RotR.Visibility = System.Windows.Visibility.Hidden;
            DCsteps_sld2.Visibility = System.Windows.Visibility.Hidden;
            LightUp.Visibility = System.Windows.Visibility.Hidden;
            LightDown.Visibility = System.Windows.Visibility.Hidden;
            Light_swch.Visibility = System.Windows.Visibility.Hidden;
            LightFlash.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region Movement

        private void FlipUp_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * 1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void FlipRight_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * 1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void FlipDown_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * -1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void FlipLeft_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * -1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * -10, StepperManager.Instance.Speed);
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsFree)
            {
                DCsteps = (int)DCsteps_sld2.Value;
                int shotStepfull = StepperManager.Instance.ShotStepFull;
                int step = shotStepfull * Convert.ToInt32(DCsteps);

                ArduinoPorts.Instance.SendCommand(2, step * -1);
                //ArduinoPorts.Instance.SendCommand(2, step * -20, StepperManager.Instance.Speed);
            }
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * 10, StepperManager.Instance.Speed);  
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps);

            ArduinoPorts.Instance.SendCommand(2, step * 1);
            //ArduinoPorts.Instance.SendCommand(2, step * 20, StepperManager.Instance.Speed);
        }

        private void RotR_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps, StepperManager.Instance.Speed3d);
        }

        private void RotL_Click(object sender, RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld2.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps * -1, StepperManager.Instance.Speed3d);
        }

        #endregion

        #region Light

        private void ToggleButton_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON)
            {
                LightUp.Visibility = System.Windows.Visibility.Visible;
                LightDown.Visibility = System.Windows.Visibility.Visible;
                ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
            }
            if (!StepperManager.Instance.IsLightON)
            {
                LightUp.Visibility = System.Windows.Visibility.Hidden;
                LightDown.Visibility = System.Windows.Visibility.Hidden;
                ArduinoPorts.Instance.SendCommand(8, 0, 0);
            }
        }        

        private void LightUp_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON)
            {
                if (StepperManager.Instance.LightValue < 250)  StepperManager.Instance.LightValue += 10;
                ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
            }
        }

        private void LightDown_Click(object sender, RoutedEventArgs e)
        {
            if (StepperManager.Instance.IsLightON)
            {
                if (StepperManager.Instance.LightValue > 0) StepperManager.Instance.LightValue -= 10;
                ArduinoPorts.Instance.SendCommand(8, 1, StepperManager.Instance.LightValue);
            }

        }

        private void Flash_Click(object sender, RoutedEventArgs e)
        {
            ArduinoPorts.Instance.SendCommand(7, 1000, 254);
        }

        #endregion
    }
}
