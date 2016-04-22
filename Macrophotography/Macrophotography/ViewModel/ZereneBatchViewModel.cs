﻿using CameraControl.Core;
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

namespace Macrophotography.ViewModel
{
    public class ZereneBatchViewModel : MacroStackViewModel
    {
                       
        #region Variables
        private ObservableCollection<Prop> _Props = new ObservableCollection<Prop>();
        private ObservableCollection<StackTask> _StackTasks = new ObservableCollection<StackTask>();
        private ObservableCollection<TaskIndicatorCodeValues> _TaskIndicatorCodes = new ObservableCollection<TaskIndicatorCodeValues>(); // populate combos with
        private ObservableCollection<OutputImageDispositionCodeValues> _OutputImageDispositionCodes = new ObservableCollection<OutputImageDispositionCodeValues>(); // populate combos with
        private ObservableCollection<ProjectDispositionCodeValues> _ProjectDispositionCodes = new ObservableCollection<ProjectDispositionCodeValues>(); // populate combos with

        private TaskIndicatorCodeValues _ActualTaskIndicatorCodeValue = new TaskIndicatorCodeValues();
        private TaskIndicatorCodeValues _ActualTaskIndicatorCodeValueSlab = new TaskIndicatorCodeValues();
        private OutputImageDispositionCodeValues _ActualOutputImageDispositionCodeValue = new OutputImageDispositionCodeValues();
        private ProjectDispositionCodeValues _ActualProjectDispositionCodeValue = new ProjectDispositionCodeValues();
        private StackTask _ActualStackTask = new StackTask();

        private PluginSetting _pluginSetting;
        
        private int _EstimatedRadius;
        private int _SmoothingRadius;
        private int _ContrastThreshold;

        private bool _IsDMap;
        private bool _IsSubStack;
        private bool _IsNotSubStack = true;
        private bool _IsStackSlabs;
        private bool _IsJpeg;
        private bool _IsTiff = true;
        private bool _IsSlabbing;


        private string _OutputImageNames;
        private string _OutputImagesDesignatedFolder;
        private string _OutputSessionFolder;
        private string _OutputSessionSlabsFolder;
        private string _SubSlabsSessionFolder;

        private StringBuilder _taskName = new StringBuilder();

        private int _Items;
        private int _Stack_items;       
        private int _Stack_overlap;
        

        public int stack_item = 0;
        public int tasknumber;

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

