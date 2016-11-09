using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Macrophotography.controls;

namespace Macrophotography
{
    public class ArduinoPorts : ViewModelBase
    {

        #region RaisePropertyChanged Variables

        private static ArduinoPorts _instance;
        private SerialPort sp = new SerialPort();
        private object _locker = new object();
        private string _port;
        string[] _portslist;
        bool _IsArduinoDetected;

        private int SPbaudrate = 38400;
        //private int SPbaudrate = 9600;

        private string cmd = "0";

        #endregion


        #region RaisePropertyChanged Methods

        public static ArduinoPorts Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ArduinoPorts();
                return _instance;
            }
            set { _instance = value; }
        }

        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged(() => Port);
            }
        }

        public string[] portslist
        {
            get { return _portslist; }
            set
            {
                _portslist = value;
                RaisePropertyChanged(() => portslist);
            }
        }

        public bool IsArduinoDetected
        {
            get { return _IsArduinoDetected; }
            set
            {
                _IsArduinoDetected = value;
                RaisePropertyChanged(() => IsArduinoDetected);
                RaisePropertyChanged(() => IsArduinoNotDetected);
            }
        }

        public bool IsArduinoNotDetected
        {
            get { return !IsArduinoDetected; }
        }

        #endregion


        # region Serial Port Manegment

        public void OpenPort(string port)
        {
            _port = port;
            // Open Serial Port
            if (!sp.IsOpen)
            {
                sp.PortName = port;
                //sp.BaudRate = 9600;
                sp.BaudRate = SPbaudrate;
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

        public void FillPorts()
        {
            portslist = SerialPort.GetPortNames();           
        }


        public void DetectArduino()
        {
            if (sp.IsOpen)
            {
                sp.DataReceived -= sp_DataReceived;
                ClosePort();
            }
            else
            {
                FillPorts();
                foreach (string port in portslist)
                {
                    //sp = new SerialPort(port, 9600);
                    sp = new SerialPort(port, SPbaudrate);
                    int intReturnASCII = 0;
                    char charReturnValue = (Char)intReturnASCII;
                    ClosePort();
                    sp.Open();
                    sp.WriteLine("1535 ");
                    Thread.Sleep(500);
                    int count = sp.BytesToRead;
                    string returnMessage = "";
                    while (count > 0)
                    {
                        intReturnASCII = sp.ReadByte();
                        returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                        count--;
                    }
                    Task.Delay(1000);
                    sp.Close();
                    if (returnMessage.Contains("YES"))
                    {
                        Port = sp.PortName;
                        OpenPort(Port);
                        IsArduinoDetected = true;
                        //Con.Content = "Desconectar";
                    }
                }
            }
        }


        # endregion


        #region Serial Port Commands

        public void SendCommand(int motor, int steps)
        {
            SendCommand(motor, steps, StepperManager.Instance.Speed);
        }

        public void SendCommand(int motor, int steps, int spd)
        {
            // Line Order for Arduino
            lock (_locker)
            {
                try
                {
                    ClosePort();
                    OpenPort(_port);
                    int checksum = motor + steps + spd;
                    cmd = cmd.Remove(0);
                    cmd = Convert.ToString(motor);
                    cmd += " ";
                    cmd += Convert.ToString(steps);
                    cmd += " ";
                    cmd += Convert.ToString(spd);
                    cmd += " ";
                    cmd += Convert.ToString(checksum);
                    sp.WriteLine(cmd);
                    StepperManager.Instance.IsBusy = true;
                }
                catch (Exception exception)
                {
                    // lst_message.Items.Add(exception.Message);
                }
            }
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Unlock Motion Control Buttons when confimation mesaage arrives fron Arduino
            try
            {
                SerialPort spL = (SerialPort)sender;
                string str = spL.ReadLine();
                //lst_message.Items.Add(str);
                if (str.Contains("ok"))
                {
                    StepperManager.Instance.IsBusy = false;
                }

                if (str.Contains("E1"))
                {
                    sp.WriteLine(cmd);
                }

            }
            catch (Exception ex)
            {

            }
        }

        #endregion



    }
}
