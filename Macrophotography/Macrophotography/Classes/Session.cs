using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraControl.Core.Classes;
using Microsoft.Win32;

namespace Macrophotography.Classes
{
    public class Session : PhotoSession
    {

        private const string _keyBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

        /// <summary>
        /// This method is to get the path for an application.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetPathForExe(string fileName)
        {
            RegistryKey localMachine = Registry.LocalMachine;
            object result = null;
            using (RegistryKey fileKey = localMachine.OpenSubKey(string.Format(@"{0}\{1}", _keyBase, fileName)))
            {

                if (fileKey != null)
                    result = fileKey.GetValue(string.Empty);
                else
                    result = string.Empty;
            }

            return (string)result;
        }

        
        /// <summary>
        /// This method uses the System.Diagnostics.ProcessStartInfo to import selected pictures into Zerene Stacker
        /// </summary>
        public void OpenInZerene()
        {

            System.Diagnostics.ProcessStartInfo Zn = new System.Diagnostics.ProcessStartInfo();
            string path = GetPathForExe("zerenstk.exe");
            bool noneSelected = false;
            Zn.FileName = path;

            //This is slightly faster than Foreach!
            for (int i = 0; i < Files.Count; i++)
                if (Files[i].IsChecked)
                    Zn.Arguments = Path.Combine(Folder, Files[i].Name) + " " + Zn.Arguments;

            noneSelected = string.IsNullOrEmpty(Zn.Arguments);

            if (noneSelected)
            {
                //TODO: fix this for later.
                System.Windows.MessageBox.Show("Please Select an image first");
                return;
            }

            System.Diagnostics.Process.Start(Zn);
        }



    }
}
