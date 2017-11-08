using System;
using System.Threading;
using NLog;
using Topshelf;

namespace SuperSyslogServer
{
    public class Program
    {
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            // Construct the Service
            HostFactory.Run(x =>
            {
                // Main Service Configuration
                x.Service<Service>(sc =>
                {
                    sc.ConstructUsing(name => new Service());
                    sc.WhenStarted(service => service.Start());
                    sc.WhenStopped(service => service.Stop());
                });

                // Name and Description
                x.SetDescription("");
                x.SetDisplayName("Super Syslog Server");
                x.SetServiceName("SuperSyslogServer");

                // Recovery
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.RestartService(0);
                    r.RestartService(0);
                    r.OnCrashOnly();
                    r.SetResetPeriod(1);
                });

                // RunAs
                x.RunAsLocalSystem();

                // Logging
                x.UseNLog();
            });
        }
    }
}