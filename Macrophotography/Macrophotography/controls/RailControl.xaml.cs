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
//using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Macrophotography.controls;
using Macrophotography;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Devices;
using Newtonsoft.Json;


namespace Macrophotography.controls
{   
    /// <summary>
    /// Interaction logic for RailControl.xaml
    /// </summary>
    public partial class RailControl : UserControl
    {
        public int LastPosition;
        public CustomConfig SelectedConfig { get; set; }

        public RailControl()
        {
            InitializeComponent();
            StepperManager.Instance.Position = 0;
            StepperManager.Instance.NearFocus = 0;
            StepperManager.Instance.FarFocus = 0;
            StepperManager.Instance.NearFocus2 = 0;
            StepperManager.Instance.FarFocus2 = 0;
            StepperManager.Instance.TotalDOF = 0;
            StepperManager.Instance.TotalDOFFull = 0;
        }


        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // Close Serial Port when Plugin is closed
            try
            {
                ArduinoPorts.Instance.ClosePort();
                ServiceProvider.DeviceManager.SelectedCameraDevice.StopLiveView();
                ServiceProvider.DeviceManager.SelectedCameraDevice.HostMode = false;
            }
            catch (Exception)
            {
            }
        }


        #region Slider Manager
        private void NearFocus_btn_Click(object sender, RoutedEventArgs e)
        {
            StepperManager.Instance.SetNearFocus();
            NearPulsing.IsPulsing = false;
            NearFocusLock.IsChecked = true;
        }

        private void FarFocus_btn_Click(object sender, RoutedEventArgs e)
        {
            StepperManager.Instance.SetFarFocus();
            FarPulsing.IsPulsing = false;
            FarFocusLock.IsChecked = true;
        }
        
        private void GetLastPosition()
        {
            //LastPosition = (int)Position_sld.Value;
            LastPosition = StepperManager.Instance.Position;
            LastPosition_txt.Text = LastPosition.ToString();            
        }
        private void MoveToNewPosition()
        {
            int ScrollSteps = 0;
            if (StepperManager.Instance.Position < LastPosition)
            {
                ScrollSteps = LastPosition - StepperManager.Instance.Position;
                ArduinoPorts.Instance.SendCommand(1, ScrollSteps * -1);
                ScrollSteps = 0;
            }
            if (StepperManager.Instance.Position > LastPosition)
            {
                ScrollSteps = StepperManager.Instance.Position - LastPosition;
                ArduinoPorts.Instance.SendCommand(1, ScrollSteps);
                ScrollSteps = 0;
            }
        }


