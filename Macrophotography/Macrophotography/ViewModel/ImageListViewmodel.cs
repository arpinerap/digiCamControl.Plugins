using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Devices;
using Microsoft.VisualBasic.FileIO;

namespace Macrophotography.ViewModel
{
    class ImageListViewmodel : ViewModelBase
    {
        public GalaSoft.MvvmLight.Command.RelayCommand<string> SendCommand { get; set; }

        public ListBox ImageLIst { get; set; }
        
        public RelayCommand SelectLiked { get; private set; }
        public RelayCommand SelectUnLiked { get; private set; }
        public RelayCommand SelectNoneCommand { get; private set; }
        public RelayCommand SelectInvertCommand { get; private set; }
        public RelayCommand SelectSeries { get; private set; }
        public RelayCommand SelectAllCommand { get; private set; }

        public ImageListViewmodel()
        {
            ServiceProvider.WindowsManager.Event -= Trigger_Event;
            SelectAllCommand = new RelayCommand(delegate { ServiceProvider.Settings.DefaultSession.SelectAll(); });
            SelectNoneCommand = new RelayCommand(delegate { ServiceProvider.Settings.DefaultSession.SelectNone(); });
            SelectLiked = new RelayCommand(delegate { ServiceProvider.Settings.DefaultSession.SelectLiked(); });
            SelectUnLiked = new RelayCommand(delegate { ServiceProvider.Settings.DefaultSession.SelectUnLiked(); });
            SelectInvertCommand = new RelayCommand(delegate { ServiceProvider.Settings.DefaultSession.SelectInver(); });
            SelectSeries =
                new RelayCommand(delegate
                {
                    try
                    {
                        ServiceProvider.Settings.DefaultSession.SelectSameSeries(
                            ServiceProvider.Settings.SelectedBitmap.FileItem.Series);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SelectSeries", ex);
                    }
                });
        }

        private void TriggerEvent(string cmd, object o)
        {
            try
            {
                switch (cmd)
                {
                    case WindowsCmdConsts.Del_Image:
                        {
                            DeleteItem();
                        }
                        break;
                    case WindowsCmdConsts.Select_Image:
                        FileItem fileItem = o as FileItem;
                        if (fileItem != null)
                        {
                            ImageLIst.SelectedValue = fileItem;
                            ImageLIst.ScrollIntoView(fileItem);
                        }
                        break;
                    case WindowsCmdConsts.ViewExternal:
                        OpenInExternalViewer();
                        break;
                    case WindowsCmdConsts.ViewExplorer:
                        OpenInExplorer();
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Unable to process TriggerEvent in ImageListViewmodel ", exception);
            }

        }

        private void Trigger_Event(string cmd, object o)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => TriggerEvent(cmd, o)));
            }
            catch (Exception)
            {
            }
        }

        private void DeleteItem()
        {
            List<FileItem> filestodelete = new List<FileItem>();
            try
            {
                filestodelete.AddRange(
                    ServiceProvider.Settings.DefaultSession.Files.Where(fileItem => fileItem.IsChecked));

                if (ServiceProvider.Settings.SelectedBitmap != null &&
                    ServiceProvider.Settings.SelectedBitmap.FileItem != null &&
                    filestodelete.Count == 0)
                    filestodelete.Add(ServiceProvider.Settings.SelectedBitmap.FileItem);

                if (filestodelete.Count == 0)
                    return;
                int selectedindex = ImageLIst.Items.IndexOf(filestodelete[0]);

                bool delete = false;
                if (filestodelete.Count > 1)
                {
                    delete = MessageBox.Show("Multile files are selected !! Do you really want to delete selected files ?", "Delete files",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                }
                else
                {
                    delete = MessageBox.Show("Do you really want to delete selected file ?", "Delete file",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes;

                }
                if (delete)
                {
                    foreach (FileItem fileItem in filestodelete)
                    {
                        if ((ServiceProvider.Settings.SelectedBitmap != null &&
                             ServiceProvider.Settings.SelectedBitmap.FileItem != null &&
                             fileItem.FileName == ServiceProvider.Settings.SelectedBitmap.FileItem.FileName))
                        {
                            ServiceProvider.Settings.SelectedBitmap.DisplayImage = null;
                        }
                        if (File.Exists(fileItem.FileName))
                            FileSystem.DeleteFile(fileItem.FileName, UIOption.OnlyErrorDialogs,
                                                  RecycleOption.SendToRecycleBin);
                        fileItem.RemoveThumbs();
                        ServiceProvider.Settings.DefaultSession.Files.Remove(fileItem);
                    }
                    if (selectedindex < ImageLIst.Items.Count)
                    {
                        ImageLIst.SelectedIndex = selectedindex + 1;
                        ImageLIst.SelectedIndex = selectedindex - 1;
                        FileItem item = ImageLIst.SelectedItem as FileItem;

                        if (item != null)
                            ImageLIst.ScrollIntoView(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error to delete file", exception);
            }
        }

        private void OpenInExplorer()
        {
            if (ServiceProvider.Settings.SelectedBitmap == null ||
                ServiceProvider.Settings.SelectedBitmap.FileItem == null)
                return;
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "explorer";
                processStartInfo.UseShellExecute = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                processStartInfo.Arguments =
                    string.Format("/e,/select,\"{0}\"", ServiceProvider.Settings.SelectedBitmap.FileItem.FileName);
                Process.Start(processStartInfo);
            }
            catch (Exception exception)
            {
                Log.Error("Error to show file in explorer", exception);
            }
        }

        private void OpenInExternalViewer()
        {
            if (ServiceProvider.Settings.SelectedBitmap == null ||
                ServiceProvider.Settings.SelectedBitmap.FileItem == null)
                return;
            if (!string.IsNullOrWhiteSpace(ServiceProvider.Settings.ExternalViewer) &&
                File.Exists(ServiceProvider.Settings.ExternalViewer))
            {
                PhotoUtils.Run(ServiceProvider.Settings.ExternalViewer,
                    "\"" + ServiceProvider.Settings.SelectedBitmap.FileItem.FileName + "\"",
                    ProcessWindowStyle.Maximized);
            }
            else
            {
                PhotoUtils.Run("\"" + ServiceProvider.Settings.SelectedBitmap.FileItem.FileName + "\"", "",
                    ProcessWindowStyle.Maximized);
            }
        }
    }
}
