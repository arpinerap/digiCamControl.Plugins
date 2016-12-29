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
        //public int LastPosition;
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
        
       
        private void Position_sld_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetLastPosition();
        }

        private void Position_sld_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoveToNewPosition();
        }
       
        #endregion

        #region Movements

        private void GetLastPosition()
        {
            //LastPosition = (int)Position_sld.Value;
            StepperManager.Instance.LastPosition = StepperManager.Instance.Position;
            //LastPosition_txt.Text = LastPosition.ToString();
        }
        private void MoveToNewPosition()
        {
            int ScrollSteps = 0;
            if (StepperManager.Instance.Position < StepperManager.Instance.LastPosition)
            {
                ScrollSteps = StepperManager.Instance.LastPosition - StepperManager.Instance.Position;
                ArduinoPorts.Instance.SendCommand(1, ScrollSteps * -1);
                ScrollSteps = 0;
            }
            if (StepperManager.Instance.Position > StepperManager.Instance.LastPosition)
            {
                ScrollSteps = StepperManager.Instance.Position - StepperManager.Instance.LastPosition;
                ArduinoPorts.Instance.SendCommand(1, ScrollSteps);
                ScrollSteps = 0;
            }
        }

        private void MoveToNearFocus()
        {
            try
            {
                GetLastPosition();
                StepperManager.Instance.Position = StepperManager.Instance.NearFocus;
                MoveToNewPosition();
            }
            catch (Exception ex)
            {
                Log.Error("Error execute MoveToNearFocus", ex);
            }
        }
        private void MoveToNearFocus2()
        {
            try
            {
                GetLastPosition();
                StepperManager.Instance.Position = StepperManager.Instance.NearFocus2;
                MoveToNewPosition();
            }
            catch (Exception ex)
            {
                Log.Error("Error execute MoveToNearFocus2", ex);
            }
        }

        private void MoveToFarFocus()
        {
            try
            {
                GetLastPosition();
                StepperManager.Instance.Position = StepperManager.Instance.FarFocus;
                MoveToNewPosition();
            }
            catch (Exception ex)
            {
                Log.Error("Error execute MoveToFarFocus", ex);
            }
        }
        private void MoveToFarFocus2()
        {
            try
            {
                GetLastPosition();
                StepperManager.Instance.Position = StepperManager.Instance.FarFocus2;
                MoveToNewPosition();
            }
            catch (Exception ex)
            {
                Log.Error("Error execute MoveToFarFocus2", ex);
            }
        }

        private void MoveTo(int position)
        {
            try
            {
                GetLastPosition();
                StepperManager.Instance.Position = position;
                MoveToNewPosition();
            }
            catch (Exception ex)
            {
                Log.Error("Error execute position", ex);
            }
        }

        #endregion

        #region Stacking Method

        CancellationTokenSource m_cancelTokenSource = null;

        private void Shot()
        {
            // Stabilization
            Thread.Sleep(StepperManager.Instance.StabilizationDelay);
            if (StepperManager.Instance.StabilizationDelay == 0) { Thread.Sleep(5000); }
            //======================
            // LED Flash ON
            if (StepperManager.Instance.IsLightON) { ArduinoPorts.Instance.SendCommand(15, 0, 0); }
            ArduinoPorts.Instance.SendCommand(15, 1, StepperManager.Instance.LightValue * 10);
            //======================
            // Shot
            ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
            //======================
            // LED Flash OFF
            ArduinoPorts.Instance.SendCommand(15, 0, 0);
            //======================
            // Wait for file transfer to be finished
            ServiceProvider.DeviceManager.SelectedCameraDevice.WaitForCamera(5000);
            Thread.Sleep(1000);
            //======================
        }

        private void Shot_MoveFar()
        {
            try
            {
                // Take Photo
                Shot();
                // Focus moving logic
                ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.ShotStepFull);
                //======================
                // Position Update
                StepperManager.Instance.Position += StepperManager.Instance.ShotStepFull;
                //======================
            }
            catch (Exception ex)
            {
                Log.Error("Error execute Shot_MoveFar", ex);
            }
        }

        private void Shot_MoveNear()
        {
            try
            {
                // Take Photo
                Shot();
                //======================
                // Focus moving logic
                ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.ShotStepFull * -1);
                //======================
                // Position Update
                StepperManager.Instance.Position -= StepperManager.Instance.ShotStepFull;
                //======================
            }
            catch (Exception ex)
            {
                Log.Error("Error execute Shot_MoveFar", ex);
            }
        }

        private async void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            await Start_btn_Click();
        }

        private async void Start2_btn_Click(object sender, RoutedEventArgs e)
        {
            int rounds = 360 / StepperManager.Instance.Degrees;

            for (int i = 0; i < rounds; i++)
            {
                int TaskNumber = i + 1;
                await Start_btn_Click();
                StepperManager.Instance.IsStacking = true;
                Progress_lbl.Content = "Finishing Task " + TaskNumber;
                Task wait = Task.Delay(2000);
                await wait;
                ArduinoPorts.Instance.SendCommand(6, 40, StepperManager.Instance.Speed3d);
                StepperManager.Instance.IsStacking = true;
                Progress_lbl.Content = "Rotating...";
                await wait;
                StepperManager.Instance.IsStacking = false;
                /*
                Thread Stack = new Thread(Start_btn_Click);
                Stack.IsBackground = true;
                Stack.Start();
                Stack.Join();
                //Task.Delay(2000);
                ArduinoPorts.Instance.SendCommand(6, 40, StepperManager.Instance.Speed3d);
                //Task.Delay(500);
                */
            }                
        }


        private async Task Start_btn_Click()
        {               
            // Update display to indicate we are running
            StepperManager.Instance.IsStacking = true;
            Progress_lbl.Content = "Starting...";

            List<string> list = new List<string>();
                
            for (int i = 0; i < StepperManager.Instance.ShotsNumberFull; i++)
                list.Add(i.ToString());
                

            // ---------------Create progress and cancel objects
            m_cancelTokenSource = new CancellationTokenSource();
            var progress = new Progress<ProgressReport>();

            progress.ProgressChanged += (o, report) =>
            {
                Progress_lbl.Content = string.Format("Shooting {0}%", report.PercentComplete);
                ProgressBarStack.Value = report.PercentComplete;
                //ProgressBarStack.Update();
            };
                              
            try
            {
                // ------------Launch the process. After launching, will "return" from this method.
                await ProcessData(list, progress, m_cancelTokenSource.Token);
                //await ProcessData(list, progress);
                
                    
            }

            catch (OperationCanceledException)
            {
                // ----------- If the operation was cancelled, the exception will be thrown as though it came from the await line
                
                Progress_lbl.Content = "Canceled";
            }

            catch (Exception)
            {
                Progress_lbl.Content = "Failed";
            } 

            finally
            {
                // -----------------Reset the UI
                Progress_lbl.Content = "Done";
                Thread.Sleep(1000);
                StepperManager.Instance.IsStacking = false;
                m_cancelTokenSource = null;
            }            
        }


        private void CancelStack_btn_Click(object sender, RoutedEventArgs e)
        {
            if (m_cancelTokenSource != null)
            {
                m_cancelTokenSource.Cancel();
                Progress_lbl.Content = "Canceled";
                Thread.Sleep(5000);

            }
        }

        private Task ProcessData(List<string> list, IProgress<ProgressReport> progress, CancellationToken m_cancelTokenSource)
        //private Task ProcessData(List<string> list, IProgress<ProgressReport> progress)
        {
            int index = 0;
            int totalProcess = list.Count;
            var progressReport = new ProgressReport();

            return Task.Run(() =>
            {               
                if (StepperManager.Instance.GoNearToFar == true)
                {
                    //Progress_lbl.Content = "Moving to Near Focus...";
                    MoveToNearFocus2();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);

                    for (int i = 0; i < totalProcess; i++)
                    {
                        m_cancelTokenSource.ThrowIfCancellationRequested();
                        progressReport.PercentComplete = index++ * 100 / totalProcess;
                        progress.Report(progressReport);
                        Shot_MoveFar();                      
                    }
                    //Progress_lbl.Content = "Moving to Near Focus...";
                    MoveToNearFocus();
                    //StepperManager.Instance.IsNotStacking = true;
                }

                if (StepperManager.Instance.GoFarToNear == true)
                {
                    //Progress_lbl.Content = "Moving to Far Focus...";
                    MoveToFarFocus2();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);

                    for (int i = 0; i < totalProcess; i++)
                    {
                        m_cancelTokenSource.ThrowIfCancellationRequested();
                        progressReport.PercentComplete = index++ * 100 / totalProcess;
                        progress.Report(progressReport);
                        Shot_MoveNear();
                    }
                    //Progress_lbl.Content = "Moving to Far Focus...";
                    MoveToFarFocus();
                    //StepperManager.Instance.IsNotStacking = true;
                }

            }, m_cancelTokenSource);
            //});
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
                //if (ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed != null)
                //{ ServiceProvider.WindowsManager.ExecuteCommand(CmdConsts.CaptureNoAf); }
                LiveViewViewModel.Instance.CaptureInThread();
            }
            catch (Exception exception)
            {
                StaticHelper.Instance.SystemMessage = exception.Message;
                Log.Error("Take test photo", exception);
            }           
        }
        
    }
}