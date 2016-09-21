# ServiceTesting.EndpointLauncher

* Build status: [![Build status](https://ci.appveyor.com/api/projects/status/v73q764yppc82t8b?svg=true)](https://ci.appveyor.com/project/Suremaker/servicetesting-endpointlauncher)
* Nuget package: [Wonga.ServiceTesting.EndpointLauncher](https://www.nuget.org/packages/Wonga.ServiceTesting.EndpointLauncher/)


## Description

The **ServiceTesting.EndpointLauncher** helps with starting and managing lifecycle of the external processes used in tests.

Features:
* ability to start and manage **console/desktop applications** as well as **web applications** (with IIS Express),
* ability to verify that applications successfully started,
* ability to ensure that applications are still running, to fail fast between tests,
* ability to restart applications during startup in case they stop,
* ability to stop managed applications on shutdown,
* **guaranteed applications stop after test process exit** (including unexpected termination).
 
## Sample usage based on NUnit 2.x

Launching endpoints for the time of test execution
```c#
[SetUpFixture]
public class EndpointSetup
{
    private static ServiceEndpoint[] _endpoints;

    [SetUp]
    public void SetUp()
    {
        _endpoints = new FluentServiceLauncher()

            // restart endpoint if dies on startup
            .WithRetries(2)

            // start next endpoint with 500ms delay
            .WithLaunchDelay(TimeSpan.FromMilliseconds(500))

            // console app with no parameters and default health validator
            .AddEndpoint(launcher => launcher.LaunchApplication("app1\\my_app.exe"))

            // console app with parameters and custom health validator
            .AddEndpoint(launcher => launcher.LaunchApplication("app2\\my_other_app.exe", "/param1 /param2"),
                new ProcessWaitHealthValidator(TimeSpan.FromSeconds(5)))

            // web app with custom HTTP 200 health validator
            .AddEndpoint(launcher => launcher.LaunchWebApplication("my_web_app_dir", 9637),
                new HttpOkStatusHealthValidator("http://localhost:9637/"))

            // launch all and throw if any did not managed to start
            .LaunchAll();
    }

    [TearDown]
    public void TearDown()
    {
        // graceful shutdown
        FluentServiceLauncher.TerminateEndpoints(_endpoints);
    }

    // method to check endpoint health between test run. Will throw if any is not working.
    public static void ValidateEndpoints() { FluentServiceLauncher.ValidateEndpoints(_endpoints); }
}
```

Example test class with endpoint health check before run:
```c#
[TestFixture]
public class SomeTestClass
{
    [TestFixtureSetUp] // It can be also [SetUp]
    public void FixtureSetup()
    {
        EndpointSetup.ValidateEndpoints();
    }

    [Test]
    public void MyTest()
    {
        // ...
    }
}
```

## How external applications are shut down when tests are aborted?

The main feature of this project is to ensure that all started processes would terminate after tests, even if tests would be aborted or test process would be killed.

It has been achieved with [ProcessWatch](https://github.com/wongatech/ServiceTesting.EndpointLauncher/tree/master/Wonga.ServiceTesting.ProcessWatch) helper process that is started for each configured endpoint and monitors both, parent test process and target process.

When parent test process exits, process watch will try to gracefully terminate target process, and eventually it will kill it.
When target process exists before tests, process watch terminates as well.

## Caveats

It has been noticed that if tests are executed with tools like OpenCover, ProcessWatch is not able to retrieve monitored process information correctly.
