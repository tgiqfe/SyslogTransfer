using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using SyslogTransfer.Lib.Syslog;

namespace SyslogTransfer.PowerShell.Cmdlet
{
    [Cmdlet(VerbsCommunications.Send, "SyslogMessage")]
    internal class SendSyslogMessage : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Server { get; set; }

        [Parameter, ValidateSet("Udp", "Tcp")]
        public string Protocol { get; set; } = "Udp";

        [Parameter]
        public Facility Facility { get; set; }

        [Parameter]
        public Severity Severity { get; set; }
        
        [Parameter]
        public Format Format { get; set; }

        


    }
}
