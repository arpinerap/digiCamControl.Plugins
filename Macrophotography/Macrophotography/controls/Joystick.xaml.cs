using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;


namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for Joystick.xaml
    /// </summary>
    public partial class Joystick : UserControl
    {

      private BackgroundWorker bwMove = new BackgroundWorker();

        CancellationTokenSource m_cancelTokenSource = null;

        CancellationTokenSource cts;

        Stopwatch stopwatch = new Stopwatch();

        bool DinamicButtonPressed = false;

   
        
        public Joystick()
        {
            InitializeComponent();
            bwMove.WorkerSupportsCancellation = true;
            bwMove.DoWork += new DoWorkEventHandler(bwMove_DoWork);
        }
        public int DCsteps;
        public System.Timers.Timer loopTimer;

        #region Movement Task
        //private Task ProcessData(List<string> list, IProgress<ProgressReport> progress, CancellationToken m_cancelTokenSource)
        private Task MovingTo(int motor, int direction, CancellationToken ct)
        {
            DCsteps = (int)DCsteps_sld.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps);

            return Task.Run(() =>
            {
                stopwatch.Start(); 
                ArduinoPorts.Instance.SendCommand(motor, DCsteps * direction, StepperManager.Instance.Speed3d);
                Task.Delay(300);

                
                //while (stopwatch.ElapsedMilliseconds > 300)
                while (DinamicButtonPressed)
                {
                    if (stopwatch.ElapsedMilliseconds < 5000)
                    {
                        //ArduinoPorts.Instance.SendCommand(motor, step * -20);
                        ArduinoPorts.Instance.SendCommand(motor, DCsteps * 5 * direction, StepperManager.Instance.Speed3d);
                        Task.Delay(500);
                    }

                    if (stopwatch.ElapsedMilliseconds > 5000)
                    {
                        //ArduinoPorts.Instance.SendCommand(motor, step * -100);
                        ArduinoPorts.Instance.SendCommand(motor, DCsteps * 10 * direction, StepperManager.Instance.Speed3d);
                        Task.Delay(500);
                    }
                }
            });
            //});
        }


        #endregion

        


        #region BackGroundWorkers

        private void bwMove_DoWork(object sender, DoWorkEventArgs e) 
        {
            //BackgroundWorker worker = sender as BackgroundWorker;
            int motor = (int)e.Argument;
            
            //Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DCsteps = (int)DCsteps_sld.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps);

            //ArduinoPorts.Instance.SendCommand(motor, step * -20);
            ArduinoPorts.Instance.SendCommand(motor, DCsteps * 1, StepperManager.Instance.Speed3d);
            Thread.Sleep(300);

            //while (Right_button.IsPressed == true && StepperManager.Instance.stopwatch.ElapsedMilliseconds > 300)
            while (DinamicButtonPressed)
            {
                if (stopwatch.ElapsedMilliseconds < 2000)
                {
                    //ArduinoPorts.Instance.SendCommand(motor, step * -20);
                    ArduinoPorts.Instance.SendCommand(5, DCsteps * 5, StepperManager.Instance.Speed3d);
                    Thread.Sleep(50);
                }

                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    //ArduinoPorts.Instance.SendCommand(motor, step * -100);
                    ArduinoPorts.Instance.SendCommand(5, DCsteps * 10, StepperManager.Instance.Speed3d);
                    Thread.Sleep(50);
                }
            }

            stopwatch.Stop();
            stopwatch.Reset();
        }




        #endregion

        #region Buttons Actions

        private void rot1_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps, StepperManager.Instance.Speed3d);
        }

        private void rot2_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(6, 4 * DCsteps * -1, StepperManager.Instance.Speed3d);
        }

        private void Up_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * -10, StepperManager.Instance.Speed);
        }

        private void Down_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(3, DCsteps * 10, StepperManager.Instance.Speed);            
        }

        private void Right_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps); 


            ArduinoPorts.Instance.SendCommand(2, step * -20);
            //ArduinoPorts.Instance.SendCommand(2, step * -20, StepperManager.Instance.Speed);
            
        }
        

        private void Left_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(DCsteps); 

            ArduinoPorts.Instance.SendCommand(2, step * 20);
            //ArduinoPorts.Instance.SendCommand(2, step * 20, StepperManager.Instance.Speed);

            /*
            int shotStepfull = StepperManager.Instance.ShotStepFull;
            int step = shotStepfull * Convert.ToInt32(shots);
            //StepperManager.Instance.SendCommand(1,step);          
            ArduinoPorts.Instance.SendCommand(1, step);
            StepperManager.Instance.Step = step;
            StepperManager.Instance.UpDatePosition();
            */
        }

        private void FlipUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * 1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void FlipDown_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * -1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void FlipLeft_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * -1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);
        }

        private void FlipRight_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(5, DCsteps * 1, StepperManager.Instance.Speed3d);
            //Task.Delay(100);

        }


        private void OpenJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOn_btn.IsOpen = false;
            rot1_button.Visibility = System.Windows.Visibility.Visible;
            rot2_button.Visibility = System.Windows.Visibility.Visible;
            DCsteps_sld.Visibility = System.Windows.Visibility.Visible;
            //FlipDown_btn.Visibility = System.Windows.Visibility.Visible;
            //FlipUp_btn.Visibility = System.Windows.Visibility.Visible;
            //FlipRight_btn.Visibility = System.Windows.Visibility.Visible;
            //FlipLeft_btn.Visibility = System.Windows.Visibility.Visible;
            LiveViewOff_btn.IsOpen = true;
            LiveViewOff2_btn.IsOpen = true;
        }

        private void CloseJoystick(object sender, System.Windows.RoutedEventArgs e)
        {
            LiveViewOff_btn.IsOpen = false;
            LiveViewOff2_btn.IsOpen = false;
            rot1_button.Visibility = System.Windows.Visibility.Hidden;
            rot2_button.Visibility = System.Windows.Visibility.Hidden;
            DCsteps_sld.Visibility = System.Windows.Visibility.Hidden;
            //FlipDown_btn.Visibility = System.Windows.Visibility.Hidden;
            //FlipUp_btn.Visibility = System.Windows.Visibility.Hidden;
            //FlipRight_btn.Visibility = System.Windows.Visibility.Hidden;
            //FlipLeft_btn.Visibility = System.Windows.Visibility.Hidden;
            LiveViewOn_btn.IsOpen = true;
        }

        
        #endregion

        private void Right_button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (bwMove.IsBusy != true)
            {
                bwMove.RunWorkerAsync(5);
            }
        }

        private void Right_button_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (bwMove.WorkerSupportsCancellation == true)
            {
                bwMove.CancelAsync();
            }


        }

        private void Right_button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //m_cancelTokenSource = new CancellationTokenSource();
            
            // ------------Launch the process. After launching, will "return" from this method.
            //await MovingTo(5, 1);   
            
        }

        private void Right_button_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            

            DinamicButtonPressed = false;
            stopwatch.Stop();
            stopwatch.Reset();
        }

        private void Right_button_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DinamicButtonPressed = true;

            // ***Instantiate the CancellationTokenSource.
            //cts = new CancellationTokenSource();

            // ------------Launch the process. After launching, will "return" from this method.
            //await MovingTo(5, 1,cts.Token);   

             //loop timer
            loopTimer = new System.Timers.Timer();
            loopTimer.Interval = 500;//interval in milliseconds
            loopTimer.Enabled = false;
            loopTimer.Elapsed += loopTimerEvent;
            loopTimer.AutoReset = true;

            loopTimer.Enabled = true;



        }

        private void Right_button_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }

            loopTimer.Enabled = false;
            
            DinamicButtonPressed = false;
            stopwatch.Stop();
            stopwatch.Reset();
        }
        
        private void loopTimerEvent(Object source, ElapsedEventArgs e)
        {
            //do whatever you want to happen while clicking on the button
            stopwatch.Start();
            ArduinoPorts.Instance.SendCommand(5, DCsteps * 1, StepperManager.Instance.Speed3d);
            Task.Delay(300);


            //while (stopwatch.ElapsedMilliseconds > 300)
            while (DinamicButtonPressed)
            {
                if (stopwatch.ElapsedMilliseconds < 5000)
                {
                    //ArduinoPorts.Instance.SendCommand(motor, step * -20);
                    ArduinoPorts.Instance.SendCommand(5, DCsteps * 5 * 1, StepperManager.Instance.Speed3d);
                    Task.Delay(500);
                }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    //ArduinoPorts.Instance.SendCommand(motor, step * -100);
                    ArduinoPorts.Instance.SendCommand(5, DCsteps * 10 * 1, StepperManager.Instance.Speed3d);
                    Task.Delay(500);
                }
            }
        }

        private void FlipUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DCsteps = (int)DCsteps_sld.Value;
            ArduinoPorts.Instance.SendCommand(4, DCsteps * 1, StepperManager.Instance.Speed3d);
            Task.Delay(100);
        }
    }
}
