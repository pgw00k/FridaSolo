
using System.IO;

using FridaDotNet;

namespace FridaCmd
{
    public class FridaCmdController : FridaController
    {
        public bool IsOutputFinalJS = false;
        public string OutputJSPath = "main.js";

        public override void StartWith<T>(T args)
        {
            FridaCmdArgument cmdarg = args as FridaCmdArgument;
            if (cmdarg != null)
            {
                IsOutputFinalJS = cmdarg.IsOutputFinalJS ;
                OutputJSPath = cmdarg.OutputFinalJSPath;
            }
            base.StartWith(args);
        }

        public override void LoadScript(string path)
        {
            base.LoadScript(path);
            if(IsOutputFinalJS && _IsHasScript)
            {
                File.WriteAllText(OutputJSPath, _FridaScriptLoader.FinalText);
            }
        }
    }
}
