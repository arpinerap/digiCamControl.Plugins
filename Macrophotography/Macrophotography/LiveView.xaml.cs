using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Translation;
using System.IO.Ports;
using System.Timers;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using Macrophotography;
using MahApps.Metro.Controls.Dialogs;

namespace Macrophotography
{
    /// <summary>
    /// Interaction logic for LiveView.xaml
    /// </summary>
    public partial class LiveView : IWindow
    {
        private Timer _timer = new Timer();

        //private bool _loading = false;

        public LiveView()
        {
            InitializeComponent();
            //if (ServiceProvider.DeviceManager != null)
            //    ServiceProvider.DeviceManager.PropertyChanged += DeviceManager_PropertyChanged;
            //RefreshItems();
            
        }

        private void _image_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left &&
                ((LiveViewViewModel) DataContext).SelectedCameraDevice.LiveViewImageZoomRatio.Value == "All")
            {
                try
                {
                    ((LiveViewViewModel) DataContext).SetFocusPos(e.MouseDevice.GetPosition(_image), _image.ActualWidth,
                        _image.ActualHeight);

                }
                catch (Exception exception)
                {
                    Log.Error("Focus Error", exception);
                    StaticHelper.Instance.SystemMessage = "Focus error: " + exception.Message;
                }
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close Serial Port when Plugin is closed
            try
            {
                if (IsVisible)
                {
                    e.Cancel = true;
                    ServiceProvider.WindowsManager.ExecuteCommand("MacroLiveView_Hide");
                }
                ArduinoPorts.Instance.SendCommand(10, 0, 0);
                System.Threading.Tasks.Task.Delay(300);
                ArduinoPorts.Instance.ClosePort();
                ServiceProvider.DeviceManager.SelectedCameraDevice.StopLiveView();
                ServiceProvider.DeviceManager.SelectedCameraDevice.HostMode = false;
            }
            catch (Exception)
            {
            }
        }



        /*private void cmb_transfer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {       
            if (_loading)
                return;
            if (ServiceProvider.DeviceManager.SelectedCameraDevice.IsBusy)
                return;
            CameraProperty property = ServiceProvider.DeviceManager.SelectedCameraDevice.LoadProperties();

            if ((string) cmb_transfer.SelectedItem == TranslationStrings.LabelTransferItem1 &&
                ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam != true)
                ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam = true;

            if ((string) cmb_transfer.SelectedItem == TranslationStrings.LabelTransferItem2)
            {
                property.NoDownload = true;
                ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam = false;
            }
            if ((string) cmb_transfer.SelectedItem == TranslationStrings.LabelTransferItem3)
            {
                property.NoDownload = false;
                ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam = false;
            }
            property.CaptureInSdRam = ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam;
        }

        private void RefreshItems()
        {
            _loading = true;
            try
            {
                if (ServiceProvider.Settings == null)
                    return;
                if (ServiceProvider.DeviceManager.SelectedCameraDevice == null)
                    return;
                CameraProperty property = ServiceProvider.DeviceManager.SelectedCameraDevice.LoadProperties();

                cmb_transfer.Items.Clear();
                if (ServiceProvider.DeviceManager.SelectedCameraDevice.GetCapability(CapabilityEnum.CaptureInRam))
                {
                    cmb_transfer.Items.Add(TranslationStrings.LabelTransferItem1);
                    cmb_transfer.Items.Add(TranslationStrings.LabelTransferItem2);
                    cmb_transfer.Items.Add(TranslationStrings.LabelTransferItem3);
                    if (ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam)
                        cmb_transfer.SelectedItem = TranslationStrings.LabelTransferItem1;
                    else if (!ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureInSdRam && property.NoDownload)
                        cmb_transfer.SelectedItem = TranslationStrings.LabelTransferItem2;
                    else
                        cmb_transfer.SelectedItem = TranslationStrings.LabelTransferItem3;
                }
                else
                {
                    cmb_transfer.Items.Add(TranslationStrings.LabelTransferItem2);
                    cmb_transfer.Items.Add(TranslationStrings.LabelTransferItem3);
                    cmb_transfer.SelectedItem = property.NoDownload
                                                    ? TranslationStrings.LabelTransferItem2
                                                    : TranslationStrings.LabelTransferItem3;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error relod list ", e);
            }
            _loading = false;
        }

        private void DeviceManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ServiceProvider.DeviceManager == null || ServiceProvider.DeviceManager.SelectedCameraDevice == null)
                return;
            if (e.PropertyName == "SelectedCameraDevice")
            {
                Dispatcher.Invoke(new Action(RefreshItems));
                var device = ServiceProvider.DeviceManager.SelectedCameraDevice as BaseCameraDevice;
                if (device != null) device.PropertyChanged += device_PropertyChanged;
            }
        }

        private void device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_loading)
                return;
            if (sender == ServiceProvider.DeviceManager.SelectedCameraDevice && e.PropertyName == "CaptureInSdRam")
            {
                Dispatcher.BeginInvoke(new Action(RefreshItems));
            }
        }*/


        public void ExecuteCommand(string cmd, object param)
        {
            switch (cmd)
            {
                case "MacroLiveView_Show":
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (DataContext == null)
                            DataContext = LiveViewViewModel.Instance;
                        ServiceProvider.Settings.ApplyTheme(this);
                        Owner = ServiceProvider.PluginManager.SelectedWindow as Window;
                        Show();
                        Activate();
                        Focus();
                    }));
                    break;
                case "MacroLiveView_Hide":
                    Dispatcher.Invoke(new Action(Hide));
                    break;
                case WindowsCmdConsts.LiveViewWnd_Message:
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (this.IsLoaded)
                                this.ShowMessageAsync("", (string)param);
                            else
                            {
                                MessageBox.Show((string)param);
                            }
                        }));
                    }
                    break;
                case CmdConsts.All_Close:
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Hide();
                        Close();
                    }));
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LiveViewViewModel.Instance.MagiCalc(3.6);
            Windows.MagniWin MW= new Windows.MagniWin();
            MW.ShowDialog();
        }

        private void MetroWindow_Activated(object sender, EventArgs e)
        {
            ArduinoPorts.Instance.DetectArduino();
            //System.Threading.Thread.Sleep(2500);
            Task.Delay(5000);
            ArduinoPorts.Instance.SendCommand(10, 1, 0);
        }

        private void Arduino_Button_Click(object sender, RoutedEventArgs e)
        {
            ArduinoPorts.Instance.DetectArduino();
            //System.Threading.Thread.Sleep(2500);
            Task.Delay(5000);
            ArduinoPorts.Instance.SendCommand(10, 1, 0);
        }
    }
}