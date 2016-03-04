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
using System.Threading;
using Macrophotography.controls;
using Macrophotography;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Devices;


namespace Macrophotography.controls
{   
    /// <summary>
    /// Interaction logic for RailControl.xaml
    /// </summary>
    public partial class RailControl : UserControl
    {
        /*
        public int Position = 0;
        public int NearFocus;
        public int FarFocus;

        public int TotalDOF = 0;

        public int dir;
        public int steps;
        public int spd;
        */
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
            }
            catch (Exception)
            {
            }
        }

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

        private void PulseButton_Click(object sender, RoutedEventArgs e)
        {
            StepperManager.Instance.IsBusy = true;
            GetLastPosition();           
            Task.Factory.StartNew(StackTask);            
        }

        private void StackTask()
        {
            try
            {
                // if (Direction_swch.IsChecked == true)
                if (StepperManager.Instance.GoNearToFar == true)
                {
                    //GetLastPosition();
                    //StepperManager.Instance.Position = Convert.ToInt32(Position_sld.Minimum);
                    StepperManager.Instance.Position = StepperManager.Instance.NearFocus2;
                    MoveToNewPosition();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);
                    //ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.PlusNearShots * StepperManager.Instance.ShotStepFull * -1);

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
                }

                // if (Direction_swch.IsChecked == false)
                if (StepperManager.Instance.GoFarToNear == true)
                {
                    //GetLastPosition();
                    //StepperManager.Instance.Position = Convert.ToInt32(Position_sld.Maximum);
                    StepperManager.Instance.Position = StepperManager.Instance.FarFocus2;
                    MoveToNewPosition();
                    Thread.Sleep(2000);
                    Thread.Sleep(StepperManager.Instance.InitStackDelay);
                    //ArduinoPorts.Instance.SendCommand(1, StepperManager.Instance.PlusFarShots * StepperManager.Instance.ShotStepFull);

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
                }                              
            }
            catch (Exception ex)
            {
                Log.Error("Error execute stacking", ex);
            }
            StepperManager.Instance.IsBusy = false;
        }

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



    }
}