using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSyslogServer.Data;

namespace SuperSyslogServer
{
    public static class SqlHelper
    {
        public static void BulkInsertSyslog(List<Syslog> syslogs)
        {
            var table = new DataTable("Syslogs");

            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("SyslogTimestamp", typeof(DateTime)));
            table.Columns.Add(new DataColumn("ServerTimestamp", typeof(DateTime)));
            table.Columns.Add(new DataColumn("Priority", typeof(string)));
            table.Columns.Add(new DataColumn("MessageId", typeof(long)));
            table.Columns.Add(new DataColumn("Severity", typeof(string)));
            table.Columns.Add(new DataColumn("Message", typeof(string)));
            table.Columns.Add(new DataColumn("IpAddress", typeof(string)));
            table.Columns.Add(new DataColumn("SyslogMessage", typeof(string)));


            foreach (var syslog in syslogs)
            {
                table.Rows.Add(syslog.Id,
                    syslog.SyslogTimestamp,
                    syslog.ServerTimestamp,
                    syslog.Priority,
                    syslog.MessageId,
                    syslog.Severity,
                    syslog.Message,
                    syslog.IpAddress,
                    syslog.SyslogMessage);
            }

            using (SqlBulkCopy bulkCopy =
                new SqlBulkCopy(
                    "data source=(LocalDb)\\MSSQLLocalDB;initial catalog=SuperSyslogServer.SuperSyslogContext;integrated security=True;MultipleActiveResultSets=True;")
            )
            {
                bulkCopy.BulkCopyTimeout = 15;
                bulkCopy.DestinationTableName = "Syslogs";
                bulkCopy.WriteToServer(table);
            }
        }

    }
}
