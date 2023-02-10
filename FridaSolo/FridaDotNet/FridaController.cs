using Frida;
using System;
using System.IO;

namespace FridaDotNet
{
    public class FridaController
    {
#if USE_EXT_JS
        public static string REPLDefinition = File.ReadAllText(@"FridaREPL.js");
#else
        public static string REPLDefinition = FridaJavaScriptHelper.RPC_Init_Definition;
#endif

        /// <summary>
        /// 用来标识的ID号（设备ID+pid）
        /// </summary>
        public string CID;

        public Device TargetDevice;
        public Session TargetSession;
        public Script TargetScript;
        public uint PID;

        protected bool _IsHasScript = false;
        protected int _ScriptPostID = 0;

        protected FridaScriptLoader _FridaScriptLoader;

        public virtual void StartWith<T>(T args)where T : IFridaArguments
        {
            Start(args);
        }

        public virtual void Start(IFridaArguments args)
        {
            TargetDevice = SingletonFridaManager.GetDeviceById(args.DeviceID);
            if(TargetDevice == null )
            {
                throw new ArgumentException($"Can not get device by ID={args.DeviceID}");
            }

            // 考虑到文件读写需要时间，所以先把文件加载完
            LoadScript(args.ScriptPath);
            string FinalScript  = $"{REPLDefinition}{_FridaScriptLoader.FinalText}";

            if (args.IsNeedSpawn)
            {
                PID = TargetDevice.Spawn(args.TargetName, null, null, null, null);
            }

            TargetSession = TargetDevice.Attach(PID);

            if (_IsHasScript)
            {
                TargetScript = TargetSession.CreateScript(FinalScript);
                TargetScript.Message += new ScriptMessageHandler(ScriptMessage);
                TargetScript.Load();
            }

            if (!args.IsPause && args.IsNeedSpawn)
            {
                TargetDevice.Resume(PID);
            }

            CID = $"[{args.DeviceID}]{PID}";
        }

        public virtual void LoadScript(string path)
        {

            if (!string.IsNullOrEmpty(path))
            {
#if DEBUG
                Console.WriteLine($"Load script {path}");
#endif
                _FridaScriptLoader = FridaScriptLoader.LoadScript(path);
                _IsHasScript = true;
            }
        }

        public virtual void ScriptMessage(object sender, ScriptMessageEventArgs e)
        {
#if _CONSOLE
            if (!string.IsNullOrEmpty(e.PayLoad))
            {
                Console.WriteLine(e.PayLoad);
            }else
            {
                Console.WriteLine($"Message from Script: {e.Message}");
            }
#else
            Console.WriteLine($"Message from Script: {e.Message}");
#endif

            if (e.Data!=null)
            {
                Console.WriteLine($"Data: {String.Join(",", e.Data)}");
            }        
        }

        public virtual void RunScript(string js)
        {
            if(string.IsNullOrEmpty(js))
            {
                return;
            }
            _ScriptPostID++;
            var fridaEvalJS = $@"[""frida:rpc"",{_ScriptPostID},""call"",""fridaEvaluate"",[""{js}""]]";
            PostJson(fridaEvalJS);
        }

        public virtual void PostJson(string js)
        {
            if (TargetScript != null)
            {
                TargetScript.Post(js);
            }
        }
    }
}
