using System;
using System.Collections.Generic;
using System.Text;

namespace FridaDotNet
{
    public interface IFridaArguments
    {
        string DeviceID { get; set; }
        string TargetName { get; set; }
        string ScriptPath { get; set; }
        bool IsPause { get; set; }
        bool IsNeedSpawn { get; set; }
    }

    public class FridaArguments : IFridaArguments
    {
        protected string _DeviceID = "local";
        protected string _TargetName = "";
        protected string _ScriptPath = "";
        protected bool _IsPause = true;
        protected bool _IsNeedSpawn = true;

        public string DeviceID { get => _DeviceID; set => _DeviceID=value; }
        public string TargetName { get => _TargetName; set => _TargetName=value; }
        public string ScriptPath { get => _ScriptPath; set => _ScriptPath=value; }
        public bool IsPause { get => _IsPause; set => _IsPause=value; }
        public bool IsNeedSpawn { get => _IsNeedSpawn; set => _IsNeedSpawn=value; }

        public override string ToString()
        {
            string info = $@"
DeviceID={DeviceID};
TargetName={TargetName};
ScriptPath={ScriptPath};
IsPause={IsPause};
";
            return info;
        }
    }
}
