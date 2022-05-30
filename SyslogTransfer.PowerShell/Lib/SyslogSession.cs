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
        public Facility Facility { get; set; }
        public Format Format { get; set; }
        public bool SslEncrypt { get; set; }
        public int SslTimeout { get; set; } 
        public string SslCertFile { get; set; }
        public string SslCertPassword { get; set; }
        public string SslCertFriendryName { get; set; }
        public bool SslIgnoreCheck { get; set; }
    }
}
