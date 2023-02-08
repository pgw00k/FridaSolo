using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GenOcean.Common;
using FridaDotNet;

namespace FridaCmd
{
    internal class FridaCmdArgument:BaseCmdArgument
    {
        public FridaArguments ControllerArgument = new FridaArguments();
        public FridaCmdArgument(string[] args) : base(args)
        {
            OptionActions.Add("-devices", ListDevices);
            OptionActions.Add("-s", GetLocalDevice);
            OptionActions.Add("-u", GetUsbDevice);
            OptionActions.Add("-f", SetTargetName);
            OptionActions.Add("-l", SetScriptPath);
            OptionActions.Add("--no-pause", SetNoPause);
        }

        public override string ToString()
        {
            return ControllerArgument.ToString();
        }

        public virtual int ListDevices(BaseCmdArgument self, string arg, int argIndex)
        {
            SingletonFridaManager.ListDevices();
            return 0;
        }

        public virtual int GetUsbDevice(BaseCmdArgument self, string arg, int argIndex)
        {
            ControllerArgument.DeviceID = SingletonFridaManager.GetDeviceIdByType(2);
            return 0;
        }

        public virtual int GetLocalDevice(BaseCmdArgument self, string arg, int argIndex)
        {
            ControllerArgument.DeviceID = SingletonFridaManager.GetDeviceIdByType(0);
            return 0;
        }

        public virtual int SetTargetName(BaseCmdArgument self, string arg, int argIndex)
        {
            ControllerArgument.TargetName = Arguments[argIndex + 1];
            return 1;
        }

        public virtual int SetScriptPath(BaseCmdArgument self, string arg, int argIndex)
        {
            ControllerArgument.ScriptPath = Arguments[argIndex + 1];
            return 1;
        }

        public virtual int SetNoPause(BaseCmdArgument self, string arg, int argIndex)
        {
            ControllerArgument.IsPause = false;
            return 0;
        }
    }
}
