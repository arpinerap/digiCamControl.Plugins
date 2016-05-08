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

namespace Macrophotography.ViewModel
{
    public class ZereneBatchViewModel : MacroStackViewModel
    {
                       
        #region Variables

        private ObservableCollection<Prop> _Props = new ObservableCollection<Prop>();
        private ObservableCollection<StackTask> _StackTasks = new ObservableCollection<StackTask>();
        private ObservableCollection<TaskIndicatorCodeValues> _TaskIndicatorCodes = new ObservableCollection<TaskIndicatorCodeValues>(); // populate combos with
        private ObservableCollection<TaskIndicatorCodeValues> _TaskIndicatorCodesSubs = new ObservableCollection<TaskIndicatorCodeValues>(); // populate SubStacks combos with
        private ObservableCollection<OutputImageDispositionCodeValues> _OutputImageDispositionCodes = new ObservableCollection<OutputImageDispositionCodeValues>(); // populate combos with
        private ObservableCollection<ProjectDispositionCodeValues> _ProjectDispositionCodes = new ObservableCollection<ProjectDispositionCodeValues>(); // populate combos with

        private AsyncObservableCollection<string> _output = new AsyncObservableCollection<string>();

        private TaskIndicatorCodeValues _ActualTaskIndicatorCodeValue = new TaskIndicatorCodeValues();
        private TaskIndicatorCodeValues _ActualTaskIndicatorCodeValueSlab = new TaskIndicatorCodeValues();
        private OutputImageDispositionCodeValues _ActualOutputImageDispositionCodeValue = new OutputImageDispositionCodeValues();
        private ProjectDispositionCodeValues _ActualProjectDispositionCodeValue = new ProjectDispositionCodeValues();
        private StackTask _ActualStackTask = new StackTask();

        private PluginSetting _pluginSetting;
        
        private Process _ZereneProcess;        


        private int _EstimatedRadius = 10;
        private int _SmoothingRadius = 5;
        private int _ContrastThreshold = 90;

        private bool _IsDMap;
        private bool _IsDMap2 = true;
        private bool _IsNotProjetFolderFixed;

        private bool _IsSubStack;
        private bool _IsNotSubStack;
        private bool _IsStackSubs;
        private bool _IsJustStackSubs;
        private bool _IsNotJustStackSubs = true;
        private bool _ProcessSubStacks;
        private bool _SinglePassStack = true;

        private bool _IsJpeg;
        private bool _IsTiff = true;
        private string _FileType;

        private string _SourceFolder = "";
        private string _ProjetFolder = "";
        private string _SingleFolder = "";
        private string _StacksFolder = "";
        private string _SubStacksFolder = "";
        //private string _OutputImagesDesignatedFolder = ServiceProvider.Settings.DefaultSession.Name + "\\SubStacks";

        private string _OutputImageNames;
        private string _launchCommand;
        private string _ZereneCommand;

        private StringBuilder _taskName = new StringBuilder();

        private int _Items;
        private int _Stack_items = 20;       
        private int _Stack_overlap = 10;
        private int _Stack_item = 0;
        private int _Tasknumber = 0;

        //public int item = 0;
        #endregion

        
        #region RaisePropertyChanged
        public ObservableCollection<Prop> Props
        {
            get { return _Props; }
            set
            {
                _Props = value;
                RaisePropertyChanged(() => Props);
            }
        }
        public ObservableCollection<StackTask> StackTasks
        {
            get { return _StackTasks; }
            set
            {
                _StackTasks = value;
                RaisePropertyChanged(() => StackTasks);
            }
        }
        public ObservableCollection<TaskIndicatorCodeValues> TaskIndicatorCodes // populate combos with
        {
            get { return _TaskIndicatorCodes; }
            set
            {
                _TaskIndicatorCodes = value;
                RaisePropertyChanged(() => TaskIndicatorCodes);              
            }
        }
        public ObservableCollection<TaskIndicatorCodeValues> TaskIndicatorCodesSubs // populate combos with
        {
            get { return _TaskIndicatorCodesSubs; }
            set
            {
                _TaskIndicatorCodesSubs = value;
                RaisePropertyChanged(() => TaskIndicatorCodesSubs);
            }
        }
        public ObservableCollection<OutputImageDispositionCodeValues> OutputImageDispositionCodes // populate combos with
        {
            get { return _OutputImageDispositionCodes; }
            set
            {
                _OutputImageDispositionCodes = value;
                RaisePropertyChanged(() => OutputImageDispositionCodes);
            }
        }
        public ObservableCollection<ProjectDispositionCodeValues> ProjectDispositionCodes // populate combos with
        {
            get { return _ProjectDispositionCodes; }
            set
            {
                _ProjectDispositionCodes = value;
                RaisePropertyChanged(() => ProjectDispositionCodes);
            }
        }

