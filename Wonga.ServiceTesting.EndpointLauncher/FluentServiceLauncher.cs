using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Wonga.ServiceTesting.EndpointLauncher.Validators;

namespace Wonga.ServiceTesting.EndpointLauncher
{
    /// <summary>
    /// FluentServiceLauncher allows to start multiple endpoints with fluent way 
    /// </summary>
    public class FluentServiceLauncher
    {
        private readonly List<Tuple<Func<ServiceEndpointLauncher, Process>, IEndpointHelathValidator>> _endpoints = new List<Tuple<Func<ServiceEndpointLauncher, Process>, IEndpointHelathValidator>>();
        private TimeSpan _launchDelay = TimeSpan.Zero;
        private uint _retries;
        private ProcessWindowStyle _processWindowStyle = ProcessWindowStyle.Minimized;

        /// <summary>
        /// Specifies endpoint window style. By default it is Minimized.
        /// </summary>
        /// <param name="processWindowStyle">Window style.</param>
        public FluentServiceLauncher SetEndpointsWindowStyle(ProcessWindowStyle processWindowStyle)
        {
            _processWindowStyle = processWindowStyle;
            return this;
        }

        /// <summary>
        /// Adds the endpoint launcher with the health validator that is used to validate the endpoint health.
        /// </summary>
        public FluentServiceLauncher AddEndpoint(Func<ServiceEndpointLauncher, Process> endpointLauncher, IEndpointHelathValidator healthValidator)
        {
            _endpoints.Add(Tuple.Create(endpointLauncher, healthValidator));
            return this;
        }

        /// <summary>
        /// Adds the endpoint launcher with the ProcessWaitHealthValidator.Default health validator.
        /// </summary>
        public FluentServiceLauncher AddEndpoint(Func<ServiceEndpointLauncher, Process> endpointLauncher)
        {
            return AddEndpoint(endpointLauncher, ProcessWaitHealthValidator.Default);
        }

        /// <summary>
        /// Allows to specify a delay between endpoint starts. By default it is TimeSpan.Zero.
        /// </summary>
        /// <param name="launchDelay"></param>
        /// <returns></returns>
        public FluentServiceLauncher WithLaunchDelay(TimeSpan launchDelay)
        {
            _launchDelay = launchDelay;
            return this;
        }

        public FluentServiceLauncher WithRetries(uint retries)
        {
            _retries = retries;
            return this;
        }

        /// <summary>
        /// Launches all endpoints and return ServiceEndpoint array. During start, it ensures that all health validators have passed.
        /// If any endpoint fail to start, all others will be killed and the exception will be thrown.
        /// </summary>
        /// <returns></returns>
        public ServiceEndpoint[] LaunchAll()
        {
            var endpoints = new List<ServiceEndpoint>();
            try
            {
                StartEndpoints(endpoints);
                ValidateEndpointsStarted(endpoints);
                return endpoints.ToArray();
            }
            catch (Exception)
            {
                TerminateEndpoints(endpoints);
                throw;
            }
        }

        private void StartEndpoints(List<ServiceEndpoint> endpoints)
        {
            var launcher = new ServiceEndpointLauncher(_processWindowStyle);
            foreach (var e in _endpoints)
            {
                endpoints.Add(new ServiceEndpoint(() => e.Item1.Invoke(launcher), e.Item2));
                Thread.Sleep(_launchDelay);
            }
        }

        /// <summary>
        /// Terminates all endpoints (in parallel)
        /// </summary>
        public static void TerminateEndpoints(IEnumerable<ServiceEndpoint> endpoints)
        {
            endpoints.AsParallel().ForAll(e => e.Terminate());
        }

        /// <summary>
        /// Verifies health of all endpoints (in parallel)
        /// </summary>
        public static void ValidateEndpoints(IEnumerable<ServiceEndpoint> endpoints)
        {
            endpoints.AsParallel().ForAll(e => e.ValidateHealth());
        }

        private void ValidateEndpointsStarted(List<ServiceEndpoint> endpoints)
        {
            endpoints.AsParallel().ForAll(ValidateStarted);
        }

        private void ValidateStarted(ServiceEndpoint endpoint)
        {
            uint attempt = 0;
            while (true)
            {
                try
                {
                    endpoint.ValidateHealth();
                    return;
                }
                catch
                {
                    if (attempt++ >= _retries)
                        throw;
                    endpoint.Restart();
                }
            }
        }
    }
}