using System;
using System.Diagnostics;
using System.IO;

namespace Wonga.ServiceTesting.EndpointLauncher
{
    public class ServiceEndpointLauncher
    {
        private readonly ProcessWindowStyle _childProcessWindowStyle;

        /// <summary>
        /// Initializes ServiceEndpointLauncher.
        /// </summary>
        /// <param name="childProcessWindowStyle">Window style of child processes. By default it would be Minimized.</param>
        public ServiceEndpointLauncher(ProcessWindowStyle childProcessWindowStyle = ProcessWindowStyle.Minimized)
        {
            _childProcessWindowStyle = childProcessWindowStyle;
        }

        /// <summary>
        /// Starts a console or windows application with specified parameters and enables parent-child process watch for it, guaranteeing that created process would be terminated after current process finish.
        /// 
        /// The working directory of created process would default to directory of <c>exePath</c>.
        /// If created application has a window, it would be displayed.
        /// 
        /// If current process ends, the parent-child process watch will attempt to gracefully shut down the child process (it applies only to processes with main windows), and eventually kill it.
        /// </summary>
        /// <param name="exePath">Application path</param>
        /// <param name="args">Application arguments</param>
        /// <returns>Created process object</returns>
        public Process LaunchApplication(string exePath, string args = "")
        {
            return LaunchApplication(exePath, args, Path.GetDirectoryName(exePath));
        }

        /// <summary>
        /// Starts a console or windows application with specified parameters and enables parent-child process watch for it, guaranteeing that created process would be terminated after current process finish.
        /// 
        /// If created application has a window, it would be displayed.
        /// 
        /// If current process ends, the parent-child process watch will attempt to gracefully shut down the child process (it applies only to processes with main windows), and eventually kill it.
        /// </summary>
        /// <param name="exePath">Application path</param>
        /// <param name="args">Application arguments</param>
        /// <param name="workingDirectory">Application working directory</param>
        /// <returns>Created process object</returns>
        public Process LaunchApplication(string exePath, string args, string workingDirectory)
        {
            var childProcess = Process.Start(new ProcessStartInfo(Path.GetFullPath(exePath), args)
            {
                WorkingDirectory = Path.GetFullPath(workingDirectory),
                WindowStyle = _childProcessWindowStyle
            });

            Process.Start(new ProcessStartInfo("Wonga.ServiceTesting.ProcessWatch.exe", string.Format("{0} {1}", Process.GetCurrentProcess().Id, childProcess.Id)) { WindowStyle = ProcessWindowStyle.Hidden });

            return childProcess;
        }
        /// <summary>
        /// Starts an IIS Express instance to host a web application with specified parameters and enables parent-child process watch for it, guaranteeing that created IIS Express instance would be terminated after current process finish.
        /// 
        /// If current process ends, the parent-child process watch will attempt to gracefully shut down the IIS Express process (it applies only to processes with main windows), and eventually kill it.
        /// </summary>
        /// <param name="webAppPath">Path to web application (usually to the location where web.config is located)</param>
        /// <param name="port">Port at which new web application would start</param>
        /// <returns>IIS Express process</returns>
        public Process LaunchWebApplication(string webAppPath, int port)
        {
            var fullWebAppPath = Path.GetFullPath(webAppPath);
            var args = string.Format("/path:\"{0}\" /port:\"{1}\" /systray:false", fullWebAppPath, port);

            var programfiles = Environment.GetEnvironmentVariable("programfiles")
                ?? Environment.GetEnvironmentVariable("programfiles(x86)");
            var iisExpressPath = programfiles + "\\IIS Express\\iisexpress.exe";


            return LaunchApplication(iisExpressPath, args, fullWebAppPath);
        }
    }
}
