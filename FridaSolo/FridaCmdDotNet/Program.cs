using GenOcean.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

using FridaDotNet;

namespace FridaCmd
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"FridaCmd v1.0.0 by Wings");
            if (args.Length <= 0)
            {
#if DEBUG
                args = new string[] {
                        "-u",
                        "-f",
                        "com.bandainamcoent.kidsf",
                        "-l",
                        @"FridaTest.js",
                        "--no-pause"
                };
#endif
            }

#if DEBUG && CHECK
            for(int i =0;i< args.Length;i++)
            {
                Console.WriteLine($"Argument[{i}]:{args[i]}");
            }
#endif

            FridaCmdArgument arg = new FridaCmdArgument(args);
            arg.ProcessArgs();
#if DEBUG
            Console.WriteLine(arg.ToString());
#endif
            FridaController controller = new FridaController(arg.ControllerArgument);


            string cmd = Console.ReadLine();
            do
            {
                controller.RunScript(cmd);
                cmd = Console.ReadLine();
            } while (!cmd.ToLower().Equals("-exit"));

            Console.WriteLine("Exit FridaCmd");

            Console.ReadKey();
        }
    }
}
