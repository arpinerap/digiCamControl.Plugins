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

namespace Macrophotography.ViewModel
{
    public class ZereneBatchViewModel : MacroStackViewModel
    {

        private ObservableCollection<Prop> _Props = new ObservableCollection<Prop>();
        private ObservableCollection<StackTask> _StackTasks = new ObservableCollection<StackTask>();
        private ObservableCollection<TaskIndicatorCodeValues> _TaskIndicatorCodes = new ObservableCollection<TaskIndicatorCodeValues>();
        private ObservableCollection<OutputImageDispositionCodeValues> _OutputImageDispositionCodes = new ObservableCollection<OutputImageDispositionCodeValues>();
        private ObservableCollection<ProjectDispositionCodeValues> _ProjectDispositionCodes = new ObservableCollection<ProjectDispositionCodeValues>();


        private string _OutputImagesDesignatedFolder;


        public int items;
        public int stack_items;
        public int stack_item;
        public int stack_overlap;
        public int tasknumber;       
        public int item = 0;

        #region RaisePropertyChanged

        public string OutputImagesDesignatedFolder
        {
            get { return _OutputImagesDesignatedFolder; }
            set
            {
                _OutputImagesDesignatedFolder = value;
                RaisePropertyChanged(() => OutputImagesDesignatedFolder);
            }
        }

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
            public int OutputImageDispositionCodeValue { get; set; }
            public int TaskIndicatorCodeValue { get; set; }
            public int Number { get; set; }
            public int Overlap { get; set; }
        }

        public class TaskIndicatorCodeValues
        {
            public String Text { get; set; }
            public int Value { get; set; }
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

        #endregion

        public ZereneBatchViewModel()
        {
            InitCommands();
            PopulateCombos();
        }

        public void PopulateCombos()
        {
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align & Stack All (PMax)", Value = 1 });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align & Stack All (DMax)", Value = 2 });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Align All Frames", Value = 3 });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (PMax)", Value = 4 });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Stack Selected (DMax)", Value = 5 });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Make Stereo Pair(s)", Value = 6 });
            TaskIndicatorCodes.Add(new TaskIndicatorCodeValues { Text = "Mark Project as Demo", Value = 7 });

            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Do not save images separately (keep only in projects)", Value = 1 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in source folders", Value = 2 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in project folders (as SavedImages/*)", Value = 3 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in designated folder (as sourcename_...)", Value = 4 });
            OutputImageDispositionCodes.Add(new OutputImageDispositionCodeValues { Text = "Save in designated folder (with no prefix)", Value = 5 });

            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "New projects are temporary (do not save)", Value = 101 });
            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "Save new projects in source folders", Value = 102 });
            ProjectDispositionCodes.Add(new ProjectDispositionCodeValues { Text = "Save new projects in designated folder", Value = 103 });

        }


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
            DMaxTask(writer);
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

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {

            StackTasks.Add(new StackTask
            {
                OutputImageDispositionCodeValue = 5,
                TaskIndicatorCodeValue = 1,
                Number = 10,
                Overlap = 2
            });

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

        private void DMaxTask(XmlTextWriter writer)
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

        private void TaskWrite(XmlTextWriter writer)
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
