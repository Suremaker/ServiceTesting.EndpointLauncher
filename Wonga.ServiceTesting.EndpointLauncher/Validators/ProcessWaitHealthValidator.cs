using System;
using System.Diagnostics;

namespace Wonga.ServiceTesting.EndpointLauncher.Validators
{
    /// <summary>
    /// Ensures that process did not exit within the specified time. If the maxWaitTime is reach, the subsequent calls to ValidateHealth will just check if service is alive.
    /// </summary>
    public class ProcessWaitHealthValidator : IEndpointHelathValidator
    {
        private readonly int _maxWaitTime;
        public static readonly ProcessWaitHealthValidator Default = new ProcessWaitHealthValidator(TimeSpan.FromSeconds(10));

        public ProcessWaitHealthValidator(TimeSpan maxWaitTime)
        {
            _maxWaitTime = (int)maxWaitTime.TotalMilliseconds;
        }

        public void ValidateHealth(Process process)
        {
            var waitTime = Math.Max(0, _maxWaitTime - GetElapsedTime(process));
            if (process.WaitForExit(waitTime))
                throw new Exception(string.Format("Process '{0} {1}' stopped unexpectedly with error code: {2}", process.StartInfo.FileName, process.StartInfo.Arguments, process.ExitCode));
        }

        private static int GetElapsedTime(Process process)
        {
            DateTimeOffset startTime = process.StartTime.ToUniversalTime();
            return (int)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
        }
    }
}