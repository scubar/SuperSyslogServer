using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NLog;
using NLog.Config;
using SuperSyslogServer.Data;
using Topshelf.Configurators;

namespace SuperSyslogServer
{
    public class Service
    {
        private readonly Timer _workerServiceTimer;
        private readonly Timer _loggingServiceTimer;
        private readonly ConcurrentQueue<Syslog> _syslogQueue = new ConcurrentQueue<Syslog>();
        private readonly RateLimiter _rateLimiter = new RateLimiter();

        internal Service()
        {
            DataContext = new SuperSyslogContext();

            _loggingServiceTimer = new Timer(1000) {AutoReset = true};
            _loggingServiceTimer.Elapsed += LoggingService;

            _workerServiceTimer = new Timer(1000) {AutoReset = true};
            _workerServiceTimer.Elapsed += WorkerService;
        }

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        private bool ServerEnabled { get; set; }
        private SuperSyslogContext DataContext { get; set; }

        internal void WorkerService(object sender, EventArgs e)
        {
            BulkInsert(2500);
        }

        internal void LoggingService(object sender, EventArgs e)
        {
            UpdateLogging();
        }

        internal void UdpListener()
        {
            Task.Run(() =>
            {
                using (var udpClient = new UdpClient(514))
                {
                    Logger.Info("UDP Listener Started.");
                    while (ServerEnabled)
                        try
                        {
                            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                            var recieveBuffer = udpClient.Receive(ref ipEndPoint);

                            var allowed = false;

                            lock (_rateLimiter)
                            {
                               allowed = _rateLimiter.AllowMessage(ipEndPoint.Address);
                            }

                            if (!allowed) continue;

                            var recieveString = Encoding.ASCII.GetString(recieveBuffer);
                            var sourceIp = ipEndPoint.Address.ToString();

                            _syslogQueue.Enqueue(new Syslog(sourceIp, recieveString));
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex.Message);
                        }
                    Logger.Info("UDP Listener Stopped.");
                }
            });
        }

        private void UpdateLogging()
        {
            var messagesPerSecond = 0;
            var allowedMessagesPerSecond = 0;
            SyslogSender topTalker;

            lock (_rateLimiter)
            {
                if (_rateLimiter.GlobalMessagesPerSecond <= 0) return;
                messagesPerSecond = _rateLimiter.GlobalMessagesPerSecond;
                allowedMessagesPerSecond = _rateLimiter.GlobalAllowedMessagesPerSecond;
                topTalker = _rateLimiter.SyslogSenders.OrderByDescending(s => s.MessagesPerSecond).First();

                _rateLimiter.Reset();
            }

            Logger.Debug($"Received/Allowed Messages per Second: {messagesPerSecond}/{allowedMessagesPerSecond} (Top Sender: {topTalker.IPAddress} Received/Allowed Messages per Second: {topTalker.MessagesPerSecond}/{topTalker.AllowedMessagesPerSecond})");

            if (topTalker.MessagesPerSecond > topTalker.AllowedMessagesPerSecond)
                Logger.Warn($"{topTalker.MessagesPerSecond - topTalker.AllowedMessagesPerSecond} Messages from {topTalker.IPAddress} were discarded due to sender rate limiter.");

            if (messagesPerSecond > allowedMessagesPerSecond)
                Logger.Warn($"{messagesPerSecond - allowedMessagesPerSecond} Messages were discarded by global rate limiter.");
        }

        private void BulkInsert(int dequeueCount)
        {
            var syslogBulkInsertList = new List<Syslog>();

            for (var i = 0; i < dequeueCount; i++)
            {
                _syslogQueue.TryDequeue(out var syslog);

                if (syslog == null) break;

                syslog.Expand();

                syslogBulkInsertList.Add(syslog);
            }

            if (syslogBulkInsertList.Count <= 0) return;

            var syslogBulkInsertTimer = Stopwatch.StartNew();
            SqlHelper.BulkInsertSyslog(syslogBulkInsertList);
            syslogBulkInsertTimer.Stop();

            Logger.Debug($"Bulk Inserted {syslogBulkInsertList.Count} Syslog(s) in {syslogBulkInsertTimer.ElapsedMilliseconds}ms. Buffer length: {_syslogQueue.Count}.");
        }

        internal void Start()
        {
            _workerServiceTimer.Start();
            _loggingServiceTimer.Start();

            Logger.Info("Starting Syslog Server");
            ServerEnabled = true;

            UdpListener();
        }

        internal void Stop()
        {
            _workerServiceTimer.Stop();
            _loggingServiceTimer.Start();

            Logger.Info("Stopping Syslog Server");
            ServerEnabled = false;
        }
    }
}