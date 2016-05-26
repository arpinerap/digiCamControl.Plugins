using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Scripting.ScriptCommands;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Plugins.ImageTransformPlugins;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using Microsoft.WindowsAPICodePack.Dialogs;
using ImageMagick;

namespace Macrophotography.ViewModel
{
    public class VisualSFMViewModel : MacroStackViewModel
    {
        #region Variables

        private string _VisualSFMCommand;
        private string _launchCommand;

        private PluginSetting _pluginSetting;

        private Process _VisualSFMProcess; 

        private int _Items;
        private string _ProjetFolder = "";
        

        #endregion

        #region RaisePropertyChanged


        public PluginSetting PluginSetting
        {
            get
            {
                if (_pluginSetting == null)
                {
                    _pluginSetting = ServiceProvider.Settings["CombineZp"];
                }
                return _pluginSetting;
            }
        }

        public int Items // Total Number of Photos to Stack ("Files" Collection)
        {
            get { return _Items; }
            set
            {
                _Items = value;
                RaisePropertyChanged(() => Items);
            }
        }


        public string VisualSFMCommand
        {
            get { return _VisualSFMCommand; }
            set
            {
                _VisualSFMCommand = value;
                RaisePropertyChanged(() => VisualSFMCommand);
            }
        }
        public string launchCommand
        {
            get { return _launchCommand; }
            set
            {
                _launchCommand = value;
                RaisePropertyChanged(() => launchCommand);
            }
        }

        public string ProjetFolder
        {
            get { return _ProjetFolder; }
            set
            {
                _ProjetFolder = value;
                RaisePropertyChanged(() => ProjetFolder);
            }
        }

        #endregion

        #region Commands

        public CameraControl.Core.Classes.RelayCommand<object> SelectAllCommand { get; private set; }
        public CameraControl.Core.Classes.RelayCommand<object> SelectNoneCommand { get; private set; }
        public CameraControl.Core.Classes.RelayCommand<object> SelectInverCommand { get; private set; }
        
        
        
        public RelayCommand SetProjetFolderCommand { get; set; }

        #endregion

        #region Folders

