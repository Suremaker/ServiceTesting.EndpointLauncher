using System;
using System.Diagnostics;

namespace Wonga.ServiceTesting.EndpointLauncher
{
    public interface IEndpointHelathValidator
    {
        void ValidateHealth(Process process);
    }
}