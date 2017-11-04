﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NLog;
using SuperSyslogServer.Data;

namespace SuperSyslogServer
{
    public class WorkerService
    {
        private readonly Timer _timer;
        private readonly ConcurrentQueue<Syslog> _syslogQueue = new ConcurrentQueue<Syslog>();

        internal WorkerService()
        {
            _timer = new Timer(1000) {AutoReset = true};
            _timer.Elapsed += ElapsedHandler;
            DataContext = new SuperSyslogContext();
        }

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        private bool ServerEnabled { get; set; }
        private int MessagesPerSecond { get; set; }
        private SuperSyslogContext DataContext { get; set; }

        internal void ElapsedHandler(object sender, EventArgs e)
        {
            UpdateMessagesPerSecond();
            StoreSyslogMessages(500);
        }

        private void UpdateMessagesPerSecond()
        {
            if (MessagesPerSecond <= 0) return;
            Logger.Debug($"Messages per Second: {MessagesPerSecond}");
            MessagesPerSecond = 0;
        }

        private void StoreSyslogMessages(int numberOfMessages)
        {
            var messagesToStore = new List<Syslog>();

            for (var i = 0; i < numberOfMessages; i++)
            {
                _syslogQueue.TryDequeue(out var syslog);

                if (syslog == null) break;

                messagesToStore.Add(syslog);
            }
            if (messagesToStore.Count > 0)
                Logger.Info($"Removed {messagesToStore.Count} Syslogs from buffer. Buffer Length: {_syslogQueue.Count}");

            DataContext.Syslog.AddRange(messagesToStore);
            DataContext.SaveChanges();
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

                            MessagesPerSecond++;

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

        internal void Start()
        {
            _timer.Start();

            Logger.Info("Starting Syslog Server");
            ServerEnabled = true;

            UdpListener();
        }

        internal void Stop()
        {
            _timer.Stop();

            Logger.Info("Stopping Syslog Server");
            ServerEnabled = false;
        }
    }
}