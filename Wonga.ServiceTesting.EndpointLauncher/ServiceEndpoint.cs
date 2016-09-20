using System;
using System.Diagnostics;

namespace Wonga.ServiceTesting.EndpointLauncher
{
    public class ServiceEndpoint
    {
        private readonly Func<Process> _processLauncher;
        private readonly IEndpointHelathValidator _healthValidator;
        public Process Process { get; private set; }

        public ServiceEndpoint(Func<Process> processLauncher, IEndpointHelathValidator healthValidator)
        {
            _processLauncher = processLauncher;
            _healthValidator = healthValidator;
            Process = _processLauncher.Invoke();
        }

        /// <summary>
        /// Terminates the endpoint
        /// </summary>
        public void Terminate()
        {
            try
            {
                if (!Process.HasExited && (!Process.CloseMainWindow() || !Process.WaitForExit(3000)))
                    Process.Kill();
            }
            catch (Exception) { }
        }

        /// <summary>
        ///Validates endpoint health (exception will be thrown if endpoint is not working)
        /// </summary>
        public void ValidateHealth()
        {
            _healthValidator.ValidateHealth(Process);
        }

        public void Restart()
        {
            Terminate();
            Process = _processLauncher.Invoke();
        }
    }
}