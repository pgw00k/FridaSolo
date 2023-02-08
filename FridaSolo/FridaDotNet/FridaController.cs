using Frida;
using System;
using System.IO;

namespace FridaDotNet
{
    public class FridaArguments
    {
        public string DeviceID = "local";
        public string TargetName = "";
        public string ScriptPath = "";
        public bool IsPause = true;
        public bool IsNeedSpawn = true;

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

    public class FridaController
    {
        public static string REPLDefinition = File.ReadAllText(@"FridaREPL.js");

        /// <summary>
        /// 用来标识的ID号（设备ID+pid）
        /// </summary>
        public string CID;

        public Device TargetDevice;
        public Session TargetSession;
        public Script TargetScript;
        public uint PID;

        protected bool _IsHasScript = false;

        public FridaController(FridaArguments args)
        {
            TargetDevice = SingletonFridaManager.GetDeviceById(args.DeviceID);
            if(TargetDevice == null )
            {
                throw new ArgumentException($"Can not get device by ID={args.DeviceID}");
            }

            if (args.IsNeedSpawn)
            {
                PID = TargetDevice.Spawn(args.TargetName, null, null, null, null);
            }

            TargetSession = TargetDevice.Attach(PID);

            if (!string.IsNullOrEmpty(args.ScriptPath))
            {
#if DEBUG
                Console.WriteLine($"Load script {args.ScriptPath}");
#endif
                var fileContext = File.ReadAllText(args.ScriptPath);

                var js = $"{REPLDefinition}{fileContext}";

                TargetScript = TargetSession.CreateScript(js);
                _IsHasScript = true;
            }

            if (_IsHasScript)
            {
                TargetScript.Message += new ScriptMessageHandler(ScriptMessage);
                TargetScript.Load();
            }

            if (!args.IsPause && args.IsNeedSpawn)
            {
                TargetDevice.Resume(PID);
            }

            CID = $"[{args.DeviceID}]{PID}";
        }

        public virtual void ScriptMessage(object sender, ScriptMessageEventArgs e)
        {
            var message = e.Message;
            Console.WriteLine($"Message from Script: {message}");
            //Console.WriteLine($"Message from Script: {e.PayLoad}");
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
            var fridaEvalJS = $@"[""frida:rpc"",1,""call"",""fridaEvaluate"",[""{js}""]]";
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
