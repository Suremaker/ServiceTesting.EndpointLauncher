using System.Diagnostics;
using NUnit.Framework;

namespace Wonga.ServiceTesting.EndpointLauncher.Tests
{
    [TestFixture]
    public class ProcessWatchTests
    {
        private const string TimeoutPath = @"c:\windows\system32\timeout.exe";

        [Test]
        public void ProcessWatch_should_kill_child_and_exit_when_parent_dies()
        {
            var parent = CreateProcess();
            var child = CreateProcess();
            var watch = CreateProcessWatch(parent, child);

            Assert.False(watch.WaitForExit(1000), "Process watch has terminated, while it should be running");
            parent.Kill();
            Assert.True(child.WaitForExit(1000), "Child process should be terminated");
            Assert.True(watch.WaitForExit(1000), "ProcessWatch should terminate");
        }

        [Test]
        public void ProcessWatch_should_kill_child_and_terminate_even_if_child_process_does_not_have_window()
        {
            var parent = CreateProcess();
            var child = CreateProcess(false);
            var watch = CreateProcessWatch(parent, child);

            Assert.False(watch.WaitForExit(1000), "Process watch has terminated, while it should be running");
            parent.Kill();
            Assert.True(child.WaitForExit(1000), "Child process should be terminated");
            Assert.True(watch.WaitForExit(1000), "ProcessWatch should terminate");
        }

        [Test]
        public void ProcessWatch_should_terminate_when_child_dies_but_parent_should_be_left_intact()
        {
            var parent = CreateProcess();
            var child = CreateProcess();
            var watch = CreateProcessWatch(parent, child);

            Assert.False(watch.WaitForExit(1000), "Process watch has terminated, while it should be running");
            child.Kill();
            Assert.True(watch.WaitForExit(1000), "Child process should be terminated");
            Assert.False(parent.WaitForExit(1000), "Parent process should stay running");
            parent.Kill();
        }

        private Process CreateProcessWatch(Process parent, Process child)
        {
            return Process.Start(new ProcessStartInfo("Wonga.ServiceTesting.ProcessWatch.exe", string.Format("{0} {1}", parent.Id, child.Id)) { UseShellExecute = false });
        }

        private Process CreateProcess(bool withWindow = true)
        {
            return Process.Start(new ProcessStartInfo(TimeoutPath, "/T 6") { UseShellExecute = false, CreateNoWindow = !withWindow, WindowStyle = withWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden });
        }
    }
}