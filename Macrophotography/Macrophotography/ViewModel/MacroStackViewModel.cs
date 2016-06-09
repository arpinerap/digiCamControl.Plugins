using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Macrophotography.ViewModel
{
    public class MacroStackViewModel : ViewModelBase
    {
        #region Variables
        private string _SourceFolder = "";
        private string _SingleFolder = "";
        private string _StacksFolder = "";
        private string _SubStacksFolder = "";
        private string _OutputFolder = "";

        private bool _IsSubStack;
        private bool _IsNotSubStack;
        private bool _IsStackSubs;
        private bool _IsJustStackSubs;
        private bool _IsNotJustStackSubs = true;
        private bool _ProcessSubStacks;
        private bool _SinglePassStack = true;

        private StringBuilder _taskName = new StringBuilder();

        


        private ObservableCollection<FileItem> _files;
        private BitmapSource _previewBitmap;
        private AsyncObservableCollection<string> _output;
        private bool _isBusy;
        protected List<string> _filenames = new List<string>();
        protected bool _shouldStop;
        public string _resulfile = "";
        protected string _tempdir = "";

        #endregion

        #region RaisePropertyChanged
        public string SourceFolder
        {
            get { return _SourceFolder; }
            set
            {
                _SourceFolder = value;
                RaisePropertyChanged(() => SourceFolder);
            }
        }
        public string SingleFolder
        {
            get { return _SingleFolder; }
            set
            {
                _SingleFolder = value;
                RaisePropertyChanged(() => SingleFolder);
            }
        }
        public string StacksFolder
        {
            get { return _StacksFolder; }
            set
            {
                _StacksFolder = value;
                RaisePropertyChanged(() => StacksFolder);
            }
        }
        public string SubStacksFolder
        {
            get { return _SubStacksFolder; }
            set
            {
                _SubStacksFolder = value;
                RaisePropertyChanged(() => SubStacksFolder);
            }
        }
        public string OutputFolder
        {
            get { return _OutputFolder; }
            set
            {
                _OutputFolder = value;
                RaisePropertyChanged(() => OutputFolder);
            }
        }

        public bool IsJustStackSubs
        {
            get { return _IsJustStackSubs; }
            set
            {
                _IsJustStackSubs = value;
                RaisePropertyChanged(() => IsJustStackSubs);
                RaisePropertyChanged(() => IsNotJustStackSubs);
                SetEnabled();
            }
        }
        public bool IsNotJustStackSubs
        {
            get { return _IsNotJustStackSubs; }
            set
            {
                _IsNotJustStackSubs = value;
                RaisePropertyChanged(() => IsNotJustStackSubs);
                RaisePropertyChanged(() => IsJustStackSubs);
                SetEnabled();
            }
        }
        public bool IsSubStack
        {
            get { return _IsSubStack; }
            set
            {
                _IsSubStack = value;
                RaisePropertyChanged(() => IsSubStack);
                RaisePropertyChanged(() => IsNotSubStack);
            }
        }
        public bool IsNotSubStack
        {
            get { return _IsNotSubStack; }
            set
            {
                _IsNotSubStack = value;
                RaisePropertyChanged(() => IsNotSubStack);
                RaisePropertyChanged(() => IsSubStack);
            }
        }
        public bool IsStackSubs
        {
            get { return _IsStackSubs; }
            set
            {
                _IsStackSubs = value;
                RaisePropertyChanged(() => IsStackSubs);
                RaisePropertyChanged(() => IsNotStackSubs);
                ProcessSubStacks = IsJustStackSubs | IsStackSubs ? true : false;
            }
        }
        public bool IsNotStackSubs
        {
            get { return !IsStackSubs; }
        }
        public bool ProcessSubStacks
        {
            get { return _ProcessSubStacks; }
            set
            {
                _ProcessSubStacks = value;
                RaisePropertyChanged(() => ProcessSubStacks);
            }
        }
        public bool SinglePassStack
        {
            get { return _SinglePassStack; }
            set
            {
                _SinglePassStack = value;
                RaisePropertyChanged(() => SinglePassStack);
            }
        }

        public StringBuilder taskName
        {
            get { return _taskName; }
            set
            {
                _taskName = value;
                RaisePropertyChanged(() => taskName);
            }
        }

        #endregion

        #region Folders

        private void SetSingleFolder()
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (SingleFolder != null | SingleFolder != "")
                {
                    SingleFolder = ServiceProvider.Settings.DefaultSession.Folder + "\\SinglePassStacks";
                    Directory.CreateDirectory(SingleFolder);
                }
                dialog.SelectedPath = SingleFolder;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SingleFolder = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error set SingleFolder ", ex);
            }
        }
        private void SetStacksFolder()
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (StacksFolder != null | StacksFolder != "")
                {
                    StacksFolder = ServiceProvider.Settings.DefaultSession.Folder + "\\SubStacks";
                    Directory.CreateDirectory(StacksFolder);
                }
                dialog.SelectedPath = StacksFolder;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StacksFolder = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error set StacksFolder ", ex);
            }
        }
        private void SetSubStacksFolder()
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (SubStacksFolder != null | SubStacksFolder != "")
                {
                    SubStacksFolder = ServiceProvider.Settings.DefaultSession.Folder + "\\StackSubStacks";
                    Directory.CreateDirectory(SubStacksFolder);
                }
                dialog.SelectedPath = SubStacksFolder;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SubStacksFolder = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error set SubStacksFolder ", ex);
            }
        }

        #endregion

        #region Commands

        public GalaSoft.MvvmLight.Command.RelayCommand<FileItem> RemoveItemCommand { get; set; }
        public RelayCommand ReloadCommand { get; set; }
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand PreviewCommand { get; set; }
        public RelayCommand GenerateCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand ConfPluginCommand { get; set; }

        public RelayCommand SetSingleFolderCommand { get; set; }
        public RelayCommand SetStacksFolderCommand { get; set; }
        public RelayCommand SetSubStacksFolderCommand { get; set; }




        #endregion


        public ObservableCollection<FileItem> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                RaisePropertyChanged(() => Files);
            }
        }

        public BitmapSource PreviewBitmap
        {
            get { return _previewBitmap; }
            set
            {
                _previewBitmap = value;
                RaisePropertyChanged(() => PreviewBitmap);
            }
        }

        public AsyncObservableCollection<string> Output
        {
            get { return _output; }
            set
            {
                _output = value;
                RaisePropertyChanged(() => Output);
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
                RaisePropertyChanged(() => IsFree);
            }
        }

        public bool IsFree
        {
            get { return !IsBusy; }
        }

        public PhotoSession Session
        {
            get { return ServiceProvider.Settings.DefaultSession; }
        }

        

        public void InitCommands()
        {
            RemoveItemCommand = new GalaSoft.MvvmLight.Command.RelayCommand<FileItem>(RemoveItem);
            ReloadCommand = new RelayCommand(Reload);
            SetSingleFolderCommand = new RelayCommand(SetSingleFolder);
            SetStacksFolderCommand = new RelayCommand(SetStacksFolder);
            SetSubStacksFolderCommand = new RelayCommand(SetSubStacksFolder);
        }

        private void RemoveItem(FileItem obj)
        {
            if (Files.Contains(obj))
                Files.Remove(obj);
        }

        public void LoadData()
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
        }

        private void Reload()
        {
            LoadData();
        }

        public static void LogFile(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            //w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }
        
        public static void LogFileShort(string logMessage, TextWriter w)
        {
            
            w.WriteLine(logMessage);
        }

        public void OnProgressChange(string text)
        {
            if (text != null)
                Output.Insert(0, text);
        }

        public void OnActionDone()
        {
            try
            {
                if (Directory.Exists(_tempdir))
                    Directory.Delete(_tempdir, true);
                if (File.Exists(_resulfile))
                    File.Delete(_resulfile);
            }
            catch (Exception)
            {
                Log.Error("Error  delete temp folder");
                throw;
            }
        }

        public void SetEnabled()
        {
            if (IsNotJustStackSubs)
            {
                IsSubStack = false;
                IsStackSubs = false;
                ProcessSubStacks = false;
                SinglePassStack = true;
            }
            else
            {
                IsSubStack = true;
                IsStackSubs = true;
                ProcessSubStacks = true;
                SinglePassStack = false;
            }
        }






    }
}