        public bool UseSmallThumb
        {
            get { return PluginSetting.GetBool("UseSmallThumb"); }
            set
            {
                PluginSetting["UseSmallThumb"] = value;
                RaisePropertyChanged(() => UseSmallThumb);
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
        public bool IsStackSlabs
        {
            get { return _IsStackSlabs; }
            set
            {
                _IsStackSlabs = value;
                RaisePropertyChanged(() => IsStackSlabs);
                RaisePropertyChanged(() => IsNotStackSlabs);
            }
        }
        public bool IsNotStackSlabs
        {
            get { return !IsStackSlabs; }
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
            }
        }
        public bool IsSlabbing
        {
            get { return _IsSlabbing; }
            set
            {
                _IsSlabbing = value;
                RaisePropertyChanged(() => IsSlabbing);
            }
        }

        public string OutputImageNames
        {
            get { return _OutputImageNames; }
            set
            {
                _OutputImageNames = value;
                RaisePropertyChanged(() => OutputImageNames);
            }
        }
        public string OutputImagesDesignatedFolder
        {
            get { return _OutputImagesDesignatedFolder; }
            set
            {
                _OutputImagesDesignatedFolder = value;
                RaisePropertyChanged(() => OutputImagesDesignatedFolder);
            }
        }
        public string OutputSessionFolder
        {
            get { return _OutputSessionFolder; }
            set
            {
                _OutputSessionFolder = value;
                RaisePropertyChanged(() => OutputSessionFolder);
            }
        }
        public string OutputSessionSlabsFolder
        {
            get { return _OutputSessionSlabsFolder; }
            set
            {
                _OutputSessionSlabsFolder = value;
                RaisePropertyChanged(() => OutputSessionSlabsFolder);
            }
        }
        public string SubSlabsSessionFolder
        {
            get { return _SubSlabsSessionFolder; }
            set
            {
                _SubSlabsSessionFolder = value;
                RaisePropertyChanged(() => SubSlabsSessionFolder);
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

        public int Stack_items
        {
            get { return _Stack_items; }
            set
            {
                _Stack_items = value;
                RaisePropertyChanged(() => Stack_items);
            }
        }
        public int Stack_overlap
        {
            get { return _Stack_overlap; }
            set
            {
                _Stack_overlap = value;
                RaisePropertyChanged(() => Stack_overlap);
            }
        }
        public int Items
        {
            get { return _Items; }
            set
            {
                _Items = value;
                RaisePropertyChanged(() => Items);
            }
        }

        #endregion


        #region Classes
        public class Prop
        {
            public string PropertyName { get; set; }
            public string SubpropertyName { get; set; }
            public bool substack { get; set; }
            public string Subpropertyvalue { get; set; }
        }
        public class StackTask
        {
            public string TaskName { get; set; }
            public int OutputImageDispositionCodeValue { get; set; }
            public int TaskIndicatorCodeValue { get; set; }
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
        public RelayCommand AddTaskCommand { get; set; }

        #endregion

        public ZereneBatchViewModel()
        {
            InitCommands();
            LoadData();
            PopulateCombos();
            UpDateDMapCommand = new RelayCommand(UpDateDMap);
            AddTaskCommand = new RelayCommand(AddTask);
        }

        public void PopulateCombos()
        {
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align & Stack All (PMax)", Value = 1, Name = "Ali&PMax" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align & Stack All (DMap)", Value = 2, Name = "Ali&DMap" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align All Frames", Value = 3, Name = "Ali" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (PMax)", Value = 4, Name = "SelecPMax" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (DMap)", Value = 5, Name = "SelecDMap" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Make Stereo Pair(s)", Value = 6, Name = "Stereo" });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Mark Project as Demo", Value = 7, Name = "Demo" });

            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Do not save images separately (keep only in projects)", Value = 1 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in source folders", Value = 2 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in project folders (as SavedImages/*)", Value = 3 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in designated folder (as sourcename_...)", Value = 4 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in designated folder (with no prefix)", Value = 5 });

            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "New projects are temporary (do not save)", Value = 101 });
            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "Save new projects in source folders", Value = 102 });
            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "Save new projects in designated folder", Value = 103 });

        }

        public void UpDateDMap()
        {
            if (ActualTaskIndicatorCodeValue.Value == 2 || ActualTaskIndicatorCodeValue.Value == 5 || ActualTaskIndicatorCodeValueSlab.Value == 2 || ActualTaskIndicatorCodeValueSlab.Value == 5)
            {
                IsDMap = true;
            }
            else
                IsDMap = false;
        }

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
            NamingTask();
            StackTasks.Add(new StackTask
            {
                TaskName = taskName.ToString(),
                OutputImageDispositionCodeValue = ActualOutputImageDispositionCodeValue.Value,
                TaskIndicatorCodeValue = ActualTaskIndicatorCodeValue.Value,
                Number = Stack_items,
                Overlap = Stack_overlap,
                EstimatedRadius = EstimatedRadius,
                SmoothingRadius = SmoothingRadius,
                ContrastThreshold = ContrastThreshold
            });
        }


        public void CopyFiles(bool preview)
        {
            int counter = 0;
            try
            {
                _filenames.Clear();
                OnProgressChange("Copying files");
                _tempdir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(_tempdir);
                foreach (FileItem fileItem in Files)
                {
                    string randomFile = Path.Combine(_tempdir, "image_" + counter.ToString("0000") + ".jpg");
                    OnProgressChange("Copying file " + fileItem.Name);
                    string source = preview
                        ? (UseSmallThumb ? fileItem.SmallThumb : fileItem.LargeThumb)
                        : fileItem.FileName;

                    AutoExportPluginHelper.ExecuteTransformPlugins(fileItem, PluginSetting.AutoExportPluginConfig,
                        source, randomFile);

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
        /*
        private void GenerateTask()
        {
            IsBusy = true;
            CopyFiles(false);
            if (!_shouldStop)
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
                ZereneStack();
            OnActionDone();
            IsBusy = false;
        }
        
        public void Combine()
        {
            try
            {
                OnProgressChange("Enfuse images ..");
                OnProgressChange("This may take few minutes too");
                _resulfile = Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(Files[0].FileName) + Files.Count + ".jpg");
                _resulfile = Path.Combine(Path.GetTempPath(), Path.GetFileName(_resulfile));
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("\"" + Path.GetDirectoryName(_filenames[0]) + "\" ");
                stringBuilder.Append("\"" + Macro + "\" ");
                stringBuilder.Append(_resulfile + " ");
                stringBuilder.Append("-q -k +j100");

                Process newprocess = new Process();
                newprocess.StartInfo = new ProcessStartInfo()
                {
                    FileName = _pathtoenfuse,
                    Arguments = stringBuilder.ToString().Replace(",", "."),
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(_filenames[0])
                };
                newprocess.Start();
                _enfuseProcess = newprocess;
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
                _enfuseProcess = null;
            }
            catch (Exception exception)
            {
                OnProgressChange("Error copy files " + exception.Message);
                Log.Error("Error copy files ", exception);
                _shouldStop = true;
            }
        }

        public void ZereneStack()
        {
            try
            {
                OnProgressChange("Zerene is stacking images ..");
                OnProgressChange("This may take few minutes too");
                _resulfile = Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(Files[0].FileName) + Files.Count + ".jpg");
                _resulfile = Path.Combine(Path.GetTempPath(), Path.GetFileName(_resulfile));
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("\"" + Path.GetDirectoryName(_filenames[0]) + "\" ");
                stringBuilder.Append("\"" + Macro + "\" ");
                stringBuilder.Append(_resulfile + " ");
                stringBuilder.Append("-q -k +j100");

                Process newprocess = new Process();
                newprocess.StartInfo = new ProcessStartInfo()
                {
                    FileName = _pathtoenfuse,
                    Arguments = stringBuilder.ToString().Replace(",", "."),
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(_filenames[0])
                };
                newprocess.Start();
                _enfuseProcess = newprocess;
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
                _enfuseProcess = null;
            }
            catch (Exception exception)
            {
                OnProgressChange("Error copy files " + exception.Message);
                Log.Error("Error copy files ", exception);
                _shouldStop = true;
            }
        }
        */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OutputImagesDesignatedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SubStacks";
            //OutputImagesDesignatedFolder = Environment.GetEnvironmentVariable("userprofile") + "\\SubStacks";


            tasknumber = 3;
            if (StackTasks.Count != 0) { tasknumber += StackTasks.Count; }

            XmlTextWriter writer = new XmlTextWriter("ZereneBatch.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(false);
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("ZereneStackerBatchScript");
            writer.WriteStartElement("WrittenBy");
            writer.WriteAttributeString("value", "Zerene Stacker 1.04 Build T201412212230");
            writer.WriteEndElement();

            writer.WriteStartElement("BatchQueue");
            writer.WriteStartElement("Batches");
            writer.WriteAttributeString("length", "1");
            writer.WriteStartElement("Batch");
            writer.WriteStartElement("Sources");
            writer.WriteAttributeString("length", "1");
            writer.WriteStartElement("Source");
            writer.WriteAttributeString("value", "%CurrentProject%");
            writer.WriteEndElement();
            writer.WriteEndElement();


            writer.WriteStartElement("ProjectDispositionCode");
            writer.WriteAttributeString("value", "102");
            writer.WriteEndElement();

            writer.WriteStartElement("Tasks");
            writer.WriteAttributeString("length", tasknumber.ToString());
            AlignTask(writer);
            PMaxTask(writer);
            DMapTask(writer);
            foreach (var stackTask in StackTasks)
            {
                //writer.WriteStartElement(property.PropertyName + "." + property.SubpropertyName);
                //writer.WriteAttributeString("value", property.Subpropertyvalue);
                //writer.WriteEndElement();
                GenericTask(writer, stackTask.OutputImageDispositionCodeValue, stackTask.TaskIndicatorCodeValue, true, stackTask.Number, stackTask.Overlap);
            }

            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            MessageBox.Show("XML File created ! ");
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

        private void GenericTask(XmlTextWriter writer, int outputImageDispositionCodeValue, int taskIndicatorCodeValue, bool substack, int number, int overlap)
        {


            writer.WriteStartElement("Task");
            writer.WriteStartElement("OutputImageDispositionCode");
            writer.WriteAttributeString("value", outputImageDispositionCodeValue.ToString());
            writer.WriteEndElement();

            if (outputImageDispositionCodeValue == 5)
            {
                writer.WriteStartElement("OutputImagesDesignatedFolder");
                writer.WriteAttributeString("value", OutputImagesDesignatedFolder);
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
                    writer.WriteStartElement("SelectedInputIndex");
                    writer.WriteAttributeString("value", stack_item.ToString());
                    writer.WriteEndElement();
                    stack_item++;

                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void StackTaskWrite(XmlTextWriter writer, OutputImageDispositionCodeValues outputImageDispositionCode, TaskIndicatorCodeValues taskIndicatorCode, int number, bool Is_substack)
        {
            int item = 0;

            writer.WriteStartElement("Task");
            writer.WriteStartElement("OutputImageDispositionCode");
            writer.WriteAttributeString("value", outputImageDispositionCode.Value.ToString());
            writer.WriteEndElement();

            if (outputImageDispositionCode.Value == 5)
            {
                writer.WriteStartElement("OutputImagesDesignatedFolder");
                writer.WriteAttributeString("value", OutputImagesDesignatedFolder);
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
                for (int i = 0; i < number; i++)
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



    }
}