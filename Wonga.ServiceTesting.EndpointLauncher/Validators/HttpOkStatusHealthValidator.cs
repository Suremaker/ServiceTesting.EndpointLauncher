using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Wonga.ServiceTesting.EndpointLauncher.Validators
{
    /// <summary>
    /// Ensures that specified statusUrl returns HTTP OK. If other status is returned, the ValidateHealth will repeat check up to maxWaitTime (and then it will fail).
    /// The class is also checking if specified process is running.
    /// </summary>
    public class HttpOkStatusHealthValidator : IEndpointHelathValidator
    {
        private readonly string _statusUrl;
        private readonly TimeSpan _maxWaitTime;
        public static readonly TimeSpan DefaultWaitTime = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(200);
        private readonly TimeSpan _pollInterval;
        private readonly HttpWebRequest _webRequest;

        public HttpOkStatusHealthValidator(string statusUrl, TimeSpan maxWaitTime, TimeSpan pollInterval)
        {
            _statusUrl = statusUrl;
            _maxWaitTime = maxWaitTime;
            _pollInterval = pollInterval;
            _webRequest = (HttpWebRequest)WebRequest.Create(_statusUrl);
        }

        public HttpOkStatusHealthValidator(string statusUrl) : this(statusUrl, DefaultWaitTime, DefaultPollInterval)
        {
        }
        public void ValidateHealth(Process process)
        {

            while (true)
            {
                if (IsStatusHttpOk())
                    return;

                if ((_maxWaitTime - GetElapsedTime(process)).TotalMilliseconds < 0 || process.WaitForExit(0))
                    throw new Exception(string.Format("Process '{0} {1}' is not responding on url: {2}", process.StartInfo.FileName, process.StartInfo.Arguments, _statusUrl));
                Thread.Sleep(_pollInterval);
            }
        }

        private bool IsStatusHttpOk()
        {
            try
            {
                return ((HttpWebResponse)_webRequest.GetResponse()).StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        private static TimeSpan GetElapsedTime(Process process)
        {
            DateTimeOffset startTime = process.StartTime.ToUniversalTime();
            return DateTimeOffset.UtcNow - startTime;
        }
    }
}