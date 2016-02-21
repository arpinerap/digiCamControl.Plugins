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
using Macrophotography.controls;
using Macrophotography;
using CameraControl.Core;
using CameraControl.Core.Classes;


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
            LastPosition = (int)Position_sld.Value;
            LastPosition_txt.Text = LastPosition.ToString();
        }
        private void MoveToNewPosition()
        {
            int ScrollSteps;
            if (Convert.ToInt32(Position_sld.Value) < LastPosition)
            {
                ScrollSteps = LastPosition - Convert.ToInt32(Position_sld.Value);
                ArduinoPorts.Instance.SendCommand(1, ScrollSteps * -1);
                ScrollSteps = 0;
            }
            if (Convert.ToInt32(Position_sld.Value) > LastPosition)
            {
                ScrollSteps = Convert.ToInt32(Position_sld.Value) - LastPosition;
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

            for (int i = 0 ; i < StepperManager.Instance.ShotsNumber ; i++) 
            {
                ServiceProvider.ExternalDeviceManager.Capture(SelectedConfig);
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