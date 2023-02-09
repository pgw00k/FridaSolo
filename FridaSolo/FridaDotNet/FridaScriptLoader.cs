using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FridaDotNet
{
    public class FridaScriptLoader
    {
        public static Regex RegexImport = new Regex(@"include\([""']{1}([\S\s]*?)(\.js)*['""]{1}\);?[\n\r]+");

        public static Dictionary<string,FridaScriptLoader> Loaders = new Dictionary<string, FridaScriptLoader>();

        public static FridaScriptLoader LoadScript(string filePath)
        {
            FridaScriptLoader loader = null;
            if (Loaders.TryGetValue(filePath,out loader))
            {
                Loaders.Remove(filePath);
            }else
            {
                loader = new FridaScriptLoader(filePath);
            }

            Loaders.Add(filePath, loader);

            return loader;
        }

        public string Text;
        public List<FridaScriptLoader> SubScripts = new List<FridaScriptLoader>();
        public string SearchRoot;

        public string FinalText
        {
            get
            {
                List<FridaScriptLoader> loaders = new List<FridaScriptLoader>();
                GetAllSubScript(ref loaders);
                loaders.Reverse();
                var hs = loaders.ToHashSet();

                string pre = "";
                foreach (var sub in hs)
                {
                    pre += $"{sub.Text}\n";
                }

                return pre + Text;
            }
        }

        public override string ToString()
        {
            return Text;
        }

        protected FridaScriptLoader(string filePath) 
        {
            SearchRoot = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(filePath)));
            Text = File.ReadAllText(filePath);
            Text = RegexImport.Replace(Text, ImportOtherJavaScript);
        }

        public string ImportOtherJavaScript(Match m)
        {
            string filePath = Path.GetFullPath(Path.Combine(SearchRoot, m.Groups[1].Value+".js"));
            if(File.Exists(filePath))
            {
                FridaScriptLoader subLoader = LoadScript(filePath);
                SubScripts.Add(subLoader);
            }
            //Console.WriteLine(m.Groups[1]);
            return "";
        }

        public void GetAllSubScript(ref List<FridaScriptLoader> subScriptList)
        {
            foreach (var sub in SubScripts)
            {
                subScriptList.Add(sub);
                sub.GetAllSubScript(ref subScriptList);
            }
        }
    }
}
