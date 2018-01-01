using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TestScriptingApp
{

    public class ScriptHost
    {
        /// <summary>
        /// this public function in the ScriptHost class 
        /// is available to the script
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public double Square(double Number)
        {
            return Number * Number;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("this is to show that the program is actually working.");
            Console.WriteLine("calling a simple script (4+4) now..");

            var script = @"4+4";
            double result;

            try
            {
                result = CSharpScript.EvaluateAsync<double>(script, null, new ScriptHost()).Result;
            }
            catch(Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                Console.WriteLine(String.Format("{0}\n\nPress a key to continue.", e.Message));
                Console.ReadKey();
                return;
            }

            Console.WriteLine(String.Format("result of the script: {0}", result));

            // very short script to demonstrate calling a function in the ScriptHost class
            // that functions like a short of container/sandbox for the script

            Console.WriteLine("calling a script that uses a function in the ScriptHost class..");

            script = @"Square(4)";

            try
            {
                result = CSharpScript.EvaluateAsync<double>(script, null, new ScriptHost()).Result;
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                Console.WriteLine(String.Format("{0}\n\nPress a key to continue.", e.Message));
                Console.ReadKey();
                return;
            }

            Console.WriteLine(String.Format("result of the script: {0}", result));
            Console.ReadLine();
        }
    }
}
