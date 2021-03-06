﻿using System;
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
using GalaSoft.MvvmLight.Command;
using Macrophotography.controls;

namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for Stepper.xaml
    /// </summary>
    public partial class Stepper : UserControl
    {
        //public int Step;
        
        public Stepper()
        {
            StepCommand = new RelayCommand<string>(StepCom);
            InitializeComponent();
            /// DataContext = new RailControl();
            
        }

        private void StepCom(string shots)
        {
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(shots);
            //StepperManager.Instance.SendCommand(1,step);          
            ArduinoPorts.Instance.SendCommand(1, step);
            StepperManager.Instance.Step = step;
            StepperManager.Instance.UpDatePosition();
        }

        public RelayCommand<string> StepCommand { get; set; }


      

    }
}
