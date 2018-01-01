using System;
using System.Windows.Forms;
using Bootstrap.Properties;
using System.Threading;
namespace Bootstrap
{
    static class Program
    {

        /// <summary>
        /// The main entry point for bootstrap application.
        /// </summary>
        /// <remarks>
        /// This sample project demonstrates bootstrapping a WPF Test application - however
        /// there is no difference in the logic required for bootstrapping a Windows Forms
        /// Application.   
        /// </remarks>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                #if !DEBUG
                CheckProcessIntegrity();
                #endif
                SplashForm.DisplaySplash(500, 500);

                // The test app loads very quickly - so we would not see the splash
                // screen without a delay.  You can obviously remove or reduce this delay 
                // for larger applications that take longer to load
                //
                Thread.Sleep(500);
                ExecuteAssembly("TestScriptingApp");
                SplashForm.CloseSplash();
            }
            catch (Exception e)
            {
                SplashForm.CloseSplash();
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                MessageBox.Show(string.Format(Resources.UnhandledError, e.Message),
                                Resources.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region  Local Bootstrap methods


        /// <summary>
        /// The prefix used to determine the resource names for the encrypted assemblies
        /// </summary>
        private static string RESOURCE_PREFIX = typeof(Program).Namespace + ".Assemblies.";

        /// <summary>
        /// The level of process integrity checks to perform
        /// </summary>
        private const int INTEGRITY_LEVEL = 4;

        /// <summary>
        /// Check the process integrity on x86 platforms
        /// </summary>
        private static void x86CheckProcessIntegrity()
        {
            AssemblyLoaderx86.AssemblyLoader.CheckProcessIntegrity(INTEGRITY_LEVEL);
        }

        /// <summary>
        /// Check the process integrity on x64 platforms
        /// </summary>
        private static void x64CheckProcessIntegrity()
        {
            AssemblyLoaderx64.AssemblyLoader.CheckProcessIntegrity(INTEGRITY_LEVEL);
        }

        /// <summary>
        /// Check the process integrity 
        /// </summary>
        private static void CheckProcessIntegrity()
        {
            if (IntPtr.Size == 8)
            {
                x64CheckProcessIntegrity();
            }
            else
            {
                x86CheckProcessIntegrity();
            }
        }

        /// <summary>
        /// Execute the assembly on x86 platforms
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to execute</param>
        private static void x86ExecuteAssembly(string assemblyName)
        {
            AssemblyLoaderx86.AssemblyLoader.ExecuteAssembly(RESOURCE_PREFIX, assemblyName);
        }

        /// <summary>
        /// Execute the assembly on x64 platforms
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to execute</param>
        private static void x64ExecuteAssembly(string assemblyName)
        {
            AssemblyLoaderx64.AssemblyLoader.ExecuteAssembly(RESOURCE_PREFIX, assemblyName);
        }

        /// <summary>
        /// Execute the given assembly
        /// </summary>
        /// <param name="assemblyName">The simple assembly name (with no extension)</param>
        private static void ExecuteAssembly(string assemblyName)
        {
            if (IntPtr.Size == 8)
            {
                x64ExecuteAssembly(assemblyName);
            }
            else
            {
                x86ExecuteAssembly(assemblyName);
            }
        }

        #endregion

    }
}