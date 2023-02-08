using System;

using Frida;
using GenOcean.Common;

namespace FridaDotNet
{
    /*
     * 这里FridaManager做成了单例，想要精简的话可以直接把Common这个脚本组去掉
     */
    
    internal class SingletonFridaManager: ISingletonFridaManager<FridaManager>
    {

    }

    internal class ISingletonFridaManager<T> : SingletonManagerBase<T>
        where T : FridaManager,new()
    {
        public static void ListDevices()
        {
            Instance.ListDevices();
        }

        public static string GetDeviceIdByType(int iType)
        {
            return Instance.GetDeviceIdByType(iType);
        }

        public static Device GetDeviceById(string id)
        {
            return Instance.GetDeviceById(id);
        }

        public static DeviceManager FridaDeviceManager
        {
            get
            {
                return Instance.FridaDeviceManager;
            }
        }
    }

    internal class FridaManager : ManagerBase
    {

        /*
         * 这里用 null 可能也行？
         * System.Windows.Threading.Dispatcher.CurrentDispatcher 之前用的
         */
        protected DeviceManager _DeviceManager = null;
        protected Device[] _Devices = null;

        public DeviceManager FridaDeviceManager
        {
            get {
                return _DeviceManager;
            }
        }

        public override void Init()
        {
            base.Init();
            _DeviceManager = new DeviceManager(System.Windows.Threading.Dispatcher.CurrentDispatcher);
            _DeviceManager.Changed += new EventHandler(OnDeviceManager_Changed);
        }

        protected virtual void OnDeviceManager_Changed(object sender, EventArgs e)
        {
        }

        public void ListDevices()
        {
#if DEBUG
            Console.WriteLine($"ListDevices:");
            Console.WriteLine($"{"Id",-20} {"Name",-20} Type");
#endif
            _Devices = _DeviceManager.EnumerateDevices();
            foreach(var device in _Devices)
            {
                Console.WriteLine($"{device.Id,-20} {device.Name,-20} {device.Type}");
            }
        }

        public string GetDeviceIdByType(int iType)
        {
            if(_Devices == null || _Devices.Length<=0)
            {
                _Devices = _DeviceManager.EnumerateDevices();
            }

            foreach (var device in _Devices)
            {
                if(device.Type.Equals((DeviceType)iType))
                {
                    return device.Id;
                }
            }

            return null;
        }

        public Device GetDeviceById(string id)
        {
            if (_Devices == null || _Devices.Length <= 0)
            {
                _Devices = _DeviceManager.EnumerateDevices();
            }

            foreach (var device in _Devices)
            {
                if (device.Id.Equals(id))
                {
                    return device;
                }
            }

            return null;
        }
    }
}
