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
using System.IO.Ports;

namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for RailControl.xaml
    /// </summary>
    public partial class RailControl : UserControl
    {

        public RailControl()
        {
            InitializeComponent();
            
            // Choose Serial Port for Arduino Comunnication
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cmb_ports.Items.Add(port);
            }

            // Steps & Speed Motor TextBoxes Default Values
            // txt_pasos.Clear();
            // txt_pasos.Text = "100";
            // txt_vel.Text = "100";
        }

        private void cmb_ports_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cmb_ports.Items.Clear();
            foreach (string port in ports)
            {
                cmb_ports.Items.Add(port);
            }
        }


        private void bt_atras_Click(object sender, RoutedEventArgs e)
        {
            // SendCommand(1, 0, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            StepperManager.Instance.SendCommand(1, 0, Convert.ToInt32(Steps_slider.Value));
        }

        private void bt_adelante_Click(object sender, RoutedEventArgs e)
        {
            //  SendCommand(1, 1, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            StepperManager.Instance.SendCommand(1, 1, Convert.ToInt32(Steps_slider.Value));
        }


        private void GoFar1(object sender, RoutedEventArgs e)
        {
            // SendCommand(1, 0, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            StepperManager.Instance.SendCommand(1, 0, Convert.ToInt32(Steps_slider.Value));
        }

        private void GoNear1(object sender, RoutedEventArgs e)
        {
            //  SendCommand(1, 1, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            StepperManager.Instance.SendCommand(1, 1, Convert.ToInt32(Steps_slider.Value));
        }



        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // Close Serial Port when Plugin is closed
            try
            {
                StepperManager.Instance.ClosePort();
            }
            catch (Exception)
            {
            }
        }

        private void CountSteps()
        {

        }
    }
}