        public AsyncObservableCollection<string> output // populate Output Log ListBox
        {
            get { return _output; }
            set
            {
                _output = value;
                RaisePropertyChanged(() => output);
            }
        }

        public TaskIndicatorCodeValues ActualTaskIndicatorCodeValue
        {
            get { return _ActualTaskIndicatorCodeValue; }
            set
            {
                _ActualTaskIndicatorCodeValue = value;
                RaisePropertyChanged(() => ActualTaskIndicatorCodeValue);
            }
        }
        public TaskIndicatorCodeValues ActualTaskIndicatorCodeValueSlab
        {
            get { return _ActualTaskIndicatorCodeValueSlab; }
            set
            {
                _ActualTaskIndicatorCodeValueSlab = value;
                RaisePropertyChanged(() => ActualTaskIndicatorCodeValueSlab);
            }
        }
        public OutputImageDispositionCodeValues ActualOutputImageDispositionCodeValue
        {
            get { return _ActualOutputImageDispositionCodeValue; }
            set
            {
                _ActualOutputImageDispositionCodeValue = value;
                RaisePropertyChanged(() => ActualOutputImageDispositionCodeValue);
            }
        }
        public ProjectDispositionCodeValues ActualProjectDispositionCodeValue
        {
            get { return _ActualProjectDispositionCodeValue; }
            set
            {
                _ActualProjectDispositionCodeValue = value;
                RaisePropertyChanged(() => ActualProjectDispositionCodeValue);
            }
        }
        public StackTask ActualStackTask
        {
            get { return _ActualStackTask; }
            set
            {
                _ActualStackTask = value;
                RaisePropertyChanged(() => ActualStackTask);
            }
        }

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


        public int EstimatedRadius
        {
            get { return _EstimatedRadius; }
            set
            {
                _EstimatedRadius = value;
                RaisePropertyChanged(() => EstimatedRadius);
                
            }
        }
        public int SmoothingRadius
        {
            get { return _SmoothingRadius; }
            set
            {
                _SmoothingRadius = value;
                RaisePropertyChanged(() => SmoothingRadius);

            }
        }
        public int ContrastThreshold
        {
            get { return _ContrastThreshold; }
            set
            {
                _ContrastThreshold = value;
                RaisePropertyChanged(() => ContrastThreshold);

            }
        }
        
