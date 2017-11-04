using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSyslogServer.Data
{
    public class Syslog
    {
        public Syslog(string ipAddress, string body)
        {
            IpAddress = ipAddress;

            //TODO: Parse Body
            SyslogParser.Parse(body, this);
        }

        public int Id { get; set; }

        public long MessageId { get; set; }

        public string Message { get; set; }

        public string IpAddress { get; set; }
        public byte Priority { get; set; }

        public byte Severity { get; set; }

        public string Facility { get; set; }

        public DateTime SyslogTimestamp { get; set; } = DateTime.UtcNow; //TODO: Parse from actual syslog

        public DateTime ServerTimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
