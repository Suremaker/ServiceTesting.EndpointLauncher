using System;
using System.Diagnostics;
using System.Net;
using NUnit.Framework;
using SimpleHttpMock;
using Wonga.ServiceTesting.EndpointLauncher.Validators;

namespace Wonga.ServiceTesting.EndpointLauncher.Tests
{
    [TestFixture]
    public class HttpOkStatusHealthValidatorTests
    {
        private const string TimeoutPath = @"c:\windows\system32\timeout.exe";
        private MockedHttpServer _server;
        private const string BaseUrl = "http://localhost:9744";
        private string StatusUrl = BaseUrl + "/status";

        [OneTimeSetUp]
        public void SetUp()
        {
            _server = new MockedHttpServerBuilder().Build(BaseUrl);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _server.Dispose();
        }

        [Test]
        public void Validator_should_check_status_endpoint()
        {
            ConfigureStatus(HttpStatusCode.OK);

            var application = new ServiceEndpointLauncher().LaunchApplication(TimeoutPath, "/T 3");
            Assert.DoesNotThrow(() => new HttpOkStatusHealthValidator(StatusUrl).ValidateHealth(application));
        }

        [Test]
        public void Validator_should_throw_if_status_is_not_OK_after_the_timeout()
        {
            var maxWaitTime = TimeSpan.FromMilliseconds(500);
            var doubleMaxTime = TimeSpan.FromMilliseconds(1000);

            ConfigureStatus(HttpStatusCode.NotFound);

            var watch = Stopwatch.StartNew();
            var application = new ServiceEndpointLauncher().LaunchApplication(TimeoutPath, "/T 3");
            var validator = new HttpOkStatusHealthValidator(StatusUrl, maxWaitTime, TimeSpan.FromMilliseconds(10));
            var ex = Assert.Throws<Exception>(() => validator.ValidateHealth(application));
            watch.Stop();
            Assert.That(watch.Elapsed, Is.LessThan(doubleMaxTime));
            Assert.That(ex.Message, Is.EqualTo(string.Format("Process '{0} /T 3' is not responding on url: {1}", TimeoutPath, StatusUrl)));
        }

        [Test]
        public void Validator_should_throw_immediately_if_process_is_down()
        {
            var maxWaitTime = TimeSpan.FromSeconds(10);
            ConfigureStatus(HttpStatusCode.NotFound);

            var watch = Stopwatch.StartNew();
            var application = new ServiceEndpointLauncher().LaunchApplication(TimeoutPath, "/T 0");

            var validator = new HttpOkStatusHealthValidator(StatusUrl, maxWaitTime, TimeSpan.FromMilliseconds(10));
            var ex = Assert.Throws<Exception>(() => validator.ValidateHealth(application));
            watch.Stop();
            Assert.That(watch.Elapsed, Is.LessThan(maxWaitTime));
            Assert.That(ex.Message, Is.EqualTo(string.Format("Process '{0} /T 0' is not responding on url: {1}", TimeoutPath, StatusUrl)));
        }

        private void ConfigureStatus(HttpStatusCode code)
        {
            var builder = new MockedHttpServerBuilder();
            builder.WhenGet("/status").Respond(code);
            builder.Reconfigure(_server, true);
        }
    }
}