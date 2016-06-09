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

        private PluginSetting _pluginSetting;

        private Process _VisualSFMProcess; 

        private int _Items;
        private string _ProjetFolder = "";
        private bool _IsProjetFolder = false;


        private int _ProgressBarValue = 0;
        private int _ProgressBarMaxValue = 100;
        

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

        public bool IsProjetFolder
        {
            get { return _IsProjetFolder; }
            set
            {
                _IsProjetFolder = value;
                RaisePropertyChanged(() => IsProjetFolder);
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

        public string ProjetFolder
        {
            get { return _ProjetFolder; }
            set
            {
                _ProjetFolder = value;
                RaisePropertyChanged(() => ProjetFolder);
            }
        }


        public int ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set
            {
                _ProgressBarValue = value;
                RaisePropertyChanged(() => ProgressBarValue);
            }
        }
        public int ProgressBarMaxValue
        {
            get { return _ProgressBarMaxValue; }
            set
            {
                _ProgressBarMaxValue = value;
                RaisePropertyChanged(() => ProgressBarMaxValue);
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
                    IsProjetFolder = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error set ProjetFolder ", ex);
                WriteLog("Error set ProjetFolder " + ex.Data);
                WriteShortLog("Error set ProjetFolder " + ex.Data);
            }
        }

        #endregion

        public VisualSFMViewModel()
        {
            Output = new AsyncObservableCollection<string>();
            InitCommands();
            //CreateTempDir(true);
            //MakeVisualSFMLaunchCommand();
            ServiceProvider.FileTransfered += ServiceProvider_FileTransfered;
            ServiceProvider.WindowsManager.Event += WindowsManager_Event;
            LoadVisualSFMData();
            ReloadCommand = new RelayCommand(LoadVisualSFMData);
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
                WriteLog("Copying files");
                WriteShortLog("Copying files");

                foreach (FileItem fileItem in Files)
                {
                    string source = preview ? fileItem.LargeThumb : fileItem.FileName;

                    MagickImageInfo info = new MagickImageInfo();
                    info.Read(source);
                    string format = info.Format.ToString();

                    OnProgressChange(fileItem.Name + " format is:  " + format);
                    WriteLog(fileItem.Name + " format is:  " + format);
                    WriteShortLog(fileItem.Name + " format is:  " + format);
                                       

                    if (format == "Jpeg")
                    {
                        OnProgressChange("Copying file " + fileItem.Name);
                        WriteLog("Copying file " + fileItem.Name);
                        WriteShortLog("Copying file " + fileItem.Name);
                        string randomFile = Path.Combine(ProjetFolder, "image_" + counter.ToString("0000") + ".jpg");
                        AutoExportPluginHelper.ExecuteTransformPlugins(fileItem, PluginSetting.AutoExportPluginConfig, source, randomFile);
                        _filenames.Add(randomFile);
                        counter++;
                    }

                    if (format == "Tiff")
                    {
                        OnProgressChange("Converting file " + fileItem.Name);
                        WriteLog("Converting file " + fileItem.Name);
                        WriteShortLog("Converting file " + fileItem.Name);
                        string randomFile = Path.Combine(ProjetFolder, "image_" + counter.ToString("0000") + ".jpg");

                        try
                        {
                            using (MagickImage image = new MagickImage(source))
                            {
                                image.Format = MagickFormat.Jpeg;
                                image.Write(randomFile);
                            }
                            _filenames.Add(randomFile);
                            counter++;
                        }
                        catch (Exception exception)
                        {
                            Log.Error("Error converting Tiff file", exception);
                        }    
                    }

                    if (format == "Nef" || format == "Cr2" || format == "Crw")
                    {
                        OnProgressChange("Converting file " + fileItem.Name);
                        WriteLog("Converting file " + fileItem.Name);
                        WriteShortLog("Converting file " + fileItem.Name);
                        string randomFile = Path.Combine(ProjetFolder, "image_" + counter.ToString("0000") + ".jpg");

                        try
                        {
                            string dcraw_exe = Path.Combine(Settings.ApplicationFolder, "dcraw.exe");
                            if (File.Exists(dcraw_exe))
                            {
                                PhotoUtils.RunAndWait(dcraw_exe,
                                    string.Format(" -e -O \"{0}\" \"{1}\"", randomFile, source));
                                _filenames.Add(randomFile);
                                counter++;
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.Error("Error converting Raw file", exception);
                        }
                    }

                    if (format != "Jpeg" && format != "Tiff" && format != "Nef" && format != "Cr2" && format != "Crw")
                    {
                        OnProgressChange(fileItem.Name + " has invalid format, will not be copied");
                        WriteLog(fileItem.Name + " has invalid format, will not be copied");
                        WriteShortLog(fileItem.Name + " has invalid format, will not be copied");
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
                WriteLog("Error copy files " + exception.Message);
                WriteShortLog("Error copy files " + exception.Message);
                Log.Error("Error copy files ", exception);
                _shouldStop = true;
            }
        }

        private void MakeVisualSFMLaunchCommand()
        {
            try
            {
                string VisualSFMFolder = @" /c C:\""Program Files""\VisualSFM_windows_64bit\VisualSFM.exe";

                VisualSFMCommand = VisualSFMFolder;
                VisualSFMCommand += " sfm+pmvs";
                VisualSFMCommand += " " + ProjetFolder;
                VisualSFMCommand += " " + ProjetFolder + @"\output.nvm";

                OnProgressChange("VisualSFM Command: " + VisualSFMCommand);
                WriteLog("VisualSFM Command: " + VisualSFMCommand);
                WriteShortLog("VisualSFM Command: " + VisualSFMCommand);

            }
            catch (Exception ex)
            {
                Log.Error("Error Make VisualSFM Launch Command ", ex);
                OnProgressChange("Could not make VisualSFM Launch Command");
                WriteLog("Could not make VisualSFM Launch Command");
                WriteShortLog("Could not make VisualSFM Launch Command");
            }
        }

        public void VisualSFMMerge()
        {
            try
            {
                OnProgressChange("VisualSFM is merging images ..");
                OnProgressChange("This may take few minutes too");


                Process newprocess = new Process();
                newprocess.StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = VisualSFMCommand,
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
            }
            catch (Exception exception)
            {
                OnProgressChange("Error VisualSFM launch: " + exception.Message);
                Log.Error("Error VisualSFM launch: ", exception);
                _shouldStop = true;
            }
        }

        public void Init()
        {
            Output.Clear();
            _shouldStop = false;

        }

        private void Generate()
        {
            Init();
            MakeVisualSFMLaunchCommand();
            Task.Factory.StartNew(GenerateTask);
        }

        private void GenerateTask()
        {
            IsBusy = true;
            CopyFiles(false);
            if (!_shouldStop)
            {
                VisualSFMMerge();
            }
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

        public void WriteLog(string log)
        {
            if (!String.IsNullOrEmpty(log))
            {
                string VisualSFMLogFile;
                VisualSFMLogFile = Path.Combine(ProjetFolder, "VisualSFMlog.txt");

                using (StreamWriter w = File.AppendText(VisualSFMLogFile))
                {
                    LogFile(log, w);
                }
            }            
        }

        public void WriteShortLog(string log)
        {
            if (!String.IsNullOrEmpty(log))
            {
                string VisualSFMLogFileShort;
                VisualSFMLogFileShort = Path.Combine(ProjetFolder, "VisualSFMlogShort.txt");

                using (StreamWriter w = File.AppendText(VisualSFMLogFileShort))
                {
                    LogFileShort(log, w);
                }
            }            
        }

        private void newprocess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnProgressChange(e.Data);
            WriteLog(e.Data);
            WriteShortLog(e.Data);

            /*string VisualSFMLogFile;
            VisualSFMLogFile = Path.Combine(ProjetFolder,"VisualSFMlog.txt");

            using (StreamWriter w = File.AppendText(VisualSFMLogFile))
            {
                LogFile(e.Data, w);
            }

            WriteLog();
            WriteShortLog();


            string VisualSFMLogFileShort;
            VisualSFMLogFileShort = Path.Combine(ProjetFolder, "VisualSFMlogShort.txt");
            using (StreamWriter w = File.AppendText(VisualSFMLogFileShort))
            {
                LogFileShort(e.Data, w);
            }*/





            /*if (e.Data.Contains("SHOWPROGRESS: Done")) 
            {
				//mProgressBar.setVisible(false);
			} 
            else if (e.Data.Contains("SHOWPROGRESS: Show")) 
            {
				//mProgressBar.setVisible(true);
			} 
            else if (e.Data.Contains("SHOWPROGRESS: Indeterminate")) 
            {
				//mProgressBar.setIndeterminate(true);
				//mProgressBar.setStringPainted(false);  // do not show progress as percent
			} */
            /*if (e.Data.Contains("SHOWPROGRESS: Max")) 
            {
                string data = e.Data;
                string lastFragment = data.Split('=').Last();
                int val = int.Parse(lastFragment);
                ProgressBarMaxValue = val;                                
				//int max = int.Parse(e.Data.Replace("^.*=",""));
				//mProgressBar.setMaximum(max);
				//mProgressBar.setIndeterminate(false);
				//mProgressBar.setStringPainted(true);  // show progress as percent
			} if (e.Data.Contains("SHOWPROGRESS: Current")) 
            {
                string data = e.Data;
                string lastFragment = data.Split('=').Last();
                int val = int.Parse(lastFragment);
                //int val = int.Parse(e.Data.Replace("^.*=", ""));
				//mProgressBar.setValue(val);
				//mProgressBar.setIndeterminate(false);
                ProgressBarValue = val;
            }*/
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