        private void Position_sld_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetLastPosition();
        }

        private void Position_sld_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoveToNewPosition();
        }

        #endregion


        #region Stacking Method

        private async void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                GetLastPosition();
                //Task.Factory.StartNew(StackTask);
                List<string> list = new List<string>();
                for (int i = 0; i < StepperManager.Instance.ShotsNumberFull; i++)
                    list.Add(i.ToString());
                Progress_lbl.Content = "Shooting ...";
                var progress = new Progress<ProgressReport>();
                progress.ProgressChanged += (o, report) =>
                {
                    Progress_lbl.Content = string.Format("Shooting {0}%", report.PercentComplete);
                    ProgressBarStack.Value = report.PercentComplete;
                    //ProgressBarStack.Update();
                };
                await ProcessData(list, progress);
                Progress_lbl.Content = "Shooting Done";
                Thread.Sleep(2000);

            }

            catch (Exception ex)
            {
                Log.Error("Error execute stacking", ex);
            }
            

     
        }

        private Task ProcessData(List<string> list, IProgress<ProgressReport> progress)
        {
            int index = 0;
            int totalProcess = list.Count;
            var progressReport = new ProgressReport();
            return Task.Run(() =>
            {
                StepperManager.Instance.IsStacking = true;
                if (StepperManager.Instance.GoNearToFar == true)
                {
                    StepperManager.Instance.Position = StepperManager.Instance.NearFocus2;
                    MoveToNewPosition();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);

                    for (int i = 0; i < totalProcess; i++)
                    {
                        progressReport.PercentComplete = index++ * 100 / totalProcess;
                        progress.Report(progressReport);
                        Thread.Sleep(StepperManager.Instance.StabilizationDelay);
                        Thread.Sleep(2000);
                        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                        Thread.Sleep(5000);
                        // wait for file transfer to be finished
                        //ServiceProvider.DeviceManager.SelectedCameraDevice.WaitForCamera(5000);
                        //======================
                        // here come the focus moving logic
                        ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.ShotStepFull);
                        StepperManager.Instance.Position += StepperManager.Instance.ShotStepFull;
                        //======================

                    }
                    GetLastPosition();
                    StepperManager.Instance.Position = StepperManager.Instance.NearFocus;
                    MoveToNewPosition();
                    //StepperManager.Instance.IsNotStacking = true;
                }

                if (StepperManager.Instance.GoFarToNear == true)
                {
                    StepperManager.Instance.Position = StepperManager.Instance.FarFocus2;
                    MoveToNewPosition();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);

                    for (int i = 0; i < totalProcess; i++)
                    {
                        Thread.Sleep(StepperManager.Instance.StabilizationDelay);
                        Thread.Sleep(2000);
                        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                        Thread.Sleep(5000);
                        // wait for file transfer to be finished
                        //ServiceProvider.DeviceManager.SelectedCameraDevice.WaitForCamera(5000);
                        //======================
                        // here come the focus moving logic
                        ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.ShotStepFull * -1);
                        StepperManager.Instance.Position -= StepperManager.Instance.ShotStepFull;
                        //======================

                    }
                    GetLastPosition();
                    StepperManager.Instance.Position = StepperManager.Instance.FarFocus;
                    MoveToNewPosition();
                    //StepperManager.Instance.IsNotStacking = true;
                }
                StepperManager.Instance.IsStacking = false;
            });
        }


        private void StackTask()
        {
            try
            {
                //StepperManager.Instance.IsStacking = true;
                if (StepperManager.Instance.GoNearToFar == true)
                {
                    StepperManager.Instance.Position = StepperManager.Instance.NearFocus2;
                    MoveToNewPosition();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);

                    for (int i = 0; i < StepperManager.Instance.ShotsNumberFull; i++)
                    {
                        Thread.Sleep(StepperManager.Instance.StabilizationDelay);
                        Thread.Sleep(2000);
                        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                        Thread.Sleep(5000);
                        // wait for file transfer to be finished
                        //ServiceProvider.DeviceManager.SelectedCameraDevice.WaitForCamera(5000);
                        //======================
                        // here come the focus moving logic
                        ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.ShotStepFull);
                        StepperManager.Instance.Position += StepperManager.Instance.ShotStepFull;
                        //======================
                        
                    }
                    GetLastPosition();
                    StepperManager.Instance.Position = StepperManager.Instance.NearFocus;
                    MoveToNewPosition();
                    //StepperManager.Instance.IsNotStacking = true;
                }

                if (StepperManager.Instance.GoFarToNear == true)
                {
                    StepperManager.Instance.Position = StepperManager.Instance.FarFocus2;
                    MoveToNewPosition();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);

                    for (int i = 0; i < StepperManager.Instance.ShotsNumberFull; i++)
                    {
                        Thread.Sleep(StepperManager.Instance.StabilizationDelay);
                        Thread.Sleep(2000);
                        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                        Thread.Sleep(5000);
                        // wait for file transfer to be finished
                        //ServiceProvider.DeviceManager.SelectedCameraDevice.WaitForCamera(5000);
                        //======================
                        // here come the focus moving logic
                        ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.ShotStepFull * -1);
                        StepperManager.Instance.Position -= StepperManager.Instance.ShotStepFull;
                        //======================
                        
                    }
                    GetLastPosition();
                    StepperManager.Instance.Position = StepperManager.Instance.FarFocus;
                    MoveToNewPosition();
                    //StepperManager.Instance.IsNotStacking = true;
                }
                             
            }
            catch (Exception ex)
            {
                Log.Error("Error execute StackTask", ex);
            }
            //StepperManager.Instance.IsBusy = false;
        }


        /*private void CombineZP()
        {
            string newFile = Path.Combine(Path.GetDirectoryName(Files[0].FileName), Path.GetFileNameWithoutExtension(Files[0].FileName) + "_enfuse" + ".jpg");
            newFile = PhotoUtils.GetNextFileName(newFile);
            dynamic data = new System.Dynamic.ExpandoObject();
            data.Files = Files.Select(x => x.FileName).ToList();
            data.ResultFile = newFile;
            data.ThumbOnly = true;
            var s = JsonConvert.SerializeObject(data);
            ServiceProvider.PluginManager.GetExecutePlugin("{F3155291-D688-49B8-B22D-E74A2D5E020E}").Execute(s);
            //.... do something with data.ResultFile ..................
        }
        */

        #endregion


        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (StepperManager.Instance.ShotStep != 0) 
            {
                StepperManager.Instance.UpdateShotStep();
                StepperManager.Instance.UpDateTotalDOFFull();              
            }           
        }

        private void UpDataTotalDOF(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            StepperManager.Instance.UpDateTotalDOF();
        }

        private void Capture_btn_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceProvider.DeviceManager.SelectedCameraDevice == null)
                return;
            Log.Debug("Macrophotography test capture started");
            try
            {
                if (ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed != null)
                { ServiceProvider.WindowsManager.ExecuteCommand(CmdConsts.CaptureNoAf); }
            }
            catch (Exception exception)
            {
                StaticHelper.Instance.SystemMessage = exception.Message;
                Log.Error("Take test photo", exception);
            }
            
        }




    }
}