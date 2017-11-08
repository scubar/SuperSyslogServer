using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SuperSyslogServer
{
    public class Telemetry
    {
        public int GlobalMaxMessagesPerSecond = 1500;
        public int SenderMaxMessagesPerSecond = 1000;
        public int GlobalMessagesPerSecond { get; set; }
        public int GlobalAllowedMessagesPerSecond { get; set; }
        public HashSet<SyslogSender> SyslogSenders { get; set; } = new HashSet<SyslogSender>();

        public bool AllowMessage(IPAddress sourceIp)
        {
            // Sender Rate Limit

            var allowed = SenderRateLimiter(sourceIp);

            if (!allowed) return false;

            // Global Rate Limit

            allowed = GlobalRateLimiter();

            return allowed;
        }

        private bool GlobalRateLimiter()
        {
            GlobalMessagesPerSecond++;

            var allowed = GlobalMessagesPerSecond <= GlobalMaxMessagesPerSecond;

            if (allowed)
                GlobalAllowedMessagesPerSecond++;

            return allowed;
        }

        private bool SenderRateLimiter(IPAddress sourceIp)
        {
            var sender = SyslogSenders.FirstOrDefault(s => sourceIp.Equals(s.IPAddress));

            if (sender == null)
            {
                sender = new SyslogSender(sourceIp);
                SyslogSenders.Add(sender);
            }

            sender.MessagesPerSecond++;

            var allowed = sender.MessagesPerSecond <= SenderMaxMessagesPerSecond;

            if (allowed)
                sender.AllowedMessagesPerSecond++;

            return allowed;
        }

        public void Reset()
        {
            GlobalMessagesPerSecond = 0;
            GlobalAllowedMessagesPerSecond = 0;
            SyslogSenders = new HashSet<SyslogSender>();
        }
    }
}