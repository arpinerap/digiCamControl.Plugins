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
using System.Management;

namespace Macrophotography
{
    public class ArduinoPorts : ViewModelBase
    {

        #region Variables

        private static ArduinoPorts _instance;
        private SerialPort sp = new SerialPort();
        private object _locker = new object();
        private string _port;
        string[] _portslist;
        bool _IsArduinoDetected;

        private int SPbaudrate = 38400;
        //private int SPbaudrate = 9600;

        private string cmd = "0";

        ManagementEventWatcher watcher;
        private TaskScheduler _taskScheduler;

        #endregion

        #region Arduino AutoDetect

        public void ArduinoInit()
        {
            IsArduinoDetected = false;

            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            //watcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3");
            //watcher.EventArrived += (sender, eventArgs) => USBChangedEvent(eventArgs);
            //watcher.Start();
        }


        private void USBChangedEvent(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(USBChangedEventAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }
        

        public void USBChangedEventAsync()
        {
            if (ArduinoPorts.Instance.IsArduinoNotDetected)
            {

                //Task.Delay(3000);
                ArduinoPorts.Instance.SearchArduino();

            }
            else if (ArduinoPorts.Instance.Port == null)
            {
                //Task.Delay(3000);
                ArduinoPorts.Instance.SearchArduino();
            }
            else
            {
                //Task.Delay(3000);
                ArduinoPorts.Instance.CheckArduino();
            }
        }

        public void SearchArduino()
        {
            Thread SearchArduino = new Thread(DetectArduino);
            SearchArduino.Start();
            SearchArduino.Join();
            ArduinoPorts.Instance.SendCommand(11, 1, 0);
        }


        public void DetectArduino()
        {
            if (sp.IsOpen)
            {
                //sp.DataReceived -= sp_DataReceived;
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
                    Thread.Sleep(1000);
                    int count = sp.BytesToRead;
                    string returnMessage = "";
                    while (count > 0)
                    {
                        intReturnASCII = sp.ReadByte();
                        returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                        count--;
                    }
                    sp.Close();
                    if (returnMessage.Contains("YES"))
                    {
                        Port = sp.PortName;
                        OpenPort(Port);
                        IsArduinoDetected = true;
                        //Con.Content = "Desconectar";
                    }
                }
                if (Port == null)
                {
                    ClosePort();
                    ClearPorts();
                    IsArduinoDetected = false;
                }
            }
        }

        public void CheckArduino()
        {
            if (sp.IsOpen)
            {
                sp.WriteLine("1535 ");
                Thread.Sleep(1000);
                string returnMessage = "";
                int intReturnASCII = 0;
                intReturnASCII = sp.ReadByte();
                returnMessage = returnMessage + Convert.ToChar(intReturnASCII);

                if (returnMessage.Contains("YES"))
                {
                    IsArduinoDetected = true;
                }
                else
                {
                    IsArduinoDetected = false;
                    ClosePort();
                    ClearPorts();

                }

            }
            else
            {
                IsArduinoDetected = false;
                ClearPorts();
            }
        }

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

        public void ClearPorts()
        {
            portslist = null;
            Port = null;
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
                    cmd += Convert.ToString(0);
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

        public void SendCommandFlash(int motor, int time, int power1, int power2)
        {
            // Line Order for Arduino
            lock (_locker)
            {
                try
                {
                    ClosePort();
                    OpenPort(_port);
                    int checksum = motor + time + power1 + power2;
                    cmd = cmd.Remove(0);
                    cmd = Convert.ToString(motor);
                    cmd += " ";
                    cmd += Convert.ToString(time);
                    cmd += " ";
                    cmd += Convert.ToString(power1);
                    cmd += " ";
                    cmd += Convert.ToString(power2);
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
            // Unlock Motion Control Buttons when confimation mesage arrives fron Arduino
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
