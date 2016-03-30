using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraControl.Core;
using CameraControl.Core.Interfaces;

namespace Macrophotography
{
    public class Plugin : IPlugin
    {
        public bool Register()
        {
            ServiceProvider.PluginManager.ToolBarPlugins.Add(new MacroPlugin());
            ServiceProvider.WindowsManager.Add(new LiveView());
            return true;
        }

        public void Init()
        {
            
        }
    }
}
