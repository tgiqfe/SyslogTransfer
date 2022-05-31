using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogTransfer.Lib.Syslog;

namespace SyslogTransfer.PowerShell.Lib
{
    internal class SyslogSession
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public DateTime Date { get; set; }
        public Facility Facility { get; set; }
        public Severity Severity { get; set; }
        public string HostName { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MsgId { get; set; }

        public bool SslEncrypt { get; set; }
        public int SslTimeout { get; set; }
        public string SslCertFile { get; set; }
        public string SslCertPassword { get; set; }
        public string SslCertFriendryName { get; set; }
        public bool SslIgnoreCheck { get; set; }

        public Format Format { get; set; }

        
    }
}
