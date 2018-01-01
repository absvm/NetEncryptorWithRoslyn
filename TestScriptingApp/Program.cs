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
            // very short script to demonstrate calling a function in the ScriptHost class
            // that functions like a short of container/sandbox for the script
            var script = @"Square(4)";

            var result = CSharpScript.EvaluateAsync<double>(script, null, new ScriptHost()).Result;

            Console.WriteLine("result of the script:");
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
