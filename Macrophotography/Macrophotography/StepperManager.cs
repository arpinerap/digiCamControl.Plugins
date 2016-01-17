using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macrophotography
{
    public class StepperManager
    {
        private SerialPort sp = new SerialPort();
        private object _locker = new object();
        private static StepperManager _instance;
        private string _port = "";
        public static StepperManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StepperManager();
                return _instance;
            }
            set { _instance = value; }
        }


        public void OpenPort(string port)
        {
            _port = port;
            // Open Serial Port
            if (!sp.IsOpen)
            {
                sp.PortName = port;
                sp.BaudRate = 9600;
                sp.Parity = Parity.None;
                sp.StopBits = StopBits.One;
                sp.DataBits = 8;
                sp.WriteTimeout = 3500;
                sp.Open();
                sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            }
        }

        public void ClosePort()
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
                //if (str.Contains("ok"))
                //{
                //    Dispatcher.Invoke(new Action(() =>
                //    {
                //        bt_adelante.IsEnabled = true;
                //        bt_atras.IsEnabled = true;
                //    }));
                //}
            }
            catch (Exception ex)
            {

            }


        }

        public void SendCommand(int motor, int dir, int steps, int spd)
        {
            // Line Order for Arduino
            lock (_locker)
            {
                try
                {
                    ClosePort();
                    OpenPort(_port);
                    string cmd = Convert.ToString(motor);
                    cmd += " ";
                    cmd += Convert.ToString(dir);
                    cmd += " ";
                    cmd += Convert.ToString(steps);
                    cmd += " ";
                    cmd += Convert.ToString(spd);
                    sp.WriteLine(cmd);
                    //bt_adelante.IsEnabled = false;
                    //bt_atras.IsEnabled = false;
                }
                catch (Exception exception)
                {
                    // lst_message.Items.Add(exception.Message);
                }
            }
        }

    }
}