        public bool IsDMap
        {
            get { return _IsDMap; }
            set
            {
                _IsDMap = value;
                RaisePropertyChanged(() => IsDMap);                
            }
        }
        public bool IsDMap2
        {
            get { return _IsDMap2; }
            set
            {
                _IsDMap2 = value;
                RaisePropertyChanged(() => IsDMap2);
            }
        }
        public bool IsNotProjetFolderFixed
        {
            get { return _IsNotProjetFolderFixed; }
            set
            {
                _IsNotProjetFolderFixed = value;
                RaisePropertyChanged(() => IsNotProjetFolderFixed);
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
                ProcessSubStacks = IsJustStackSubs | IsStackSubs ? true : false;
                IsNotSubStack = IsJustStackSubs ? true : false;
                IsStackSubs = IsJustStackSubs ? false : false;
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

        public bool IsJpeg
        {
            get { return _IsJpeg; }
            set
            {
                _IsJpeg = value;
                RaisePropertyChanged(() => IsJpeg);
                RaisePropertyChanged(() => IsTiff);
            }
        }
        public bool IsTiff
        {
            get { return _IsTiff; }
            set
            {
                _IsTiff = value;
                RaisePropertyChanged(() => IsTiff);
                RaisePropertyChanged(() => IsJpeg);
                FileType = _IsTiff ? "tif" : "jpg";
            }
        }
        public string FileType
        {
            get { return _FileType; }
            set
            {
                _FileType = value;
                RaisePropertyChanged(() => FileType);
            }
        }

        public string SourceFolder
        {
            get { return _SourceFolder; }
            set
            {
                _SourceFolder = value;
                RaisePropertyChanged(() => SourceFolder);
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
        /*public string OutputImagesDesignatedFolder
        {
            get { return _OutputImagesDesignatedFolder; }
            set
            {
                _OutputImagesDesignatedFolder = value;
                RaisePropertyChanged(() => OutputImagesDesignatedFolder);
            }
        }*/

        public string OutputImageNames
        {
            get { return _OutputImageNames; }
            set
            {
                _OutputImageNames = value;
                RaisePropertyChanged(() => OutputImageNames);
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
        public string ZereneCommand
        {
            get { return _ZereneCommand; }
            set
            {
                _ZereneCommand = value;
                RaisePropertyChanged(() => ZereneCommand);
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

        public int Stack_items // Number of Photos in each SubSlab
        {
            get { return _Stack_items; }
            set
            {
                _Stack_items = value;
                RaisePropertyChanged(() => Stack_items);
            }
        } 
        public int Stack_item // Actual Photo in the SubSlab
        {
            get { return _Stack_item; }
            set
            {
                _Stack_item = value;
                RaisePropertyChanged(() => Stack_item);
            }
        } 
        public int Stack_overlap // Number of Photos of overlap
        {
            get { return _Stack_overlap; }
            set
            {
                _Stack_overlap = value;
                RaisePropertyChanged(() => Stack_overlap);
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
        public int Tasknumber // Total Number of SubSlabTasks
        {
            get { return _Tasknumber; }
            set
            {
                _Tasknumber = value;
                RaisePropertyChanged(() => Tasknumber);
            }
        } 

        #endregion


        #region Classes
        public class Prop
        {
            public string PropertyName { get; set; }
            public string SubpropertyName { get; set; }            
            public string Subpropertyvalue { get; set; }
            //public bool substack { get; set; }
        }
        public class StackTask
        {
            public string TaskName { get; set; }
            public int TaskNumber { get; set; }
            public int ProjectDispositionCodeValue { get; set; }
            public int OutputImageDispositionCodeValue { get; set; }
            public int TaskIndicatorCodeValue { get; set; }
            public string SourceFolder { get; set; }
            public string OutputImagesDesignatedFolder { get; set; }
            public string ProjetFolder { get; set; }
            public bool Substack { get; set; }
            public string FileType { get; set; }
            public int Number { get; set; }
            public int Overlap { get; set; }
            public int EstimatedRadius { get; set; }
            public int SmoothingRadius { get; set; }
            public int ContrastThreshold { get; set; }
                     
        }

        public class TaskIndicatorCodeValues
        {
            public String Text { get; set; }
            public int Value { get; set; }
            public String Name { get; set; }
        }
        public class OutputImageDispositionCodeValues
        {
            public String Text { get; set; }
            public int Value { get; set; }
        }
        public class ProjectDispositionCodeValues
        {
            public String Text { get; set; }
            public int Value { get; set; }
        }

        public RelayCommand UpDateDMapCommand { get; set; }
        public RelayCommand UpDateDMap2Command { get; set; }
        public RelayCommand AddTaskCommand { get; set; }
        public RelayCommand DeleteTaskCommand { get; set; }
        public RelayCommand MoveUpTaskCommand { get; set; }
        public RelayCommand MoveDownTaskCommand { get; set; }
        public RelayCommand MakeBatchCommand { get; set; }
        public CameraControl.Core.Classes.RelayCommand<object> SelectAllCommand { get; private set; }
        public CameraControl.Core.Classes.RelayCommand<object> SelectNoneCommand { get; private set; }

        #endregion


        #region Task ListBox Management

        public void NamingTask()
        {
            taskName.Clear();

            string taskSubs = _IsNotSubStack == true ? "SimpleStack" : "SubStacks";

            taskName.Append("Work");
            taskName.Append(StackTasks.Count + 1);
            taskName.Append(".- " + taskSubs + "_");
            taskName.Append(ActualTaskIndicatorCodeValue.Name);

        }
        private void AddTask()
        {
            if (IsJustStackSubs) // Just Stack SubStaks
            {
                NamingTask();
                taskName.Append("--Stack Slabs--");
                StackTasks.Add(new StackTask
                {
                    TaskName = taskName.ToString(),
                    TaskNumber = StackTasks.Count,
                    OutputImageDispositionCodeValue = ActualOutputImageDispositionCodeValue.Value,
                    //OutputImagesDesignatedFolder = ServiceProvider.Settings.DefaultSession.Name + "\\SubStacksOutput_" + StackTasks.Count.ToString(),
                    SourceFolder = StacksFolder,
                    OutputImagesDesignatedFolder = SubStacksFolder,
                    TaskIndicatorCodeValue = ActualTaskIndicatorCodeValueSlab.Value,
                    Substack = false,
                    FileType = FileType,
                    Number = Stack_items,
                    Overlap = Stack_overlap,
                    EstimatedRadius = EstimatedRadius,
                    SmoothingRadius = SmoothingRadius,
                    ContrastThreshold = ContrastThreshold
                });
            }

            else
            {
                if (IsSubStack) 
                {
                    if (IsStackSubs) // Make SubStacks + Process SubStacks
                    {
                        NamingTask();
                        StackTasks.Add(new StackTask
                        {
                            TaskName = taskName.ToString(),
                            TaskNumber = StackTasks.Count,
                            OutputImageDispositionCodeValue = ActualOutputImageDispositionCodeValue.Value,
                            SourceFolder = "%CurrentProject%",
                            OutputImagesDesignatedFolder = StacksFolder,
                            TaskIndicatorCodeValue = ActualTaskIndicatorCodeValue.Value,
                            Substack = true,
                            FileType = FileType,
                            Number = Stack_items,
                            Overlap = Stack_overlap,
                            EstimatedRadius = EstimatedRadius,
                            SmoothingRadius = SmoothingRadius,
                            ContrastThreshold = ContrastThreshold
                        });

                        NamingTask();
                        taskName.Append("--Stack Slabs--");
                        StackTasks.Add(new StackTask
                        {
                            TaskName = taskName.ToString(),
                            TaskNumber = StackTasks.Count,
                            OutputImageDispositionCodeValue = ActualOutputImageDispositionCodeValue.Value,
                            SourceFolder = StacksFolder,
                            OutputImagesDesignatedFolder = SubStacksFolder,
                            //OutputImagesDesignatedFolder = ServiceProvider.Settings.DefaultSession.Name + "\\SubStacksOutput_" + StackTasks.Count.ToString(),
                            TaskIndicatorCodeValue = ActualTaskIndicatorCodeValueSlab.Value,
                            Substack = false,
                            FileType = FileType,
                            Number = Stack_items,
                            Overlap = Stack_overlap,
                            EstimatedRadius = EstimatedRadius,
                            SmoothingRadius = SmoothingRadius,
                            ContrastThreshold = ContrastThreshold
                        });
                    }
                    else // Just Make SubStacks
                    {
                        NamingTask();
                        StackTasks.Add(new StackTask
                        {
                            TaskName = taskName.ToString(),
                            TaskNumber = StackTasks.Count,
                            OutputImageDispositionCodeValue = ActualOutputImageDispositionCodeValue.Value,
                            SourceFolder = "%CurrentProject%",
                            OutputImagesDesignatedFolder = StacksFolder,
                            TaskIndicatorCodeValue = ActualTaskIndicatorCodeValue.Value,
                            Substack = true,
                            FileType = FileType,
                            Number = Stack_items,
                            Overlap = Stack_overlap,
                            EstimatedRadius = EstimatedRadius,
                            SmoothingRadius = SmoothingRadius,
                            ContrastThreshold = ContrastThreshold
                        });
                    }
                }
                else // Make Single Pass Stack
                {
                    NamingTask();
                    StackTasks.Add(new StackTask
                    {
                        TaskName = taskName.ToString(),
                        OutputImageDispositionCodeValue = ActualOutputImageDispositionCodeValue.Value,
                        SourceFolder = "%CurrentProject%",
                        OutputImagesDesignatedFolder = SingleFolder,
                        TaskIndicatorCodeValue = ActualTaskIndicatorCodeValue.Value,
                        Substack = false,
                        FileType = FileType,
                        Number = Stack_items,
                        Overlap = Stack_overlap,
                        EstimatedRadius = EstimatedRadius,
                        SmoothingRadius = SmoothingRadius,
                        ContrastThreshold = ContrastThreshold
                    });
                }
            }                                 
        }
        private void DeleteTask()
        {
            StackTasks.Remove(ActualStackTask);
        }
        private void MoveUpTask()
        {
            int pos = StackTasks.IndexOf(ActualStackTask);
            if (pos != 0) StackTasks.Move(pos, --pos);
        }
        private void MoveDownTask()
        {
            int pos = StackTasks.IndexOf(ActualStackTask);
            if (pos != StackTasks.Count -1) StackTasks.Move(pos, ++pos);
        }

        #endregion


        #region Folders

        private void SetProjetFolder()
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (ProjetFolder != null | ProjetFolder != "")
                {
                    ProjetFolder = ServiceProvider.Settings.DefaultSession.Folder + "\\ZereneProjet";
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


        #region Make Zerene Batch XML File

        private void PopulateCombos()
        {
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align & Stack All (PMax)", Value = 1, Name = "Ali&PMax" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align & Stack All (DMap)", Value = 2, Name = "Ali&DMap" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align All Frames", Value = 3, Name = "Ali" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (PMax)", Value = 4, Name = "SelecPMax" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (DMap)", Value = 5, Name = "SelecDMap" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Make Stereo Pair(s)", Value = 6, Name = "Stereo" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Mark Project as Demo", Value = 7, Name = "Demo" });

            TaskIndicatorCodesSubs.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (PMax)", Value = 4, Name = "SelecPMax" });
            TaskIndicatorCodesSubs.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (DMap)", Value = 5, Name = "SelecDMap" });
            TaskIndicatorCodesSubs.Add(new TaskIndicatorCodeValues { Text = "Make Stereo Pair(s)", Value = 6, Name = "Stereo" });

            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Do not save images separately (keep only in projects)", Value = 1 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in source folders", Value = 2 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in project folders (as SavedImages/*)", Value = 3 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in designated folder (as sourcename_...)", Value = 4 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in designated folder (with no prefix)", Value = 5 });

            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "New projects are temporary (do not save)", Value = 101 });
            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "Save new projects in source folders", Value = 102 });
            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "Save new projects in designated folder", Value = 103 });
        }
        private void PreferenceAdd(string propertyName, string subpropertyName, string subpropertyValue)
        {
            Props.Add(new Prop { PropertyName = propertyName, SubpropertyName = subpropertyName, Subpropertyvalue = subpropertyValue });
        }
        private void PreferenceWrite(XmlTextWriter writer)
        {
            writer.WriteStartElement("Preferences");
            if (Props != null)
            {
                foreach (var property in Props)
                {
                    writer.WriteStartElement(property.PropertyName + "." + property.SubpropertyName);
                    writer.WriteAttributeString("value", property.Subpropertyvalue);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        private void MakeZereneBatchFile()
        {
            //OutputImagesDesignatedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SubStacks";
            //OutputImagesDesignatedFolder = ServiceProvider.Settings.DefaultSession.Name + "\\SubStacks";
            //OutputImagesDesignatedFolder = Environment.GetEnvironmentVariable("userprofile") + "\\SubStacks";
            Items = Files.Count();

            XmlTextWriter writer = new XmlTextWriter(_tempdir + "\\ZereneBatch.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(false);
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("ZereneStackerBatchScript");
            writer.WriteStartElement("WrittenBy");
            writer.WriteAttributeString("value", "Zerene Stacker 1.04 Build T201412212230");
            writer.WriteEndElement();

            writer.WriteStartElement("BatchQueue");
            writer.WriteStartElement("Batches");
            writer.WriteAttributeString("length", StackTasks.Count.ToString());
            foreach (var stackTask in StackTasks)
            {
                writer.WriteStartElement("Batch");
                    writer.WriteStartElement("Sources");
                    writer.WriteAttributeString("length", "1");
                        writer.WriteStartElement("Source");
                        //writer.WriteAttributeString("value", "%CurrentProject%");
                        writer.WriteAttributeString("value", stackTask.SourceFolder);
                        writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("ProjectDispositionCode");
                    writer.WriteAttributeString("value", stackTask.ProjectDispositionCodeValue.ToString()); 
                    writer.WriteEndElement();

                    if (stackTask.ProjectDispositionCodeValue == 103)
                    {
                        writer.WriteStartElement("ProjectsDesignatedFolder");
                        writer.WriteAttributeString("value", stackTask.ProjetFolder); 
                        writer.WriteEndElement();
                    }

                    PopulatePreferences(stackTask.ContrastThreshold, stackTask.EstimatedRadius, stackTask.SmoothingRadius, stackTask.FileType);
                    GenericTask(writer, stackTask.OutputImageDispositionCodeValue, stackTask.OutputImagesDesignatedFolder, stackTask.TaskIndicatorCodeValue, stackTask.Substack, stackTask.Number, stackTask.Overlap);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            MessageBox.Show("XML File created ! ");
        }
        private void PopulatePreferences(int contrastThreshold, int estimatedRadius, int smoothingRadius, string fileType)
        {
            PreferenceAdd("AlignmentControl", "AlignmentSettingsChanged", "false");
            PreferenceAdd("AlignmentControl", "AllowRotation", "true");
            PreferenceAdd("AlignmentControl", "AllowScale", "true");
            PreferenceAdd("AlignmentControl", "AllowShiftX", "true");
            PreferenceAdd("AlignmentControl", "AllowShiftY", "true");
            PreferenceAdd("AlignmentControl", "BrightnessSettingsChanged", "false");
            PreferenceAdd("AlignmentControl", "CorrectBrightness", "true");
            PreferenceAdd("AlignmentControl", "MaxRelDegRotation", "20");
            PreferenceAdd("AlignmentControl", "MaxRelPctScale", "20");
            PreferenceAdd("AlignmentControl", "MaxRelPctShiftX", "20");
            PreferenceAdd("AlignmentControl", "MaxRelPctShiftY", "20");
            PreferenceAdd("AlignmentControl", "Order.Automatic", "false");
            PreferenceAdd("AlignmentControl", "Order.NarrowFirst", "true");

            PreferenceAdd("AllowReporting", "UsageStatistics", "false");

            PreferenceAdd("BatchFileChooser", "LastDirectory", "");

            PreferenceAdd("ColorManagement", "DebugPrintProfile", "false");
            PreferenceAdd("ColorManagement", "InputOption", "Use_EXIF_and_DCF_rul");
            PreferenceAdd("ColorManagement", "InputOption.AssumedProfile", "sRGB IEC61966-2.1");
            PreferenceAdd("ColorManagement", "OutputOption", "CopyInput");

            PreferenceAdd("DepthMapControl", "AlgorithmIdentifier", "1");
            PreferenceAdd("DepthMapControl", "ContrastThresholdLevel", "0.0");
            PreferenceAdd("DepthMapControl", "ContrastThresholdPercentile", contrastThreshold.ToString()); // Contrast Threshold
            PreferenceAdd("DepthMapControl", "EstimationRadius", estimatedRadius.ToString()); //Estimation Radius
            PreferenceAdd("DepthMapControl", "SaveDepthMapImage", "");
            PreferenceAdd("DepthMapControl", "SaveDepthMapImageDirector", "");
            PreferenceAdd("DepthMapControl", "SaveUsedPixelImages", "false");
            PreferenceAdd("DepthMapControl", "SmoothingRadius", smoothingRadius.ToString());  //Smoothing Radius
            PreferenceAdd("DepthMapControl", "UseFixedContrastThresholdLevel", "true");
            PreferenceAdd("DepthMapControl", "UseFixedContrastThresholdPercentile", "true");
            PreferenceAdd("DepthMapControl", "UsedPixelFractionThreshold", "0.5");

            PreferenceAdd("FileIO", "UseExternalTIFFReader", "false");

            PreferenceAdd("Interpolator", "RenderingSelection", "Interpolator.Spline4x4");
            PreferenceAdd("Interpolator", "ShowAdvanced", "false");

            PreferenceAdd("OutputImageNaming", "Template", "Slab {outseq} {method}");

            PreferenceAdd("Prerotation", "Degrees", "0");
            PreferenceAdd("Prerotation", "Selected", "false");

            PreferenceAdd("Presize", "UserSetting.Scale", "1.0");
            PreferenceAdd("Presize", "UserSetting.Selected", "false");
            PreferenceAdd("Presize", "Working.Scale", "1.0");

            PreferenceAdd("PyramidControl", "GritSuppressionMethod", "1");
            PreferenceAdd("PyramidControl", "RetainUDRImage", "false");

            PreferenceAdd("SaveImage", "BitsPerColor", "16");
            PreferenceAdd("SaveImage", "CompressionQuality", "0.75");

            PreferenceAdd("SaveImage", "FileType", fileType); //Output File Type
            PreferenceAdd("SaveImage", "RescaleImageToAvoidOverflow", "true");

            PreferenceAdd("SkewSequence", "FirstImage.MaximumShiftXPct", "-3.0");
            PreferenceAdd("SkewSequence", "FirstImage.MaximumShiftYPct", "0.0");
            PreferenceAdd("SkewSequence", "LastImage.MaximumShiftXPct", "3.0");
            PreferenceAdd("SkewSequence", "LastImage.MaximumShiftYPct", "0.0");
            PreferenceAdd("SkewSequence", "NumberOfOutputImages", "3");
            PreferenceAdd("SkewSequence", "Selected", "false");

            PreferenceAdd("StackingControl", "FrameSkipFactor", "1");
            PreferenceAdd("StackingControl", "FrameSkipSelected", "false");
            PreferenceAdd("StereoOrdering", "LeftRightIndexSeparation", "1");
        }
        private void GenericTask(XmlTextWriter writer, int outputImageDispositionCodeValue, string outputImagesDesignatedFolder, int taskIndicatorCodeValue, bool substack, int number, int overlap)
        {

            Stack_item = 0;

            //Tasknumber = !substack ? 1 : (Items / (number - overlap));

            if (!substack)
            {
                Tasknumber = 1;
            }
            else
            {
                Tasknumber = Items / (number - overlap);
            }

            writer.WriteStartElement("Tasks");
            writer.WriteAttributeString("length", Tasknumber.ToString());

            for (int j = 0; j < Tasknumber; j++)
            {
                writer.WriteStartElement("Task");
                writer.WriteStartElement("OutputImageDispositionCode");
                writer.WriteAttributeString("value", outputImageDispositionCodeValue.ToString());
                writer.WriteEndElement();

                if (outputImageDispositionCodeValue == 5 | outputImageDispositionCodeValue  == 4)
                {
                    writer.WriteStartElement("OutputImagesDesignatedFolder");
                    writer.WriteAttributeString("value", outputImagesDesignatedFolder);
                    writer.WriteEndElement();
                }

                PreferenceWrite(writer);

                writer.WriteStartElement("TaskIndicatorCode");
                writer.WriteAttributeString("value", taskIndicatorCodeValue.ToString());
                writer.WriteEndElement();
                if (substack == true)
                {
                    writer.WriteStartElement("SelectedInputIndices");
                    writer.WriteAttributeString("length", number.ToString());
                    for (int i = 0; i < number; i++)
                    {
                        if (Stack_item == Items)
                        {
                            break;
                        }

                        writer.WriteStartElement("SelectedInputIndex");
                        writer.WriteAttributeString("value", Stack_item.ToString());
                        writer.WriteEndElement();
                        Stack_item++;
                        //if (Stack_item == Items) return;
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                Stack_item -= overlap;
            }
            writer.WriteEndElement();


        }

        #endregion


        public ZereneBatchViewModel()
        {
            Output = new AsyncObservableCollection<string>();
            InitCommands();
            LoadZereneData();
            PopulateCombos();
            CreateTempDir();
            MakeZereneLaunchCommand();
            ServiceProvider.FileTransfered += ServiceProvider_FileTransfered;
            ServiceProvider.WindowsManager.Event += WindowsManager_Event;
            UpDateDMapCommand = new RelayCommand(UpDateDMap);
            UpDateDMap2Command = new RelayCommand(UpDateDMap2);
            AddTaskCommand = new RelayCommand(AddTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask);
            MoveUpTaskCommand = new RelayCommand(MoveUpTask);
            MoveDownTaskCommand = new RelayCommand(MoveDownTask);
            MakeBatchCommand = new RelayCommand(MakeZereneBatchFile);
            ReloadCommand = new RelayCommand(LoadZereneData);
            PreviewCommand = new RelayCommand(Preview);
            GenerateCommand = new RelayCommand(Generate);
            StopCommand = new RelayCommand(Stop);
            SelectAllCommand = new CameraControl.Core.Classes.RelayCommand<object>(delegate { ServiceProvider.Settings.DefaultSession.SelectAll(); });
            SelectNoneCommand = new CameraControl.Core.Classes.RelayCommand<object>(delegate { ServiceProvider.Settings.DefaultSession.SelectNone(); });            
        }



        private void CreateTempDir()
        {            
            _tempdir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempdir);
            OnProgressChange("Temporal Folder: " + _tempdir);

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
        
        
        
        private void newprocess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnProgressChange(e.Data);
        }
        public void UpDateDMap()
        {
            if (ActualTaskIndicatorCodeValue.Value == 2 || ActualTaskIndicatorCodeValue.Value == 5)
            {
                IsDMap = true;
            }
            else
                IsDMap = false;
        }
        public void UpDateDMap2()
        {
            if (ActualTaskIndicatorCodeValueSlab.Value == 2 || ActualTaskIndicatorCodeValueSlab.Value == 5)
            {
                IsDMap2 = true;
            }
            else
                IsDMap2 = false;
        }
        public void UpDateIsNotProjetFolderFixed()
        {
            if (ActualProjectDispositionCodeValue.Value == 103)
            {
                IsNotProjetFolderFixed = true;
            }
            else
                IsNotProjetFolderFixed = false;
        }        
        public void LoadZereneData()
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
        

        public void CopyFiles(bool preview)
        {
            int counter = 0;
            try
            {
                _filenames.Clear();
                
                OnProgressChange("Copying files");
                
                foreach (FileItem fileItem in Files)
                {
                    string randomFile = Path.Combine(_tempdir, "image_" + counter.ToString("0000") + ".jpg");
                    OnProgressChange("Copying file " + fileItem.Name);
                    string source = preview ? fileItem.LargeThumb : fileItem.FileName;

                    AutoExportPluginHelper.ExecuteTransformPlugins(fileItem, PluginSetting.AutoExportPluginConfig, source, randomFile);

                    _filenames.Add(randomFile);
                    counter++;
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
        private async void MakeZereneLaunchCommand()
        {
            string userName = Environment.UserName;
            string mLaunchCmdDir = "C:\\Users\\" + userName + "\\AppData\\Roaming\\ZereneStacker";
            string fileName = "zerenstk.launchcmd";
            string mLaunchCmdFile = Path.Combine(mLaunchCmdDir, fileName);
            launchCommand = null;

            try
            {
                using (StreamReader sr = new StreamReader(mLaunchCmdFile))
                {
                    String line = await sr.ReadToEndAsync();
                    launchCommand = line;
                }
            }
            catch (Exception ex)
            {
                OnProgressChange("Could not read the Zerene Command File");
            }

            try
            {
                ZereneCommand = @"C:\Program Files\ZereneStacker\jre\bin\java.exe";
                OnProgressChange("Zerene Command: " + ZereneCommand);

                launchCommand = launchCommand.Replace(@"""C:\Program Files\ZereneStacker\jre\bin\javaw.exe""", "");  // to make SHOWPROGRESS lines available 
                launchCommand += " -noSplashScreen";
                launchCommand += " -exitOnBatchScriptCompletion";
                launchCommand += " -runMinimized";
                launchCommand += " ";
                launchCommand += _tempdir;

                OnProgressChange("Launch Command: " + launchCommand);
            }
            catch (Exception ex)
            {
                OnProgressChange("Could not change Zerene Command File");
            }
        }

        

        

        private void Stop()
        {
            _shouldStop = true;
            try
            {
                if (_ZereneProcess != null)
                    _ZereneProcess.Kill();
            }
            catch (Exception)
            {

            }
        }
        private void Preview()
        {
            Output.Clear();
            _shouldStop = false;
            Task.Factory.StartNew(PreviewTask);
        }
        private void Generate()
        {
            Init();
            Task.Factory.StartNew(GenerateTask);
        }
        public void Init()
        {
            Output.Clear();
            _shouldStop = false;

        }

        public void ZereneStack()
        {
            try
            {
                OnProgressChange("Zerene is stacking images ..");
                OnProgressChange("This may take few minutes too");

                _resulfile = Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(Files[0].FileName) + Files.Count + ".jpg");
                _resulfile = Path.Combine(Path.GetTempPath(), Path.GetFileName(_resulfile));


                Process newprocess = new Process();
                newprocess.StartInfo = new ProcessStartInfo()
                {
                    FileName = ZereneCommand,
                    Arguments = launchCommand,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(_filenames[0])
                };
                newprocess.Start();
                _ZereneProcess = newprocess;
                newprocess.OutputDataReceived += newprocess_OutputDataReceived;
                newprocess.ErrorDataReceived += newprocess_OutputDataReceived;
                newprocess.BeginOutputReadLine();
                newprocess.BeginErrorReadLine();
                newprocess.WaitForExit();
                if (File.Exists(_resulfile))
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
                _ZereneProcess = null;
            }
            catch (Exception exception)
            {
                OnProgressChange("Error copy files " + exception.Message);
                Log.Error("Error copy files ", exception);
                _shouldStop = true;
            }
        }        
        private void GenerateTask()
        {
            IsBusy = true;
            CopyFiles(false);
            if (!_shouldStop)
                MakeZereneBatchFile();
                ZereneStack();
            if (File.Exists(_resulfile))
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
            }
            OnActionDone();
            IsBusy = false;
        }       
        void PreviewTask()
        {
            IsBusy = true;
            CopyFiles(true);
            if (!_shouldStop)
                MakeZereneBatchFile();
                ZereneStack();
            OnActionDone();
            IsBusy = false;
        }


        #region Specific Tasks --- OLD ---

        private void StackTaskWrite(XmlTextWriter writer, OutputImageDispositionCodeValues outputImageDispositionCode, string outputImagesDesignatedFolder, TaskIndicatorCodeValues taskIndicatorCode, int number, bool Is_substack)
        {
            int item = 0;
            _Items = _filenames.Count;


            for (int i = 0; i < _Items; i++)

                writer.WriteStartElement("Task");
            writer.WriteStartElement("OutputImageDispositionCode");
            writer.WriteAttributeString("value", outputImageDispositionCode.Value.ToString());
            writer.WriteEndElement();

            if (outputImageDispositionCode.Value == 5)
            {
                writer.WriteStartElement("OutputImagesDesignatedFolder");
                writer.WriteAttributeString("value", outputImagesDesignatedFolder);
                writer.WriteEndElement();
            }

            PreferenceWrite(writer);

            writer.WriteStartElement("TaskIndicatorCode");
            writer.WriteAttributeString("value", taskIndicatorCode.Value.ToString());
            writer.WriteEndElement();
            if (Is_substack == true)
            {
                writer.WriteStartElement("SelectedInputIndices");
                writer.WriteAttributeString("length", number.ToString());
                for (int j = 0; j < number; j++)
                {
                    writer.WriteStartElement("SelectedInputIndex");
                    writer.WriteAttributeString("value", item.ToString());
                    writer.WriteEndElement();
                    item++;

                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void AlignTask(XmlTextWriter writer)
        {
            writer.WriteStartElement("Task");
            writer.WriteStartElement("OutputImageDispositionCode");
            writer.WriteAttributeString("value", "3");
            writer.WriteEndElement();
            //writer.WriteStartElement("Preferences");
            //writer.WriteEndElement();
            PreferenceWrite(writer);
            writer.WriteStartElement("TaskIndicatorCode");
            writer.WriteAttributeString("value", "3");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void PMaxTask(XmlTextWriter writer)
        {
            writer.WriteStartElement("Task");
            writer.WriteStartElement("OutputImageDispositionCode");
            writer.WriteAttributeString("value", "3");
            writer.WriteEndElement();
            //writer.WriteStartElement("Preferences");
            //writer.WriteEndElement();
            PreferenceWrite(writer);
            writer.WriteStartElement("TaskIndicatorCode");
            writer.WriteAttributeString("value", "1");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void DMapTask(XmlTextWriter writer)
        {
            writer.WriteStartElement("Task");
            writer.WriteStartElement("OutputImageDispositionCode");
            writer.WriteAttributeString("value", "3");
            writer.WriteEndElement();
            //writer.WriteStartElement("Preferences");
            //writer.WriteEndElement();
            PreferenceWrite(writer);
            writer.WriteStartElement("TaskIndicatorCode");
            writer.WriteAttributeString("value", "2");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }               

        private void TaskWrite2(XmlTextWriter writer)
        {
            writer.WriteStartElement("Task");
            if (StackTasks != null)
            {
                foreach (var stacktask in StackTasks)
                {
                    //writer.WriteStartElement(stacktask.PropertyName + "." + property.SubpropertyName);
                    //writer.WriteAttributeString("value", property.Subpropertyvalue);
                    //writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        #endregion

    }
}
