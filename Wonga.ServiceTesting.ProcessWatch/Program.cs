using System;
using System.Diagnostics;

namespace Wonga.ServiceTesting.ProcessWatch
{
    class Program
    {
        static int Main(string[] args)
        {
            int parentPid;
            int childPid;

            if (args.Length != 2 || !int.TryParse(args[0], out parentPid) || !int.TryParse(args[1], out childPid))
            {
                Console.WriteLine("Usage: Wonga.ServiceTesting.ProcessWatch.exe [parentPID] [childPID]");
                return 1;
            }

            try
            {
                var parent = Process.GetProcessById(parentPid);
                var child = Process.GetProcessById(childPid);

                while (true)
                {
                    if (child.WaitForExit(500))
                        return 0;
                    if (parent.WaitForExit(500))
                    {
                        KillChildProcess(child);
                        return 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }

        private static void KillChildProcess(Process childProcess)
        {
            if (!childProcess.CloseMainWindow() || !childProcess.WaitForExit(3000))
                childProcess.Kill();
        }
    }
}
