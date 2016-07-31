using System;
using System.Windows.Controls;

namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for Joystick.xaml
    /// </summary>
    public partial class Joystick : UserControl
    {
        public Joystick()
        {
            InitializeComponent();
        }

        public int DCsteps;

        private void rot1_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps, StepperManager.Instance.Speed3d);
        }

        private void rot2_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps * -1, StepperManager.Instance.Speed3d);
        }

        private void Up_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * -10, StepperManager.Instance.Speed);
        }

        private void Down_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * 10, StepperManager.Instance.Speed);            
        }

        private void Right_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps); 


            ArduinoPorts.Instance.SendCommand(2, step * -20);
            //ArduinoPorts.Instance.SendCommand(2, step * -20, StepperManager.Instance.Speed);
            
        }

        private void Left_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps); 

            ArduinoPorts.Instance.SendCommand(2, step * 20);
            //ArduinoPorts.Instance.SendCommand(2, step * 20, StepperManager.Instance.Speed);

            /*
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(shots);
            //StepperManager.Instance.SendCommand(1,step);          
            ArduinoPorts.Instance.SendCommand(1, step);
            StepperManager.Instance.Step = step;
            StepperManager.Instance.UpDatePosition();
            */
        }

        private void FlipUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * 1, StepperManager.Instance.Speed3d);
        }

        private void FlipDown_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * -1, StepperManager.Instance.Speed3d);
        }

        private void FlipLeft_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * 1, StepperManager.Instance.Speed3d);
        }

        private void FlipRight_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * -1, StepperManager.Instance.Speed3d);
        }


        private void OpenJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOn_btn.IsOpen = false;
            rot1_button.Visibility = System.Windows.Visibility.Visible;
            rot2_button.Visibility = System.Windows.Visibility.Visible;
            DCsteps_sld.Visibility = System.Windows.Visibility.Visible;
            //FlipDown_btn.Visibility = System.Windows.Visibility.Visible;
            //FlipUp_btn.Visibility = System.Windows.Visibility.Visible;
            //FlipRight_btn.Visibility = System.Windows.Visibility.Visible;
            //FlipLeft_btn.Visibility = System.Windows.Visibility.Visible;
            LiveViewOff_btn.IsOpen = true;
            LiveViewOff2_btn.IsOpen = true;
        }

        private void CloseJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOff_btn.IsOpen = false;
            LiveViewOff2_btn.IsOpen = false;
            rot1_button.Visibility = System.Windows.Visibility.Hidden;
            rot2_button.Visibility = System.Windows.Visibility.Hidden;
            DCsteps_sld.Visibility = System.Windows.Visibility.Hidden;
            //FlipDown_btn.Visibility = System.Windows.Visibility.Hidden;
            //FlipUp_btn.Visibility = System.Windows.Visibility.Hidden;
            //FlipRight_btn.Visibility = System.Windows.Visibility.Hidden;
            //FlipLeft_btn.Visibility = System.Windows.Visibility.Hidden;
            LiveViewOn_btn.IsOpen = true;
        }

    }
}