        private void SetProjetFolder()
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (ProjetFolder != null | ProjetFolder != "")
                {
                    ProjetFolder = ServiceProvider.Settings.DefaultSession.Folder + "\\VisualSFMProjet";
                    Directory.CreateDirectory(ProjetFolder);
                }
                dialog.SelectedPath = ProjetFolder;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ProjetFolder = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error set ProjetFolder ", ex);
            }
        }

        #endregion

        public VisualSFMViewModel()
        {
            Output = new AsyncObservableCollection<string>();
            InitCommands();
            CreateTempDir(true);
            MakeVisualSFMLaunchCommand();
            ServiceProvider.FileTransfered += ServiceProvider_FileTransfered;
            ServiceProvider.WindowsManager.Event += WindowsManager_Event;
            LoadVisualSFMData();
            ReloadCommand = new RelayCommand(LoadVisualSFMData);
            PreviewCommand = new RelayCommand(Preview);
            GenerateCommand = new RelayCommand(Generate);
            StopCommand = new RelayCommand(Stop);
            SelectAllCommand = new CameraControl.Core.Classes.RelayCommand<object>(delegate { ServiceProvider.Settings.DefaultSession.SelectAll(); });
            SelectNoneCommand = new CameraControl.Core.Classes.RelayCommand<object>(delegate { ServiceProvider.Settings.DefaultSession.SelectNone(); });
            SelectInverCommand = new CameraControl.Core.Classes.RelayCommand<object>(delegate { ServiceProvider.Settings.DefaultSession.SelectInver(); });
            SetProjetFolderCommand = new RelayCommand(SetProjetFolder);        
        }

        #region Run VisualSFM Process

        private void CreateTempDir(bool folder)
        {
            _tempdir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (folder == true) { Directory.CreateDirectory(_tempdir); }
            OnProgressChange("Temporal Folder: " + _tempdir);
        }

        public void CopyFiles(bool preview)
        {
            int counter = 0;
            try
            {
                _filenames.Clear();

                OnProgressChange("Copying files");

                foreach (FileItem fileItem in Files)
                {
                    OnProgressChange("Copying file " + fileItem.Name);
                    string source = preview ? fileItem.LargeThumb : fileItem.FileName;

                    MagickImageInfo info = new MagickImageInfo();
                    info.Read(source);
                    string format = info.Format.ToString();

                    if (format == "Jpeg")
                    {
                        string randomFile = Path.Combine(_tempdir, "image_" + counter.ToString("0000") + ".jpg");
                        AutoExportPluginHelper.ExecuteTransformPlugins(fileItem, PluginSetting.AutoExportPluginConfig, source, randomFile);
                        _filenames.Add(randomFile);
                        counter++;
                    }

                    if (format == "Tiff")
                    {
                        string randomFile = Path.Combine(_tempdir, "image_" + counter.ToString("0000") + ".tif");
                        using (MagickImage image = new MagickImage(source))
                        {
                            image.Format = MagickFormat.Tiff;
                            image.Write(randomFile);
                        }
                        _filenames.Add(randomFile);
                        counter++;
                    }
                    if (format != "Jpeg" && format != "Tiff")
                    {
                        OnProgressChange(fileItem.Name + "has invalid format, will not be copied");
                    }


                    if (_shouldStop)
                    {
                        OnActionDone();
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                OnProgressChange("Error copy files " + exception.Message);
                Log.Error("Error copy files ", exception);
                _shouldStop = true;
            }
        }

        private void MakeVisualSFMLaunchCommand()
        {
            try
            {
                string VisualSFMFolder = @"C:\Program Files\VisualSFM_windows_64bit";
                string VisualSFMFile = "VisualSFM.exe";
                VisualSFMCommand = Path.Combine(VisualSFMFolder, VisualSFMFile);
                OnProgressChange("VisualSFM Command: " + VisualSFMCommand);

                launchCommand = " sfm+pmvs";
                launchCommand += " " + _tempdir;
                launchCommand += OutputFolder + "output.nvm";
                OnProgressChange("Launch Command: " + launchCommand);
            }
            catch (Exception ex)
            {
                OnProgressChange("Could not change VisualSFM Command File");
            }
        }

        public void VisualSFMMerge()
        {
            try
            {
                OnProgressChange("VisualSFM is merging images ..");
                OnProgressChange("This may take few minutes too");

                _resulfile = Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(Files[0].FileName) + Files.Count + ".jpg");
                _resulfile = Path.Combine(Path.GetTempPath(), Path.GetFileName(_resulfile));


                Process newprocess = new Process();
                newprocess.StartInfo = new ProcessStartInfo()
                {
                    FileName = VisualSFMCommand,
                    Arguments = launchCommand,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(_filenames[0])
                };
                newprocess.Start();
                _VisualSFMProcess = newprocess;
                newprocess.OutputDataReceived += newprocess_OutputDataReceived;
                newprocess.ErrorDataReceived += newprocess_OutputDataReceived;
                newprocess.BeginOutputReadLine();
                newprocess.BeginErrorReadLine();
                newprocess.WaitForExit();
                /*if (File.Exists(_resulfile))
                {
                    //string localfile = Path.Combine(Path.GetDirectoryName(_files[0].FileName),
                    //                                Path.GetFileName(_resulfile));
                    //File.Copy(_resulfile, localfile, true);
                    //ServiceProvider.Settings.DefaultSession.AddFile(localfile);
                    PreviewBitmap = BitmapLoader.Instance.LoadImage(_resulfile);
                }
                else
                {
                    OnProgressChange("No output file something went wrong !");
                }
                _ZereneProcess = null;*/
            }
            catch (Exception exception)
            {
                OnProgressChange("Error copy files " + exception.Message);
                Log.Error("Error copy files ", exception);
                _shouldStop = true;
            }
        }

        public void Init()
        {
            Output.Clear();
            _shouldStop = false;

        }
        private void Preview()
        {
            Init();
            Task.Factory.StartNew(PreviewTask);
        }
        private void Generate()
        {
            Init();
            Task.Factory.StartNew(GenerateTask);
        }

        private void PreviewTask()
        {
            IsBusy = true;
            CopyFiles(true);
            if (!_shouldStop)
            {
                VisualSFMMerge();
                OnActionDone();
            }
            IsBusy = false;
        }
        private void GenerateTask()
        {
            IsBusy = true;
            CopyFiles(false);
            if (!_shouldStop)
            {
                VisualSFMMerge();
            }

            /*if (File.Exists(_resulfile))
            {
                string newFile = Path.Combine(Path.GetDirectoryName(Files[0].FileName),
                    Path.GetFileNameWithoutExtension(Files[0].FileName) + "_enfuse" + ".jpg");
                newFile = PhotoUtils.GetNextFileName(newFile);

                File.Copy(_resulfile, newFile, true);

                if (ServiceProvider.Settings.DefaultSession.GetFile(newFile) == null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        FileItem im = new FileItem(newFile);
                        im.Transformed = true;
                        ServiceProvider.Settings.DefaultSession.Files.Add(im);
                    }));
                }
            }*/
            OnActionDone();
            IsBusy = false;
        }
        private void Stop()
        {
            _shouldStop = true;
            try
            {
                if (_VisualSFMProcess != null)
                    _VisualSFMProcess.Kill();
            }
            catch (Exception)
            {

            }
        }

        private void newprocess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnProgressChange(e.Data);
        }

        #endregion

        public void LoadVisualSFMData()
        {
            if (Session.Files.Count == 0 || ServiceProvider.Settings.SelectedBitmap.FileItem == null)
                return;
            var files = Session.GetSelectedFiles();
            if (files.Count > 0)
            {
                Files = files;
            }
            else
            {
                Files = Session.GetSeries(ServiceProvider.Settings.SelectedBitmap.FileItem.Series);
            }
            Items = Files.Count;
        }
        private void ServiceProvider_FileTransfered(object sender, FileItem fileItem)
        {
            if (Files.Count > 0)
            {
                if (Files[0].Series != fileItem.Series)
                {
                    Files.Clear();
                }
            }
            Files.Add(fileItem);
        }
        private void WindowsManager_Event(string cmd, object o)
        {

        }         
    }
}
