using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
            SyslogMessage = body;
        }

        public Syslog() { }

        /// <summary>
        /// Parses Syslog data and populates fields.
        /// </summary>
        public void Expand()
        {
            //TODO: Parse Syslog and populate fields.
        }

        [Key]
        public int Id { get; set; }
        [Index]
        public DateTime SyslogTimestamp { get; set; } = DateTime.UtcNow;
        [Index]
        public DateTime ServerTimestamp { get; set; } = DateTime.UtcNow;
        [Index]
        public byte Priority { get; set; }
        [Index]
        public long MessageId { get; set; }
        [Index]
        public byte Severity { get; set; }
        public string Message { get; set; }
        public string IpAddress { get; set; }
        public string SyslogMessage { get; set; }
    }
}
