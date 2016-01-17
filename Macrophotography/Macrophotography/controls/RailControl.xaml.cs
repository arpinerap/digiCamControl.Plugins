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
        private SerialPort sp = new SerialPort();
        private object _locker = new object();


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

        private void OpenPort()
        {
            // Open Serial Port
            if (!sp.IsOpen)
            {
                sp.PortName = (string)cmb_ports.SelectedItem;
                sp.BaudRate = 9600;
                sp.Parity = Parity.None;
                sp.StopBits = StopBits.One;
                sp.DataBits = 8;
                sp.WriteTimeout = 3500;
                sp.Open();
                sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            }
        }

        private void ClosePort()
        {
            // Close Serial Port
            if (sp.IsOpen)
            {
                sp.DataReceived -= sp_DataReceived;
                sp.Close();
            }
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Active Motion Control Buttons when confimation mesaage arrives fron Arduino
            try
            {
                SerialPort spL = (SerialPort)sender;
                string str = spL.ReadLine();
                //lst_message.Items.Add(str);
                if (str.Contains("ok"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        bt_adelante.IsEnabled = true;
                        bt_atras.IsEnabled = true;
                    }));
                }
            }
            catch (Exception ex)
            {

            }


        }

        private void SendCommand(int motor, int dir, int steps, int spd)
        {
            // Line Order for Arduino
            lock (_locker)
            {
                try
                {
                    ClosePort();
                    OpenPort();
                    string cmd = Convert.ToString(motor);
                    cmd += " ";
                    cmd += Convert.ToString(dir);
                    cmd += " ";
                    cmd += Convert.ToString(steps);
                    cmd += " ";
                    cmd += Convert.ToString(spd);
                    sp.WriteLine(cmd);
                    bt_adelante.IsEnabled = false;
                    bt_atras.IsEnabled = false;
                }
                catch (Exception exception)
                {
                    // lst_message.Items.Add(exception.Message);
                }
            }
        }

        private void bt_atras_Click(object sender, RoutedEventArgs e)
        {
            // SendCommand(1, 0, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            SendCommand(1, 0, Convert.ToInt32(Steps_slider.Value), Convert.ToInt32(SpeedStep_slider.Value));
        }

        private void bt_adelante_Click(object sender, RoutedEventArgs e)
        {
            //  SendCommand(1, 1, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            SendCommand(1, 1, Convert.ToInt32(Steps_slider.Value), Convert.ToInt32(SpeedStep_slider.Value));
        }


        private void GoFar1(object sender, RoutedEventArgs e)
        {
            // SendCommand(1, 0, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            SendCommand(1, 0, Convert.ToInt32(Steps_slider.Value), Convert.ToInt32(SpeedStep_slider.Value));
        }

        private void GoNear1(object sender, RoutedEventArgs e)
        {
            //  SendCommand(1, 1, Convert.ToInt32(txt_pasos.Text), Convert.ToInt32(txt_vel.Text));
            SendCommand(1, 1, Convert.ToInt32(Steps_slider.Value), Convert.ToInt32(SpeedStep_slider.Value));
        }



        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // Close Serial Port when Plugin is closed
            try
            {
                ClosePort();
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