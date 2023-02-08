using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using FridaDotNet;
using GenOcean.Common;

namespace FridaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /*
         * 绑定只识别属性，使用字段的话无法绑定成功
         */

        public ObservableCollection<Frida.Device> Devices { get; set; }
        public ObservableCollection<Frida.Process> Processes { get; set; }

        protected FileStream LoggerFileStream;
        protected LoggerConsoleWriter ConsoleWriter;

        protected FridaController _FridaController;
        

        public MainWindow()
        {
            // 不设置DataContext绑定不会生效
            DataContext = this;
            Devices = new ObservableCollection<Frida.Device>();
            Processes = new ObservableCollection<Frida.Process>();

            InitializeComponent();

            LoggerFileStream = new FileStream("Log.txt",FileMode.Create,FileAccess.Write,FileShare.Read);
            ConsoleWriter = new LoggerConsoleWriter(LoggerFileStream);
            ConsoleWriter.RegisterLogCallback(WriteLog);

#if DEBUG
            Console.WriteLine("Window init finish!");
#endif

            tbTargetName.Text = Properties.Settings.Default.TargetName;
            tbScriptPath.Text = Properties.Settings.Default.ScriptPath;

            RefreshDeviceList();
        }

        ~MainWindow()
        {
            //ConsoleWriter.Dispose();
            //ConsoleWriter.Close();
        }

        protected virtual void RefreshDeviceList()
        {
            var devices = SingletonFridaManager.FridaDeviceManager.EnumerateDevices();

#if DEBUG
            Console.WriteLine($"Get {devices.Length} devices!");
#endif

            Array.Sort(devices, delegate (Frida.Device a, Frida.Device b)
            {
                var aHasIcon = a.Icon != null;
                var bHasIcon = b.Icon != null;
                if (aHasIcon == bHasIcon)
                    return a.Id.CompareTo(b.Id);
                else
                    return bHasIcon.CompareTo(aHasIcon);
            });

            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }

        }
        protected virtual void RefreshProcessList()
        {
            var device = lsvDevice.SelectedItem as Frida.Device;
            if (device == null)
            {
                Processes.Clear();
                return;
            }

            try
            {
                var processes = device.EnumerateProcesses(Frida.Scope.Full);
                Array.Sort(processes, delegate (Frida.Process a, Frida.Process b) {
                    var aHasIcon = a.Icons.Length != 0;
                    var bHasIcon = b.Icons.Length != 0;
                    if (aHasIcon == bHasIcon)
                        return a.Name.CompareTo(b.Name);
                    else
                        return bHasIcon.CompareTo(aHasIcon);
                });
                Processes.Clear();
                foreach (var process in processes)
                {
                    Processes.Add(process);
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnumerateProcesses failed: " + ex.Message);
                Processes.Clear();
            }
        }

        protected virtual void WriteLog(string log)
        {
            lbConsole.Items.Add(log);
        }

        private void lsvDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //RefreshProcessList();
        }

        private void lsvProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnRefreshDeviceList_Click(object sender, RoutedEventArgs e)
        {
            RefreshDeviceList();
        }

        private void btnSpawn_Click(object sender, RoutedEventArgs e)
        {

            var device = lsvDevice.SelectedItem as Frida.Device;

            

            Properties.Settings.Default["ScriptPath"] = tbScriptPath.Text;
            Properties.Settings.Default["TargetName"] = tbTargetName.Text;

            Properties.Settings.Default.Save();

            FridaArguments args = new FridaArguments();
            args.DeviceID = device.Id;
            args.TargetName = tbTargetName.Text;
            args.ScriptPath = tbScriptPath.Text;
            args.IsPause = false;
            args.IsNeedSpawn = true;

            _FridaController = new FridaController(args);

        }

        private void btnBrowserSciptFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if((bool)ofd.ShowDialog())
            {
                tbScriptPath.Text = ofd.FileName;
            }
        }

        private void btnRefreshProcessList_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lbConsole.Items.Clear();
        }

        private void btnScriptRun_Click(object sender, RoutedEventArgs e)
        {
            if (_FridaController != null)
            {
                var fridaEvalJS = $@"[""frida:rpc"",1,""call"",""fridaEvaluate"",[""{tbJSScript.Text}""]]";
                _FridaController.RunScript(fridaEvalJS);
            }
        }

        private void btnScriptPost_Click(object sender, RoutedEventArgs e)
        {
            if (_FridaController != null)
            {
                _FridaController.RunScript(tbJSScript.Text);
            }
        }
    }
}
