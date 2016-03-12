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
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps);
        }

        private void rot2_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps * -1);
        }

        private void Up_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps);
        }

        private void Down_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * -1);            
        }

        private void Right_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * -1);
        }

        private void Left_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps);
        }

        private void FlipUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * -1);
        }

        private void FlipDown_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * 1);
        }

        private void FlipLeft_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(2, DCsteps * 1);
        }

        private void FlipRight_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(2, DCsteps * -1);
        }


        private void OpenJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOn_btn.IsOpen = false;
            rot1_button.Visibility = System.Windows.Visibility.Visible;
            rot2_button.Visibility = System.Windows.Visibility.Visible;
            DCsteps_sld.Visibility = System.Windows.Visibility.Visible;
            FlipDown_btn.Visibility = System.Windows.Visibility.Visible;
            FlipUp_btn.Visibility = System.Windows.Visibility.Visible;
            LiveViewOff_btn.IsOpen = true;
        }

        private void CloseJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOff_btn.IsOpen = false;
            rot1_button.Visibility = System.Windows.Visibility.Hidden;
            rot2_button.Visibility = System.Windows.Visibility.Hidden;
            DCsteps_sld.Visibility = System.Windows.Visibility.Hidden;
            FlipDown_btn.Visibility = System.Windows.Visibility.Hidden;
            FlipUp_btn.Visibility = System.Windows.Visibility.Hidden;
            LiveViewOn_btn.IsOpen = true;
        }
    }
}
