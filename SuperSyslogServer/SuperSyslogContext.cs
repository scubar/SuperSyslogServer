using SuperSyslogServer.Data;

namespace SuperSyslogServer
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class SuperSyslogContext : DbContext
    {
        public SuperSyslogContext()
            : base("name=SuperSyslogContext")
        {
        }

        public virtual DbSet<Syslog> Syslog { get; set; }
    }
}