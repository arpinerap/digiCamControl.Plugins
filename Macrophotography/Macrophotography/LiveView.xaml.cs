﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
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
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace Macrophotography
{
    /// <summary>
    /// Interaction logic for LiveView.xaml
    /// </summary>
    public partial class LiveView
    {
        private Timer _timer = new Timer();

        private bool _loading = false;

        public LiveView()
        {
            InitializeComponent();
            ServiceProvider.Settings.ApplyTheme(this);
            if (ServiceProvider.DeviceManager != null)
                ServiceProvider.DeviceManager.PropertyChanged += DeviceManager_PropertyChanged;
            RefreshItems();
            
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
                ArduinoPorts.Instance.ClosePort();
                ServiceProvider.DeviceManager.SelectedCameraDevice.StopLiveView();
                ServiceProvider.DeviceManager.SelectedCameraDevice.HostMode = false;
            }
            catch (Exception)
            {
            }
        }

        private void cmb_transfer_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        }


    }
}