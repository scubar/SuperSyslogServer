using System.Net;

namespace SuperSyslogServer
{
    public class SyslogSender
    {
        public SyslogSender(IPAddress ipAddress)
        {
            IPAddress = ipAddress;
        }

        public SyslogSender() { }

        public IPAddress IPAddress { get; set; }

        public int MessagesPerSecond { get; set; }

        public int AllowedMessagesPerSecond { get; set; }
    }
}