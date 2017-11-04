using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SuperSyslogServer.Data;

namespace SuperSyslogServer
{
    public static class SyslogParser
    {
        public static Syslog Parse(string message, Syslog syslog)
        {
            string txt = message;

            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "(\\d+)";  // Integer Number 1
            string re3 = ".*?"; // Non-greedy match on filler
            string re4 = "(\\d+)";  // Integer Number 2
            string re5 = ".*?"; // Non-greedy match on filler
            string re6 = "((?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Sept|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))"; // Month 1
            string re7 = ".*?"; // Non-greedy match on filler
            string re8 = "((?:(?:[0-2]?\\d{1})|(?:[3][01]{1})))(?![\\d])";  // Day 1
            string re9 = ".*?"; // Non-greedy match on filler
            string re10 = "((?:(?:[0-1][0-9])|(?:[2][0-3])|(?:[0-9])):(?:[0-5][0-9])(?::[0-5][0-9])?(?:\\s?(?:am|AM|pm|PM))?)"; // HourMinuteSec 1
            string re11 = ".*?";    // Non-greedy match on filler
            string re12 = "((?:[a-z][a-z]+))";  // Word 1
            string re13 = ".*?";    // Non-greedy match on filler
            string re14 = "(\\d+)"; // Integer Number 3
            string re15 = ".*?";    // Non-greedy match on filler
            string re16 = "((?:[a-z][a-z]+))";  // Word 2
            string re17 = ".*?";    // Non-greedy match on filler
            string re18 = "((?:[a-z][a-z]+))";  // Word 3

            Regex r = new Regex(re1 + re2 + re3 + re4 + re5 + re6 + re7 + re8 + re9 + re10 + re11 + re12 + re13 + re14 + re15 + re16 + re17 + re18, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(txt);
            if (m.Success)
            {
                String priority = m.Groups[1].ToString();
                String msgId = m.Groups[2].ToString();
                String month1 = m.Groups[3].ToString();
                String day1 = m.Groups[4].ToString();
                String time1 = m.Groups[5].ToString();
                String facility = m.Groups[6].ToString();
                String severity = m.Groups[7].ToString();
                String word2 = m.Groups[8].ToString();
                String msg = m.Groups[9].ToString();


                syslog.Priority = Convert.ToByte(priority);
                syslog.MessageId = Convert.ToInt64(msgId);
                //TODO: DateTime
                syslog.Facility = facility;
                syslog.Severity = Convert.ToByte(severity);
                syslog.Message = msg;
            }
            
            return syslog;
        }
    }
}

